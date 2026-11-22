# SplintIt VR - Hiking Demo: setup (about 10 minutes)

This folder is a **scaffold**: C# scripts plus an editor script that builds the whole scene for you.
It was written without access to Unity, so the first compile happens on your machine. If the Console shows an error,
see "If something does not compile" at the bottom (usually a one-line namespace fix).

The full repository includes the configured XRI **3.0.7 Starter Assets**, **XR Device Simulator**, and
an already-wired `Scenes/HikingDemo.unity`. Open that scene and press Play; the setup/import steps below
are only needed when transferring the demo into another Unity project.

## 1. Create the project
1. Unity Hub -> Unity **6000.x (Unity 6)** -> template **VR Core** (URP) or **Mixed Reality** (URP).
2. Package Manager (Window > Package Manager) - make sure these are installed:
   - **XR Interaction Toolkit 3.0.7** (then, on its page, Samples > import **Starter Assets** and **XR Device Simulator**)
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
2. The builder instantiates the Starter Assets **XR Origin (XR Rig)** prefab, which includes left/right controllers,
   near/far and poke interactors, and an Input Action Manager with **XRI Default Input Actions** assigned.
   It also adds the simulator, one **XR Interaction Manager**, and one **UI EventSystem**. A bare **XR Origin (VR)**
   created through Unity's GameObject menu has no controllers and cannot replace this prefab.
3. Re-running the menu item regenerates the scene from scratch (your manual edits to that scene are overwritten, so duplicate the scene first if you customise it).

## 4. Play it
- **No headset:** press Play, focus the **Game** view, and press **Tab** to select a controller. Aim the controller ray
  at a mode button and left-click (trigger). Hold **G** to grab an item after the opening sequence; release **G** to drop it.
  Hold **Space** to manipulate the right controller or **Left Shift** for the left. **R** switches mouse move/rotate mode.
  The scene already contains a simulator; do not add another.
- **Quest (Link / Air Link):** disable **XR Device Simulator**, then press Play with the headset connected.
- **Standalone Quest build:** File > Build Settings > Android > Switch Platform > Build And Run. The simulator is
  tagged `EditorOnly`, so it is excluded from builds.

Items intentionally remain disabled on the main menu and during the opening sequence. Select a mode first.
For simulator height, the rig uses device-relative tracking and a 1.6 m camera offset; comfort settings apply over that baseline.

Round: **Guided** (no timer, glowing hints) or **Evaluation** (150 s, no hints). When a round starts, the friend
**collapses** in a short ragdoll cutscene and settles by the log; then splinting begins. Pick items from the SUPPLIES
table, put the useful ones on the PREP table, then: support along the arm -> padding -> two straps (release and hold
still ~1 s) -> press CHECK SPLINT. Left menu button = pause.

## Collapse cutscene, ragdoll and poke/push
- `RagdollController` (on `Props > NPC_InjuredHiker`) adds a Rigidbody + collider + `CharacterJoint` to every gray
  limb. The body is **kinematic (standing)** until the round starts, then `DemoFlow` plays the cutscene: `StandUp()` ->
  `Collapse()` (the friend falls) -> after `collapseSeconds` the **injured arm is returned to the splint pose and
  pinned** (`RestInjuredArm()`).
- The injured arm (upper arm + forearm + hand) is a self-contained assembly with no joint to the torso, so on
  collapse it falls as one piece, and `RestInjuredArm()` snaps it back to its authored resting pose - exactly where
  the splint placement zones are - then pins it kinematic. This is what keeps the round playable: the zones, ghost
  outlines and the arm the player works on all line up. The rest of the body (torso, head, legs, good arm) stays
  collapsed on the ground and can be **poked / pushed**.
- `Pokeable` (on every limb and every grabbable item in newly built scenes) pushes the Rigidbody away from your fingertip when a poke/direct interactor touches it.
  Held items and pinned/kinematic bodies absorb the poke instead of moving.
  Distant controller-ray/near-far hovers do not shove props away before you can grab them.
- Tuning: cutscene length = `DemoFlow.collapseSeconds`; fall shove = `RagdollController.collapseShove`; limb push
  strength = `Pokeable.pushStrength`. The injured limb stays gray/placeholder like the rest (no gore).


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
