# Progression design: from first coat to school legend

A plan for how the player grows across minutes, sessions and weeks, built from interactive
missions. Mission names here are English working titles; in-game text will be Georgian.

---

## 1. Design rules

1. **Every mission is something you do with your hands.** Walk, pick up, carry, place, pour,
   mix, heat, test, fix, deliver. Reading is never the mission.
2. **Show, then name.** The player sees the result first (colour, foam, glow, a plant
   recovering); the chemistry word appears afterwards in one short line.
3. **One objective at a time.** The HUD shows one line; the target object glows (the existing
   `Focus` system); the waypoint arrow points to it when it's off-screen.
4. **Failure is funny, never punishing.** Wrong mixes foam, fizz, puff purple smoke or splash.
   Clean up with [E], try again. There's no lost progress and no lost coins.
5. **Rewards are things you can see.** A new machine on your bench, a trophy on the shelf, a
   plant that blooms, a classmate who now waves at you. Coins and reputation come too, but
   they're never the only reward.
6. **Text is allowed, but small and optional.** Letters, notes, the notebook and the periodic
   chart are for players who want more. Nothing *requires* reading more than one line.

---

## 2. The loop at three scales

| Scale | What the player does | Example |
|---|---|---|
| **Moment** (10–60 s) | notice → walk → interact → see result | A flask on the bench bubbles red; you add a drop of base and it turns green. |
| **Mission** (5–15 min) | a small problem solved through 3–6 actions | The greenhouse plant is wilting: fetch water samples, test pH, fix the water, watch it recover. |
| **Act** (several days) | a chapter of missions ending in a big hands-on showpiece | Build and demonstrate a working battery at the science fair. |

Session lengths:
- **5 minutes:** one daily errand (water plants, collect materials, help a classmate with one
  thing) or repeat a favourite station activity.
- **15 minutes:** one full mission.
- **30 minutes:** two missions, or one stage of a multi-day research project.
- **Days:** an act ends, a new room or station opens, a project you started yesterday has
  finished overnight (crystals grew, the plant flowered).
- **Weeks:** lab expansions, complete collections, classmate friendships, the advanced floor,
  science fair trophies.

---

## 3. Anatomy of a mission

Every mission uses the same nine beats, so the player always knows the rhythm:

1. **Hook:** something visible changes in the world (smoke from a vent, a classmate waving, a
   dying plant, a flashing machine).
2. **Ask:** one short line from the person involved ("My battery won't light up!").
3. **Go:** the target glows; the arrow points the way.
4. **Gather:** pick up 1–3 items in the world (samples, jars, wires). They appear in your hands
   or on a small carry icon.
5. **Do:** the hands-on activity at a station (build, mix, test, balance).
6. **See:** an animated result in the world, not just in a panel.
7. **Learn:** one line naming what happened ("That was a *neutralisation*: acid + base → salt + water").
8. **Return:** bring the result back (place it, hand it over, plug it in).
9. **Reward:** a visible change, plus coins, reputation and sometimes a lab item.

### Mission verbs (the building blocks)

All missions are made from these actions; each already exists in some form or is a small addition:

| Verb | What the player does | Built on |
|---|---|---|
| **Fetch** | Find an object in a room and pick it up | new: world pickup + carry icon |
| **Deliver / Place** | Put it on a bench, shelf, plant or machine | new: drop zone with `Focus` glow |
| **Build** | Snap particles or atoms together | lesson practice (Atom, Molecule) |
| **Sort** | Drag things into groups | lesson practice (Sort) |
| **Mix / Pour** | Combine liquids, watch colour and foam | ExperimentGames, `SafeMishaps` |
| **Test** | Use a tool (indicator, scale, thermometer, flame) and read the result | lab activities (pH, analysis, thermo) |
| **Balance** | Place molecule groups on both sides until the scale levels | lesson practice (Equation, Scale) |
| **Fix** | Find and correct the wrong thing in a setup | new: "spot the problem" scene |
| **Wait** | Leave something overnight and come back | research `Wait` stage |
| **Show** | Present a finished result to someone | research `Submit`, science fair |

