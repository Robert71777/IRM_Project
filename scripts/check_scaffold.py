#!/usr/bin/env python3
"""Static sanity checks for the SplintIt Unity scaffold (no Unity needed).

Catches the mistakes that break a Unity import: unbalanced braces/parens, a MonoBehaviour whose class
name does not match its file name, missing expected files, and builder->runtime field mismatches.
It does NOT compile C#; the first real compile happens when the user opens the project in Unity.

Usage: python3 check_scaffold.py <path to the Scripts folder>   (contains Runtime/ and Editor/)
"""
import re, sys, pathlib

EXPECTED_RUNTIME = ["SplintTypes", "Billboard", "Feedback", "RoundTimer", "TrackingGuard", "ComfortSettings",
                    "SaveSystem", "SplintItem", "PlacementZone", "NpcReaction", "SplintBuildController", "DemoFlow"]
EXPECTED_EDITOR = ["HikingDemoSceneBuilder", "HikingDemoSceneBuilder.Gameplay"]

def strip(src):
    src = re.sub(r'//[^\n]*', '', src)
    src = re.sub(r'/\*.*?\*/', '', src, flags=re.S)
    src = re.sub(r'@"(?:[^"]|"")*"', '""', src)
    src = re.sub(r'"(?:\\.|[^"\\])*"', '""', src)
    src = re.sub(r"'(?:\\.|[^'\\])'", "''", src)
    return src

def main(root):
    root = pathlib.Path(root)
    errors = []
    for folder, names in (("Runtime", EXPECTED_RUNTIME), ("Editor", EXPECTED_EDITOR)):
        for n in names:
            if not (root / folder / (n + ".cs")).exists():
                errors.append(f"missing file {folder}/{n}.cs")

    for f in sorted(root.rglob("*.cs")):
        raw = f.read_text()
        s = strip(raw)
        for a, b in (("{", "}"), ("(", ")"), ("[", "]")):
            if s.count(a) != s.count(b):
                errors.append(f"{f.name}: unbalanced {a}{b} ({s.count(a)} vs {s.count(b)})")
        for m in re.finditer(r'class\s+(\w+)\s*:\s*MonoBehaviour', s):
            expected = f.stem.split('.')[0]
            if m.group(1) != expected:
                errors.append(f"{f.name}: MonoBehaviour '{m.group(1)}' must live in {m.group(1)}.cs")
        if '/Editor/' in str(f).replace('\\', '/') and 'using UnityEditor' not in raw and 'partial class' not in raw:
            errors.append(f"{f.name}: Editor script without 'using UnityEditor'")
        if '/Runtime/' in str(f).replace('\\', '/') and 'using UnityEditor' in raw:
            errors.append(f"{f.name}: Runtime script must not use UnityEditor (breaks player builds)")

    # every DemoFlow public field that the builder is expected to wire must be assigned in WireSystems
    flow = (root / "Runtime/DemoFlow.cs").read_text()
    wire = (root / "Editor/HikingDemoSceneBuilder.Gameplay.cs").read_text()
    fields = re.findall(r'public\s+(?:TMP_Text|Button|GameObject|SplintBuildController|RoundTimer|NpcReaction|TrackingGuard|ComfortSettings)\s+([\w, ]+);', flow)
    names = [x.strip() for grp in fields for x in grp.split(',')]
    for n in names:
        if not re.search(r'flow\.' + n + r'\b', wire):
            errors.append(f"builder never assigns DemoFlow.{n}")

    if errors:
        print("PROBLEMS:"); [print(" -", e) for e in errors]; sys.exit(1)
    print(f"OK: {len(list(root.rglob('*.cs')))} scripts passed static checks.")

if __name__ == "__main__":
    main(sys.argv[1] if len(sys.argv) > 1 else "assets/Scripts")
