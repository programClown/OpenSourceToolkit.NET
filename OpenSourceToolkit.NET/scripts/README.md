# Scripts Manual (Batteries Included)

This folder contains helper scripts used during localization maintenance.

## `check_resx_keys.py` (Localization / `.resx` sanity + untranslated audit)

### Purpose

- Ensure localized `.resx` files have the **same key set** as the default file (no missing/extra keys).
- Detect **still-English** values (localized value identical to the default value), excluding an internal ignore list.
- (Optional) Normalize / remove duplicates and extras in a single target file in a safe, deterministic way.

### Basic usage

`check_resx_keys.py` always needs the **default** `.resx` as the first argument:

```bash
python OpenSourceToolkit.NET/scripts/check_resx_keys.py OpenSourceToolkit.NET/Localization/ToolkitStrings.resx ...
```

By default, the script looks for localized files next to the default file.
You can optionally pass a directory as the second positional argument:

```bash
python OpenSourceToolkit.NET/scripts/check_resx_keys.py OpenSourceToolkit.NET/Localization/ToolkitStrings.resx OpenSourceToolkit.NET/Localization ...
```

### Common flags

- **`-f <locale>` / `--file <locale>`**: check / operate on a single locale file (`ToolkitStrings.<locale>.resx`).
- **`--all`**: check all localized files in the directory (must NOT be combined with `-f`).
- **`--show-untranslated`**: print keys whose values are still identical to default (English).
- **`--duplicates`**: in-place cleanup for a single locale file:
  - removes extra keys not present in default
  - resolves duplicate keys
  - rewrites the target file in default key order (atomic write)
- **`-v` / `--verbose`**: show detailed missing key lists (and more details where applicable).
- **`-s` / `--summary`**: show one-line summaries (structure-only mode).

> Note: Passing `-f all` is intentionally rejected. Use `--all`.

## Workflows

### 1) Structural check for one locale

Verify the file has **Missing=0** and **Extra=0** (key sets match the default):

```bash
python OpenSourceToolkit.NET/scripts/check_resx_keys.py OpenSourceToolkit.NET/Localization/ToolkitStrings.resx -f tr
```

### 2) Find + list “still-English” keys for one locale

Use `--duplicates` to normalize first (safe cleanup), then list untranslated keys:

```bash
python OpenSourceToolkit.NET/scripts/check_resx_keys.py OpenSourceToolkit.NET/Localization/ToolkitStrings.resx -f tr --duplicates --show-untranslated
```

The output includes:

- `WARNING: N keys still have English values (untranslated)`
- A list of keys under **“Untranslated keys (still English)”**

### 3) Translate a locale file (recommended loop)

1. Run the untranslated list command:

```bash
python OpenSourceToolkit.NET/scripts/check_resx_keys.py OpenSourceToolkit.NET/Localization/ToolkitStrings.resx -f <locale> --duplicates --show-untranslated
```

1. Edit:

- `OpenSourceToolkit.NET/Localization/ToolkitStrings.<locale>.resx`

1. For each key listed as “still English”, translate the `<value>...</value>`.

#### Important rules while editing values

- Keep placeholders **exactly**: `{0}`, `{1}`, etc.
- Keep escaped XML entities **exactly**: `&amp;`, `&lt;`, `&gt;`, etc.
- Keep literal formatting exactly: `\n`, punctuation, surrounding spaces if intentional.

1. Re-run:

```bash
python OpenSourceToolkit.NET/scripts/check_resx_keys.py OpenSourceToolkit.NET/Localization/ToolkitStrings.resx -f <locale> --duplicates
```

Repeat until the untranslated warning disappears (or only your intentionally ignored keys remain).

### 4) Audit all locales at once (structure + untranslated)

Run a full sweep across all localized files:

```bash
python OpenSourceToolkit.NET/scripts/check_resx_keys.py OpenSourceToolkit.NET/Localization/ToolkitStrings.resx --all --show-untranslated
```

This prints:

- a table with Keys/Missing/Extra per locale
- an “Untranslated Keys (still English)” section per file (only if any exist)

### 5) Structural-only “quick scan” (all locales)

If you only care about Missing/Extra counts:

