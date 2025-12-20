from __future__ import annotations

import re
from pathlib import Path


def parse_resx(path: str) -> dict[str, str]:
    content = Path(path).read_text(encoding="utf-8")
    pattern = r'(<data\s+name="([^"]+)"[^>]*>.*?</data>)'
    result: dict[str, str] = {}
    for match in re.finditer(pattern, content, re.DOTALL):
        raw_block = match.group(1)
        key = match.group(2)
        value_match = re.search(r"<value>(.*?)</value>", raw_block, re.DOTALL)
        value = value_match.group(1).strip() if value_match else ""
        if key not in result:
            result[key] = value
    return result


default_path = "OpenSourceToolkit.NET/Localization/ToolkitStrings.resx"
uk_path = "OpenSourceToolkit.NET/Localization/ToolkitStrings.uk.resx"

default_kv = parse_resx(default_path)
uk_kv = parse_resx(uk_path)

untranslated = sorted(k for k, v in uk_kv.items() if default_kv.get(k, "") == v)
print(f"untranslated_count={len(untranslated)}")
for k in untranslated:
    print(k)
