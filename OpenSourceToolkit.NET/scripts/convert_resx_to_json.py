#!/usr/bin/env python3
"""
Convert .resx files to JSON format for WASM-compatible runtime localization.

This script converts ToolkitStrings*.resx files to JSON format, which can be
loaded at runtime in the browser without relying on .NET ResourceManager.

Usage:
    python convert_resx_to_json.py

Output:
    Creates Localization/*.json files alongside the .resx files:
    - ToolkitStrings.json          (invariant/English)
    - ToolkitStrings.de.json       (German)
    - ToolkitStrings.fr.json       (French)
    - etc.
"""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path
from typing import NamedTuple


class ResxEntry(NamedTuple):
    """Represents a single resource entry."""
    key: str
    value: str
    comment: str | None


def parse_resx_file(path: Path) -> list[ResxEntry]:
    """
    Parse a .resx file and return a list of ResxEntry objects.
    
    Extracts only <data> entries, ignoring metadata and schema.
    """
    content = path.read_text(encoding='utf-8')
    entries = []
    
    # Match <data name="..."> ... </data> blocks
    pattern = r'<data\s+name="([^"]+)"[^>]*>(.*?)</data>'
    
    for match in re.finditer(pattern, content, re.DOTALL):
        key = match.group(1)
        block = match.group(2)
        
        # Extract value
        value_match = re.search(r'<value>(.*?)</value>', block, re.DOTALL)
        value = value_match.group(1).strip() if value_match else ""
        
        # Extract optional comment
        comment_match = re.search(r'<comment>(.*?)</comment>', block, re.DOTALL)
        comment = comment_match.group(1).strip() if comment_match else None
        
        # Decode XML entities
        value = decode_xml_entities(value)
        if comment:
            comment = decode_xml_entities(comment)
        
        entries.append(ResxEntry(key=key, value=value, comment=comment))
    
    return entries


def decode_xml_entities(text: str) -> str:
    """Decode common XML entities to their character equivalents."""
    replacements = [
        ('&amp;', '&'),
        ('&lt;', '<'),
        ('&gt;', '>'),
        ('&quot;', '"'),
        ('&apos;', "'"),
    ]
    for entity, char in replacements:
        text = text.replace(entity, char)
    return text


def convert_resx_to_json(resx_path: Path, json_path: Path) -> tuple[int, int]:
    """
    Convert a single .resx file to JSON format.
    
    Returns: (entry_count, error_count)
    """
    entries = parse_resx_file(resx_path)
    
    # Build dictionary preserving order
    data = {}
    for entry in entries:
        data[entry.key] = entry.value
    
    # Write JSON with proper formatting
    json_path.write_text(
        json.dumps(data, ensure_ascii=False, indent=2),
        encoding='utf-8'
    )
    
    return len(entries), 0


def get_culture_from_filename(filename: str, base_name: str) -> str:
    """
    Extract culture code from localized filename.
    
    Examples:
        ToolkitStrings.resx -> "" (invariant)
        ToolkitStrings.de.resx -> "de"
        ToolkitStrings.zh-Hans.resx -> "zh-Hans"
    """
    stem = filename.replace('.resx', '')
    if stem == base_name:
        return ""  # Invariant/English
    
    # Remove base name prefix and dot
    if stem.startswith(base_name + '.'):
        return stem[len(base_name) + 1:]
    
    return ""


def main() -> int:
    """Main entry point."""
    # Find the Localization directory
    script_dir = Path(__file__).parent
    localization_dir = script_dir.parent / "Localization"
    
    if not localization_dir.exists():
        print(f"ERROR: Localization directory not found: {localization_dir}")
        return 1
    
    # Find all ToolkitStrings*.resx files
    default_resx = localization_dir / "ToolkitStrings.resx"
    if not default_resx.exists():
        print(f"ERROR: Default .resx file not found: {default_resx}")
        return 1
    
    resx_files = sorted(localization_dir.glob("ToolkitStrings*.resx"))
    
    print("=" * 60)
    print("RESX to JSON Converter")
    print("=" * 60)
    print(f"Source directory: {localization_dir}")
    print(f"Found {len(resx_files)} .resx file(s)")
    print()
    
    total_entries = 0
    converted_files = []
    
    for resx_path in resx_files:
        # Determine output JSON path
        json_filename = resx_path.name.replace('.resx', '.json')
        json_path = localization_dir / json_filename
        
        culture = get_culture_from_filename(resx_path.name, "ToolkitStrings")
        culture_display = culture if culture else "(invariant)"
        
        try:
            count, errors = convert_resx_to_json(resx_path, json_path)
            total_entries += count
            converted_files.append((json_path.name, count, culture))
            print(f"  [OK] {resx_path.name} -> {json_path.name} ({count} entries) [{culture_display}]")
        except Exception as e:
            print(f"  [ERROR] {resx_path.name}: {e}")
            return 1
    
    print()
    print("-" * 60)
    print(f"Converted {len(converted_files)} file(s) with {total_entries} total entries")
    print()
    
    # Print summary of cultures
    print("Cultures exported:")
    for filename, count, culture in converted_files:
        culture_display = culture if culture else "en (default)"
        print(f"  - {culture_display}: {filename}")
    
    print()
    print("=" * 60)
    print("CONVERSION COMPLETE")
    print("=" * 60)
    print()
    print("Next steps:")
    print("  1. Add the .json files to the project as EmbeddedResource")
    print("  2. Update ToolkitLocalization.cs to load from JSON")
    print("  3. Test language switching in the browser")
    
    return 0


if __name__ == "__main__":
    sys.exit(main())