---

## 4. The six acts

Each act matches a reputation rank (already in the game) and one or two curriculum chapters.
An act has **story missions** (main path, always one active), **classmate quests** (optional,
character-driven) and **errands** (small, repeatable). It ends with a **showpiece mission**
that uses everything from the act.

### Act 1: "First day in the lab" · Beginner lab assistant · chapter 1 (matter)

*Unlocks: classroom, personal lab, coat, first bench.*

| Mission | What you actually do |
|---|---|
| **Safety first** | Take the coat from the glowing closet (yellow arrows). Then walk the lab and **spot 3 hazards**: an open flame next to paper, a bottle with no label, a spill on the floor. Press [E] on each to fix it. |
| **What is matter?** | The teacher puts objects on her desk: an ice cube, a balloon, a lamp, a stone. **Sort** them into "matter / not matter" by dragging. A wrong guess plays a silly sound and a small hint. |
| **Three states** | At the bench, **heat** the ice: watch it melt, then boil, steam rising. Then **cool** the steam on a cold plate and watch drops form. |
| **Showpiece: the steam machine** | Fill a small kettle, heat it, and the steam spins a little paper wheel. The wheel goes on your lab shelf as a trophy. |

*Errands:* tidy a spill, put jars back on the shelf, water the classroom plant.

### Act 2: "Tiny building blocks" · Curious researcher · chapters 2–3 (atoms, elements)

*Unlocks: atom table, element station, storage room, Mariam's quests.*

| Mission | What you actually do |
|---|---|
| **Build the first atom** | Protons, neutrons and electrons sit in trays. **Drag** them onto the nucleus and the orbit. The atom glows and its symbol appears when it's complete. |
| **Mystery element boxes** | Three closed boxes in the storage room. Open one, **inspect** the sample (shiny? soft? glows in the flame?), then **match** it to its spot on the periodic wall chart. The chart lights up that tile for good. |
| **Flame colours** | At the burner, hold a wire with each sample in the flame: red, yellow, green. Match each colour to its element. |
| **Showpiece: the element gallery** | Fill an empty display case with 6 discovered elements. Classmates stop by to look; Mariam gives you a rare mineral. |

*Classmate quest (Mariam, the mineral collector):* she lost three minerals around the school.
Find them (hallway corner, greenhouse pot, reading nook), then **test hardness** by scratching
them against glass and metal to tell which is which.

### Act 3: "Things that stick together" · Trusted chemist · chapters 4–5 (molecules, bonds)

*Unlocks: molecule table, reading corner, Ana's quests, first lab expansion.*

| Mission | What you actually do |
|---|---|
| **Make water** | Grab 2 hydrogen and 1 oxygen ball and **connect** them with bond sticks. The molecule wobbles and a water drop forms. |
| **Molecule kitchen** | Ana asks for CO₂, O₂ and CH₄. **Build** each; she puts it in a jar labelled with your name. |
| **Salt crystals** | Dissolve salt in warm water, leave it overnight (**Wait**), and come back to cubic crystals on the string. |
| **Showpiece: the crystal lamp** | Grow a large crystal over 2 days, then **place** it on a lamp base in your lab. It glows at night; the evening atmosphere picks it up. |

*Classmate quest (Ana, colours and pigments):* make 3 paint colours from safe ingredients
(beet juice, turmeric, red cabbage), then **test** each with a drop of lemon and a drop of soap
to see which ones change colour. The colour-changers go up on her art board.

### Act 4: "Things that change" · Lab star · chapter 6 (reactions)

*Unlocks: reaction table, greenhouse, Nika's quests, balance machine.*

