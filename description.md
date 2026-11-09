# SplintIt VR — Full Project Description

## 1. Project Overview

**SplintIt VR** is a standalone virtual-reality first-aid training application for Meta Quest 2 and Meta Quest 3. The player learns how to construct an improvised arm splint using everyday objects in simulated emergency situations.

The experience focuses on learning by doing rather than watching videos or reading instructions. The player identifies useful materials, physically manipulates them, positions them correctly, and secures them around an injured arm.

The application is educational but approachable and game-like:

- Stylized low-poly visuals
- No blood, gore, or graphic injuries
- Short individual play sessions
- Hand tracking and controller support
- Optional time pressure
- Guided learning followed by independent evaluation
- Local progress without accounts or online services

The application is not a professional medical certification tool. Educational content must be reviewed before final release.

## 2. Core Design Goal

SplintIt VR helps inexperienced users develop:

1. **Procedural understanding** — knowing the order of actions.
2. **Physical familiarity** — practicing grabbing, positioning, wrapping, and securing.
3. **Improvisation skills** — using different everyday objects instead of memorizing one solution.
4. **Confidence under pressure** — performing the procedure calmly while a timer is active.
5. **Error awareness** — understanding why a result succeeded or failed.

The desired user outcome is:

> “I understand what I need to do, I know what objects can help, and I can repeat the process calmly.”

## 3. Target Users

### Primary user: Andrei

Andrei is 21 years old, a student, physically active, and interested in hiking and sports. He is familiar with video games but has limited VR experience and no formal first-aid training.

He:

- Uses his own headset at home
- Plays in short sessions
- May choose seated or standing play
- Needs clear instructions and immediate feedback
- Wants to learn without risking harm to another person
- May become stressed by unfamiliar VR controls or time pressure

### Demonstration participant

A second user may temporarily use someone else’s headset during a class, laboratory session, or demonstration. This user may have never used VR, does not need an account, should not modify the owner’s progress, and needs a fast tutorial and a simple entry point.

## 4. Core Gameplay Loop

Each gameplay round follows this sequence:

1. Configure comfort and controls.
2. Select a scenario and game mode.
3. Read the scenario briefing.
4. Observe the injured NPC and the affected arm.
5. Identify appropriate everyday materials.
6. Move materials to the preparation area.
7. Position the rigid support along the injured limb.
8. Add protective padding or clothing.
9. Secure the construction using straps, belts, or fabric.
10. Request an evaluation or wait for the round to end.
11. Receive success or failure feedback.
12. Review mistakes, time, and stars.
13. Retry, open the tutorial, select another scenario, or return to the menu.

The central interaction is always:

> **Select → position → protect → secure → evaluate**

## 5. Scenarios

### 5.1 Hiking trail

This is the primary MVP scenario. The player encounters an adult friend with a stylized arm injury on a hiking trail.

Possible objects include:

- A branch or stick
- A rigid piece of wood
- A jacket
- A scarf
- A belt or strap
- Distractors such as a bottle, rock, phone, or backpack

The scene contains a trail, vegetation, rocks, a log, camp props, and a clearly defined preparation area.

### 5.2 Playground

The player assists an adult near a playground. The environment includes a ground area, benches, a swing, a slide, and a seesaw. Alternative rigid and textile materials provide a different object set while preserving the same mechanics.

### 5.3 Car breakdown

The player helps an adult near a stopped vehicle. The environment includes a road, curb, vehicle, accessible trunk, rigid support material, textile material, and straps or belts.

This scenario changes the spatial layout and object source while preserving the same interaction rules.

## 6. Game Modes

### Guided mode

Guided mode is intended for first-time players and learning:

- No time limit
- Highlighted objects
- Step-by-step instructions
- Optional hints
- Placement outlines
- Immediate confirmation after each valid action
- Explanations for unsuitable objects
- Ability to repeat or restart the exercise

Guided mode teaches the procedure and controls but does not produce an independent evaluation record.

### Evaluation mode

Evaluation mode tests independent performance:

- Timer enabled
- Fewer or optional hints
- Objects reset at the beginning
- Mandatory steps must be completed
- Mistakes are recorded
- Success requires all essential criteria
- Results contribute to local records and progress

A failed evaluation does not erase previous successful evaluations.

### Panic mode

Panic mode is an optional advanced mode that simulates controlled stress. It may include distorted or intensified ambient sound, pulsing time-pressure audio, adjustable visual effects, increased urgency, and reduced assistance.

The user must activate Panic mode explicitly. Effects can be disabled from the pause menu, but doing so marks the round as assisted and prevents it from being treated as an independent evaluation.

