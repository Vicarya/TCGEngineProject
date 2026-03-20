
import os
import sys

def list_uncommented_cs_files(root_dir):
    uncommented_files = []
    for dirpath, _, filenames in os.walk(root_dir):
        for filename in filenames:
            if filename.endswith(".cs"):
                filepath = os.path.join(dirpath, filename)
                try:
                    with open(filepath, "r", encoding="utf-8") as f:
                        content = f.read()
                        if "/// <summary>" not in content:
                            uncommented_files.append(filepath)
                except Exception as e:
                    print(f"Error reading file {filepath}: {e}", file=sys.stderr)
    return uncommented_files

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python list_uncommented_cs_files.py <root_directory>", file=sys.stderr)
        sys.exit(1)

    root_directory = sys.argv[1]
    files = list_uncommented_cs_files(root_directory)
    for f in files:
        print(f)