```bash
python OpenSourceToolkit.NET/scripts/check_resx_keys.py OpenSourceToolkit.NET/Localization/ToolkitStrings.resx --all -s
```

### 6) Usual case: the default (English) `.resx` gained new keys

This is the most common workflow when `ToolkitStrings.resx` was extended and you need to propagate new keys into every locale.

#### Step A — Detect which locales are missing keys

Run a structure scan across all locales:

```bash
python OpenSourceToolkit.NET/scripts/check_resx_keys.py OpenSourceToolkit.NET/Localization/ToolkitStrings.resx --all
```

If you want the **exact missing key names**, use verbose mode:

```bash
python OpenSourceToolkit.NET/scripts/check_resx_keys.py OpenSourceToolkit.NET/Localization/ToolkitStrings.resx --all -v
```

#### Step B — Add the missing keys to each locale file (copy from default)

For each locale that shows `Missing > 0`:

- Open `OpenSourceToolkit.NET/Localization/ToolkitStrings.<locale>.resx`
- For every missing key (from the `--all -v` output):
  - find that key in `ToolkitStrings.resx` (keys may be located anywhere; not necessarily near the end)
  - copy the full `<data ...>...</data>` block for that key
  - paste it into the locale file (anywhere is fine; ordering will be normalized later)
  - **translate the `<value>...</value>` immediately** into the target language
    - keep placeholders/escapes exactly (`{0}`, `&amp;`, `&lt;`, `\n`, etc.)
    - only leave the English value temporarily if you intentionally plan a follow-up “untranslated” pass

> Important: Do **not** assume new keys were appended at the end. Keys are often added near the feature/view they belong to.
> Why: `--duplicates` is intentionally conservative. If a locale is missing keys from the default file, `--duplicates` will refuse to rewrite the file (to avoid data loss). So you must add missing keys first.

#### Step C — Normalize the locale file after inserting keys

Once keys are present, normalize ordering and remove any accidental extras/duplicates:

```bash
python OpenSourceToolkit.NET/scripts/check_resx_keys.py OpenSourceToolkit.NET/Localization/ToolkitStrings.resx -f <locale> --duplicates
```

Repeat for each locale you updated.

#### Step D — Translate the newly added keys

If you intentionally left any pasted values in English (or missed a few), get the list of still-English keys:

```bash
python OpenSourceToolkit.NET/scripts/check_resx_keys.py OpenSourceToolkit.NET/Localization/ToolkitStrings.resx -f <locale> --duplicates --show-untranslated
```

Translate the listed keys in `ToolkitStrings.<locale>.resx` (see workflow **3**), then re-run `--duplicates` until the warning disappears.

#### Step E — Final audit across all locales

```bash
python OpenSourceToolkit.NET/scripts/check_resx_keys.py OpenSourceToolkit.NET/Localization/ToolkitStrings.resx --all --show-untranslated
```

## Notes / gotchas

- **`--duplicates` rewrites** the target `.resx` file (atomic write). This is intended and keeps key ordering consistent with the default file.
- **Ignored untranslated keys**: the script contains an internal `IGNORED_UNTRANSLATED_KEYS` set for keys that are intentionally allowed to match English.
- **Tech terms / region-specific choices (policy)**:
  - **Acronyms can be intentionally identical** across locales (e.g. `RGB`, `HSL`, `HSV`, `CMYK`, `LAB`). In that case, keep the value identical and add the key to `IGNORED_UNTRANSLATED_KEYS` to avoid false-positive “still English” warnings.
  - **Asian locales may still need localized *descriptions*** around tech tokens. Example: for the Color tool, the `Hex:` prefix is localized for:
    - `ToolkitStrings.ja.resx`: `16進数:` (note: the actual value includes a trailing space)
    - `ToolkitStrings.ko.resx`: `16진수:` (note: the actual value includes a trailing space)
    - `ToolkitStrings.zh-Hans.resx`: `十六进制:` (note: the actual value includes a trailing space)
    while the color space acronyms remain unchanged.
  - **Rule of thumb**: translate user-facing *words* (“Copy …”, “Guidelines”, etc.) per locale; keep standardized technical abbreviations if that matches common UI usage for the locale.
- If you see `dump_bash_state: command not found` in Git Bash, it’s unrelated to these scripts and can be ignored.
