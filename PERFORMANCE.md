# UI and movement update

The interface follows a pixel laboratory field-notebook style: cream paper,
chalkboard green, reagent accents, stepped panel corners, and bracketed shortcut
labels. The Georgian text and existing menus remain in place. Decorative panels
do not intercept pointer input; buttons, inputs, sliders and scrollbars do.

Walking is 4.25 world units/second (previous prefab value: 5.4). Acceleration is
28 units/second squared; braking is 40. Velocity reaches rest exactly, diagonals
remain normalized, and modal windows stop motion immediately. Walk playback scales
with physical velocity; footsteps use distance traveled rather than held keys.

## Walking FPS investigation

Unity 6000.6.9f1, local Editor, 180-frame CPU Profiler recordings. A scripted
horizontal walking path exercised the Rigidbody, eight-direction animation and
following camera. These are Editor measurements, not standalone FPS guarantees.

| Walking configuration | Median CPU frame | Approximate FPS from median | 95th percentile | Median allocation/frame |
| --- | ---: | ---: | ---: | ---: |
| Lab systems selected in Inspector | 15.258 ms | 66 | 19.549 ms | 118,269 bytes |
| Same walk, selection cleared | 3.457 ms | 289 | 5.842 ms | 16,447 bytes |

The selected systems object's runtime Inspectors dominated the first capture:
OnGUI averaged 7.696 ms inclusive and IMGUIContainer averaged 9.623 ms inclusive
(nested measurements, so do not add these). The active Transform and AudioSource
Inspectors also allocated memory during repainting. Clearing selection removed
the large walking penalty; physics was not the dominant cost in this capture.

`LabPlayModePerformance` temporarily clears a single selected LabGame systems
object when entering Play mode, then restores it after Play mode exits if no
different selection was made. This editor-only fix does not change player builds.
Selecting that large runtime object again can still incur Inspector overhead.

The 180-frame Editor captures used a scripted left/right movement path, with
velocity and eight-way Animator parameters driven directly to hold the same
movement conditions. The controller was disabled only for that measurement;
normal controller behavior was verified separately by the gameplay checks.

Additional runtime changes avoid formatting unchanged coin text, isolate the
moving objective marker in its own Canvas, and stop button scale writes once
their animation settles.

Profiler captures are under ignored `Logs/`: `idle-before.data`,
`walk-before.data`, and `walk-deselected.data`. Screenshots and the end-to-end
check result are in `Logs/LabScreens/` and `Logs/lab-checks-result.txt`.

## Validation

`LabGameChecks.Run` passes on an isolated test save. It covers character creation,
tutorial, progression, persistence, table collision/sliding, menus, cosmetics,
eight-direction animation, normalized walking, stopping and modal movement locks,
personal lab purchases/layout/doors, and all four chemistry activities.

Run the checks only in a disposable Editor session: this existing check runner
closes Unity on completion. It never uses the player's normal save directory.
