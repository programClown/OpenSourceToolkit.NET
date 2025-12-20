from __future__ import annotations

import argparse
import re
import sys
import tempfile
import shutil
from pathlib import Path
from collections import defaultdict


IGNORED_UNTRANSLATED_KEYS = {
    "Cron_Expression_Watermark",
    "Hmac_Sha256_Label",
    "Hmac_Sha512_Label",
    "StopwatchTimer_ClearLaps_Button",
    "Color_ValuePrefix_Hex",
    "Color_ValuePrefix_Rgb",
    "Color_ValuePrefix_Hsl",
    "Color_ConverterSection_RGB_Label",
    "Color_ConverterSection_HSL_Label",
    "Color_ConverterSection_HSV_Label",
    "Color_ConverterSection_CMYK_Label",
    "Color_ConverterSection_LAB_Label",
}


def _configure_stdio() -> None:
    try:
        sys.stdout.reconfigure(encoding="utf-8", errors="backslashreplace")
    except Exception:
        pass
    try:
        sys.stderr.reconfigure(encoding="utf-8", errors="backslashreplace")
    except Exception:
        pass


def _parse_resx_entries(path: Path) -> list[tuple[str, str, str]]:
    """
    Parse a .resx file and return a list of (key, value, raw_xml_block).
    This preserves the original XML structure for each data entry.
    """
    content = path.read_text(encoding='utf-8')
    entries = []

    # Match <data name="..."> ... </data> blocks
    pattern = r'(<data\s+name="([^"]+)"[^>]*>.*?</data>)'
    for match in re.finditer(pattern, content, re.DOTALL):
        raw_block = match.group(1)
        key = match.group(2)

        # Extract value
        value_match = re.search(r'<value>(.*?)</value>', raw_block, re.DOTALL)
        value = value_match.group(1).strip() if value_match else ""

        entries.append((key, value, raw_block))

    return entries


def _extract_resx_keys(path: Path) -> set[str]:
    """Extract just the key names from a .resx file."""
    entries = _parse_resx_entries(path)
    return set(key for key, _, _ in entries)


def _extract_resx_keys_and_values(path: Path) -> dict[str, str]:
    """Extract key-value pairs from a .resx file (uses first occurrence)."""
    entries = _parse_resx_entries(path)
    result = {}
    for key, value, _ in entries:
        if key not in result:
            result[key] = value
    return result


def _get_resx_header(path: Path) -> str:
    """Extract the header portion of a .resx file (before the first <data> entry)."""
    content = path.read_text(encoding='utf-8')

    # Find the first <data entry and take everything before it
    match = re.search(r'(<data\s+name=")', content)
    if match:
        return content[:match.start()]
    return content


