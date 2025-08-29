import os
import json

# Path to the directory containing the images and countries.json
directory = os.path.dirname(os.path.abspath(__file__))

# Load country code to name mapping
with open(os.path.join(directory, 'countries.json'), encoding='utf-8') as f:
    countries = json.load(f)

for code, name in countries.items():
    old_filename = f"{code.lower()}.png"
    new_filename = f"{name}.png"
    old_path = os.path.join(directory, old_filename)
    new_path = os.path.join(directory, new_filename)
    if os.path.exists(old_path):
        # Avoid overwriting files with the same name
        if not os.path.exists(new_path):
            os.rename(old_path, new_path)
            print(f"Renamed {old_filename} -> {new_filename}")
        else:
            print(f"Skipped {old_filename}: {new_filename} already exists.")
    else:
        print(f"File not found: {old_filename}")