# XRI Starter Assets source

The assets in `Assets/Samples/XR Interaction Toolkit/3.0.7/Starter Assets` and the adjacent
`Starter Assets.meta` were copied unchanged from Unity's official
[XR Interaction Toolkit Examples repository](https://github.com/Unity-Technologies/XR-Interaction-Toolkit-Examples/tree/aafbb9e7c46e4d13b7900d0c822afc3cee98c79e/Assets/Samples/XR%20Interaction%20Toolkit/3.0.7/Starter%20Assets).

- Commit: `aafbb9e7c46e4d13b7900d0c822afc3cee98c79e` (XRI 3.0.7 update).
- The sample's original GUIDs are preserved, including the rig's nested prefabs and action references.
- The project continues to use XRI 3.0.7, Input System 1.11.2, Core Utils 2.3.0 and Unity 6000.0.23f1.
- The rig, input actions, scripts, and other non-material files were also compared with the official
  Unity registry package for XRI 3.0.7. The example repository has ten materials reserialized by a newer
  rendering pipeline; those were retained as supplied by Unity's GitHub repository.
- Official package archive SHA-256: `aa9af908f3d37e4b6f9f668e6f3e1d5921120fd4d61101440e5a99de8077d990`.

The hiking scene references the supplied `XR Origin (XR Rig)` prefab. Scene overrides set device tracking
and a 1.6 m camera offset for keyboard simulation. The scene builder makes the same overrides when regenerating
the demo and includes the existing simulator sample with the `EditorOnly` tag.

Run `python3 scripts/check_xr_setup.py` for static checks of scene references, controller prefabs, and
the simulator-to-grip/trigger bindings. These checks do not compile C# or replace a Unity Play mode check.

## Upstream license notice

From the upstream `LICENSE.md` at the commit above:

XR Interaction Toolkit Examples © 2019 Unity Technologies

Licensed under the Unity Companion License for Unity-dependent projects (see [Unity Companion License](http://www.unity3d.com/legal/licenses/Unity_Companion_License)).

Unless expressly provided otherwise, the Software under this license is made available strictly on an “AS IS” BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED. Please review the license for details on these and other terms and conditions.
