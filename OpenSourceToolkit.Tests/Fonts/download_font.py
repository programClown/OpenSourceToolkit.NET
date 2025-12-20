"""
This script downloads the Roboto font and its license for use in unit tests.
Specifically, the PdfSharp library requires a physical font file to be present
and loaded via a custom FontResolver to reliably render text in PDFs during tests,
especially in CI/CD environments where system fonts may not be available.

We use the Google Fonts repository to fetch the standard Roboto font.
"""
import urllib.request
import os

# Get absolute path to the script directory (which is now the Fonts folder)
output_dir: str = os.path.dirname(os.path.abspath(__file__))

if not os.path.exists(output_dir):
    os.makedirs(output_dir)

def download_file(url: str, filename: str):
    output_path = os.path.join(output_dir, filename)
    print(f"Downloading {url} to {output_path}...")
    try:
        urllib.request.urlretrieve(url, output_path)
        print(f"Done: {filename}")
        if os.path.exists(output_path) and os.path.getsize(output_path) > 0:
            print(f"File {filename} downloaded successfully.")
        else:
            print(f"File {filename} is empty or missing.")
    except Exception as e:
        print(f"Failed to download {filename}: {e}")

# 1. Download Font (Variable Font version)
font_url: str = "https://raw.githubusercontent.com/google/fonts/main/ofl/roboto/Roboto%5Bwdth%2Cwght%5D.ttf"
download_file(font_url, "Roboto-Regular.ttf")

# 2. Download License (OFL)
license_url: str = "https://raw.githubusercontent.com/google/fonts/main/ofl/roboto/OFL.txt"
download_file(license_url, "LICENSE.txt")
