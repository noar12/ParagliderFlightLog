#!/usr/bin/env bash
set -euo pipefail

# Usage: clean-bin-obj.sh [-n|--dry-run]
dry=false
while [ $# -gt 0 ]; do
  case "$1" in
    -n|--dry-run) dry=true ;;
    -h|--help) echo "Usage: $0 [-n|--dry-run]"; exit 0 ;;
    *) echo "Unknown option: $1"; exit 1 ;;
  esac
  shift
done

if $dry; then
  echo "Dry run: directories that would be removed:"
  find . -type d \( -iname bin -o -iname obj \) -prune -print
  exit 0
fi

echo "Removing all 'bin' and 'obj' directories under $(pwd)"
find . -type d \( -iname bin -o -iname obj \) -prune -exec rm -rf '{}' +
echo "Done."
