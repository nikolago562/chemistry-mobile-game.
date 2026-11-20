# Handoff — Chemistry Lab (Unity 6000.6.9f1)

Georgian-language cozy chemistry life-sim. Scene: `Assets/Scenes/MainLab.unity`. All player-facing text must be Georgian.

## Repo state
- Branch `life-sim-expansion` (2 commits: `dde750d`, `35542a2`). `main` has no commits. Nothing pushed.
- Editor: `D:/unity/editors/6000.6.9f1/Editor/Unity.exe`.

## How to verify
```bash
"/d/unity/editors/6000.6.9f1/Editor/Unity.exe" -batchmode -projectPath "D:/UnityGame/chemistry game" -executeMethod LabGameChecks.Run -logFile <log>
```
Look for `CURRICULUM_CHECKS_PASSED`, `LIFE_SIM_CHECKS_PASSED` and `LAB_PLAYMODE_CHECKS_PASSED`; result also in `Logs/lab-checks-result.txt`; screenshots in `Logs/LabScreens/`.
`LabExpansionChecks.Run` needs the scene already open (wrap it with `EditorSceneManager.OpenScene(...)` in a temporary editor script); it never exits, so expect a timeout after `LAB_EXPANSION_CHECKS_PASSED`.

## Architecture (everything is built in code at play time from `LabGame.Awake`)
- `LabGame.cs` — wiring, menus (keys: Tab, N, B, L, R research, C collection, J noticeboard), runtime cosmetics, welcome-back message.
- `Progression/GameSaveManager.cs` — JSON save (`chem-lab.json`); new fields all default-safe so old saves load. `LabProgress.cs` — reputation, mastery, materials, collections, `SchoolCalendar` (time of day from local clock, saved daily weather, events). `ResearchProjects.cs` — 7 multi-stage projects + weekly science fair. `TeacherQuests.cs`, `TutorialManager.cs`.
- `UI/TeacherDialogueUI.cs` — shared dialogue box (typewriter, voice blips, camera zoom, `Tour` page hooks, `PointAt` camera pans, `Snap` picture-in-picture). `UI/TeacherGuide.cs` — 6 tutorial lessons (welcome, coat, tour, lab, school, growth). `UI/ExperimentGamesUI.cs` — 6 activities + safe failures + `SafeMishaps`. `UI/LifeSimUI.cs` — research board, collection/progress/storage, noticeboard/fair. `UI/GameHUD.cs` — compact HUD, `HudFade`, `Highlight`.
- `World/SchoolAreas.cs` — hallway, storage, reading, greenhouse, exhibition (rooms at world x 200–390). `World/Classmates.cs` — 7 NPCs (data + dialogue + `SchoolNPC`). `World/LabAtmosphere.cs` — lighting, windows, rain, clock, glows, `AmbientAudio`. `World/PersonalLab.cs` + `PersonalDecor.cs` — catalog (equipment + decorations), 3 expansions, swap-on-place. `World/DecorArt.cs` — procedural pixel sprites.

## Learning progression (beginner curriculum → advanced floor)
- `Progression/Curriculum.cs` — all lesson data (8 chapters, 34 lessons + 8 chapter reviews), gates (`StationOpen`, `ActivityOpen`, `TierOpen`), rewards (`Finish`), mistakes. Current lesson/chapter, unlocked stations and tiers are derived from saved `lessons` records, so nothing else needs saving.
- `UI/CurriculumUI.cs` — lesson player (teach in dialogue → demo → guided → free → 3-question test → reward), learning map [M], station practice hubs. Practice kinds: Sort, Atom, Molecule, Equation, Element, Ions, Pairs, Beaker, Drops, Scale.
- Tutorial now ends with the notebook (no chemistry). The tour hands straight into lesson 1.
- Classroom benches are themed stations (atom, element, reaction, molecule, solution, acid-base); new advanced desk at (37.5, -22). `ChemChallengeUI` is the advanced floor: tiers 1–4 (8 correct answers unlock the next; stats `tier:N`), timed bonus from tier 3, 10-question exam at tier 4; science fair opens at tier 4; rankings and teacher quests open after the curriculum.
- Save fields: `lessons`, `lessonMistakes`, `notebookPages` (default empty; old saves start at lesson 1).

## Gameplay-first rules (interaction layer)
- `World/Focus.cs` — every interactable (stations, `WorldInteractable`, personal-lab equipment, doors) gets a coloured floor outline by `Glow` kind (mint equipment, gold reward, violet teacher, coral urgent, blue door, green done). It wakes near the player, sparkles when focused, blinks from afar when it is the objective. Only one object is focused at a time (objective → closest → doors → optional); only the focused one shows its prompt and takes [E].
- No menu dock. Menus are world objects: periodic wall chart (classroom, x 57.4), notebook lectern and shop/room-plan stand (personal lab back corners), shelf, cabinet, boards. The keys still work as shortcuts.
- Opening: 2-line welcome → closet → 1 line → teacher desk starts lesson 1 directly. The classroom tour and other guide lessons are optional, offered when you talk to the teacher.
- Lessons: one-line setup → demo → guided → free → reward (shows the concept after the action). The 3 quiz questions per lesson now only feed the chapter reviews.
- HUD: one objective line + progress bar; streak and combo stay hidden until the advanced floor opens.

- **Playtest mode is ON:** `GameSaveManager.FreshStartEachLaunch = true` discards the save on every launch and starts a new player with 9999 coins. Set it to `false` before release. Automated checks and the rain probe use their own save folder and aren't affected.
- Lessons start at their chapter's station (ch1 teacher desk, ch2 atom table, …). That station glows as the objective and the waypoint arrow points to it; solved steps move on by themselves.
- The teacher uses the student 8-way animator: she works facing her bench or chalkboard and, when talked to, says "ერთი წამით..." and turns smoothly (`TeacherNPC.Attend`).
- Missions (`Progression/Missions.cs`): ordered missions of steps (text, done rule, world target); the current step owns the HUD line, objective glow and waypoint, and completion pays coins, reputation and an optional lab item. World props set flags with `Missions.Set`. Act 1 lives in `World/ActOne.cs` (safety hazards, desk sorting, three states on the experiment cart, steam machine → `steam_wheel` in the lab). Plan for the later acts: `Docs/progression-design.md`.
- Fonts: `LabUI.Prewarm` loads all Georgian glyphs at boot (see `Docs/rain-lag-research.md`). `RainPerfProbe.Run` repeats the performance measurements.

## Known gaps / risks
- Not hand-played yet; pacing and difficulty are untested by a human. Curriculum is ~42 lessons; no human has timed them.
- Existing saves keep all items but restart the theory path, so the advanced quizzes are locked for them until it is finished.
- Time of day follows the real clock (same hour each day = same part of day).
- Interface font is FiraGO-Regular (Medium/Bold via its weight table, NotoSans fallback). The old NotebookGeorgian asset is unused.
- FiraGO TMP assets are ~28 MB each (fine for GitHub, slow first push).
- Georgian text was hand-reviewed for grammar; a native-speaker pass is still worthwhile.
- Bash heredocs containing apostrophes break in this environment; write Python edit scripts with the Write tool instead.
