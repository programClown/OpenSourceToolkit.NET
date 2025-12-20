#!/usr/bin/env python3
"""
Count total LoC (non-empty, comments removed) across all *.cs and *.axaml files.

Default output: a single integer (the total).

Notes:
- C#: strips // and /* */ while respecting string/char literals (including verbatim strings @"...").
- AXAML: strips XML comments <!-- ... -->.
- Uses `git ls-files` when available to avoid counting generated files in bin/obj.
"""

from __future__ import annotations

import argparse
import os
import subprocess
from pathlib import Path


def _strip_xml_comments(text: str) -> str:
    out: list[str] = []
    i = 0
    n = len(text)
    in_comment = False

    while i < n:
        if not in_comment and text.startswith("<!--", i):
            in_comment = True
            i += 4
            continue

        if in_comment:
            if text.startswith("-->", i):
                in_comment = False
                i += 3
                continue
            # Preserve newlines so line counting stays stable.
            ch = text[i]
            if ch == "\n":
                out.append("\n")
            i += 1
            continue

        out.append(text[i])
        i += 1

    return "".join(out)


def _strip_csharp_comments(text: str) -> str:
    out: list[str] = []
    i = 0
    n = len(text)

    NORMAL = 0
    LINE_COMMENT = 1
    BLOCK_COMMENT = 2
    STRING = 3
    VERBATIM_STRING = 4
    CHAR = 5

    state = NORMAL

    while i < n:
        ch = text[i]
        nxt = text[i + 1] if i + 1 < n else ""

        if state == NORMAL:
            if ch == "/" and nxt == "/":
                state = LINE_COMMENT
                i += 2
                continue
            if ch == "/" and nxt == "*":
                state = BLOCK_COMMENT
                i += 2
                continue
            if ch == "@" and nxt == '"':
                state = VERBATIM_STRING
                out.append('@')
                out.append('"')
                i += 2
                continue
            if ch == '"':
                state = STRING
                out.append(ch)
                i += 1
                continue
            if ch == "'":
                state = CHAR
                out.append(ch)
                i += 1
                continue

            out.append(ch)
            i += 1
            continue

        if state == LINE_COMMENT:
            if ch == "\n":
                out.append("\n")
                state = NORMAL
            i += 1
            continue

        if state == BLOCK_COMMENT:
            if ch == "*" and nxt == "/":
                state = NORMAL
                i += 2
                continue
            if ch == "\n":
                out.append("\n")
            i += 1
            continue

        if state == STRING:
            out.append(ch)
            if ch == "\\" and i + 1 < n:
                out.append(text[i + 1])
                i += 2
                continue
            if ch == '"':
                state = NORMAL
            i += 1
            continue

        if state == VERBATIM_STRING:
            out.append(ch)
            if ch == '"':
                # In verbatim strings, "" is an escaped quote.
                if nxt == '"':
                    out.append('"')
                    i += 2
                    continue
                state = NORMAL
            i += 1
            continue

        if state == CHAR:
            out.append(ch)
            if ch == "\\" and i + 1 < n:
                out.append(text[i + 1])
                i += 2
                continue
            if ch == "'":
                state = NORMAL
            i += 1
            continue

    return "".join(out)


def _count_non_empty_lines(text: str) -> int:
    return sum(1 for line in text.splitlines() if line.strip())


def _try_git_root(start: Path) -> Path | None:
    try:
        p = subprocess.run(
            ["git", "-C", str(start), "rev-parse", "--show-toplevel"],
            stdout=subprocess.PIPE,
            stderr=subprocess.DEVNULL,
            text=True,
            check=True,
        )
        return Path(p.stdout.strip())
    except Exception:
        return None


def _git_files(repo_root: Path) -> list[Path]:
    p = subprocess.run(
        ["git", "-C", str(repo_root), "ls-files"],
        stdout=subprocess.PIPE,
        stderr=subprocess.DEVNULL,
        text=True,
        check=True,
    )
    return [repo_root / line for line in p.stdout.splitlines() if line]


def _walk_files(repo_root: Path) -> list[Path]:
    skip_dirs = {
        ".git",
        ".vs",
        ".idea",
        ".vscode",
        "bin",
        "obj",
        "TestResults",
        "terminals",
    }
    files: list[Path] = []
    for root, dirs, filenames in os.walk(repo_root):
        dirs[:] = [d for d in dirs if d not in skip_dirs]
        for name in filenames:
            files.append(Path(root) / name)
    return files


def main() -> int:
    parser = argparse.ArgumentParser(description="Count total LoC (no comments) for *.cs and *.axaml.")
    parser.add_argument("path", nargs="?", default=".", help="Repo root (defaults to current directory).")
    parser.add_argument("--no-git", action="store_true", help="Do not use git; walk the filesystem instead.")
    args = parser.parse_args()

    start = Path(args.path).resolve()
    repo_root = _try_git_root(start) if not args.no_git else None
    if repo_root is None:
        repo_root = start

    all_paths = _git_files(repo_root) if (not args.no_git and _try_git_root(repo_root) is not None) else _walk_files(repo_root)

    total = 0
    for path in all_paths:
        suffix = path.suffix.lower()
        if suffix not in {".cs", ".axaml"}:
            continue
        if not path.is_file():
            continue

        try:
            text = path.read_text(encoding="utf-8", errors="ignore")
        except Exception:
            continue

        if suffix == ".cs":
            stripped = _strip_csharp_comments(text)
        else:
            stripped = _strip_xml_comments(text)

        total += _count_non_empty_lines(stripped)

    print(total)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())


