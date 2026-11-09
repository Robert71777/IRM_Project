# Hiking scene spec (what HikingDemoSceneBuilder produces)

Player stands at the origin facing +Z. 1 unit = 1 m.

## Look: colourful landscape, gray characters
- Sky: procedural, saturated blue; warm sun (48 deg, soft shadows); tri-light ambient; light blue distance fog.
- Ground bright green with stacked colour patches (lime, emerald, yellow-green); earthy clearing under the player; sandy winding trail.
- Pond with sand shore and lily pads; rolling hills (greens/teal); violet/blue mountains with snowcaps; fluffy clouds.
- ~70 low-poly trees: pines (two greens) and round trees (lime, green, golden, orange, pink); rocks (gray-violet, mossy); 14 red mushrooms; ~240 flowers (red, yellow, white, purple, orange, cyan).
- Camp: orange + teal pyramid tents, campfire (stone ring, logs, emissive-look flames, warm point light), stumps, signpost with red/blue boards.
- Characters: **one flat gray** (`#A8A8AC`) material, primitive shapes, no faces. Everything else keeps colour.
- Flat-shaded icosahedron/cone meshes are generated in code and saved under `Assets/SplintItDemo/Meshes`.

## Layout (constants at top of the builder)
| Thing | Position (x, z) | Note |
|---|---|---|
| NPC root | (0.26, 0.85) rotated 180 | injured right forearm ends up at world x=0, centred in front of the player |
| Forearm centre | (0, 0.70), y=1.0 | zones are parented to the NPC |
| Supplies table | (-0.84, 0.175), 0.92 x 0.55 | top at 0.85 m, 8 items |
| Prep table | (0.62, 0.30), 0.70 x 0.50 | teal, "PREP AREA" label, prep zone radius 0.42 |
| Main board (menu/result/pause) | (0, 1.5, 0.62) | 0.8 x 0.54 m, faces the player |
| Round board (timer/checklist/buttons) | (1.35, 1.4, 0.5) | 0.76 x 0.86 m |
| Wrist timer | left controller, top | falls back to floating at (-0.35, 1.0, 0.25) |

Zone radii: support/padding 0.22, straps 0.13 at +-0.09 m along the arm. Support angle tolerance 35 deg.

## Extension points
- New scenario: copy the builder, change `BuildLandscape/BuildCamp`, item list in `BuildItems`, keep zones/UI/wiring.
- New item: `NewItem(...)` + body primitives + add to the returned array; for straps also `Band(...)` and `CollectLoose(...)`.
- Real art: replace primitives with prefabs inside the item roots (keep colliders on children, Rigidbody on the root).