def fix_duplicates(default_resx: Path, target_resx: Path) -> dict:
    """
    Fix duplicates in the target .resx file.
    Returns stats about what was done.
    """
    # Parse files
    default_entries = _parse_resx_entries(default_resx)
    default_keys_values = {key: value for key, value, _ in default_entries}
    default_order = [key for key, _, _ in default_entries]

    target_entries = _parse_resx_entries(target_resx)

    # Group target entries by key
    target_by_key = defaultdict(list)
    for key, value, raw_block in target_entries:
        target_by_key[key].append((value, raw_block))

    # Statistics
    stats = {
        "total_in_target": len(target_entries),
        "unique_keys": len(target_by_key),
        "duplicates_found": 0,
        "duplicates_identical": 0,
        "duplicates_one_english": 0,
        "duplicates_different": 0,
        "keys_written": 0,
        "untranslated_count": 0,
        "untranslated_keys": [],
        "success": False,
    }

    # Build the new content
    final_entries = {}  # key -> (value, raw_block)
    untranslated_keys = []  # Track keys that are still English

    for key in default_order:
        if key not in target_by_key:
            # Missing key - skip (shouldn't happen after our insertions)
            continue

        occurrences = target_by_key[key]
        english_value = default_keys_values.get(key, "")

        if len(occurrences) == 1:
            # No duplicate, use as-is
            final_entries[key] = occurrences[0]
            # Check if this is still English (untranslated)
            if occurrences[0][0] == english_value and key not in IGNORED_UNTRANSLATED_KEYS:
                untranslated_keys.append(key)
        else:
            # Duplicate found!
            stats["duplicates_found"] += 1

            # Check if all duplicates are identical
            unique_values = set(v for v, _ in occurrences)

            if len(unique_values) == 1:
                # All identical - just use the first
                stats["duplicates_identical"] += 1
                final_entries[key] = occurrences[0]
                # Check if this is still English
                if occurrences[0][0] == english_value and key not in IGNORED_UNTRANSLATED_KEYS:
                    untranslated_keys.append(key)
            else:
                # Different values - figure out which to keep
                non_english = [(v, b) for v, b in occurrences if v != english_value]

                if len(non_english) >= 1:
                    # Keep the first non-English version
                    stats["duplicates_one_english"] += 1
                    final_entries[key] = non_english[0]
                else:
                    # All are English somehow - just use first
                    stats["duplicates_different"] += 1
                    final_entries[key] = occurrences[0]
                    if key not in IGNORED_UNTRANSLATED_KEYS:
                        untranslated_keys.append(key)

    stats["untranslated_count"] = len(untranslated_keys)
    stats["untranslated_keys"] = untranslated_keys

    # Track keys that are in target but not in default (extra keys - to be REMOVED)
    extra_keys = []
    for key in target_by_key:
        if key not in default_order:
            extra_keys.append(key)
            # DO NOT add to final_entries - we're discarding these

    # Safety check: ensure we have all default keys
    missing_keys = set(default_order) - set(final_entries.keys())
    if missing_keys:
        print(f"  Error: Missing {len(missing_keys)} keys from default file!")
        for k in sorted(missing_keys)[:10]:
            print(f"    - {k}")
        if len(missing_keys) > 10:
            print(f"    ... and {len(missing_keys) - 10} more")
        print(f"  Not modifying {target_resx.name} to prevent data loss.")
        return stats

    stats["extra_keys_removed"] = len(extra_keys)

    # Build the new file content
    header = _get_resx_header(target_resx)

    # Build data section in the order of the default file ONLY
    data_lines = []
    for key in default_order:
        if key in final_entries:
            _, raw_block = final_entries[key]
            # Indent properly
            indented = "  " + raw_block.strip()
            data_lines.append(indented)

    # Build final content
    new_content = header.rstrip() + "\n\n"
    new_content += "\n".join(data_lines)
    new_content += "\n</root>\n"

    # Write atomically using temp file
    temp_path = target_resx.with_suffix('.tmp')
    try:
        temp_path.write_text(new_content, encoding='utf-8')

        # Verify the temp file has EXACTLY the default key count
        temp_entries = _parse_resx_entries(temp_path)
        if len(temp_entries) != len(default_keys_values):
            print(f"  Error: Temp file has wrong key count ({len(temp_entries)} vs expected {len(default_keys_values)})")
            temp_path.unlink()
            return stats

        # Replace original with temp
        shutil.move(str(temp_path), str(target_resx))
        stats["keys_written"] = len(temp_entries)
        stats["success"] = True

    except Exception as e:
        print(f"  Error writing file: {e}")
        if temp_path.exists():
            temp_path.unlink()
        return stats

    return stats


