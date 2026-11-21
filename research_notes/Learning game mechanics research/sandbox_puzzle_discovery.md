# Sandbox, Open-Ended Puzzle and Discovery/Combination Games That Teach Systems

Scope: Zachtronics (SpaceChem, Opus Magnum, Shenzhen I/O, TIS-100, Infinifactory), Human Resource Machine / 7 Billion Humans, Baba Is You, Little Alchemy / Doodle God / Infinite Craft, Potion Craft, Kerbal Space Program, Foldit, Melon Sandbox / The Powder Toy / CHEMIST by THIX / MEL VR, Cultist Simulator.
Target: a 2D top-down cozy chemistry school life-sim (Unity, Georgian-language). Its current loop is linear: lesson → demo → guided step → free step → quiz/reward. It also has stations, missions, collections, research projects, a weekly science fair and minigames.
Note on method: about 20 searches or fetches. Several primary pages (PC Gamer articles on Opus Magnum and KSP) were truncated on fetch, so some claims come from search-result summaries of those pages. Those are flagged. "Application" bullets are inferences, not cited facts.

## 1. What makes experimentation open-ended vs. scripted? How do these games reward curiosity without a teacher giving the answer?

### Takeaway
The open-ended games share one move. They give a small set of tools that combine in emergent ways and a goal with many valid solutions, and they never show "the" solution. Curiosity is rewarded by the system's own consistent response (the machine works, the rule flips, the bottle moves on the map), not by a tutor's approval. Cultist Simulator shows the limit: opacity raises the "aha", but early versions were so opaque they were almost unplayable.

