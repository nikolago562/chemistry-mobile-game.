# Why Edutainment Is Boring, and What Research Says Works Instead

Context for every "Applies to our game" note: a 2D top-down cozy chemistry school life-sim (Unity, Georgian-language, single-player). Current loop per lesson: teacher dialogue -> demo -> guided step -> free step -> quiz/reward, about 42 lessons across 8 chapters, plus stations, missions, collections, research projects, a weekly science fair and minigames. The developer says it feels "one-typed".

## 1. Chocolate-dipped broccoli, intrinsic vs extrinsic integration, Malone & Lepper

### Takeaway
The classic failure is putting the game and the learning side by side: a fun wrapper, then a block of school content. The fix with the strongest experimental support is intrinsic integration: the thing you learn is the game mechanic itself. In Zombie Division this produced more learning and about 7x longer voluntary play. A later replication suggests the mechanism is attention (players only attend to what the game task needs), not just motivation.

### Cited Findings
- Amy Bruckman's 1999 talk/paper "Can Educational Be Fun?" coined "chocolate-dipped broccoli" for software that sweetens content with unrelated fun. Math Blaster is her main example. The more common "chocolate-covered" wording is usually attributed to Brenda Laurel (Utopian Entrepreneur, 2001). — [The Avocado](https://the-avocado.org/2021/11/17/thats-edutainment-duolingo-and-chocolate-dipped-broccoli); [Tedium](https://tedium.co/2019/05/09/edutainment-math-blaster-chocolate-covered-broccoli)
- At the GDC 1999 roundtables, many participants agreed with Bruckman that educational software is "sweetened" broccoli, and the phrase came up in every session. — [Ken Kahn, "Does Easy Do It?"](https://www.toontalk.com/English/easydoit.htm)
- A Warwick paper describes the pattern as games that alternate textbook content with simplistic "fun" segments, citing Math Blaster. — [Warwick paper](https://www2.warwick.ac.uk/fac/sci/dcs/research/em/publications/papers/downloads/126b.pdf)
- Habgood & Ainsworth (2011, Journal of the Learning Sciences), "Motivating children to learn effectively: exploring the value of intrinsic integration in educational games":
  - Study 1: 58 children played the intrinsic, extrinsic or control version of Zombie Division (division maths, ages 7–11) for 2 hours. Children learned more from the intrinsic version under fixed time limits.
  - When given free time, children played the intrinsic version 7 times longer.
  - Study 2: 16 children with free choice of game.
  - The authors argue that educational software usually uses the game as extrinsic sugar-coating. — [SHU repository](https://shura.shu.ac.uk/3556); [Nottingham repository](https://nottingham-repository.worktribe.com/output/23577510)
- Habgood's design principle: deliver learning material "through the flow of gameplay". — [Habgood, Jones & Mallinson (Outnumb3r3d)](https://core.ac.uk/download/42540786.pdf); [Habgood 2007 thesis](https://eprints.nottingham.ac.uk/10385/1/Habgood_2007_Final.pdf)
- Cutting & Iacovides (2022) say Habgood's data do not show the extra learning came from motivation. Their experiment points to attention instead: "players only pay attention to features needed for the game task and ignore task-irrelevant information." — [Cutting & Iacovides, White Rose](https://eprints.whiterose.ac.uk/189921/1/LearningByDoingCuttingAndIacovides090822.pdf)
- Malone & Lepper (1987), "Making learning fun: A taxonomy of intrinsic motivations for learning":
  - Individual motivations: Challenge (goals, uncertain outcomes, performance feedback, self-esteem), Curiosity (sensory and cognitive), Control (contingency, choice, power) and Fantasy.
  - Interpersonal motivations: Cooperation, Competition, Recognition.
  - On fantasy: "The most effective fantasies are those more fully integrated with the content to be learned."
  - The taxonomy builds on Malone (1981) on computer games. — [Karl Kapp summary](https://karlkapp.com/motivation-and-games-motivational-theories-for-instructional-games/); [Malone 1981 "What makes things fun to learn" PDF](https://ntnu.no/wiki/download/attachments/112986425/What_makes_things_fun_to_learn.pdf?api=v2); [learning-theories.com](https://learning-theories.com/?p=522)

### Inferences
- Applies to our game: the quiz at the end of each lesson is literally an extrinsic broccoli segment. The chemistry happens in the demo, guided and free steps, and then it gets checked in a different, non-game format. Make the station actions the assessment instead. If the player can produce the right precipitate, pH or flame colour, they have shown the knowledge. Log it silently instead of asking multiple-choice questions.
- Intrinsic integration rule of thumb: the core verb should be impossible to do well without the target concept. Examples:
  - Choose a reagent by predicting the reaction.
  - Balance an equation to set how much material to buy or pour.
  - Identify an unknown from evidence.
  Dialogue that explains a concept and then tests it is extrinsic.
- Cutting & Iacovides' attention account matters for a cozy life-sim with lots of decoration. Anything salient that does not depend on the chemistry draws attention away from it, which argues against heavy ambient distraction during learning moments.
- Malone & Lepper's levers that the current loop barely uses:
  - Curiosity: there is no unknown to discover; the teacher reveals everything first.
  - Control: the player follows set steps.
  - Fantasy integration: it is not yet clear whether the "school student" fantasy needs chemistry. For contrast, a student who runs a dye shop or a mystery club would.
  - Recognition: the weekly science fair can carry this.

### Gaps
- Could not get the full Habgood & Ainsworth 2011 text (publisher returned 403), so effect sizes for Study 1 learning gains are not in these notes.
- No primary text of Bruckman's 1999 paper was retrieved; the attribution comes from secondary sources and Kahn's roundtable summary.

## 2. Quiz-gating, lesson->test structure, and self-determination theory

### Takeaway
SDT research finds that games are enjoyed, and replayed, to the extent they satisfy autonomy, competence and relatedness. A fixed lesson->test sequence works against autonomy (no meaningful choice) and gives a weak sense of competence (passing a quiz instead of seeing your skill change the world). Meta-analytic evidence also finds learning games do not reliably beat conventional teaching on motivation, so being a game does not make something motivating.

### Cited Findings
- Ryan, Rigby & Przybylski (2006): autonomy, competence and relatedness each independently predicted game enjoyment and future play. — [Przybylski, Rigby & Ryan 2010 PDF, Rochester](https://www.rochester.edu/warner/lida/wp-content/uploads/2022/11/02bfe513dd59366750000000.pdf); [ACU research bank](https://acuresearchbank.acu.edu.au/item/865zw/a-motivational-model-of-video-game-engagement)
- Przybylski, Rigby & Ryan (2010, Review of General Psychology 14(2), 154–166) propose that the appeal and well-being effects of games come from satisfying basic psychological needs. — [Essex repository](https://repository.essex.ac.uk/7228/)
- Related work contrasts "having to" with "wanting to" play (harmonious vs obsessive engagement). — [ACU](https://acuresearchbank.acu.edu.au/item/89x90/having-to-versus-wanting-to-play-background-and-consequences-of-harmonious-versus-obsessive-engagement-in-video-games)
- Wouters et al. (2013): serious games were not significantly more motivating than conventional instruction (d = 0.26, n.s.; k = 31, N = 2,216). — [Utrecht research portal](https://research-portal.uu.nl/en/publications/a-meta-analysis-of-the-cognitive-and-motivational-effects-of-seri/); [e-teaching.org](https://www.e-teaching.org/materialien/literatur/wouters-van-nimwegen-van-ooostendorp-van-der-spek-2013)
- Koster: boredom comes when the game stops teaching. Once mastered, "the game becomes boring". If players cannot find a pattern in the noise, they get frustrated and quit. — [Koster AGC 2003 keynote](https://www.raphkoster.com/wp-content/uploads/2026/07/Austin-Game-Conference-2003-Keynote-Theory-of-Fun.pdf); [Theory of Fun PDF](https://www.raphkoster.com/gaming/atof/theoryoffun.pdf)

### Inferences
- Applies to our game:
  - Autonomy: let the player choose the order of lessons within a chapter, or which station or mission to do next. Offer more than one valid route to the goal (several reagents or methods work).
  - Competence: show mastery as capability, such as new recipes or stations unlocking, or an NPC problem that is now solvable, instead of a quiz score.
  - Relatedness: classmates and the teacher should need the player's chemistry (a classmate's request, the canteen's soap problem), not just deliver it to them.
- Koster's point maps directly to "one-typed". All 42 lessons share the same 5-beat template, so players master the template itself within the first chapter. After that, only the content changes and the activity stays the same, which reads as boring even with many interactions.
- Quiz-gating also makes failure a block on progress rather than information. Prefer soft gates, where the next thing is available but harder or less rewarding without mastery.

### Gaps
- I found no controlled study that isolates "quiz gate vs. no quiz gate" in a learning game. The claim that quiz-gating harms motivation is an inference from SDT and the edutainment critique, not a directly measured effect.
- I did not fetch Rigby & Ryan's PENS (Player Experience of Need Satisfaction) instrument details. It is a possible playtest questionnaire.

## 3. Variety, pacing, teach-without-telling, difficulty and flow

### Takeaway
Nintendo's level-design method introduces one mechanic, develops it, twists it and resolves it (kishōtenketsu). The mechanic itself teaches, through safe early exposure, rather than a lecture. PhET's "implicit scaffolding" is the science-sim version: guide through affordances, constraints, cueing and feedback so students feel they are exploring freely. Koster's theory frames boredom (fully learned) and frustration (no learnable pattern) as the two failure edges, which is essentially the flow channel.

### Cited Findings
- Koichi Hayashida and Shigeru Miyamoto use a four-step process for introducing a mechanic in a level: introduce, develop, twist, resolve. It is based on the kishōtenketsu four-act structure and was used for Super Mario 3D Land and 3D World. — [Wikipedia: Koichi Hayashida](https://en.wikipedia.org/wiki/Koichi_Hayashida); [Engadget](https://www.engadget.com/2015/03/17/super-mario-3d-world-design/); [MCV/Game Maker's Toolkit](https://www.mcvuk.com/development/video-nintendos-level-design-secrets-in-four-steps)
- The secondary coverage notes that the exact wording of the stages is an interpretation of Hayashida interviews. — [Kotaku](https://kotaku.com/what-made-super-mario-3d-world-so-great-1691916400)
- PhET implicit scaffolding (Podolefsky, Moore & Perkins):
  - Student exploration is framed through affordances, constraints, cueing and feedback, without explicit guidance.
  - It was refined over 10 years, 125 sims and more than 600 student interviews. — [arXiv 1306.6544](https://arxiv.org/pdf/1306.6544)
- "Guiding without feeling guided": implicit scaffolding reduces the explicit instruction needed (Perkins, Podolefsky, Paul 2013, Energy Skate Park Basics). — [CAS catalogue record](https://katalog.lib.cas.cz/KNAV/EdsRecord/edsair,edsair.doi...........7990b2830874dee022e9c43f7d147283)
- Koster: boredom is the opposite of learning, and frustration comes from noise without a pattern. Chris Crawford said first that fun is "the emotional response to learning". — [Koster "Theory of Fun 10 Years Later"](https://www.raphkoster.com/wp-content/uploads/2026/07/Theory-of-Fun-10-Years-Later.pdf); [Game Developer interview](https://gamedeveloper.com/design/raph-koster-s-theory-of-fun-ten-years-on)
- Malone's "Challenge" depends on uncertain outcomes and performance feedback. — [Karl Kapp](https://karlkapp.com/motivation-and-games-motivational-theories-for-instructional-games/)

### Inferences
- Applies to our game: use kishōtenketsu across and within lessons, not only the teach->test template.
  - Ki: a safe, wordless first encounter at a station (pour acid on chalk and watch it fizz).
  - Shō: develop it (different carbonates, different amounts).
  - Ten: a twist that reframes it (the "chalk" doesn't fizz, so it is something else; or you need the gas, so capture it).
  - Ketsu: resolve by applying it in a mission or for an NPC.
- Swap "teacher explains -> demo" for "player tries -> teacher names what they just saw". This also matches productive failure (section 4).
- A concrete variety audit: list the distinct verbs (pour, heat, mix, measure, observe, identify, sort/collect, deliver, build, choose). Then make each chapter's twist change the verb or goal, not just the substance:
  - identify an unknown
  - make X with limited resources
  - fix a broken process
  - optimize (cheapest or fastest)
  - explain to an NPC
  - predict before acting
- Pacing: alternate learning-dense beats with cozy downtime (life-sim chores, collections, social). The current loop is learning-dense every time, and the science fair is the only big-scale variation.
- Flow: difficulty should rise within a chapter and drop briefly at each new concept (the sawtooth curve). Avoid a uniform guided->free step size across all lessons.

### Gaps
- No primary Csikszentmihalyi source was retrieved in this session. Flow is described here through Koster's boredom/frustration framing only.
- No primary source was retrieved for Super Mario Bros. 1-1 ("teach without telling") or Portal's test-chamber tutorial design (e.g., Valve developer commentary). They are commonly cited but uncited here.

## 4. Productive failure, desirable difficulties, spacing, retrieval, interleaving

### Takeaway
Problem solving before instruction beats instruction followed by problem solving for conceptual understanding and transfer: a 53-study meta-analysis (Sinha & Kapur 2021) supports this, especially for secondary students. Practice testing (retrieval) and distributed practice (spacing) are the highest-utility learning techniques, and interleaving is moderate. Games can deliver all of these as play (recurring requests, mixed missions, try-first puzzles) instead of quizzes.

### Cited Findings
- Sinha & Kapur (2021, Review of Educational Research 91(5), 761–798):
  - Meta-analysis of 53 studies and 166 comparisons of problem-solving-then-instruction (PS-I) vs. instruction-then-problem-solving (I-PS).
  - Domains include maths, physics, chemistry, biology and medicine.
  - 45% of students were in grades 6–10.
  - The benefit holds more for secondary and undergraduate students than primary students.
  - It requires some prior knowledge. — [Phys.org/ETH](https://phys.org/news/2021-09-productively-wiser.html); [OUCI record](https://ouci.dntb.gov.ua/works/4KnMgZq7)
- In an ETH Linear Algebra course (about 650 students), students who did productive-failure tasks before lectures had a success rate about 20% higher. This is a press-release figure from a single course. — [Phys.org](https://phys.org/news/2021-09-productively-wiser.html)
- "When Productive Failure Fails" (Sinha & Kapur 2019 working paper) identifies poor design fidelity as the key reason productive failure fails to outperform alternatives. — [eScholarship](https://escholarship.org/content/qt89k7p1wr/qt89k7p1wr.pdf?t=sgiugo)
- Dunlosky et al. (2013, Psychological Science in the Public Interest) rated ten study techniques:
  - High utility: practice testing and distributed practice, across ages, abilities and tasks.
  - Moderate: elaborative interrogation, self-explanation and interleaved practice.
  - Low: rereading and highlighting. — [APS release](https://www.psychologicalscience.org/news/releases/which-study-strategies-make-the-grade.html); [AFT summary](https://www.aft.org/ae/fall2013/dunlosky); [Duke database](https://fds.duke.edu/db/aas/pn/faculty/emarsh/publications/252743)
- Clark et al. (2016): multi-session games had g = 0.44, while single-session games had g = 0.08 (n.s.). — [Clark et al. 2016, PMC](https://pmc.ncbi.nlm.nih.gov/articles/PMC4748544)
- Wouters et al. (2013): learning effects were larger with multiple training sessions. — [Utrecht](https://research-portal.uu.nl/en/publications/a-meta-analysis-of-the-cognitive-and-motivational-effects-of-seri/)

### Inferences
- Applies to our game:
  - Productive failure: open some lessons with a problem the player can't quite solve yet (a classmate's stained shirt, a cloudy aquarium). Let them try at stations, then have the teacher's lesson explain why their attempts behaved as they did. Keep failure safe and funny (cozy tone), and design for "fidelity": the attempts must expose the target idea.
  - Retrieval without tests: NPC requests, mission orders and the science fair should reuse earlier chapters' concepts weeks later, which spaces practice through the in-game calendar. A recurring "customer" asking for a pH-7 solution in chapter 6 is a spaced retrieval check.
  - Interleaving: missions should mix concept types so the player must decide which idea applies. Right now each lesson's free step tests only the concept just taught, which removes the "which tool?" judgement.
  - Desirable difficulty argues against hint-heavy guided steps. Fade guidance across a chapter instead of giving every lesson the same guided step.

### Gaps
- Could not retrieve Sinha & Kapur's own effect sizes (the publisher returned 403). I have seen figures of g ≈ 0.36 overall and larger with high design fidelity, but I could not verify them, so do not cite them.
- I found no source quantifying spacing or interleaving inside commercial games specifically.

## 5. Meta-analyses: what features correlated with learning

### Takeaway
Learning games beat conventional teaching modestly (d ≈ 0.29–0.33). The gains come from multiple sessions, scaffolding and simple, focused designs, not from rich narrative, realism or competition. Surprisingly, in Clark et al., more contextual "dressing" correlated with smaller effects.

### Cited Findings
- Wouters et al. (2013), Journal of Educational Psychology 105(2): learning d = 0.29, retention d = 0.36 (k = 77, N = 5,547). The benefit was larger when the game was supplemented with other instruction, used over multiple sessions, and played in groups. — [Utrecht](https://research-portal.uu.nl/en/publications/a-meta-analysis-of-the-cognitive-and-motivational-effects-of-seri/); [e-teaching.org](https://www.e-teaching.org/materialien/literatur/wouters-van-nimwegen-van-ooostendorp-van-der-spek-2013)
- A follow-up review reports that the narrative effect in Wouters was not significant, with a trend favouring games without narrative. — [Clearinghouse TUM summary](https://www.clearinghouse.edu.tum.de/wp-content/uploads/2018/12/CHU_KR-3_Wouters2_2013_Digitale-Spiele-und-Motivation.pdf)
- Clark, Tanner-Smith & Killingsworth (2016), Review of Educational Research 86(1), K-16 studies from 2000–2012. All figures below are from the same paper. — [PMC full text](https://pmc.ncbi.nlm.nih.gov/articles/PMC4748544); [SRI brief](https://sri.com/publication/digital-games-design-and-learning-a-systematic-review-and-meta-analysis-brief)

| Clark et al. (2016) comparison | Effect size |
|---|---|
| Games vs. non-game conditions (57 comparisons) | g = 0.33 |
| Augmented vs. standard game designs (20 comparisons) | g = 0.34 |
| Multiple sessions | g = 0.44 |
| Single session | g = 0.08 (n.s.) |
| Enhanced scaffolding | g = 0.48 |
| Enhanced scaffolding, value-added comparisons | g = 0.41 |
| Teacher-provided scaffolding | g = 0.58 |
| Simple points/badges mechanics | g = 0.53 |
| More elaborate mechanics | g = 0.25 (p = .07) |
| No story | g = 0.44 |
| Thin story | g = 0.47 |
| Medium-depth story | g = −0.03 |
| Thick story | g = 0.36 (n.s.) |
| Irrelevant storyline | g = 0.63 |
| Relevant storyline | g = 0.17 |
| Schematic visuals | g = 0.48 |
| Cartoon visuals | g = 0.32 |
| Realistic visuals | g = −0.01 |
| Single non-competitive play | g = 0.45 |
| Single competitive play | g = −0.06 |
| Multiplayer | g = −0.05 |
| Simplistically intrinsic integration | g = 0.49 |
| Not fully intrinsic | g = 0.33 |
| Intrinsic | g = 0.19 |

  - The gap between simple and elaborate mechanics was not significant (p = .07).
  - The storyline result weakened after controls. More contextualization overall correlated with smaller effects (slope −0.07, p = .01).
  - The visual-realism gap became marginal after controls.
  - Intrinsic integration categories did not differ significantly.
  - Supplemental non-game instruction made no significant difference (0.36 vs 0.32).

### Inferences
- Applies to our game:
  - Single-player, non-competitive play is the best-performing configuration in Clark et al., which supports the format.
  - Long play over many sessions (42 lessons) is a strength.
  - The teacher NPC can act as the "teacher-provided scaffolding" (the strongest moderator) if she reacts to what the player actually did, instead of delivering a fixed script.
- Caution against adding more story or dressing as the cure for boredom. The meta-analyses suggest the problem is the activity structure, not a lack of fantasy. The cure is varied, meaningful verbs.
- Clark et al.'s non-significant intrinsic-integration result conflicts somewhat with Habgood. The difference is that Habgood ran a controlled within-game manipulation, while Clark coded heterogeneous studies post hoc. Treat intrinsic integration as well-supported for engagement and plausible for learning.

### Gaps
- I did not read the remaining ~69k characters of Clark et al., which cover the value-added moderator details.

## 6. Commercial edutainment history

### Takeaway
1990s edutainment collapsed largely for business reasons: the Mattel/Learning Company deal, cost-cutting, and the collapse of the retail software channel. The Cooney Center argues the collapse was not caused by lack of demand or by an inability to make good products. The titles that lasted (Carmen Sandiego, Oregon Trail, Zoombinis, The Incredible Machine) are generally ones where the learning was the play: deduction, resource management, logic, physics construction.

### Cited Findings
- Mattel bought The Learning Company in 1998–99:
  - The price is reported as about $3.5–4.2B depending on source.
  - The unit lost about $300M in 1999.
  - It was later sold for a small fraction; AV Club says $27M. — [EdSurge](https://edsurge.com/news/2016-02-07-is-the-educational-games-industry-falling-into-the-same-trap-it-did-20-years-ago); [AV Club](https://www.avclub.com/shark-tanks-mr-wonderful-helped-kill-the-educational-1829339516); [Wikipedia: Mattel Interactive](https://www.wikipedia.org/wiki/Mattel_Media)
- SoftKey (Kevin O'Leary) bought The Learning Company in 1995, then made acquisitions, cut back the companies it bought and laid off staff. — [AV Club](https://www.avclub.com/shark-tanks-mr-wonderful-helped-kill-the-educational-1829339516); [LA Business Journal](https://labusinessjournal.com/news/mattel-how-the-giant-toy-maker-fell-so-far-so-fast/)
- A 2000 industry source named the collapse of the retail channel for consumer software as a major problem. — [Brown CS92 archive](https://cs.brown.edu/courses/cs092/2000/mattel.html)
- Joan Ganz Cooney Center report ("What in the World Happened to Carmen Sandiego?"): the industry blames lack of demand or the difficulty of making great products, and "neither of these assertions is correct". The report attributes the decline to market and business factors. Former staff said "edutainment" became a toxic term. — [Cooney Center](https://joanganzcooneycenter.org/?p=7052); [EdSurge summary](https://www.edsurge.com/news/report-looks-at-failure-lessons-learned-from-edutainment-era)
- Zoombinis research (EdGE at TERC, NSF $1.9M):
  - Gameplay builds implicit computational thinking: problem decomposition, pattern recognition, abstraction and algorithm design.
  - Data detectors identify that thinking in play logs, and teachers then "bridge" implicit learning to explicit learning in class.
  - A 45-class study (grades 3–8) found it effective as both a learning tool and an assessment tool. — [Baker lab HLM paper](https://learninganalytics.upenn.edu/ryanbaker/ZoombinisHLM.pdf); [Pizza Pass CHI 2018](https://learninganalytics.upenn.edu/ryanbaker/CHI2018-PizzaPass3.pdf); [TERC](https://www.terc.edu/?p=2136)

### Inferences
- Applies to our game: the Zoombinis model fits well.
  1. Players solve logic puzzles with no explicit instruction.
  2. The game silently detects the strategies they use.
  3. Explicit naming comes afterward, from a teacher.
  In our game, the teacher NPC could do step 3 after play rather than before it. Station logs can serve as the stealth assessment that replaces quizzes.
- Carmen Sandiego (deduction from clues), Oregon Trail (resource trade-offs under uncertainty) and The Incredible Machine (build and test contraptions) each centre on one strong intrinsic verb. Chemistry analogues include: identify the unknown substance (detective), manage reagents and budget for a project (Oregon Trail), and build an apparatus chain to produce X (Incredible Machine).

### Gaps
- I did not retrieve primary design or research sources on Oregon Trail, Carmen Sandiego or The Incredible Machine specifically. The characterizations above are my inference, not cited.
- The full Cooney Center PDF, with its specific lessons, was not read.
- I did not find specific failed titles with documented post-mortems beyond the Math Blaster critique.

## 7. Chemistry education games and their research

### Takeaway
Rigorous evidence on commercial chemistry games is thin. ChemCaper has only small qualitative studies, and Collisions has awards and classroom use but I found no outcome studies. PhET is the best-researched exemplar: in a study of 80 general chemistry students, they learned from a simulation with no instructions, thanks to implicit scaffolding. A 2014–2024 systematic review finds positive effects mainly on motivation and engagement.

### Cited Findings
- PhET:
  - Moore, Herzog & Perkins (2013, Chemistry Education Research and Practice): 80 general chemistry students used a PhET sim with no instructions. Students in large classes could use implicit-scaffolded sims through exploration. — [RSC CERP 2013](https://pubs.rsc.org/en/content/articlehtml/2013/rp/c3rp20157k)
  - PhET's earlier interface design principles were empirically derived from observing student use (Adams et al.). — [PhET interview paper](https://phet.colorado.edu/publications/PhET%20Interview%20Paper%20Final.pdf)
- ChemCaper (RPG, aligned with IGCSE / Year 7):
  - Mah (2019) thesis: 20 international-school students (below formal chemistry age, no structured teaching) showed high engagement. Most could turn gameplay into chemistry knowledge: apparatus, separation techniques, symbols and basic bonding. — [UM thesis](https://knova.um.edu.my/student_works_2010s/6194)
  - A Zhao & Jamaludin gamification study (54 students) found ChemCaper was the students' most preferred tool. — [Springer](https://link.springer.com/content/pdf/10.1007/s10639-023-11791-9.pdf); [Education Alliance Finland](https://educationalliancefinland.com/node/187)
- Collisions is a system of eight chemistry games grounded in the rules of chemistry, covering 50+ concepts: atoms, ions, covalent bonding, acids/bases, intermolecular forces, phase change and equilibrium. It was a 2019 Webby honoree and is presented to teachers via AACT and ChemEd X (Jen Lee talk). — [ChemEd X](https://chemedx.org/node/4721); [Common Sense Education](https://www.commonsense.org/education/app/collisions-play-chemistry); [AACT webinar](https://teachchemistry.org/professional-development/webinars/using-collisions-to-teach-chemistry-through-game-based-learning)
- A systematic review of game-based chemistry education (2014–2024, CERP 2026) found positive impact, particularly on motivation and engagement. — [RSC CERP review](https://pubs.rsc.org/en/content/articlehtml/2026/rp/d5rp00248f)

### Inferences
- Applies to our game:
  - "Grounded in the rules of chemistry" (Collisions) and PhET's affordances and constraints both point the same way: stations should run a real, if simplified, rule system the player can poke at. Hard-coded scripted outcomes leave the player with nothing to discover.
  - A small rule engine for things like solubility, pH or reactivity series lets open-ended experiments happen and makes "free steps" genuinely free.
  - PhET also shows that well-designed sims don't need preceding lectures. That supports cutting or shortening the teacher dialogue that opens every lesson.

### Gaps
- I could not confirm a link between Collisions and "Roy Moore". The developer credit is unverified (ChemEd X lists a talk by Jen Lee).
- I found no peer-reviewed learning-outcome study for Collisions, Elemental or ChemCraft. Those games were not researched in this session.
- I found no rigorous controlled pre/post study for ChemCaper.