def main(argv: list[str]) -> int:
    _configure_stdio()
    parser = argparse.ArgumentParser(
        description=(
            "Verify that all localized .resx files contain all keys from a default .resx file. "
            "Only checks <data name=...> entries."
        )
    )
    parser.add_argument(
        "default_resx",
        type=Path,
        help="Path to the default .resx file (e.g. ToolkitStrings.resx)",
    )
    parser.add_argument(
        "directory",
        type=Path,
        nargs="?",
        default=None,
        help="Directory containing localized .resx files (defaults to default_resx parent)",
    )
    parser.add_argument(
        "-v", "--verbose",
        action="store_true",
        help="Show all missing keys (default: only show counts)",
    )
    parser.add_argument(
        "-s", "--summary",
        action="store_true",
        help="Show only a one-line summary per file",
    )
    parser.add_argument(
        "-f", "--file",
        type=str,
        default=None,
        help="Check only a specific localized file (e.g. 'fr' for ToolkitStrings.fr.resx)",
    )
    parser.add_argument(
        "--all",
        action="store_true",
        help="Check all localized files in the directory (requires omitting -f/--file).",
    )
    parser.add_argument(
        "--duplicates",
        action="store_true",
        help="Find and fix duplicate keys. Rewrites files atomically in default file order.",
    )
    parser.add_argument(
        "--show-untranslated",
        action="store_true",
        help="Show keys whose values are still identical to the default (English) values.",
    )

    args = parser.parse_args(argv)

    default_resx: Path = args.default_resx
    directory: Path = args.directory if args.directory is not None else default_resx.parent
    verbose: bool = args.verbose
    summary_only: bool = args.summary
    single_file: str = args.file
    fix_dupes: bool = args.duplicates
    show_untranslated: bool = args.show_untranslated
    all_files: bool = args.all

    if single_file == "all":
        print("ERROR: Use --all to check all languages. Do not pass -f all.", file=sys.stderr)
        return 2

    if not default_resx.is_file():
        print(f"ERROR: default_resx not found: {default_resx}", file=sys.stderr)
        return 2

    if not directory.is_dir():
        print(f"ERROR: directory not found: {directory}", file=sys.stderr)
        return 2

    try:
        default_keys = _extract_resx_keys(default_resx)
        default_kv = _extract_resx_keys_and_values(default_resx)
    except Exception as e:
        print(f"ERROR: {e}", file=sys.stderr)
        return 2

    localized_files = sorted(directory.glob(f"{default_resx.stem}.*{default_resx.suffix}"))
    localized_files = [p for p in localized_files if p.name != default_resx.name]

    if not all_files and not single_file:
        print("ERROR: Specify a language using -f/--file, or use --all to check all languages.", file=sys.stderr)
        print("Example: python check_resx_keys.py ToolkitStrings.resx -f tr", file=sys.stderr)
        print("Example: python check_resx_keys.py ToolkitStrings.resx --all", file=sys.stderr)
        return 2

    if all_files and single_file:
        print("ERROR: Use either -f/--file OR --all (not both).", file=sys.stderr)
        return 2

    # Filter to single file if specified
    target_name = None
    if single_file:
        target_name = f"{default_resx.stem}.{single_file}{default_resx.suffix}"
        localized_files = [p for p in localized_files if p.name == target_name]

    if not localized_files:
        if single_file:
            print(f"File not found: {target_name}")
        else:
            print(f"No localized files found in {directory} matching {default_resx.stem}.*{default_resx.suffix}")
        return 0

    # Handle --duplicates mode
    if fix_dupes:
        if not single_file:
            print("ERROR: --duplicates requires -f/--file to specify which localized file to fix", file=sys.stderr)
            print("Example: python check_resx_keys.py ToolkitStrings.resx --duplicates -f uk", file=sys.stderr)
            return 2

        if not localized_files:
            print(f"ERROR: File not found: {target_name}", file=sys.stderr)
            return 2

        target_path = localized_files[0]

        print(f"\n=== Duplicate Key Fixer ===")
        print(f"Default file: {default_resx.name} ({len(default_keys)} keys)")
        print(f"Target file:  {target_path.name}")
        print()

        # Count duplicates before fixing
        entries = _parse_resx_entries(target_path)
        key_counts = defaultdict(int)
        for key, _, _ in entries:
            key_counts[key] += 1
        dupe_count = sum(1 for k, c in key_counts.items() if c > 1)
        extra_count = sum(1 for k in key_counts if k not in default_keys)

        print(f"Target has {len(entries)} entries, {len(key_counts)} unique keys")
        print(f"  - {dupe_count} duplicate key(s)")
        print(f"  - {extra_count} extra key(s) not in default (will be removed)")

        # Show which keys are duplicated
        if verbose and dupe_count > 0:
            print("\nDuplicated keys:")
            for key, count in sorted(key_counts.items()):
                if count > 1:
                    print(f"  - {key} (x{count})")

        if verbose and extra_count > 0:
            print("\nExtra keys to be removed:")
            for key in sorted(key_counts.keys()):
                if key not in default_keys:
                    print(f"  - {key}")

        # Fix duplicates and remove extra keys
        stats = fix_duplicates(default_resx, target_path)

        print()
        print(f"{'='*50}")
        if stats["success"]:
            print(f"[OK] Fixed! {stats['keys_written']} keys written")
            print(f"  - {stats['duplicates_found']} duplicates resolved:")
            print(f"      {stats['duplicates_identical']} identical (kept first)")
            print(f"      {stats['duplicates_one_english']} had English fallback (kept translated)")
            print(f"      {stats['duplicates_different']} different translations (kept first non-English)")
            print(f"  - {stats.get('extra_keys_removed', 0)} extra keys removed")
            untranslated = stats.get('untranslated_count', 0)
            if untranslated > 0:
                print(f"  [!] WARNING: {untranslated} keys still have English values (untranslated)")
                if show_untranslated:
                    print("\nUntranslated keys (still English):")
                    for k in stats.get("untranslated_keys", []):
                        print(f"  - {k}")
        else:
            print(f"[FAIL] Could not fix (safety check failed)")
        print(f"{'='*50}")

        return 0 if stats["success"] else 1

    # Regular check mode (existing functionality)
    had_missing = False
    results = []
    untranslated_by_file: dict[str, list[str]] = {}

    for resx_path in localized_files:
        try:
            keys = _extract_resx_keys(resx_path)
        except Exception as e:
            print(f"ERROR: {e}", file=sys.stderr)
            had_missing = True
            continue

        missing = sorted(default_keys - keys)
        extra = sorted(keys - default_keys)

        if missing or extra:
            had_missing = True

        # Collect results for summary
        results.append({
            "file": resx_path.name,
            "total": len(keys),
            "missing": len(missing),
            "extra": len(extra),
            "missing_keys": missing,
        })

        if show_untranslated:
            try:
                localized_kv = _extract_resx_keys_and_values(resx_path)
            except Exception:
                localized_kv = {}

            untranslated = []
            for k, default_value in default_kv.items():
                if k in IGNORED_UNTRANSLATED_KEYS:
                    continue
                if localized_kv.get(k, None) == default_value:
                    untranslated.append(k)
            untranslated_by_file[resx_path.name] = untranslated

    # Output based on verbosity
    if summary_only:
        # One-line summary
        total_missing = sum(r["missing"] for r in results)
        ok_count = sum(1 for r in results if r["missing"] == 0)
        print(f"Checked {len(results)} files, {ok_count} complete, {total_missing} total missing keys")
    else:
        # Table output
        print(f"\n{'File':<30} {'Keys':>6} {'Missing':>8} {'Extra':>6}")
        print("-" * 52)
        for r in results:
            status = "OK" if r["missing"] == 0 else "X"
            print(f"{r['file']:<30} {r['total']:>6} {r['missing']:>8} {r['extra']:>6} {status}")

        if verbose and had_missing:
            print("\n--- Missing Keys ---")
            for r in results:
                if r["missing_keys"]:
                    print(f"\n{r['file']} ({r['missing']} missing):")
                    for k in r["missing_keys"]:
                        print(f"  - {k}")

        if show_untranslated:
            print("\n--- Untranslated Keys (still English) ---")
            any_untranslated = False
            for r in results:
                fname = r["file"]
                keys_list = untranslated_by_file.get(fname, [])
                if keys_list:
                    any_untranslated = True
                    print(f"\n{fname} ({len(keys_list)} untranslated):")
                    for k in keys_list:
                        print(f"  - {k}")
            if not any_untranslated:
                print("\nAll localized files have zero untranslated keys (excluding ignored list).")

    if had_missing:
        return 1

    if not summary_only:
        print(f"\n[OK] All {len(localized_files)} files contain all {len(default_keys)} keys.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))