| Mission | What you actually do |
|---|---|
| **Volcano with a reason** | Mix baking soda and vinegar in different amounts. Too little: a tiny fizz. Right amount: a big foamy eruption. The result line names it: gas forms. |
| **The scale never lies** | Do a reaction inside a sealed bag on a scale. The mass stays the same. Then open the bag and the gas escapes, so the scale drops. |
| **Balance the machine** | The balance machine has two pans. **Place** molecule groups (2H₂ + O₂ → 2H₂O) until it levels and lights green. Only then does the written equation appear. |
| **Showpiece: Nika's battery** | Nika's battery won't work. Fetch lemons, copper and zinc strips from storage; **build** the cell chain on the bench; connect the wires. The LED lights up, and Nika now builds machines for your lab. |

*Classmate quest (Giorgi, nervous about experiments):* he's afraid of the burner. Do three
safe experiments **with him watching**: he moves next to your bench and reacts. On the third
he tries one himself, and after that you see him at the benches doing his own.

### Act 5: "Solutions and the greenhouse" · Young scientist · chapters 7–8 (solutions, acids and bases)

*Unlocks: solution station, acid-base table, exhibition hall, Professor Levani's visits.*

| Mission | What you actually do |
|---|---|
| **The dying plant** | A greenhouse plant is wilting. **Fetch** a sample of its water, **test** it with indicator paper (red: too acidic), **add** a weak base drop by drop until the strip turns green, then water the plant. It perks up over the next day. |
| **Too strong!** | Ketevan's cleaning solution is too concentrated: it's dark and smoking. **Dilute** it step by step and watch the colour fade to the safe mark. Overdo it and it's too weak, so add a little back. |
| **Red cabbage rainbow** | Make indicator from red cabbage, then fill 6 tubes from acid to base. A rainbow forms on the rack and stays on your shelf. |
| **The unknown sample** | Professor Levani brings an unlabelled bottle. Run 4 tests (colour, pH, flame, does it dissolve?) and pick what it is. Each test adds a clue card to the bench; you don't need to remember anything. |
| **Showpiece: the colour machine** | The science fair machine produces the wrong colour. Trace the pipes, find the wrong bottle (an acid where a base should be), swap it, and the machine runs a colour show. |

*Classmate quest (Luka, competitive):* he challenges you to three timed lab races
(fastest neutralisation, most accurate dilution, first balanced machine). Winning earns his
respect and a rematch every week.

### Act 6: "School legend" · School legend · advanced floor

*Unlocks: challenge desk tiers 1–4, weekly rankings, final lab expansion.*

| Mission | What you actually do |
|---|---|
| **Master's trial** | The glowing gold challenge desk: random problems, equation balancing and stoichiometry. This is optional mastery content for players who want harder puzzles. |
| **Teach Giorgi** | Run one of your own experiments for Giorgi. You set up the bench and he follows; if you set it up wrong, he gets foam in his face (harmless). |
| **Showpiece: the grand fair** | Bring your best project to the exhibition hall: set up your stand by **placing** 3 items, then demonstrate it live. Judges walk past and react. First place gives the golden fair coat. |

---

## 5. Research projects (multi-day, already in the game)

The 7 existing projects become the long-form missions, each tied to an act and reworked so
every stage is physical:

| Project | Act | Stages as actions |
|---|---|---|
| Growing crystals | 3 | fetch salt → dissolve → hang the string → **wait overnight** → measure the crystal |
| Colour-changing solution | 3 | fetch cabbage → boil → filter → test 3 liquids |
| Unknown sample | 5 | 4 physical tests → place clue cards → pick the answer |
| Glowing indicator | 5 | mix → test under the lamp → adjust → show the teacher |
| Water quality | 5 | collect rainwater (rainy days only), tap and greenhouse water → test each → chart on the board |
| Simple battery | 4 | fetch fruit and metals → assemble → measure → light the LED |
| Safe fragrance | 6 | gather petals → extract → mix → a classmate sniff test (they react) |

The `Observe` stage, today a multiple-choice question, becomes a **look-and-choose** moment:
the result sits in front of you and you click the tube or crystal that matches, never a written answer.

