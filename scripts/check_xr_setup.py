#!/usr/bin/env python3
"""Check the committed XR scene/prefab wiring without requiring Unity.

This validates serialized references and input bindings, not C# compilation or Play mode.
Usage: python3 scripts/check_xr_setup.py [--scene path/to/scene.unity]
"""
import argparse
import json
from pathlib import Path
import re
import sys


ROOT = Path(__file__).resolve().parents[1]
SAMPLES = ROOT / "Assets/Samples/XR Interaction Toolkit/3.0.7"
STARTER = SAMPLES / "Starter Assets"
RIG = STARTER / "Prefabs/XR Origin (XR Rig).prefab"
SIMULATOR = SAMPLES / "XR Device Simulator/XR Device Simulator.prefab"
ACTIONS = STARTER / "XRI Default Input Actions.inputactions"
SCENE = ROOT / "Assets/SplintItDemo/Scenes/HikingDemo.unity"


def require(condition, message):
    if not condition:
        raise ValueError(message)


def guid(path):
    match = re.search(r"^guid: (\w{32})$", Path(str(path) + ".meta").read_text(), re.M)
    require(match, f"Missing GUID: {path}")
    return match[1]


def documents(path):
    text = path.read_text()
    blocks = re.findall(r"^--- !u!\d+ &(\d+)(?: stripped)?\n(.*?)(?=^--- !u!|\Z)", text, re.M | re.S)
    require(blocks, f"No Unity YAML documents: {path}")
    result = dict(blocks)
    require(len(result) == len(blocks), f"Duplicate YAML file IDs: {path}")
    # Inline references with a GUID belong to another asset, rather than this file.
    for ref in re.findall(r"\{fileID: (-?\d+)\}", text):
        require(ref == "0" or ref in result, f"Dangling local file ID {ref}: {path}")
    return text, result


def main(scene):
    manifest = json.loads((ROOT / "Packages/manifest.json").read_text())
    require(manifest["dependencies"]["com.unity.xr.interaction.toolkit"] == "3.0.7",
            "Reimport/revalidate Starter Assets when changing the XRI package version")

    assets = {}
    for meta in (ROOT / "Assets").rglob("*.meta"):
        match = re.search(r"^guid: (\w{32})$", meta.read_text(), re.M)
        if match:
            require(match[1] not in assets, f"Duplicate asset GUID: {meta} and {assets.get(match[1])}")
            assets[match[1]] = Path(str(meta)[:-5])

    rig_guid, simulator_guid, actions_guid = guid(RIG), guid(SIMULATOR), guid(ACTIONS)
    scene_text, scene_docs = documents(scene)
    for asset_guid, label in [(rig_guid, "configured controller rig"), (simulator_guid, "device simulator")]:
        instances = [body for body in scene_docs.values()
                     if f"m_SourcePrefab: {{fileID: 100100000, guid: {asset_guid}," in body]
        require(len(instances) == 1, f"Scene must contain exactly one {label}")
        require("propertyPath: m_IsActive\n      value: 0" not in instances[0], f"Scene disables its {label}")
        if asset_guid == simulator_guid:
            require("propertyPath: m_TagString\n      value: EditorOnly" in instances[0],
                    "Simulator must be excluded from headset builds with EditorOnly")
    require("m_Name: XR Origin (VR)\n" not in scene_text, "Bare XR Origin remains in scene")

    visited = set()

    def check_prefab(path):
        if path in visited:
            return
        visited.add(path)
        text, _ = documents(path)
        for dep in re.findall(r"m_SourcePrefab: \{fileID: \d+, guid: (\w{32}),", text):
            require(dep in assets and assets[dep].is_file(), f"Missing nested prefab/model {dep}: {path}")
            if assets[dep].suffix == ".prefab":
                check_prefab(assets[dep])

    check_prefab(RIG)
    rig_text, rig_docs = documents(RIG)
    for side in ("Left", "Right"):
        require(f"m_Name: {side} Controller\n" in rig_text, f"Missing {side.lower()} controller")
    require(re.search(r"m_ActionAssets:\n  - \{fileID: -?\d+, guid: " + actions_guid, rig_text),
            "Rig does not enable the interaction action asset")

    # A scene override must address a real field on a real source-prefab component.
    rig_instance = next(body for body in scene_docs.values()
                        if f"m_SourcePrefab: {{fileID: 100100000, guid: {rig_guid}," in body)
    for target, field in re.findall(
            r"- target: \{fileID: (\d+), guid: " + rig_guid + r", type: 3\}\n      propertyPath: ([\w.]+)",
            rig_instance):
        require(target in rig_docs, f"Rig override targets missing source component {target}")
        require(field.split(".")[0] + ":" in rig_docs[target], f"Unknown rig override field {field}")

    left = (STARTER / "Prefabs/Interactors/Left_NearFarInteractor.prefab").read_text()
    require("m_EnableUIInteraction: 1" in left and "m_EnableFarCasting: 1" in left,
            "Controller interactor cannot point at world-space UI")
    for field in ("m_SelectInput", "m_UIPressInput"):
        require(re.search(field + r":\n(?:(?!\n  \w).)*m_InputActionReferencePerformed: \{fileID: -?\d+, guid: "
                          + actions_guid, left, re.S), f"Left controller has no {field} action reference")
        right = (STARTER / "Prefabs/Interactors/Right_NearFarInteractor.prefab").read_text()
        require(re.search(r"propertyPath: " + field + r"\.m_InputActionReferencePerformed\n      value: *\n"
                          r"      objectReference: \{fileID: -?\d+, guid: " + actions_guid, right),
                f"Right controller has no {field} action override")

    maps = {item["name"]: item for item in json.loads(ACTIONS.read_text())["maps"]}
    for side in ("Left", "Right"):
        interaction = maps[f"XRI {side} Interaction"]["bindings"]
        for action, control in [("Select", "GripButton"), ("UI Press", "TriggerButton")]:
            expected = f"<XRController>{{{side}Hand}}/{{{control}}}"
            require(any(binding["action"] == action and binding["path"] == expected for binding in interaction),
                    f"{side} {action} is not bound to {control}")

    controller_maps = json.loads((SIMULATOR.parent / "XR Device Controller Controls.inputactions").read_text())["maps"]
    bindings = [binding for item in controller_maps for binding in item["bindings"]]
    for action, control in [("Grip", "<Keyboard>/g"), ("Trigger", "<Mouse>/leftButton")]:
        require(any(binding["action"] == action and binding["path"] == control for binding in bindings),
                f"Simulator {action} is not bound to {control}")

    for field in ("guidedButton", "evaluationButton"):
        target = re.search(r"^  " + field + r": \{fileID: (\d+)\}", scene_text, re.M)
        require(target and target[1] in scene_docs, f"Missing DemoFlow.{field} reference")
        require("m_Interactable: 1" in scene_docs[target[1]], f"{field} is disabled")

    print(f"OK: scene references, {len(visited)} nested rig prefabs, both controller bindings, simulator controls and mode buttons.")
    print("Static validation only; run the scene in Unity to verify interaction behavior.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--scene", type=Path, default=SCENE)
    args = parser.parse_args()
    try:
        main(args.scene)
    except (ValueError, OSError, KeyError) as error:
        print(f"XR SETUP ERROR: {error}", file=sys.stderr)
        sys.exit(1)
