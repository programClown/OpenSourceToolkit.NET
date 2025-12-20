#!/usr/bin/env python3
"""
Script to identify all missing keys in localized .resx files and generate
the XML snippets needed to add them.
"""
import xml.etree.ElementTree as ET
from pathlib import Path
import sys

def get_resx_keys_with_values(resx_path: Path) -> dict:
    """Extract all key-value pairs from a .resx file."""
    tree = ET.parse(resx_path)
    return {
        data.get('name'): data.find('value').text if data.find('value') is not None else ''
        for data in tree.findall('.//data')
    }

def find_missing_keys(default_path: Path, target_path: Path) -> list[str]:
    """Find keys in default that are missing from target."""
    default_keys = set(get_resx_keys_with_values(default_path).keys())
    target_keys = set(get_resx_keys_with_values(target_path).keys())
    return sorted(default_keys - target_keys)

def generate_xml_snippet(keys: list[str], default_values: dict) -> str:
    """Generate XML snippet for missing keys with placeholder values."""
    lines = []
    for key in keys:
        value = default_values.get(key, '')
        # Escape XML special characters
        if value:
            value = value.replace('&', '&amp;').replace('<', '&lt;').replace('>', '&gt;')
        lines.append(f'  <data name="{key}" xml:space="preserve">')
        lines.append(f'    <value>{value}</value>')
        lines.append('  </data>')
    return '\n'.join(lines)

def main():
    base_dir = Path(__file__).parent.parent / 'Localization'
    default_path = base_dir / 'ToolkitStrings.resx'
    
    if not default_path.exists():
        print(f"Error: Default file not found: {default_path}")
        sys.exit(1)
    
    default_values = get_resx_keys_with_values(default_path)
    print(f"Default file has {len(default_values)} keys\n")
    
    # Find all localized files
    localized_files = sorted(base_dir.glob('ToolkitStrings.*.resx'))
    
    total_missing = 0
    
    for resx_file in localized_files:
        culture = resx_file.stem.split('.')[-1]
        missing_keys = find_missing_keys(default_path, resx_file)
        
        if missing_keys:
            total_missing += len(missing_keys)
            print(f"\n{'='*60}")
            print(f"FILE: {resx_file.name} - {len(missing_keys)} MISSING KEYS")
            print('='*60)
            
            # Group keys by prefix for better organization
            grouped = {}
            for key in missing_keys:
                prefix = key.split('_')[0] if '_' in key else 'Other'
                if prefix not in grouped:
                    grouped[prefix] = []
                grouped[prefix].append(key)
            
            print("\nMissing keys by category:")
            for prefix in sorted(grouped.keys()):
                print(f"  {prefix}: {len(grouped[prefix])} keys")
                for key in grouped[prefix][:5]:
                    print(f"    - {key}")
                if len(grouped[prefix]) > 5:
                    print(f"    ... and {len(grouped[prefix]) - 5} more")
            
            # Output file for XML snippets
            output_file = base_dir / f'_missing_{culture}.xml'
            xml_content = generate_xml_snippet(missing_keys, default_values)
            output_file.write_text(xml_content, encoding='utf-8')
            print(f"\nXML snippet written to: {output_file.name}")
        else:
            print(f"{resx_file.name}: ✓ Complete (0 missing)")
    
    print(f"\n{'='*60}")
    print(f"TOTAL: {total_missing} missing keys across all files")
    print('='*60)

if __name__ == '__main__':
    main()