## 7. Interaction Design

The application supports both hand tracking and controllers.

### Basic interactions

The player can:

- Select UI elements
- Grab objects
- Hold objects with one or two hands
- Rotate objects
- Move objects through space
- Release objects
- Place materials into preparation zones
- Attach bands at marked points
- Undo or reposition incomplete elements

### Object states

Interactive objects may have these states:

- Available
- Grabbed
- Held with two hands
- Placed in the preparation area
- Positioned incorrectly
- Partially wrapped
- Correctly secured
- Resettable

### Simplified physical interactions

The application should feel physical without requiring complex cloth or physics simulation:

- A jacket can switch between loose and folded states.
- A scarf can switch between loose, placed, and support states.
- A strap can switch between loose, partially wrapped, and secured.
- A band can snap to a valid fixation point after the correct gesture.
- A dropped or inaccessible object can be returned to the preparation area.

Clarity, reliability, and comfort are more important than perfect physical realism.

## 8. Splint Construction Rules

The evaluation system checks:

- Whether the selected support is appropriate
- Whether padding or protective material is present
- Whether the support is placed in the correct region
- Whether the support orientation is correct
- Whether the injury area is covered appropriately
- Whether the limb is supported above and below the injury
- Whether fixation points are used correctly
- Whether the required number of securing elements is present
- Whether the required order of steps has been followed
- Whether the task was completed before the timer expired

A round succeeds only when every mandatory criterion is satisfied. A high number of stars cannot compensate for an incomplete splint.

## 9. Feedback and Evaluation

Feedback must be understandable to a beginner and must never rely only on color. The system should use visual highlights, text explanations, icons, sound effects, controller haptics where supported, NPC reactions, and checklist updates.

### Success result

The success screen may display:

- A positive limb-stabilized message
- Completion time
- Correct objects
- Completed steps
- Number of mistakes
- Star rating
- Scenario and mode
- Retry, tutorial, next scenario, and menu actions

### Failure result

The failure screen explains:

- Which steps were missed
- Which object choices were unsuitable
- Which placement was incorrect
- Whether the timer expired
- What should be corrected next time

The result screen always provides actionable next steps instead of only displaying success or failure.

## 10. Scoring and Stars

Stars represent performance within the simulation. Scoring may consider completion status, completion time, incorrect object choices, placement errors, hints used, assistance, and the selected mode.

Records are compared only between rounds with the same:

- Scenario
- Game mode
- Assistance status

Guided sessions, restarted rounds, interrupted sessions, and assisted rounds must not replace independent evaluation records.

## 11. Tutorial

The tutorial is available on first launch and from the main menu. It uses a neutral training arm on a table and has no time limit.

It teaches:

1. Selecting UI
2. Grabbing an object
3. Holding an object
4. Rotating an object
5. Using two hands
6. Releasing an object
7. Positioning a support
8. Applying protective material
9. Securing a band
10. Requesting an evaluation

The tutorial provides object outlines, step indicators, immediate confirmation, repeatable exercises, an explicit skip option, and the ability to reopen it later.

## 12. User Interface and UX

### Main menu

The main menu provides:

- Continue
- New session
- Demo
- Scenario selection
- Tutorial
- Progress
- Settings
- Exit

The interface is spatial VR UI rather than a traditional flat-screen menu.

### Scenario selection

Each scenario card displays its name, thumbnail, short description, recommended order, best result, available modes, and completion status. All scenarios remain accessible, with recommendations rather than unnecessary locks.

### In-game UI

The player can see:

- Wrist-mounted timer
- Current checklist
- Current objective
- Hint button
- Pause button
- Placement indicators
- Interaction icons
- Error and confirmation messages

The timer has clear normal, low-time, paused, and expired states. Time information must use text and symbols, not color alone.

### Pause menu

Pause completely stops the timer, object interaction, stress effects, and NPC progression.

The pause menu provides:

- Continue
- Comfort settings
- Help
- Restart
- Return to menu

Restart and returning to the menu require confirmation when a round is active.

## 13. Comfort and Accessibility

The application supports:

- Seated play
- Standing play
- Adjustable play-area height
- Dominant-hand selection
- Snap turning
- Vignette intensity
- Adjustable effect intensity
- Adjustable volume
- Subtitles
- Reduced visual effects
- Pause at any time
- Clear interaction distances
- Accessible object placement zones

Comfort settings can be changed before or during a session.

If tracking is lost or the headset is removed:

1. The application automatically pauses.
2. The timer stops.
3. Interactions are disabled.
4. The user is informed.
5. The session resumes only after tracking is restored and the user confirms.
6. Objects are stabilized before gameplay resumes.

