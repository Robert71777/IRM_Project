#!/usr/bin/env bash
# Usage: assemble_demo.sh <skill_dir> <output_dir>
# Produces <output_dir>/SplintItDemo.zip containing Assets/SplintItDemo/{Scripts,SETUP.md}, ready to drop into a Unity project.
set -euo pipefail
SKILL="$1"; OUT="$2"
STAGE="$(mktemp -d)"
mkdir -p "$STAGE/SplintItDemo/Scripts"
cp -r "$SKILL/assets/Scripts/." "$STAGE/SplintItDemo/Scripts/"
cp "$SKILL/assets/SETUP.md" "$STAGE/SplintItDemo/SETUP.md"
python3 "$SKILL/scripts/check_scaffold.py" "$STAGE/SplintItDemo/Scripts"
mkdir -p "$OUT"
( cd "$STAGE" && rm -f "$OUT/SplintItDemo.zip" && python3 -c "
import shutil,sys; shutil.make_archive('$OUT/SplintItDemo','zip','$STAGE','SplintItDemo')" )
echo "Wrote $OUT/SplintItDemo.zip"