### Cited Findings
**Zachtronics (Opus Magnum and family): minimal toolset, open problems, multiple solutions**
- Zachtronics' stated approach: "create a certain minimum set of tools that create emergent properties when used together" and "as long as the toolset is expressive and the problem is open-ended, we will see creative solutions from players", "including optimizations we hadn't really dreamed of before." — [Game Developer, Road to the IGF: Opus Magnum](https://gamedeveloper.com/business/road-to-the-igf-zachtronics-i-opus-magnum-i-)
- The team deliberately avoids designing puzzles around one optimal strategy, so players find approaches the designers did not expect. — [Game Developer, Road to the IGF: Opus Magnum](https://gamedeveloper.com/business/road-to-the-igf-zachtronics-i-opus-magnum-i-)
- "We deliberately don't offer any kind of in-game rewards for optimization". Some players are "more interested in simply challenging themselves on their own terms." — [Game Developer, Road to the IGF: Opus Magnum](https://gamedeveloper.com/business/road-to-the-igf-zachtronics-i-opus-magnum-i-)
- PC Gamer's subtitle frames Opus Magnum as encouraging "discovery and play rather than paralysis and frustration". Only the headline and subtitle could be fetched. — [PC Gamer, Bruno Dias, 24 Nov 2017](https://www.pcgamer.com/perfectly-solving-opus-magnums-puzzles-is-impossible-but-thats-ok)
- Matthew Burns (Zachtronics) on fusing world and mechanics: in Opus Magnum "there's this design space, here's this thing that we can do with alchemy, and here's this world that this alchemy takes place in". This is from a search summary. — [Game Developer, How Zachtronics makes personality-filled puzzlers](https://gamedeveloper.com/design/how-zachtronics-makes-personality-filled-puzzlers-like-i-opus-magnum-i-)
- GDC talk "Inside the Indie Mind: Zach Barth" covers the studio's process from foundation to mechanics to story integration. Only the talk listing was seen. — [GDC Vault](https://gdcvault.com/play/1024969/Inside-the-Indie-Mind-Zach)

**Baba Is You: the rules themselves are manipulable objects**
- Rules are physical word blocks the player pushes, so pushing blocks changes how the game works. The game began at the 2017 Nordic Game Jam (theme "Not There"). It won "Game" and "Excellence in Design" at the IGF. — [Wikipedia](https://en.wikipedia.org/wiki/Baba_Is_You); [Game Developer, Road to the IGF: Baba Is You](https://www.gamedeveloper.com/business/road-to-the-igf-hempuli-oy-s-i-baba-is-you-i-)
- Teikari built most levels by "trying to figure out surprising/amusing interactions between them", then reverse-engineered a level that required each one. — [Game Developer, Designing Baba Is You's rule-writing system](https://www.gamedeveloper.com/design/designing-i-baba-is-you-i-s-delightfully-innovative-rule-writing-system)
- "Variant" levels were often based on "tester-found alternative solutions". Unintended solutions were kept as extra content instead of being patched out. Levels were cut if the player could "ignore and solve the level via other means." — [Game Developer, Designing Baba Is You's rule-writing system](https://www.gamedeveloper.com/design/designing-i-baba-is-you-i-s-delightfully-innovative-rule-writing-system)
- Teikari's caveat: "the levels very easily become overwhelming as the number of initially-active rules increases." — [Game Developer, Designing Baba Is You's rule-writing system](https://www.gamedeveloper.com/design/designing-i-baba-is-you-i-s-delightfully-innovative-rule-writing-system)

**Potion Craft: an explorable map instead of a recipe list**
- Instead of following recipes, the player gets an empty map with a potion bottle in the centre. Each ingredient, and how it is prepared (e.g., ground), moves the bottle in a direction. Effects are found by navigating to them. — [PC Gamer, "a crafting system I don't hate"](https://pcgamer.com/potion-craft-has-finally-given-me-a-crafting-system-i-dont-hate); [Destructoid impressions](https://www.destructoid.com/potion-craft-alchemist-simulator-relaxing-alchemy-shop-sim-impressions/)
- Critique: the map exploration is not well connected to the shop customers. — summarised from the same coverage set ([Destructoid](https://www.destructoid.com/potion-craft-alchemist-simulator-relaxing-alchemy-shop-sim-impressions/))
- Commercial result: 100k copies in the first three days of Early Access (Sept 2021), and #1 on Steam Global Top Sellers for three days. — [Gamespress / tinyBuild press release](https://gamespress.com/Potion-Craft-sells-100k-copies-in-the-first-three-days); [TechRaptor](https://Techraptor.net/gaming/news/potion-craft-steam-sales-hit-100k-sales-in-three-days)
- Creator Mikhail Chuprakov cites Kingdom Come: Deliverance's hands-on alchemy as an inspiration. — [Shacknews](https://shacknews.com/article/127752/how-kingdom-come-deliverance-influenced-potion-craft-alchemist-simulator)

**Kerbal Space Program: learning a real system through an accurate sandbox**
- A 2025 arXiv teaching paper argues KSP's accurate orbital sandbox "allows for the intuitive discovery of Kepler's laws in real time and with more direct control than traditional applets". It notes the skills take 10+ hours to learn, and it advises against KSP 2 for classrooms. It is a teaching proposal, not an outcome study. — [arXiv 2508.13259](https://arxiv.org/html/2508.13259v1)
- An APS 2025 abstract describes using KSP in intro astronomy because students struggle to build intuition for Kepler's laws. — [APS AAPT abstract](https://archive.aps.org/aaptsm/2025/a02/1)
- Montgomery County Community College ran a KSP-based math/physics lab. The instructor wrote a six-chapter workbook from algebra through differential equations. Reported benefits are anecdotal. — [MC3 news](https://mc3.edu/news/2021/01/kerbal-math-physics-lab)
- TeacherGaming (maker of MinecraftEdu) made KerbalEdu with Squad, sold at US$17 per copy or 25 copies for US$330 in early access. — [Lowyat](https://www.lowyat.net/2014/18343/kerbal-space-program-to-build-educational-mod/); [PC Gamer](https://pcgamer.com/kerbaledu-launches-to-bring-kerbal-space-program-to-more-classrooms)
- Sales: no official figure. A third-party estimate is about 2.9M Steam copies (medium confidence). An older 2016 community figure was about 1M. — [Raijin.gg](https://raijin.gg/app/220200/Kerbal_Space_Program); [KSP forum](https://forum.kerbalspaceprogram.com/topic/132220-squads-balance-sheet-and-income-statment)

**Cultist Simulator: discovery as the game itself (and its risk)**
- Weather Factory "wanted to make a game where understanding the game was itself the game". There is no tutorial, and hints are embedded in flavour text. Kennedy has said "aha!" moments are most exciting after confusion, but "early versions were so opaque as to be almost unplayable." These quotes come from search summaries of Game Developer coverage of his GDC 2019 talk. — [Game Developer, Designing experimental game Cultist Simulator for commercial success](https://gamedeveloper.com/design/designing-experimental-game-i-cultist-simulator-i-for-commercial-success); [Game Developer, house of cards](https://gamedeveloper.com/design/why-the-i-cultist-simulator-i-devs-built-their-lovecraftian-game-on-a-house-of-cards)
- Cards let mechanics be revealed piece by piece, so learning the rules mirrors the protagonist learning the occult universe. A noted flaw is that players found grinding money could bypass the intended discovery. — [Game Developer coverage](https://gamedeveloper.com/design/designing-experimental-game-i-cultist-simulator-i-for-commercial-success); [Cultist Simulator retrospective](https://www.gamedeveloper.com/business/cultist-simulator-the-retrospective)

**Human Resource Machine / 7 Billion Humans: programming concepts as visual puzzles**
- Tomorrow Corporation pitches HRM as teaching players to "think like a programmer". It introduces loops (via jumps) and memory as it progresses. There is a free Hour of Code edition and an education discount. — [Wikipedia: Human Resource Machine](https://en.wikipedia.org/wiki/Human_Resource_Machine); [Tomorrow Corporation](https://tomorrowcorporation.com/?p=1994)

**Foldit: open-ended play producing real science**
- Foldit players solved the structure of the Mason-Pfizer monkey virus retroviral protease. It had resisted solution for over a decade, and players did it in about three weeks (Nature Structural & Molecular Biology, 2011). — [UW News](https://www.washington.edu/news/2011/09/19/gamers-succeed-where-scientists-fail); [Baker Lab](https://www.bakerlab.org/2015/11/26/crystal-structure-of-a-monomeric-retroviral-protease-solved-by-protein-folding-game-players/)
- Players wrote over 5,400 "recipes" (macros). The top ones closely matched an unpublished algorithm that scientists developed in the same period (PNAS 2011). — [PubMed 22065763](https://pubmed.ncbi.nlm.nih.gov/22065763/)
- A study of 57,000 players found humans better than software at some pattern-recognition aspects of structure prediction. It was published in Nature, 2010 per my own knowledge; the search results did not give the year. — [Scientific American](https://www.scientificamerican.com/article/gaming-the-system-video-gamers-help-researchers-untangle-protein-folding-problem/)

### Inferences
- Open-ended means four things together: (a) a small composable toolset, (b) a goal checked by the simulation, not by a script that checks the method, (c) more than one accepted solution path, and (d) the system's response is consistent and readable, so the player learns from it, not from the teacher.
- **Mechanic: minimal composable toolset + outcome-only validation.** *Application:* the "free step" should check the result (e.g., "pH 7 solution in beaker" or "make a gas that turns limewater cloudy"), not whether the player followed the teacher's order. Accept any valid route, such as neutralising with NaOH or with Ca(OH)₂.
- **Mechanic: rules as objects (Baba).** *Application:* a lab-bench minigame where reaction "rule cards" (e.g., "acid + metal → salt + H₂") are physical tiles the player arranges. Swapping a tile changes what happens, which teaches that reactions follow general patterns.
- **Mechanic: tester-found alternatives become content (Baba variants).** *Application:* log unusual successful solutions (playtests or telemetry) and turn the good ones into "alternative method" bonus missions or teacher remarks ("ოჰ, ასე არ მიფიქრია!", "Oh, I never thought of that!").
- **Mechanic: navigation-map crafting (Potion Craft).** *Application:* a "property map" station where adding reagents moves a marker on a 2D pH × temperature (or concentration) plane. Target zones are named products or indicator colours. It fits a top-down 2D game, and the map can reveal itself as the player explores. Avoid Potion Craft's weak point by tying map targets to missions and NPC requests.
- **Mechanic: hidden-then-legible discovery (Cultist).** *Application:* the linear lesson can be the safety net, while optional "mystery reagents" or unlabeled bottles in the storeroom work Cultist-style, with hints in item descriptions or a classmate's notebook. Keep a fallback hint system, because the "almost unplayable" warning applies doubly to kids.
- **Mechanic: player-authored "recipes" (Foldit macros).** *Application:* let players save a sequence of lab actions as a named procedure in their lab notebook and reuse it. That is the same idea as Foldit recipes and HRM programs, and it teaches the scientific method as repeatable procedure.

### Gaps
- No Zach Barth primary quote found on why SpaceChem/Infinifactory use open-ended validation. The GDC talk content was not accessible.
- No sales figures found for Baba Is You, Opus Magnum, SpaceChem, or Human Resource Machine.
- No controlled outcome studies found for KSP or Foldit as teaching tools. Evidence is practice-based or anecdotal.
- The Potion Craft creator interview (DTF, in Russian) on map design was not fully accessible.

## 2. How do histograms, leaderboards and optimization metrics create replay and self-competition?

### Takeaway
Zachtronics scores each puzzle on several independent axes (e.g., cost, cycles, area in Opus Magnum). On completion it shows where your solution falls on a histogram of all players, plus a friends list. This turns "solved" into "solved, but could be better on X". Because the axes trade off against each other, there is no single "best" solution, and it gives replay without in-game rewards. 7 Billion Humans uses a lighter version: steps and speed against developer-set targets.

### Cited Findings
- Opus Magnum metrics are cost, area and cycles. — [Game Developer, Road to the IGF: Opus Magnum](https://gamedeveloper.com/business/road-to-the-igf-zachtronics-i-opus-magnum-i-)
- Zachtronics games show results as histograms of other players who completed the puzzle, including Steam friends. — [Wikipedia: Zachtronics](https://en.wikipedia.org/wiki/Zachtronics)
- Opus Magnum's histograms appear after every completed level, alongside your rank against Steam friends. — [Steam discussion / community commentary](https://steamcommunity.com/app/558990/discussions/0/3800524254170628358); [biggieblog, Tracking the global Opus Magnum records](https://biggieblog.com/tracking-the-global-opus-magnum-records/)
- PC Gamer (Bruno Dias) argues the histograms encourage optimising size, cost and speed and work better than a leaderboard, and that a good-looking machine is its own reward. This comes from a search summary; the article body was truncated on fetch. — [PC Gamer](https://www.pcgamer.com/perfectly-solving-opus-magnums-puzzles-is-impossible-but-thats-ok)
- The headline claim "Perfectly solving Opus Magnum's puzzles is impossible, but that's OK": multiple axes mean no single perfect answer. — [PC Gamer](https://www.pcgamer.com/perfectly-solving-opus-magnums-puzzles-is-impossible-but-thats-ok)
- Zachtronics deliberately offers no in-game rewards for optimisation. — [Game Developer](https://gamedeveloper.com/business/road-to-the-igf-zachtronics-i-opus-magnum-i-)
- TIS-100's earlier leaderboard had 4 categories per puzzle on an honor system. A histogram avoids ranking individuals head-to-head. — [search summary of community sources](https://itch.io/post/2542485)
- A community site tracks global Opus Magnum records, including Pareto-frontier style records across metrics, showing self-organised competition beyond the in-game histogram. — [biggieblog](https://biggieblog.com/tracking-the-global-opus-magnum-records/)
- Caveat from players: the histogram's left-most bar can be misread as the true optimum. — [Steam discussion](https://steamcommunity.com/app/558990/discussions/0/3800524254170628358)
- 7 Billion Humans ranks solutions by program size (steps) and speed (cycles) against developer-set averages. — [Wikipedia: 7 Billion Humans](https://en.wikipedia.org/wiki/7_Billion_Humans)
- Foldit uses a score (lower energy = higher score) and competition among players and teams. — [Scientific American](https://www.scientificamerican.com/article/gaming-the-system-video-gamers-help-researchers-untangle-protein-folding-problem/)

### Inferences
- Histograms work because they are non-punishing. Seeing yourself "in the middle hump" is fine, and the spread itself tells you better solutions exist, which is a hint with no spoiler.
- Multiple orthogonal metrics stop players from converging on one meta. Each axis is a new puzzle on the same level.
- **Mechanic: per-experiment multi-metric score.** *Application:* after each free step or research project, show 2–3 chemistry-meaningful metrics, such as **reagent used (g/ml, i.e., waste)**, **steps/time**, and **purity/yield** or **safety (spills/heat)**. These map onto real green-chemistry values (atom economy, waste), so optimising is learning.
- **Mechanic: histogram instead of leaderboard.** *Application:* for an offline, single-player kid audience, show a histogram of classmate NPCs' "results" (pre-baked distributions) or the player's own past attempts. Online histograms are optional later. Avoid ranked leaderboards, which hurt the cozy tone.
- **Mechanic: no extrinsic reward for optimisation.** *Application:* keep the quiz/reward for completion, and make optimisation purely cosmetic or bragging (a star on the notebook page, a teacher remark). Don't gate progress on it.
- **Mechanic: science-fair as the social leaderboard.** *Application:* the weekly science fair can judge entries on the same metrics ("most efficient", "purest", "most creative method"), with multiple ribbon categories so different playstyles win.

### Gaps
- No primary Zachtronics/Barth statement found explaining the histogram design choice.
- No data found on how much histograms increase replay (e.g., percent of players who re-solve levels).

## 3. How is failure made funny or spectacular instead of punishing?

### Takeaway
KSP is the canonical case. Its creator's roots are in dismantling fireworks, and the studio frames lost kerbals and explosions as a source of humour and story. Melon Sandbox and The Powder Toy go further: destruction and chain reactions are the toy, and their scale (150M+ downloads, 10–19M MAU for Melon Sandbox) shows the appeal of consequence-free spectacle. The pattern is cheap retry, a visible and funny cause-and-effect, and failure that shows *why* it failed.

### Cited Findings
- PC Gamer interview (Kerbal Kon) with Felipe Falanghe and Bob Holtzman discusses "why failure is fun". The full text was not retrievable; the search summary says Falanghe described small wins gradually offsetting the worry about failing. — [PC Gamer, 2013](https://pcgamer.com/kerbal-space-program-dev-on-random-solar-systems-the-joy-of-failure-and-the-cult-of-steam)
- KSP originated in Falanghe's childhood fascination with bottle rockets and fireworks. He and friends dismantled fireworks to see how they worked. "We lost a lot of kerbals in those days," he joked. — [Game Informer, The Story of KSP](https://gameinformer.com/games/kerbal_space_program/b/pc/archive/2013/09/01/the-story-of-kerbal-space-program); [Engadget, 2013](https://www.engadget.com/2013-09-04-the-atypical-story-of-kerbal-space-programs-indie-flight-to-suc.html)
- Holtzman describes the audience as anyone who has "planned for success in spite of prior failure or loves to tinker and laugh." — [Game Informer](https://gameinformer.com/games/kerbal_space_program/b/pc/archive/2013/09/01/the-story-of-kerbal-space-program)
- A KSP postmortem was presented at GDC 2015. Only the announcement was seen. — [Game Developer](https://www.gamedeveloper.com/design/-i-kerbal-space-program-i-postmortem-coming-to-gdc-2015)
- Melon Sandbox (formerly Melon Playground), a ragdoll physics sandbox from PlayDucky, has passed 150M downloads across iOS, Android and PC. It has a 40-minute average session and peaked at 19M MAU. It keeps about 10M monthly players and had 4.4M players on its biggest day. It paid out over $1M to UGC creators in 2025, many under 20. These figures are company-reported. — [PocketGamer.biz, 150m installs](https://www.pocketgamer.biz/melon-sandbox-150m-installs-and-ugc-evolution/); [PocketGamer.biz, 10m monthly](https://www.pocketgamer.biz/melon-sandbox-adds-lava-themed-maps-as-sandbox-hit-maintains-10m-monthly-players/); [PocketGamer.biz, $1m payouts](https://www.pocketgamer.biz/melon-sandbox-surpasses-1m-payouts-in-2025)
- The Powder Toy is a GPL-3 open-source falling-sand sandbox simulating heat, pressure, gravity and electricity. It has about 258 materials (Feb 2025), including chemical, nuclear and electronic ones; some reactions are realistic and some invented. It has a Lua modding API and came to Steam in June 2024. — [Wikipedia](https://en.wikipedia.org/wiki/The_Powder_Toy); [Steam](https://store.steampowered.com/app/1148350/The_Powder_Toy/)
- Opus Magnum is framed by PC Gamer as avoiding "paralysis and frustration". Solutions always run visibly, so a broken machine is watched, not just reported. — [PC Gamer](https://www.pcgamer.com/perfectly-solving-opus-magnums-puzzles-is-impossible-but-thats-ok)

### Inferences
- Failure feels fun when (1) it is instant and cheap to retry, (2) it is visual and slightly absurd (kerbals' faces, ragdolls), (3) the cause is readable from the replay, and (4) nothing permanent is lost.
- **Mechanic: cartoon "lab mishap" with readable cause.** *Application:* wrong mixes trigger cozy, non-scary mishaps such as foam overflow, a coloured smoke puff, a soot-faced character sprite, or a kitten startled off a shelf. Each comes with a one-line "what happened" card in Georgian (e.g., "ძალიან ბევრი ნატრიუმის ბიკარბონატი, CO₂ გამოიყო!", "Too much sodium bicarbonate, CO₂ was released!"). The failure becomes a micro-lesson.
- **Mechanic: "blooper" collection.** *Application:* add a "Mishaps" page to the collections menu. Each distinct failure type found is recorded with an illustration. Failure then feeds the collection loop instead of only costing time, and curious kids will try to collect them.
- **Mechanic: free retry / undo.** *Application:* in free steps and the sandbox, offer one-tap reset and refill of reagents. Keep quiz attempts as the only "stakes".
- **Mechanic: safe sandbox room (Powder Toy/Melon).** *Application:* unlock an after-school "free lab" with no objectives. It is a 2D particle or tile simulation of a few properties (heat, pH, gas), clearly labelled as simplified, where kids can make things fizz and burn safely. It also gives the science fair raw material.
- **Safety framing:** real chemistry has real hazards, so mishaps should model correct safety responses (goggles, fume hood, teacher's fire blanket). This keeps it funny without teaching recklessness. This is an inference; see Gaps.

### Gaps
- Full KSP interview quotes on failure, and the GDC 2015 postmortem content, were not retrievable.
- No research found on whether comedic failure affects learning retention in educational games.
- No sourced guidance found on how chemistry edu-apps balance "spectacular" failure against safety messaging.

## 4. How does combination/discovery (unknown recipes, "?" slots, encyclopedia filling) drive collection motivation?

### Takeaway
Combination games (Little Alchemy, Doodle God, Infinite Craft) reached huge audiences through one loop: start with 4 elements, drag two together, get a new item, and watch the "found X / total" counter rise. Doodle God passed 100M downloads mostly organically (95%). Infinite Craft added a global "First Discovery" tag and reached 300M+ recipes per day. A known total with visible gaps (Little Alchemy, Doodle God) drives completionism. An unbounded space with global firsts (Infinite Craft) drives novelty-seeking and sharing.

### Cited Findings
- Little Alchemy starts from four basic elements. The first game had about 100 items and grew to 580+ (sources vary: 560/580/589). It is free with no ads or IAP. It began as a Chrome Web Store experiment. Designer Jakub Koziol, Recloak Games. — [web.dev case study](https://web.dev/case-studies/little-alchemy); [App Store](https://apps.apple.com/app/id542467126); [Giant Bomb](https://www.giantbomb.com/games/3030-43795/)
- The App Store pitch is "every combination is a little puzzle to solve", with achievements as a secondary layer. A 2024 UX critique of Little Alchemy 2 said it could better help players find and revisit information. — [App Store](https://apps.apple.com/app/id542467126); [Pratt IXD critique](https://ixd.prattsi.org/2024/02/design-critique-little-alchemy-2-ios-app/)
- The Doodle God series (JoyBits) passed 100M downloads across Doodle God, Doodle Devil and Doodle Farm since 2010. 95% were organic, and about 60% came via Flash portals like Kongregate. It later passed 150M and then 170M+. The current App Store listing claims "over 190 million players". These are company figures counting installs, not unique users. — [GamesBeat](https://gamesbeat.com/after-100m-downloads-joybits-readies-an-ambitious-doodle-god-sequel-for-tablets); [PocketGamer.biz 170M](https://www.pocketgamer.biz/doodle-god-series-uses-flash-to-surpass-170-million-downloads-worldwide); [App Store](https://apps.apple.com/app/id376374689)
- Infinite Craft (Neal Agarwal, 2024) uses an LLM to generate combinations. Results are consistent across all players, so the first player globally to make an item gets a "First Discovery" label. Firsts became harder over time. — [Wikipedia](https://en.wikipedia.org/wiki/Infinite_Craft); [Prima Games](https://primagames.com/tips/what-does-first-discovery-mean-in-infinite-craft-answered); [Escapist](https://www.escapistmagazine.com/infinite-craft-first-discovery-explained/)
- Agarwal reported over 300M recipes created per day. That figure is via Wikipedia citing the NYT and was not verified at source. Another report says nearly 1,000 items crafted per second in early Feb 2024. — [Wikipedia](https://en.wikipedia.org/wiki/Infinite_Craft); [Dot Esports](https://dotesports.com/?p=1156488)
- Infinite Craft escalates from fire and water to "God", "the Big Bang", "Yin Yoda". Absurd, surprising outputs are part of the appeal. — [PC Gamer](https://www.pcgamer.com/this-browser-based-endless-crafting-game-starts-you-off-with-fire-and-water-but-it-quickly-escalates-to-god-the-big-bang-and-yin-yoda)
- Cultist Simulator reveals mechanics card-by-card, so the "collection" is of understanding, not items. — [Game Developer](https://gamedeveloper.com/design/designing-experimental-game-i-cultist-simulator-i-for-commercial-success)

### Inferences
- Three distinct motivators: **completion** (X/580 counter, empty "?" slots), **surprise** (an unexpected output), and **status** (Infinite Craft's First Discovery, shareable).
- Combination games teach weakly: Little Alchemy logic is associative, not chemical. A chemistry game can keep the loop but make the rules *real*, so collecting means learning.
- **Mechanic: "?" slots in a known-size encyclopedia.** *Application:* a periodic-table / compound album (საცნობარო წიგნი, "reference book") where unknown entries show a silhouette, the category (acid/base/salt/gas) and the count found ("23/60 ნაერთი", "23/60 compounds"). Completing a row (e.g., all alkali-metal chlorides) unlocks a teacher bonus lesson.
- **Mechanic: free-combination bench.** *Application:* a station where any two reagents can be combined. Real reactions produce the product, a one-line equation and an encyclopedia entry. Non-reactions give "no reaction", which is also learning (e.g., Cu + HCl). Use a curated reaction table, not an LLM, for correctness and safety.
- **Mechanic: progressive hints instead of answers.** *Application:* the hint system goes "category hint" → "one ingredient revealed" → "full recipe" at a cost (time or a coin). Doodle God and Little Alchemy both have hint economies (general knowledge, not sourced in this research).
- **Mechanic: element collecting (Mendeleev-style).** *Application:* tie element unlocks to missions and research projects. Showing the periodic table filling in over the term gives a long-arc goal across the weekly loop.
- **Mechanic: "first discovery" status, localised.** *Application:* offline version: "first in your class", where NPC classmates react and the teacher mentions it in the next lesson. That gives social reward without online infrastructure.

### Gaps
- No primary interview with Little Alchemy's designer or JoyBits on why the combination loop works. Little Alchemy download totals were not found.
- No sourced information found on Mendeleev/element-collecting games specifically (e.g., "Elemental" puzzle apps). This needs a separate search.
- No data found on completion rates for combination games, i.e., how many players reach 100%.
- SpaceChem specifics (molecule-building as chemistry) were not researched in depth due to the tool budget.

## 5. How do sandbox chem apps represent real reactions (visual feedback, safety, realism)?

### Takeaway
The commercial chem apps trade breadth for realism. CHEMIST by THIX offers a realistic iPad bench (17 apparatus, 180 inorganic plus 60+ organic reagents), but reviewers complain that reactions are limited mostly to metals/acids and that concentration can't be controlled. MEL VR avoids simulating a lab at all and instead puts kids inside the molecular view (atoms, bonds, rearrangement). The Powder Toy mixes realistic and invented reactions freely. None of them had a controlled learning study.

### Cited Findings
- CHEMIST by THIX: an iPad virtual lab with 17 lab apparatus, 180 built-in inorganic reagents and 60+ organic chemicals. It is paid (about $8.99). Latest version 5.2 (Jan 2021). — [App Store](https://apps.apple.com/app/id440666387); [Apptopia](https://apptopia.com/ios/app/440666387/about)
- CHEMIST user reviews: "barely any reactions outside of metal and acids". Users can't do liquid nitrogen or solid chlorine. They ask for adjustable solution concentration and more equipment such as a burette. — [App Store reviews](https://apps.apple.com/gb/app/chemist-by-thix/id440666387)
- BEAKER by THIX (a smaller sibling, 150 chemicals, $0.99 with IAP) has a third-party estimate of 2.5M+ downloads. — [App Store](https://apps.apple.com/us/app/beaker-by-thix/id961227503); [mwm.ai](https://mwm.ai/apps/beaker-by-thix/961227503)
- MEL Science VR: 70+ VR lessons aligned to school chemistry and physics, covering isotopes, ions, the periodic table, isomers, gas laws, etc. It explicitly does not mimic a classic lab, but lets kids experiment on atoms and molecules, e.g., "inside" a reaction with molecules flying around. Lesson packs are $9.99 each and school licences about $50 per device per year. — [MEL Science VR](https://melscience.com/vr/); [THE Journal](https://thejournal.com/articles/2017/06/15/mel-science-launches-virtual-reality-chemistry-lessons.aspx); [App Store](https://apps.apple.com/us/app/id1369900784)
- No controlled study of MEL VR found. The only research link is a general Chemistry Education Research and Practice reference on VR depicting particle shape and motion. — [The Analytical Scientist](https://theanalyticalscientist.com/fields-applications/daydreaming-in-the-classroom)
- The Powder Toy simulates heat, pressure, gravity and electricity across about 258 materials, mixing real-world and invented reactions. — [Wikipedia](https://en.wikipedia.org/wiki/The_Powder_Toy)
- Foldit represents molecules visually and lets players manipulate them, with a physical energy score as feedback. It is a model of "real science rules + game-like visual feedback" that produced publishable results. — [UW News](https://www.washington.edu/news/2011/09/19/gamers-succeed-where-scientists-fail)

### Inferences
- There are two representation strategies: **macroscopic bench** (CHEMIST: colour changes, bubbles, precipitates) and **sub-microscopic view** (MEL VR, Foldit: atoms and bonds). Chemistry education commonly argues for linking macro, submicro and symbolic levels. That is general knowledge, not sourced here.
- The most common realism complaint is too few reactions and no quantitative controls (concentration). Breadth of *valid* combinations matters more than graphical fidelity.
- **Mechanic: triple-view feedback.** *Application:* each reaction at a station plays (1) a cozy 2D bench animation (colour change, bubbles, precipitate falling), (2) an optional "zoom-in" particle view of molecules rearranging, and (3) the balanced equation in Georgian notation. Unlock the zoom-in as a research-project reward.
- **Mechanic: one quantitative knob.** *Application:* add a concentration or amount slider at key stations (titration, neutralisation), since CHEMIST users specifically asked for it. Pair it with the Potion Craft-style map or a pH meter readout.
- **Mechanic: curated reaction table with explicit "no reaction" and "simplified" labels.** *Application:* hand-author a data table (ScriptableObjects in Unity) of reagent pairs → product/visual/equation. Every unlisted pair gives a sensible "no visible reaction" plus a reason category. That covers the breadth problem cheaply and keeps chemistry correct.
- **Mechanic: safety as feedback, not text.** *Application:* hazardous combos (e.g., bleach + acid) are blocked by the teacher NPC with a short explanation, or trigger a fume-hood minigame. This teaches safety inside the sandbox without needing real danger visuals.

### Gaps
- No download or player data found for CHEMIST by THIX itself.
- No peer-reviewed learning-outcome studies found for CHEMIST, BEAKER, MEL VR or The Powder Toy.
- No developer interviews found from THIX or MEL on how they chose which reactions to simulate.
- No Georgian-language chemistry games or apps were researched. That is out of scope here but relevant to localisation.
