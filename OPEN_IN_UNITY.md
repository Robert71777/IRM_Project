# Opening SplintIt VR in Unity

This repository is now a Unity project you can open directly from Unity Hub.

```
Assets/SplintItDemo/      scripts (+ asmdefs) and SETUP.md
Assets/Samples/          XRI 3.0.7 Starter Assets and XR Device Simulator
Packages/manifest.json    package dependencies (XRI, OpenXR, URP, uGUI/TMP, ...)
ProjectSettings/          editor version + player settings
```

## 1. Open it
1. Install **Unity 6000.0.x** (Unity 6) via Unity Hub.
   - `ProjectSettings/ProjectVersion.txt` pins `6000.0.23f1`. If you have a different
     6000.0.x patch, Hub will offer to open with your installed version — that is fine.
2. Unity Hub > **Add > Add project from disk** > select this folder.
3. Open it. The first import compiles the packages in `manifest.json` and may take a few minutes.

## 2. One-time setup inside Unity (not pre-baked)
These cannot be reliably committed as files because the serialized asset references are
Unity-version specific, so do them once in the editor:

1. **TextMeshPro essentials:** Window > TextMeshPro > **Import TMP Essential Resources**.
   (The scene builder refuses to run until this is done.)
2. **Enable XR:** Edit > Project Settings > **XR Plug-in Management**:
   - Desktop (PC/Link) tab: tick **OpenXR**.
   - Android tab: tick **OpenXR**, then under OpenXR add **Meta Quest Support** and the
     **Oculus Touch Controller Profile** interaction profile.
3. **Render pipeline (if the scene looks pink/unlit):** the code falls back to the
   Standard/Built-in shaders automatically, but for the intended look assign a URP asset
   under Project Settings > Graphics. A VR Core template project already has one.

## 3. Build the scene and play
1. Open `Assets/SplintItDemo/Scenes/HikingDemo.unity`. Its configured XR rig and simulator are included.
   To regenerate the scenery/gameplay, use **SplintIt > Build Hiking Demo Scene** — generates
   `Assets/SplintItDemo/Scenes/HikingDemo.unity`, materials, meshes, and adds the scene
   to Build Settings.
2. Play:
   - **No headset:** press Play and focus the **Game** view. Do not add a second simulator.
     Press **Tab** to select a controller, aim its ray at **GUIDED** or **EVALUATION**, and left-click
     (simulated trigger). After the opening sequence, aim at an item and hold **G** (grip) to pick it up;
     release **G** to drop it. Hold **Space** to manipulate the right controller or **Left Shift** for the left;
     **R** switches the mouse between moving and rotating the controller.
   - **Quest via Link/Air Link:** disable the **XR Device Simulator** object, then press Play with the headset connected.
   - **Standalone Quest:** File > Build Settings > Android > Switch Platform > Build And Run.
     The simulator is tagged `EditorOnly` and is excluded from the build.

The rig uses device-relative tracking with a 1.6 m camera offset so simulated head poses start above
the ground. Its Input Action Manager enables **XRI Default Input Actions**; both controllers include
near/far and poke interactors. Creating only **GameObject > XR > XR Origin (VR)** does not supply that setup.
The GitHub source revision and license are recorded in `references/XRI-STARTER-ASSETS.md`.

## Status / caveats
- These scripts were written and statically checked **without** a Unity install, so the
  first real compile happens on your machine. If the Console shows an error, see
  `Assets/SplintItDemo/SETUP.md` → "If something does not compile" (usually a one-line
  namespace fix for a different XRI/Unity version).
- `SplintBuildController`, `SaveSystem`, `ComfortSettings`, and `Billboard` were missing
  from the original assets and were implemented to match every call site — review them
  first if behaviour looks off.
- Layout/reach/timer values are tuning constants at the top of
  `Assets/SplintItDemo/Scripts/Editor/HikingDemoSceneBuilder.cs`.
- Educational demo only. The splint order (support → padding → straps) follows the project
  description; real splinting pads between limb and support. Content must be reviewed by a
  qualified first-aid instructor before any release.
