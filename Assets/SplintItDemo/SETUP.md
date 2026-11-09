# SplintIt VR - Hiking Demo: setup (about 10 minutes)

This folder is a **scaffold**: C# scripts plus an editor script that builds the whole scene for you.
It was written without access to Unity, so the first compile happens on your machine. If the Console shows an error,
see "If something does not compile" at the bottom (usually a one-line namespace fix).

## 1. Create the project
1. Unity Hub -> Unity **6000.x (Unity 6)** -> template **VR Core** (URP) or **Mixed Reality** (URP).
2. Package Manager (Window > Package Manager) - make sure these are installed:
   - **XR Interaction Toolkit 3.x** (then, on its page, Samples > import **Starter Assets**)
   - **XR Plugin Management** + **OpenXR** (with the VR templates these are already there)
   - **Unity UI (uGUI)** (includes TextMeshPro)
3. Window > TextMeshPro > **Import TMP Essential Resources** (needed once, the builder checks for it).
4. Edit > Project Settings > XR Plug-in Management > **Android tab**: tick **OpenXR**, add the **Meta Quest Support** feature
   and the **Oculus Touch Controller Profile**. (Optional: Hand Tracking Subsystem for hand tracking.)

## 2. Import the demo
1. Copy the `SplintItDemo` folder into your project's `Assets/` folder (you should get `Assets/SplintItDemo/Scripts/...`).
2. Wait for compilation. The menu **SplintIt** appears in the top bar.

## 3. Build the scene
1. Menu **SplintIt > Build Hiking Demo Scene**. It creates `Assets/SplintItDemo/Scenes/HikingDemo.unity`, adds it to Build Settings,
   and creates materials/meshes under `Assets/SplintItDemo/`.
2. If the log says "Could not create an XR Origin", add one by hand (GameObject > XR > **XR Origin (VR)**), put it at (0,0,0),
   and make sure there is an **XR Interaction Manager** and a **UI EventSystem** (GameObject > XR > UI EventSystem). Then save.
3. Re-running the menu item regenerates the scene from scratch (your manual edits to that scene are overwritten, so duplicate the scene first if you customise it).

## 4. Play it
- **No headset:** in Package Manager > XR Interaction Toolkit > Samples, import **XR Device Simulator**, drag its prefab into the scene, press Play.
- **Quest (Link / Air Link):** press Play with the headset connected.
- **Standalone Quest build:** File > Build Settings > Android > Switch Platform > Build And Run.

Round: **Guided** (no timer, glowing hints) or **Evaluation** (150 s, no hints). Pick items from the SUPPLIES table, put the useful ones on the
PREP table, then: support along the arm -> padding -> two straps (release and hold still ~1 s) -> press CHECK SPLINT.
Left menu button = pause.

## Replace the gray placeholder characters
- The NPC is `Props > NPC_InjuredHiker > Visual_REPLACE_WITH_YOUR_MODEL` (all gray primitives).
- Delete the children of that object and parent your own model there. Keep it facing local +Z, seated at the log, injured forearm
  horizontal at about `y = 1.0` and `x = 0.26` in the NPC's local space (that is where the placement zones are).
- Zones (`Zone_Support`, `Zone_Padding`, `Zone_StrapUpper`, `Zone_StrapLower`) can be moved if your arm sits elsewhere.
- `NpcReaction.swayRoot` is the transform that gently sways; assign your model's torso or leave empty.

## Tune after the first headset test
All layout numbers are constants at the top of `Scripts/Editor/HikingDemoSceneBuilder.cs`
(table positions, NPC position, zone radii). If items are too far to reach, move `SuppliesCenter`, `PrepCenter` and `NpcPos` closer, or use the
Seated/Height buttons on the pause screen. Timer length: `RoundTimer.durationSeconds` on the `GameSystems` object.

## If something does not compile
- `XRGrabInteractable` / `XRInteractionManager` not found: your XRI version differs. XRI 3.x uses
  `UnityEngine.XR.Interaction.Toolkit.Interactables` (grab) and `UnityEngine.XR.Interaction.Toolkit` (manager). XRI 2.x used only the root namespace:
  delete the `.Interactables` using line.
- `linearVelocity` not found: you are on Unity 2022; replace with `velocity`.
- `TrackedDeviceGraphicRaycaster` / `XRUIInputModule` not found: they live in `UnityEngine.XR.Interaction.Toolkit.UI`.
- `FindFirstObjectByType` not found: Unity 2022 earlier than 2022.3.? - use `FindObjectOfType`.
- Anything else: copy the error into Claude together with the file name.

*Educational demo. Not a medical certification tool; splinting content must be reviewed by a qualified first-aid instructor before release.*
