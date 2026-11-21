# Gamified Learning Apps & Classroom Games: Mechanics That Drive Engagement and Learning

Context for applying findings: 2D top-down cozy chemistry school life-sim (Unity, Georgian-language, single-player, offline). Current loop is linear: teacher lesson → demo → guided step → free step → quiz/reward. It also has stations, missions, collections, research projects, a weekly science fair and minigames. Each "Game application" note below is an inference, not a sourced fact.

## 1. Which mechanics measurably increased retention/daily use, and which feel manipulative?

### Takeaway
The best-documented retention wins come from Duolingo. Leagues raised learning time by 17% and tripled the number of highly engaged learners. Streak protection items (Streak Wager: +14% D7; Weekend Amulet: +2.1% D7 / +4% D14) and a growth-mindset in-app coach (+7.2% D14) also lifted retention. Gamification that ties into the learning loop works. Gamification bolted on top does nothing: Duolingo's first "moves counter" was completely neutral. The mechanics people call manipulative are the ones that tie loss or limits to monetization: Prodigy's membership upsell, Duolingo's switch from hearts to energy, and Lumosity's unsupported health claims.

### Cited Findings

**Duolingo: streaks**
- Streak Wager: learners spend in-game currency to bet they will keep their streak 7 more days and win double if they do. It gave statistically significant gains in D1, D7 and D14 retention, with "Day-7 retention showing the greatest improvement at +14%". Learners offered the wager also returned more often and completed more lessons. — [Duolingo blog](https://blog.duolingo.com/how-streaks-keep-duolingo-learners-committed-to-their-language-goals/)
- Weekend Amulet: an item equipped on Friday that protects the streak over the weekend. It was chosen because DAU "decline by as much as 5-10% on weekends". Learners offered it were "4% more likely to come back a week later" and "5% less likely to lose their streak". — [Duolingo blog](https://blog.duolingo.com/how-streaks-keep-duolingo-learners-committed-to-their-language-goals/)
- The same interview gives different Weekend Amulet figures: +2.1% D7 and +4% D14. It also says the DAU gain was worth more than the revenue lost from streak repair. The figures conflict with the blog's "4% a week later", probably because the metric definitions differ. — [First Round Review](https://review.firstround.com/the-tenets-of-a-b-testing-from-duolingos-master-growth-hacker/)
- Duolingo's research found that learners who binge lessons were much more likely to abandon the app than those who pace themselves, so it frames streaks as "a marathon rather than a sprint". Allowing rest days led to more activity in the long run. — [Duolingo blog](https://blog.duolingo.com/how-streaks-keep-duolingo-learners-committed-to-their-language-goals/)
- Over four years of retention work, the share of DAU with a streak of 7 days or more rose almost 3x, to more than half of DAU. Users who reached a 10-day streak were substantially less likely to drop off. Several changes to streak freezes "generated substantial improvements to retention" (no figures given). — [Lenny's Newsletter, Jorge Mazal (ex-Duolingo CPO)](https://www.lennysnewsletter.com/p/how-duolingo-reignited-user-growth)

**Duolingo: leagues/leaderboards**
- Leagues were the Retention Team's first breakthrough: "Overall learning time increased by 17%", and the number of highly engaged learners (≥1 hr/day, 5 days/week) "tripled". — [Lenny's Newsletter](https://www.lennysnewsletter.com/p/how-duolingo-reignited-user-growth)
- Over four years, current-user retention rate (CURR) rose 21%, which meant daily churn among the best users fell by more than 40%. DAU grew 4.5x over the same period. In simulation, CURR's effect on DAU was 5x that of the next-best metric. — [Lenny's Newsletter](https://www.lennysnewsletter.com/p/how-duolingo-reignited-user-growth)
- An earlier, purely cosmetic gamification test (a counter of moves) was "completely neutral" for retention and DAU. — [Lenny's Newsletter](https://www.lennysnewsletter.com/p/how-duolingo-reignited-user-growth)
- League design: weekly leaderboards of 30 randomly assigned users with a Sunday deadline, ten tiers from Bronze to Diamond, and at most one promotion per week. Promotion rates fall as tiers rise: Bronze 20/30, Silver 15/30, Gold 10/30, middle tiers 7/30, Obsidian 5/30. The bottom of each league is demoted, and the top 3 earn 60 gems. Matchmaking pairs users "with others at a similar pace". The site's "+25% lesson completion" headline has no source. — [Deconstructor of Fun Duolingo teardown](https://duolingo.deconstructoroffun.com/mechanics/leagues)
- Anecdotal criticism: users report others botting or repeating short lessons to climb, and say leagues stop motivating once they reach Diamond. — [search summary of user posts, e.g. MetaFilter](https://www.metafilter.com/199369/I-Am-Going-Out-In-A-Blaze-Of-Glory)

**Duolingo: badges, notifications, coach**
- Badges V1 raised DAU 2.4%, session starts 4.1%, session ends 4.5%, store purchases 13% and friends added 116%. An earlier badge awarded only for signing up had no effect and was dropped. Lesson: reward meaningful accomplishments, and offer a collectable set rather than a single badge. — [First Round Review](https://review.firstround.com/the-tenets-of-a-b-testing-from-duolingos-master-growth-hacker/)
- Notifications go out about 23.5 hours after last use. The most personal copy ("Hi, it's Duo") raised DAU 5%. — [First Round Review](https://review.firstround.com/the-tenets-of-a-b-testing-from-duolingos-master-growth-hacker/)
- In-lesson coach (Duo): growth-mindset copy, which praises effort and improvement, beat both "Whoa, you're amazing" copy and control, with +7.2% D14 retention. Some staff found the coach annoying, but the data won. — [First Round Review](https://review.firstround.com/the-tenets-of-a-b-testing-from-duolingos-master-growth-hacker/)
- Delaying sign-up so users try the product first raised DAU about 20%. — [First Round Review](https://review.firstround.com/the-tenets-of-a-b-testing-from-duolingos-master-growth-hacker/)
- Duolingo refuses to test ideas that conflict with its mission, such as putting lessons behind a paywall. — [First Round Review](https://review.firstround.com/the-tenets-of-a-b-testing-from-duolingos-master-growth-hacker/)

**Duolingo: hearts → energy (manipulation backlash)**
- In 2025 Duolingo replaced hearts for free users with "Energy". Hearts were lost only on mistakes, while energy drains on every exercise. Duolingo's rationale was that losing a heart per mistake "was not the most effective way to support learning", and it said learners were doing more under Energy. — [Duolingo blog](https://blog.duolingo.com/duolingo-energy/); [Android Authority](https://www.androidauthority.com/quitting-duolingo-energy-system-3599842/)
- The change drew backlash: change.org petitions to restore hearts (Sept 2025), complaints that a perfect lesson still costs energy, and complaints that there is no opt-out. — [change.org petition](https://www.change.org/p/restore-the-hearts-feature-in-duolingo-and-remove-energy); [PCWorld](https://www.pcworld.com/article/2924235/why-i-fell-out-of-love-with-duolingo.html)

**Duolingo: spaced repetition**
- Half-Life Regression (Settles & Meeder, ACL 2016) predicts each word's memory half-life. It cut recall-prediction error by more than 45% compared with baselines (including Leitner), and an operational user study showed a 12% increase in daily student engagement. The training data covered 13M learning traces and is public. — [ACL paper P16-1174](https://preview.aclanthology.org/landing_page/P16-1174); [Duolingo blog](https://blog.duolingo.com/how-we-learn-how-you-learn)

**Prodigy Math: monetization criticism**
- Feb 2021: Campaign for a Commercial-Free Childhood (now Fairplay) and 21 other groups filed an FTC complaint. They counted 16 membership ads and only 4 math problems in one 19-minute session, said the home version shows up to 4x more ads than math questions, and said the game shows players who has paid membership and who does not. Membership was estimated at $59.88–$107.40 per child per year. — [EdWeek](https://www.edweek.org/technology/popular-interactive-math-game-prodigy-is-target-of-complaint-to-federal-trade-commission/2021/02); [Fairplay complaint PDF](https://fairplayforkids.org/wp-content/uploads/2021/02/Prodigy_Complaint_Feb21.pdf); [Axios](https://www.axios.com/2021/02/19/prodigy-math-game-ftc-complaint)
- Prodigy's response: 95% of players use the free version, and it cited a Johns Hopkins report that Prodigy "appears to positively relate to student achievement" in two schools. I found no later report of FTC action. — [EdWeek](https://www.edweek.org/technology/popular-interactive-math-game-prodigy-is-target-of-complaint-to-federal-trade-commission/2021/02)

**Lumosity: what not to claim**
- FTC, Jan 2016: Lumos Labs paid $2M, with a $50M judgment suspended. The FTC alleged unsupported claims that the training improved everyday, school and work performance and delayed age-related decline, dementia and Alzheimer's. It also alleged Lumos failed to disclose that some testimonials were solicited through contests. Lumos had to offer subscribers an easy way to cancel auto-renewal. — [FTC press release](https://ftc.gov/news-events/press-releases/2016/01/lumosity-pay-2-million-settle-ftc-deceptive-advertising-charges); [CNN Money](https://money.cnn.com/2016/01/05/technology/lumosity-brain-train-app-ftc-settlement)

### Inferences (game application)
- **Streaks, adapted for a cozy offline game.** Use a "lab days attended" streak counted in in-game days, not real-world days, plus an optional real-day "visit streak" that never punishes. Copy the Weekend Amulet idea with an automatic rest-day token: the school closes on a weekend day in the in-game calendar and the streak freezes. A Streak-Wager equivalent fits too, such as betting lab tokens that you will finish this week's science fair project with three experiments for a double payout. Avoid real-time guilt notifications because the game is offline and cozy.
- **Leagues, made single-player.** Build an NPC classmate league: about 10–15 named classmates with weekly "science points" totals scaled to the player's own pace (Duolingo matches users "at a similar pace"). Tie the weekly science fair to the league reset. Promote the player to a higher club tier with gentle, fixed promotion rules, and skip demotion to protect the cozy tone.
- **Badges.** Make them meaningful and collectable as sets: a "First distillation" badge plus element-group sets. Do not award trivial badges for things like opening the game, since Duolingo's sign-up badge did nothing.
- **Avoid.** Do not add an energy meter that drains even on correct answers, or any limit tied to purchases. The cozy genre and a school audience make Prodigy-style status gaps (paid vs free) especially toxic. Mistakes should cost nothing, or at most a little time with funny feedback.
- **Claims.** In store pages and marketing, do not promise better grades or exam results unless a study backs it. Say what the game covers, such as "covers the grade 8 curriculum topics X, Y, Z".

### Gaps
- I found no Duolingo primary source quantifying the hearts system's effect on retention or learning, and no independent retention data for Energy.
- I found no peer-reviewed independent study of Prodigy learning gains beyond the Johns Hopkins report cited in the press.
- The figures for Duolingo's later "one lesson keeps your streak" change (+3.3% D14 in a secondary source) were not verified against a primary source.

## 2. How do these apps make each session short and satisfying?

### Takeaway
Sessions are short by design: one lesson extends a streak, there is a daily XP goal, and a Kahoot round is a fixed set of timed questions. Each session ends with a clear payoff such as XP, badge progress or a podium. Pacing beats bingeing: Duolingo found bingers churn more and introduced rest-day mechanics. Khan Academy's evidence points to a modest weekly dose (30 min/week) as the useful target.

### Cited Findings
- Duolingo found binge learners much more likely to abandon the app than paced learners, and designed streak items to encourage steady short sessions. — [Duolingo blog](https://blog.duolingo.com/how-streaks-keep-duolingo-learners-committed-to-their-language-goals/)
- Badges increased both session starts (+4.1%) and session ends (+4.5%). The ends metric matters because a session that reaches its end has a satisfying stopping point. — [First Round Review](https://review.firstround.com/the-tenets-of-a-b-testing-from-duolingos-master-growth-hacker/)
- Kahoot: Wang & Tahir's 2020 review of 93 studies found positive effects on learning performance, classroom dynamics, attitudes and anxiety, though some studies found little or no effect. Students named stressful time pressure, not enough time to answer, and fear of losing as the main problems. Teachers said speed-based scoring can reduce reflection and push students to guess. — [Wang & Tahir 2020, Computers & Education](https://research.gold.ac.uk/id/eprint/39435)
- In one dental-education Kahoot study, 75% of students said in-class competition motivated them, while 24% found the live leaderboard stressful. — [CDHA journal PDF](https://files.cdha.ca/profession/journal/2841.pdf)
- A 2024 Frontiers study says Kahoot's music and bright colours energise the classroom. This is a descriptive claim, not a measured effect. — [Frontiers in Education 2024](https://frontiersin.org/journals/education/articles/10.3389/feduc.2024.1430729/pdf)
- Khan Academy: 30+ minutes per week (18+ hours a year) is associated with about 20% greater-than-expected gains on MAP Growth (effect size 0.36; observational). In Centennial SD, students using it 30+ min/week showed 33% higher growth than those using it under 15 min/week. — [Khan Academy blog, Nov 2024](https://blog.khanacademy.org/?p=18194); [Khan Academy district report](https://blog.khanacademy.org/school-district-reports-test-scores-rise-with)
- Khan Academy now measures progress as "skills to proficient", meaning mastery of skills rather than minutes spent. Actual use often falls far below the recommended dose: one Stanford researcher noted an average of about 6 hours per *year*. — [Khan Academy blog](https://blog.khanacademy.org/?p=13201); [Stanford News 2026](https://news.stanford.edu/stories/2026/01/online-mastery-math-practice-software-educational-impact-research)
- Duolingo's daily-engagement gain from spaced repetition (+12%) suggests that putting the right review item in a short session is itself an engagement lever. — [ACL P16-1174](https://preview.aclanthology.org/landing_page/P16-1174)

### Inferences (game application)
- Break the current linear loop (lesson → demo → guided → free → quiz) into 5–10 minute "periods" that each end on a payoff beat: a stamp in the lab notebook, a new collection entry, or a classmate reaction. Let the player stop after any period with an autosave. School bells already exist in the audio work, so a bell can mark the end of each period.
- Add a daily "lab goal" the player picks (casual, regular or serious, like Duolingo's self-set goals). It fills a ring and finishes with a short celebration.
- **Kahoot-style quiz.** Make the timer optional or generous, with an untimed "calm mode" toggle, since the literature flags time-pressure anxiety and guessing. Use upbeat music during the quiz and a podium of NPC classmates at the end. Do not award points for speed alone. Weight correctness first and give speed a small bonus.
- **Spaced review offline.** Bring back earlier reactions and elements as "warm-up" questions at the start of each lesson or in minigames. A simple Leitner box per concept (correct → interval x2, wrong → reset) is enough. Half-life regression is overkill here.
- **Khan-style mastery.** Track each concept (e.g. "balancing equations", "acid/base indicator") at four levels (Attempted → Familiar → Proficient → Mastered) on a visible skill map. Lab stations and research projects unlock when concepts are proficient, not when lessons are merely finished.

### Gaps
- I found no Duolingo primary data on optimal lesson length (minutes) or on the daily XP goal's effect.
- I found no Kahoot study that isolates music, the timer and the leaderboard as separate variables, so the effect of each is unknown.
- I did not research Quizlet in depth: there were no verified numbers for its Learn mode, Match game timer or spaced-repetition features.

## 3. How does "learn by doing first, explanation after" work (Brilliant, DragonBox)?

### Takeaway
Brilliant poses a problem or interactive manipulation before it gives the explanation, and DragonBox hides algebra rules inside a puzzle-card game, revealing the symbols step by step. Meta-analytic evidence for problem-solving-before-instruction ("productive failure") is moderate (g ≈ 0.36, up to 0.87 after bias correction). It does not hold for young children (grades 2–5). Evidence that DragonBox transfers to real algebra tests is mixed: in-game mastery is high, but test-score gains are weak. Intrinsic integration, where the learning content is the core mechanic, has some experimental support (Habgood) and a failed replication.

### Cited Findings
- Brilliant often asks a question before presenting a procedure. The first attempt gives the explanation a purpose and shows what the learner already understands. Feedback after a wrong answer arrives "while the reasoning is still fresh" so learners can revise. — [Brilliant: How Brilliant teaches math](https://brilliant.org/resources/choosing-brilliant/how-brilliant-teaches-math/)
- Brilliant describes itself as having no videos and being fully interactive. It contrasts itself with Khan Academy, where students watch an explanation and then practise. — [Brilliant: Why Brilliant](https://brilliant.org/help/why-brilliant/); [brilliant.org/llms.txt](https://brilliant.org/llms.txt)
- Sinha & Kapur 2021 (Review of Educational Research) analysed 53 studies and 166 comparisons. Problem solving followed by instruction beat instruction followed by problem solving with g = 0.36 [0.20, 0.51]. High-fidelity productive-failure designs reached g 0.37–0.58, and the bias-corrected estimate was g 0.87. Younger learners (grades 2–5) and domain-general skills showed the reverse trend, favouring instruction first. — [ETH research collection / RER](https://www.research-collection.ethz.ch/entities/publication/087241d3-ca0e-469e-a7f3-df9c122e2585); [Frontiers 2022](https://frontiersin.org/articles/10.3389/feduc.2022.956416/pdf)
- Adding scaffolding to problem-solving-first was not significantly better than plain PS-I (g −0.08). — [ETH research collection](https://research-collection.ethz.ch/handle/20.500.11850/492056)
- DragonBox Washington State Algebra Challenge (June 2013): 4,192 K-12 students solved more than 390,000 equations in 5 days. 92.9% of students who played 1.5 hours reached in-game mastery, and those who reached it did so in an average of 41 min 44 s. — [DragonBox Algebra Challenge](https://dragonbox.com/about/algebra-challenge); [UW CSE News](https://news.cs.washington.edu/?p=10336); [THE Journal](https://thejournal.com/articles/2014/04/24/adaptive-game-based-platform-helps-students-master-concepts-in-algebra-challenge.aspx)
- Caveat: in a study of 267 middle-schoolers, DragonBox players enjoyed it more but did not gain more than students using an intelligent tutoring system. A CMU-linked review called DragonBox ineffective on typical equation-solving tests. That claim is secondary and should be verified. — [eScholarship PDF](https://escholarship.org/content/qt8453p0gf/qt8453p0gf.pdf?t=sgiu9f); [Montclair design-based study](https://researchwith.montclair.edu/en/publications/a-design-based-approach-to-playful-algebra-learning-with-dragonbo/)
- Habgood's PhD (Nottingham, 2007) defines "intrinsic integration": the learning content is delivered through the game's core mechanic. In Zombie Division, players fight with division: skeletons carry numbers and players choose a weapon by divisor. Across 58 children playing 2 hours, the intrinsic version produced more learning than the extrinsic version (quiz between levels) and the control. A separate time-on-task study measured motivation (the "7x more engaging" claim on the mathgamedesign list refers to free-choice play time). — [Habgood 2007 thesis](https://eprints.nottingham.ac.uk/10385/1/Habgood_2007_Final.pdf); [mathgamedesign group](https://groups.google.com/g/mathgamedesign/c/uync5tVLdK0)
- A replication found similar motivation for the intrinsic and extrinsic versions. Its authors suggested the original's lower challenge and different instructional sequence may have biased the result. — [core.ac.uk replication](https://core.ac.uk/download/187128942.pdf)
- Minecraft Education Chemistry Update (Feb 2018) is a resource pack with 10 lesson plans and a lab journal. The Element Constructor builds any of the 118 elements and 400+ stable isotopes by setting proton, neutron and electron counts. The Compound Creator combines elements into 30+ compounds. The Lab Table runs reactions to make products such as bleach, glow sticks and balloons. The Material Reducer breaks a block into its elements by percentage. Some players complain the compound list is too short. — [Minecraft Education blog](https://education.minecraft.net/blog/introducing-chemistry-update-for-minecraft-education-edition); [THE Journal](https://thejournal.com/articles/2018/02/21/minecraft-chemistry-update-goes-live.aspx); [Microsoft UK Stories](https://ukstories.microsoft.com/features/new-minecraft-tools-and-items-will-teach-kids-about-chemistry/); [Minecraft wiki](https://minecraft.wiki/w/Chemistry_Update); [Minecraft feedback forum](https://feedback.minecraft.net/hc/en-us/community/posts/10253494006797-Let-s-talk-about-Education-Edition)

### Inferences (game application)
- **Reorder part of the loop.** The current loop runs teacher lesson → demo → guided → free. For curious, older players (13+), try a "mystery bench" opener: the player gets two unlabelled flasks and an indicator, mixes them freely and sees the colour change. Then the teacher explains what happened, using the player's own result ("You saw it turn pink, here's why"). Keep lesson-first as the default or for younger players, since the meta-analysis shows a reversal for grades 2–5. Tag levels by target age.
- **Intrinsic integration.** The core verbs (pour, heat, mix, measure) should be the chemistry itself. Avoid "answer a quiz to unlock the door". Example: the right ratio of reagents in the mixing minigame produces the right colour or precipitate, so correctness is visible in the world rather than shown as a ✓.
- **DragonBox-style gradual symbol reveal.** Start reactions as picture cards (coloured atom tokens snapping together). Then swap in Georgian element names, then chemical symbols, then full balanced equations as the player progresses. Balancing equations is close to DragonBox's "keep both sides equal" rule, so make it a puzzle where atoms on both sides of a scale must match.
- **Minecraft-style stations as collections.** An "atom builder" station (set p/n/e → element appears in the collection), a "compound bench" (combine elements → compound card) and a "breakdown machine" (decompose household objects → element percentages) map directly onto the existing stations and collections. Each successful creation adds an entry to a periodic-table collection book, which gives players an endless completion goal.
- **Validate in-game mastery against a quiz outside the game.** DragonBox shows that in-game mastery does not guarantee test transfer. Occasionally ask "notebook" questions in plain symbolic form (no game wrapping) to check transfer.

### Gaps
- I found no Brilliant engineering or product blog post with A/B data on whether putting interactive steps first raises completion. All the evidence is design philosophy or secondary reviews.
- There is no peer-reviewed study of Minecraft Education Chemistry learning outcomes, and no current list of its features after 2018.
- The original CMU DragonBox paper was not fetched, so its result comes from a secondary source.

## 4. What competitive/social loops work for a single-player game (ghost rivals, NPC leagues, personal bests)?

### Takeaway
Competition works best when the field is matched to the player's level and resets often. Duolingo uses 30-person weekly cohorts "at a similar pace", promotion rates that tighten in higher tiers, and weekly resets. Kahoot shows that a live leaderboard motivates most players (75%) but stresses a sizeable minority (24%), and that speed scoring causes guessing. For offline single-player play, those same structures can be simulated with NPC rivals and personal-best ghosts.

### Cited Findings
- Leagues use 30 randomly assigned users per weekly cohort, matched by pace. Promotion is generous at the bottom (67% in Bronze) and tight at the top (17% in Obsidian). Players move at most one tier per week, and the top 3 earn gems. — [Deconstructor of Fun](https://duolingo.deconstructoroffun.com/mechanics/leagues)
- Leagues drove learning time up 17% and tripled the number of highly engaged learners. — [Lenny's Newsletter](https://www.lennysnewsletter.com/p/how-duolingo-reignited-user-growth)
- Badges raised friends added by 116%, so social connection grew out of achievement display. — [First Round Review](https://review.firstround.com/the-tenets-of-a-b-testing-from-duolingos-master-growth-hacker/)
- Kahoot leaderboards: 75% of students found competition motivating and 24% found the live leaderboard stressful. Fear of losing and the difficulty of catching up after a wrong answer are frequent complaints. — [CDHA journal](https://files.cdha.ca/profession/journal/2841.pdf); [Wang & Tahir 2020](https://research.gold.ac.uk/id/eprint/39435)
- Some users game leagues by repeating easy lessons. Leaderboards on raw XP reward grinding rather than learning. — [MetaFilter thread (anecdotal)](https://www.metafilter.com/199369/I-Am-Going-Out-In-A-Blaze-Of-Glory)
- Prodigy's display of who has paid membership was singled out by advocates as harmful social comparison for children. — [EdWeek](https://www.edweek.org/technology/popular-interactive-math-game-prodigy-is-target-of-complaint-to-federal-trade-commission/2021/02)

### Inferences (game application)
- **NPC classmate league.** Each in-game week, 8–12 named classmates (each with a personality and specialty, such as "Nino, the organic chemistry nerd") gain science points along curves seeded from the player's last two weeks, so they are always slightly ahead or behind. The weekly science fair ends the week with a ceremony, a podium and tier promotion (Bronze → Silver → Gold Lab Club). No demotion, or a soft "stay" only.
- **Score learning, not grinding.** Award league points for new mastery and first-time discoveries, and give repeated easy tasks diminishing returns. This avoids the XP-farming exploit.
- **Personal-best ghosts.** For timed minigames (titration, mixing, sorting), show a translucent "yesterday-you" ghost or time marker, plus "beat your best" medals. This gives competition with nothing social at stake.
- **Rival NPC.** A single friendly rival who challenges the player before each science fair, with dialogue that reacts to the player's results, adds narrative stakes. Make the rival's rubber-banding gentle.
- **Quiz podium.** Show the player against 3–4 NPCs, Kahoot style. Weight correctness heavily and let the player see they could catch up, since "difficulty catching up" was a Kahoot complaint. Consider a final double-points question.

### Gaps
- I found no published data on NPC or bot leaderboards in learning apps. Duolingo has never confirmed bots in leagues (the teardown says nothing about them).
- Typing of the Dead, which turns typing skill into a zombie-shooter mechanic and is a strong intrinsic-integration example, was not researched in this pass. I have no citable numbers on it.

## 5. How do characters/mascots add personality?

### Takeaway
Duolingo's owl is the clearest case with measured impact. Messages from "Duo" raised DAU 5%, and an in-lesson Duo giving growth-mindset encouragement raised D14 retention 7.2%. Copy that praises effort beats copy that praises talent. The tone matters as much as the character's presence.

### Cited Findings
- The notification copy "Hi, it's Duo", the most personal variant, raised DAU 5%. — [First Round Review](https://review.firstround.com/the-tenets-of-a-b-testing-from-duolingos-master-growth-hacker/)
- The in-lesson coach (Duo cheering): growth-mindset messages ("praise effort and improvement rather than innate ability") beat "Whoa, you're amazing" messages and control, with D14 retention +7.2%. Some team members found it annoying, but the data supported keeping it. — [First Round Review](https://review.firstround.com/the-tenets-of-a-b-testing-from-duolingos-master-growth-hacker/)
- Duolingo's guilt-tripping owl persona is widely discussed as a double-edged "price of guilt". This is commentary, not data. — [Shahzeb psychology audit (opinion)](https://www.shahzebspeaks.com/brand-psychology-audits/duolingo-psychology-audit-the-owl-the-streak-and-the-price-of-guilt/)
- Prodigy builds its appeal on an RPG wizard avatar, collectable pets and battles in which a correct math answer powers the attack. The same structure is where its membership upsell sits (premium pets and gear). — [Fairplay complaint PDF](https://fairplayforkids.org/wp-content/uploads/2021/02/Prodigy_Complaint_Feb21.pdf); [Wikipedia: Prodigy Education](https://en.wikipedia.org/wiki/Prodigy_Education)

### Inferences (game application)
- The teacher NPC, who already walks, swirls and pours at her desk, is the natural "Duo". Give her short, varied, effort-praising barks ("You checked the colour twice, that's how real chemists work") instead of generic "Great!". Rotate the lines so they don't repeat, and localise them into natural Georgian.
- Add a lab pet or mascot (for example a small creature that lives in a flask) as a Prodigy-style collectable companion. It reacts to experiments and unlocks cosmetic variants through mastery. Nothing is ever sold.
- Since the game is offline with no push notifications, put "Duo-style" nudges in game. When the player loads a save: "The greenhouse needs your pH test today!" Keep guilt out of it: "I saved your seat" rather than "You abandoned me".
- Give classmates distinct personalities and react to the player's results in the league and at the science fair. This combines social comparison with character warmth.

### Gaps
- I found no quantitative data on mascots in Prodigy, Kahoot or Brilliant, and no study isolating the owl's personality from the content of its messages.
- I found no verified current user numbers for Prodigy, Kahoot, Quizlet or Brilliant in this pass. Do not cite such numbers in the report without a separate lookup.
