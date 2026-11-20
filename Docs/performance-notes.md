# Performance notes: "FPS drops to 30 when walking"

Date: 2026-11-20 · Unity 6000.6.9f1 · built-in renderer

## Why walking dropped to 30 FPS

**It was the Unity Inspector, not the game.** The quality level is *Ultra* with VSync on,
so on a 60 Hz screen any frame slower than 16.7 ms waits for the next refresh and the game
falls from 60 to 30 FPS.

When the systems object (`ლაბორატორიის სისტემები`: LabGame, AudioManager and 8 AudioSources,
about 40 components) or the AudioManager is selected during Play, the Inspector redraws it.
The footstep AudioSource only plays while you walk, and a playing AudioSource makes the
Inspector repaint every frame:

| | Standing | Walking |
|---|---|---|
| With the systems object selected | 123–144 fps | **38–47 fps**, 5–8 ms of Inspector GUI, ~150 KB garbage per frame |
| With nothing selected | 132 fps | **136 fps** (no difference) |

`Assets/Editor/LabPlayModePerformance.cs` already deselected the main object on Play; it
now deselects **any** scene object on Play and restores the selection when you stop. If you
select an object *during* Play, the Inspector cost comes back. In a build there's no Inspector.

## Other optimisations made

| Change | Before | After |
|---|---|---|
| Classmates check schedule and quest markers twice a second instead of every frame; cached walk goal | 4.3 KB garbage/frame | ~0 |
| "Today" date string cached once per day (`SchoolCalendar.DayKey`), used by `LabProgress.Today` and the weather | allocated on every call, many times a frame | once a day |
| Save manager compares dates instead of formatting a string every frame | 0.12 KB/frame | 0 |
| Bubbles and steam puffs reuse a pool instead of creating and destroying a GameObject each | ~8 objects/s churn, 1.4 KB/frame | pooled |
| Highlight glow re-checks type and objective 5× a second (prompt choice stays exact) | 0.8 KB/frame | ~0 |
| HUD waypoint recomputed 5× a second, text set only when it changes | text mesh rebuilt every frame | on change |
| Station prompts rebuild their text only while shown | every frame in range | only when focused |
| Collection shelf scans the catalogue once a second | every frame | 1×/s |
| Only the current school room and your lab are active | 2,828 active sprites (1,426 floor tiles) | ~295 |
| World labels share one material per style | one material copy per label (no batching) | shared |

**Total game garbage:** about 7.6 KB per frame → about 0.3 KB per frame, with no garbage
collections during play. **Game scripts:** 0.8 ms → 0.6 ms per frame.

Rendering stayed at about 6–7 ms (1080p, GPU-synced, batch mode). Switching off hidden rooms
didn't measurably change it: culling those sprites was cheap.

## How to measure

`Assets/Editor/RainPerfProbe.cs`:
- `RainPerfProbe.RunWalk`: walks the player around the classroom aisles and compares with
  standing; reports FPS, script time, garbage per script and the profiler markers per frame.
  Results go to `Logs/walk-probe.txt`. Run it without `-batchmode` to include the real Game view.
- `RainPerfProbe.Run`: clear vs rain (see `Docs/rain-lag-research.md`).

```
Unity.exe -projectPath "<project>" -executeMethod RainPerfProbe.RunWalk -logFile walk.log
```

## Tips

- Test performance in a **build**, or with nothing selected and the Scene view closed.
- The remaining frame time in the editor is mostly editor overhead (`EditorLoop`, `OnGUI`).