The application must never restart the timer or interactions suddenly.

## 14. Demo Mode

Demo mode is designed for shared headsets. It provides temporary progress, fast comfort setup, a short tutorial, direct entry into the Hiking scenario, Guided mode by default, a temporary result screen, a next-participant action, and explicit reset confirmation.

Demo data must not modify the owner’s profile, affect local records, unlock the completion diploma, or persist after leaving Demo.

Resetting Demo removes only the guest session. Exiting Demo returns to the owner’s local profile.

## 15. Progress and Persistence

The application uses local JSON storage.

Persistent data includes:

- Comfort settings
- Dominant hand
- Subtitle and volume preferences
- Completed scenarios
- Evaluation results
- Best times
- Mistake counts
- Star ratings
- Diploma eligibility

Data is separated by scenario and mode. There are no required accounts, cloud saves, global leaderboards, or online services.

A symbolic diploma becomes available after one successful independent evaluation in each of the three scenarios. It must clearly state that it is not professional medical certification.

If a save file cannot be read, the application must inform the user, preserve the existing file, allow a temporary session, and avoid silently overwriting data.

## 16. Visual Direction

The visual style is friendly, low-poly, and readable:

- Soft shapes
- Clean silhouettes
- Warm lighting
- Natural but simplified environments
- Fresh greens for outdoor scenes
- Stylized characters
- No graphic injuries
- No blood
- No distorted limbs
- Strong contrast for interactive objects
- Clear distinction between usable objects and scenery

The visual style should make first-aid practice feel safe and approachable rather than frightening.

## 17. Audio Direction

Audio supports both immersion and interaction:

- Forest ambience
- Park ambience
- Road ambience
- Menu music
- Object grab and release sounds
- Textile and strap sounds
- UI confirmation sounds
- Success and failure signals
- Low-time pulse
- NPC voice lines
- Tutorial narration

Panic effects must be adjustable and must never mask important instructions.

Voice lines should be short and functional: briefing, request for help, hints, step confirmations, completion feedback, and NPC thanks.

## 18. NPC Design

The injured person is an adult NPC with a stylized arm injury. The NPC should have animations for:

- Idle
- Losing balance
- Holding the injured arm
- Reacting to player interaction
- Showing relief or thanks

The injury is communicated through posture, animation, and a clear visual indicator rather than graphic detail. The NPC supports the learning experience without excessive dialogue.

## 19. Technical Direction

The intended technology stack is:

- Unity 6000.x
- Unity XR Interaction Toolkit
- XR Hands
- OpenXR
- Optional Meta XR All-in-One SDK
- Blender for custom models
- ProBuilder for grayboxing
- Unity uGUI for VR panels
- JSON for local persistence
- Git and Git LFS for source control and large assets

The graybox uses real-world scale:

> 1 Unity unit = 1 meter

The initial prototype must validate reachability of nearby objects, NPC and limb scale, comfortable object size, stick handling, accessibility of two or three fixation points, and readability of wrist UI.

## 20. MVP Scope

The first implementable version includes:

- One Hiking scenario
- One adult injured NPC
- One training tutorial
- Three or four usable objects
- Object grabbing and positioning
- Padding or protection step
- Strap or band fixation
- Checklist validation
- Countdown timer
- Guided mode
- Evaluation mode
- Success and failure screens
- Basic comfort settings
- Pause and tracking-loss recovery
- Local result storage

The ideal-version extensions are:

- Playground scenario
- Car breakdown scenario
- Panic mode
- Star ratings
- Progress history
- Local records
- Symbolic diploma
- Demo mode
- Expanded audio and NPC dialogue

## 21. Experience Principles

All future implementation should follow these principles:

1. **Hands-on first:** the player must physically perform the task.
2. **Guidance before pressure:** teach interactions before introducing a timer.
3. **Clear over realistic:** simplify physics when realism creates confusion.
4. **Safe and non-graphic:** never use gore to communicate the injury.
5. **Errors are educational:** explain every important failure.
6. **No silent state changes:** pauses, resets, assistance, and persistence must be explicit.
7. **Comfort is always available:** the player can pause or reduce effects at any time.
8. **Improvisation matters:** different scenarios should use different everyday materials.
9. **Progress remains trustworthy:** guided or assisted rounds must not appear as independent records.
10. **Short repeatable sessions:** the player should be able to retry quickly and compare meaningful results.

## 22. Product Definition

**SplintIt VR is a friendly, hands-on VR first-aid trainer where players learn to improvise and secure an arm splint from everyday objects, first with guidance and then under controlled pressure, while receiving clear feedback about their decisions and technique.**
