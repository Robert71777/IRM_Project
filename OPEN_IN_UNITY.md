# Opening SplintIt VR in Unity

This repository is now a Unity project you can open directly from Unity Hub.

```
Assets/SplintItDemo/      scripts (+ asmdefs) and SETUP.md
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
1. Menu **SplintIt > Build Hiking Demo Scene** — generates
   `Assets/SplintItDemo/Scenes/HikingDemo.unity`, materials, meshes, and adds the scene
   to Build Settings.
2. Play:
   - **No headset:** Package Manager > XR Interaction Toolkit > Samples > import
     **XR Device Simulator**, drag its prefab into the scene, press Play.
   - **Quest via Link/Air Link:** press Play with the headset connected.
   - **Standalone Quest:** File > Build Settings > Android > Switch Platform > Build And Run.

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
