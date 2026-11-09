# SplintIt VR - rules implemented by the demo

Source: the user's `description_MD.txt` (SplintIt VR full project description). Re-read it each time the skill runs;
if it differs from this summary, the description wins - adjust the scripts and tell the user.

## Loop
Select -> position -> protect -> secure -> evaluate. Briefing on the main board, then a round.

## Modes
| | Guided | Evaluation |
|---|---|---|
| Timer | none (free stopwatch) | countdown, default 150 s, "low time" at 30 s |
| Highlights | glowing hint cones above next items + cyan target outlines | none (prep board outline only) |
| Hint button | on (counted) | off |
| Stars | none | 3 = no mistakes and >=40% time left; 2 = <=2 mistakes; 1 = success otherwise |
| Saved | never | yes (local JSON), success needed for "best" entries |

## Items (Hiking)
Useful: Branch (rigid support), Jacket (padding), Belt + Scarf (straps). Distractors: Bottle, Rock, Phone, Backpack.
A distractor put on the prep board or arm = mistake + one-sentence explanation + item returns to the supply table.

## Order and validity
- Padding only after the support; straps only after the padding (out-of-order = mistake, item returns to prep).
- Support must be within 35 degrees of the arm direction (either end first).
- Two straps: one above, one below the injury. A strap needs ~1 s of stillness to become "secured" (partial wrap state before).
- Everything is derived from what is currently in each zone, so picking an item back up is a safe undo.
- Success = all four criteria met (materials, support, padding, both straps) and not timed out. Mistakes lower stars, they do not fail the round.

## Safety / comfort / robustness (from the description)
- Pause (left menu button or board button): timer, physics time and grabbing freeze; resume re-stabilises objects.
- Headset removal / tracking loss / app focus loss auto-pauses; Resume stays disabled until tracking is back.
- Restart and Main Menu need a second tap to confirm.
- Dropped/fallen items return to the supply table; RETURN ITEMS button fetches loose ones.
- Seated/standing and height offset (pause screen), stored locally.
- Save file unreadable -> left untouched, player told, session temporary (nothing written). Abandoned rounds never write records.
- Timer state always has a symbol, not colour alone: `TIME`, `(!) LOW TIME`, `|| PAUSED`, `(X) TIME UP`.
- Visuals: low-poly, no blood or graphic injury; injury = orange band + "!" sign that turns green "OK" on success.

## Medical-content flag
Order follows the description (support, then padding, then straps). Real-world splinting usually pads between limb and support.
Say this to the user once: the content needs review by a first-aid professional (the description itself requires it).

## Not in the demo (mention, do not build unless asked)
Playground and Car scenarios, full tutorial, diploma, demo/guest mode (a `SaveSystem.Temporary` flag exists for it), settings menu beyond comfort, hand-tracking tuning, voice-over.
