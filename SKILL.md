---
name: splintit-vr-demo
description: Builds the Unity (Meta Quest / OpenXR) playable demo of the SplintIt VR first-aid game, an improvised arm-splint trainer. Delivers a drop-in Unity scaffold, not a finished app, with a one-click editor builder for the colourful low-poly Hiking-trail scene, gray placeholder characters, items to pick, a Guided round and a timed Evaluation round. Use this skill whenever the user mentions SplintIt, SplintIt VR, the splint/first-aid VR game, wants a demo, prototype, playable build or vertical slice of it, wants the Hiking scenario, or asks to regenerate or tweak that demo (items, timer, layout, scene look), even if they do not say "skill" or "Unity".
---

# SplintIt VR demo builder

Produces `SplintItDemo.zip`: C# runtime scripts + an editor script that builds the full scene inside Unity, plus a setup guide.
The user chose: **Unity scaffold**, **build the improvised splint** (support + padding + straps), **guided round plus a short timed evaluation round**,
**colourful landscape**, **gray characters** (they will make the final character designs themselves).

## Why it is a scaffold
There is no Unity in this environment, so nothing here can be compiled or played. The scripts follow XR Interaction Toolkit 3.x / Unity 6 APIs and
were statically checked only. Always say this plainly in the final message, and point to `assets/SETUP.md` ("If something does not compile").
Never claim the demo "works" or was "tested".

## Workflow
1. **Read the description again** if `/mnt/user-data/uploads/` contains it (the SplintIt description file). Compare with `references/game-rules.md`.
   If the user has changed items, scenario, timer, scoring or modes, adapt the scripts accordingly (see "Customising"). Do not ask questions the description already answers.
2. **Copy the skill's assets** to a working folder and apply any customisation there (never edit the skill folder itself; it may be read-only).
3. **Check**: `python3 scripts/check_scaffold.py <work>/Scripts`. Fix whatever it reports.
4. **Assemble**: `bash scripts/assemble_demo.sh <skill_dir> /mnt/user-data/outputs` -> `SplintItDemo.zip` (re-runs the check).
5. **Deliver** with `present_files` on the zip. Final message, short: what's inside, the 4-line quick start
   (install XRI 3 + import TMP Essentials -> copy folder into `Assets/` -> menu **SplintIt > Build Hiking Demo Scene** -> Play / XR Device Simulator),
   the "not compiled" caveat, how to swap the gray characters, and the medical-review flag from `references/game-rules.md`.

## What the scaffold contains
- `Scripts/Runtime/` - `SplintBuildController` (rules, derived state, evaluation), `DemoFlow` (menu/round/result/pause, tracking-loss recovery),
  `SplintItem`, `PlacementZone`, `RoundTimer`, `NpcReaction`, `SaveSystem`, `ComfortSettings`, `TrackingGuard`, `Feedback` (procedural sfx + haptics), `Billboard`, `SplintTypes`.
- `Scripts/Editor/HikingDemoSceneBuilder*.cs` - menu **SplintIt > Build Hiking Demo Scene**: generates lighting, landscape, camp, tables, gray NPC, 8 items,
  zones, XR rig (via XRI menu items), world-space UI boards, and wires every reference. Layout constants are at the top of the file.
- `SETUP.md` - project setup, playing, replacing characters, tuning, compile-error fixes.

Details to read only when needed:
- `references/game-rules.md` - modes, scoring, validity rules, robustness behaviours, what is deliberately out of scope.
- `references/scene-spec.md` - visual palette, layout table, extension points.
- `references/troubleshooting.md` - symptom -> fix table to paste to the user.

## Customising (typical requests)
- **Timer length**: `RoundTimer.durationSeconds` default 150 (and `lowTimeSeconds` 30).
- **Items**: edit `BuildItems` in `HikingDemoSceneBuilder.Gameplay.cs` (`NewItem` + body primitives; straps also need `Band` + `CollectLoose`). Roles: `RigidSupport`, `Padding`, `Strap`, `Distractor`.
  If the number of required straps/materials changes, update `RequiredStraps` and the checklist strings in `SplintBuildController`.
- **Look**: palette hex strings live in `BuildLandscape`/`BuildCamp`; keep `CharacterGray` the only material on characters.
- **Reach problems**: move `SuppliesCenter`, `PrepCenter`, `NpcPos` (and `ArmLocal` if the arm moves).
- **Other scenarios** (Playground, Car): out of scope for this demo; offer to copy the builder as a new scenario (see `scene-spec.md`).
- After any change re-run the check script and the assemble step.

## Conventions to keep
- Characters stay flat gray and faceless; no blood or graphic injury (injury = orange band + "!" sign).
- Feedback never relies on colour alone: symbols/text accompany every state.
- Completion state is derived from zone occupancy; do not add stored flags that can go stale when items are picked back up.
- Runtime scripts must not reference `UnityEditor` (would break Quest builds).