---

## 6. Classmates as quest-givers

Each classmate has a **3-step friendship line**. Every step is a mission, and every step
changes something you can see.

| Classmate | Step 1 | Step 2 | Step 3 (friend) |
|---|---|---|---|
| Nika, inventor | Find his lost wires | Fix his battery (Act 4 showpiece) | He builds you a free machine for your lab |
| Mariam, collector | Find 3 minerals | Hardness test | Her mineral case appears in your lab |
| Giorgi, nervous | Experiments with him watching | Teach him one | He becomes your lab partner and helps with long projects |
| Ana, colours | Make 3 paints | Indicator paints | She paints a mural on your lab wall |
| Luka, competitive | Race 1 | Races 2–3 | Weekly rematch board in the hallway |
| Ketevan, lab technician | Restock the storage room | Fix the strong cleaner | You get a free daily supply drop |
| Prof. Levani, visitor | Unknown sample | Rare element hunt | An expedition souvenir and a rare cosmetic |

Classmates show what they need in the world: a "!" bubble, holding the broken object,
standing next to the problem. Talking takes 1–2 short lines, then the job starts.

---

## 7. Daily and weekly rhythm

- **Daily errands (pick any, all optional):** water the greenhouse, collect the free storage
  supply, help one classmate with a small thing, tidy one spill, read one fact in the reading
  corner (the only reading activity, and it's optional).
- **Overnight results:** projects with a `Wait` stage finish while you're away; the welcome-back
  message points to the glowing result.
- **Weekly:** science fair judging (Sunday), a competition day, Professor Levani's visit
  (Thursday–Friday), and an inspection day where the teacher checks that your lab is safe (a
  small spot-the-hazard pass).
- **Weather and time:** rainy days fill the rain barrel (water project), evenings make crystal
  lamps and glowing indicators look their best, and mornings bring birds to the greenhouse.

---

## 8. Rewards and growth

| Reward | Where you see it |
|---|---|
| New station or machine | Appears on a bench and glows the first time |
| Trophy or showpiece | Its own spot on your lab shelf |
| Classmate friendship | They wave, visit your lab and help with projects |
| Lab expansion | The room grows; new floor space to fill |
| Cosmetics | Coats, goggles, hats, worn and seen by classmates |
| Collections | Display cases fill up visibly (minerals, elements, fair ribbons) |
| Coins | For the shop shelf and expansions |
| Reputation rank | Title under your name; opens new rooms and acts |

A simple growth path to show progress at a glance: **one shelf of trophies, one per act**.
Looking at your lab tells you how far you've come.

---

## 9. What stays as text (all optional)

- The **notebook** (lectern in your lab): one page per discovery, written after you did it.
- The **periodic wall chart**: fills in as you discover elements.
- The **noticeboard**: today's events.
- The **collection cabinet**: names and fun facts.
- Optional "what's happening here?" buttons inside activities.

---

## 10. Building it (for later)

Most of this reuses what exists. New pieces, in order of value:

1. **Mission runner:** a list of missions with steps (Fetch, Place, Talk, Activity, Wait, Show),
   driving the single HUD objective and the `Focus` objective glow. Research projects already
   use a similar stage system and could become missions directly.
2. **World pickups and drop zones:** carry an item (small icon over the player), place it on a
   glowing target.
3. **Spot-the-problem scenes:** 3–5 wrong things in a room, each fixed with [E] (used for
   safety, inspections and the colour machine).
4. **World-side results:** activity results that appear on the real bench (a crystal, a lit LED,
   a coloured tube rack), not only inside the activity panel.
5. **Classmate friendship lines:** 3 missions each, tied to the existing NPC system.

---

## Open questions

- Should acts be **strictly in order**, or can players do classmate quests from later acts early?
- Is **one active story mission** enough, or do you also want a visible list of side quests?
- Should the advanced challenge floor stay **quiz-style**, or should it also become hands-on
  (timed lab races, like Luka's)?
