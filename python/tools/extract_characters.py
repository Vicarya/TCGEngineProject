import json
import os
import argparse

def extract_unique_characters(json_path, output_path):
    """
    Extracts all unique characters from the card data JSON and saves them to a text file.
    """
    if not os.path.exists(json_path):
        print(f"Error: JSON file not found at {json_path}")
        return

    unique_chars = set()

    with open(json_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    for card_data in data:
        # Add all characters from various text fields
        for key, value in card_data.items():
            if isinstance(value, str):
                unique_chars.update(value)

    # Convert set to a sorted list and then to a string
    sorted_chars = sorted(list(unique_chars))
    char_string = "".join(sorted_chars)

    with open(output_path, 'w', encoding='utf-8') as f:
        f.write(char_string)

    print(f"Successfully extracted {len(unique_chars)} unique characters from '{os.path.basename(json_path)}' to '{os.path.basename(output_path)}'")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Extract unique characters from a card data JSON file.")
    
    # Define default paths relative to the script location
    script_dir = os.path.dirname(os.path.abspath(__file__))
    default_json_path = os.path.normpath(os.path.join(script_dir, '..', '..', 'Assets', 'StreamingAssets', 'weiss_schwarz_cards.sample.json'))
    default_output_path = os.path.normpath(os.path.join(script_dir, 'required_characters.txt'))

    parser.add_argument('--input', type=str, default=default_json_path,
                        help=f'Path to the input JSON file. Defaults to: {default_json_path}')
    parser.add_argument('--output', type=str, default=default_output_path,
                        help=f'Path to the output text file. Defaults to: {default_output_path}')
    
    args = parser.parse_args()
    
    extract_unique_characters(args.input, args.output)
