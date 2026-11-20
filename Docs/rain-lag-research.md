# Research note: "the game lags when it rains"

Date: 2026-11-20 · Unity 6000.6.9f1 · built-in render pipeline · Windows, D3D12

## Short answer

Rain itself does **not** make the game slower. Measured in batch mode and in a real editor
window, with the profiler attached, rain and clear weather cost the same in every room and at
every time of day. The game's own scripts take about 1 ms per frame either way.

The stutter that feels like "rain lag" comes from two other sources:

1. **Font loading in the middle of play (fixed).** The FiraGO fonts are *dynamic* TextMeshPro
   assets. The first time a letter appears in a given weight (Regular / Medium / Bold),
   TextMeshPro adds it to the font atlas and reads the font's OpenType tables
   (`FontEngine.Get_GPOS_Lookups`, `GetOpenTypeFontFeatures`, `Get_GSUB_Lookups`). That costs
   **50–90 ms in a single frame**. A rainy day shows new text (the weather word in the status
   line, the rain event, the rain-barrel prompt), so the first rainy session is exactly when
   these stalls happen. The Medium and Bold assets also have *Clear Dynamic Data On Build*
   switched on, so in a built game every letter is loaded mid-play again.
2. **Editor overhead (not a game problem).** In the editor, most of the remaining slow frames
   are `EditorLoop` (Scene view, Inspector, other editor windows repainting) and
   `Semaphore.WaitForSignal` (waiting on the GPU or vsync). A build doesn't have these.

## What was ruled out

| Suspect | Why it looked plausible | Result |
|---|---|---|
| Window raindrops (`LabAtmosphere`) | Moved every frame | 6 sprites per window, ~0.1 ms total. No difference rain vs clear. |
| Full-screen colour grade | Darker and more opaque in rain | Same one quad drawn in all weathers; GPU-synced render time unchanged. |
| Rain ambience (`AmbientAudio`) | Rain loop only audible in rain | Mixed on the audio thread; no main-thread cost. |
| `SchoolCalendar.Raining` | Read many times per frame | Small string allocation; same calls in both weathers. |
| Weather save every 5 s | `Refresh()` can write the save | Only writes when the part of day changes; no repeated writes. |
| Garbage collection | Could cause periodic spikes | ~7 KB/frame in both weathers; at most 1 collection per 270 frames. |

## How it was measured

`Assets/Editor/RainPerfProbe.cs` (`RainPerfProbe.Run`) opens `MainLab`, creates a throwaway
profile in `Logs/rain-probe-profile` (your real save isn't touched), then alternates clear and
rain weather. For each phase (270 frames after a 30-frame warm-up) it records:

- main-thread time, script time (`BehaviourUpdate`) and garbage allocated per frame,
- camera render time at 1920×1080, with a one-pixel readback so GPU work is included,
- real frame time (`Time.unscaledDeltaTime`) when run in a window,
- for every frame slower than 25 ms: the profiler markers with the most self time.

Run it with:

```
Unity.exe -projectPath "<project>" -executeMethod RainPerfProbe.Run -logFile probe.log
```

(add `-batchmode` for a headless run). Results go to `Logs/rain-probe.txt`.

### Batch mode, before the fix (afternoon and evening, three rooms)

| Phase | Avg frame | p95 | Slow frames (>25 ms) | Max |
|---|---|---|---|---|
| Classroom, clear | 9.9 ms | 17.3 ms | 22 (includes first-text font loading) | 173 ms |
| Classroom, rain | 6.8 ms | 11.8 ms | 0 | 17 ms |
| Evening, clear | 6.3 ms | 11.4 ms | 0 | 16 ms |
| Evening, rain | 6.9 ms | 11.9 ms | 3 | 29 ms |
| Hallway, clear | 7.0 ms | 12.4 ms | 1 | 92 ms |
| Hallway, rain | 7.9 ms | 13.8 ms | 1 | 48 ms |
| Greenhouse, clear | 6.8 ms | 12.1 ms | 0 | 19 ms |
| Greenhouse, rain | 6.0 ms | 10.3 ms | 0 | 15 ms |

### Editor window with the profiler, before the fix

| Phase | FPS | Slow frames | Top cost inside slow frames |
|---|---|---|---|
| Clear (first) | 55 | 61 | EditorLoop 83 ms, **FontEngine GPOS lookups 48 ms**, OpenType features 11 ms |
| Rain | 50 | 54 | EditorLoop 14 ms, GPU wait 8.5 ms |
| Clear | 54 | 31 | EditorLoop 20 ms, GPU wait 10 ms |
| Rain | 55 | 27 | EditorLoop 17 ms, GPU wait 9 ms |
| Clear | 51 | 30 | EditorLoop 15 ms, GPU wait 11 ms |
| Rain | 55 | 25 | EditorLoop 14 ms, GPU wait 12 ms |

Rain and clear trade places from run to run: the difference is noise, not weather.

### After the fix (batch mode)

Steady 104–115 fps in every phase. The font stall in the first phase dropped from 48 ms to
23 ms, and after that no font work appears in slow frames at all. Rain and clear stay within
noise of each other.

## The fix

`LabUI.Prewarm` (called from `LabUI.Initialize` at startup) adds every Georgian letter, the
ASCII range and the symbols the interface uses („“ — • → ★ × …) to the main font, every weight
in its weight table and its fallbacks, once, while the game loads. Text that appears later
(including on rainy days) never stalls on font loading.

## Recommendations

- Judge performance in a **build** or with the Game view maximised and the Scene view closed;
  `EditorLoop` was the largest single cost in editor slow frames.
- If any stall remains on first load, switch the FiraGO assets to *Static* with a Georgian
  character set baked in the editor. Atlases get bigger, but there's no runtime font work at all.
- Optional cleanup (not needed for speed today): read `SchoolCalendar.Raining` once per frame
  in `LabAtmosphere.Update` instead of once per window.
