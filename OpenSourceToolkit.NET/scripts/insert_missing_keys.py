#!/usr/bin/env python3
"""
Script to insert missing keys from _missing_*.xml files into their
respective .resx files.
"""
import xml.etree.ElementTree as ET
from pathlib import Path
import re
import sys

def insert_missing_keys(resx_path: Path, missing_xml_path: Path) -> int:
    """Insert missing keys from the XML snippet file into the .resx file.
    Returns the number of keys inserted."""
    
    if not missing_xml_path.exists():
        return 0
    
    # Read the missing keys XML content
    missing_content = missing_xml_path.read_text(encoding='utf-8')
    if not missing_content.strip():
        return 0
    
    # Read the .resx file
    resx_content = resx_path.read_text(encoding='utf-8')
    
    # Find the closing </root> tag and insert before it
    if '</root>' not in resx_content:
        print(f"  Error: No </root> tag found in {resx_path.name}")
        return 0
    
    # Insert the missing content before </root>
    new_content = resx_content.replace(
        '</root>',
        f'\n  <!-- Auto-inserted missing keys (English fallback) -->\n{missing_content}\n</root>'
    )
    
    # Write back
    resx_path.write_text(new_content, encoding='utf-8')
    
    # Count how many keys were inserted
    key_count = missing_content.count('<data name=')
    return key_count

def main():
    base_dir = Path(__file__).parent.parent / 'Localization'
    
    # Find all _missing_*.xml files
    missing_files = list(base_dir.glob('_missing_*.xml'))
    
    if not missing_files:
        print("No _missing_*.xml files found. Run find_missing_keys.py first.")
        sys.exit(1)
    
    print(f"Found {len(missing_files)} files with missing keys to process\n")
    
    total_inserted = 0
    
    for missing_file in sorted(missing_files):
        # Extract culture code from filename: _missing_ja.xml -> ja
        culture = missing_file.stem.replace('_missing_', '')
        resx_file = base_dir / f'ToolkitStrings.{culture}.resx'
        
        if not resx_file.exists():
            print(f"  Warning: {resx_file.name} not found, skipping")
            continue
        
        count = insert_missing_keys(resx_file, missing_file)
        if count > 0:
            print(f"✓ {resx_file.name}: Inserted {count} missing keys")
            total_inserted += count
            # Remove the temporary file after successful insertion
            missing_file.unlink()
        else:
            print(f"○ {resx_file.name}: No keys to insert")
    
    print(f"\n{'='*50}")
    print(f"Done! Inserted {total_inserted} total keys")
    print('='*50)

if __name__ == '__main__':
    main()
