# Chemistry challenges and research stations

The existing pixel notebook palette, Georgian typeface, player controller, tutorial, and save format remain the base of this expansion.

## Earn chemcoins

- The teacher/main and classroom quiz tables open a choice between **16 general chemistry questions** and **26 equation challenges**. Equation choices reuse the existing reaction assets.
- Every round has four shuffled choices, free hints, and an explanation. Incorrect equations report the actual mismatched element and the atom counts on each side, including formulas with parentheses.
- A correct answer uses the existing coin reward, streak, daily quest, collection, combo, and weekly score systems. A hint or retry lowers the base reward to 60%, rounded to at least one coin. Incorrect answers cost no coins and reset the combo. Each displayed round pays once; the next round is explicit, so the player can read the explanation.
- Research milestones award **20 coins each, once per UTC day**: membrane diffusion, active pumping, a correct chromatography prediction, and gas compression. Total research rewards: 80 coins/day. Normal exploration and resets are free. Claims persist across closing windows, restarting, and save reloads.

## New equipment

Buy and place equipment through **[L] My Lab** after entering your personal lab. Approach a placed table and use **E**.

| Equipment | Price | Activity |
| --- | ---: | --- |
| Membrane transport | 110 | Moving particles, a gated channel, diffusion, a six-ATP pump, pause, speed and reset controls. Goals: at least 11 particles on the right and six uphill pump operations. |
| Color detective | 120 | Three model inks, two model solvents, a fastest-pigment prediction, and an animated eight-second paper separation. Compare bands and Rf. |
| Gas piston | 140 | Moving fixed-count particles, adjustable volume and Kelvin temperature, pause and a 300 K reset. Hold half the initial volume at the initial temperature for two seconds to demonstrate doubled pressure. |

The original four stations are also available. The owned-equipment list now scrolls, so seven stations fit in the workshop UI. Purchases and placements use the existing persistent inventory and collision checks.

## Scientific scope and sources

These are original, offline Unity models. No PhET code, artwork, logo, or simulation files are bundled. PhET's current [licensing page](https://phet.colorado.edu/en/licensing/) was checked when considering direct integration; its regular HTML simulations currently use CC BY-NC 4.0, while commercial embedding requires a separate license. A native implementation also avoids a browser/WebView dependency.

Concept references: OpenStax [active transport](https://openstax.org/books/biology-2e/pages/5-3-active-transport) and [ideal gas law](https://openstax.org/books/chemistry-2e/pages/9-2-relating-pressure-volume-amount-and-temperature-the-ideal-gas-law). The membrane is a concentration-only teaching model without charge, voltage, or real pump stoichiometry. Chromatography uses fictional pigment/Rf profiles to teach differential affinity; it does not predict actual ink/solvent measurements. Gas pressure follows the ideal-gas ratio P/P0 = (T/300) * (2/V). Scale and animation time are illustrative.

## Validation

`LabExpansionChecks.Run` uses a separate profile under ignored `Logs/expansion-qa-profile`, restores the normal save path afterward, and leaves Unity open. It checks all authored reaction balances, distinct choices, feedback, hints, duplicate payment protection, movement locking, new purchases/placement/reload, closed channels, ATP cost/reset, research claims, band animation, Boyle's law, and UTC reward rollover. Screenshots are under `Logs/LabScreens/20-...25-...`.

The original `LabGameChecks.Run` continues to exercise onboarding, existing progression, collisions, cosmetics, and the four original activities. That existing runner closes Unity on completion.

## Room and Georgian copy refinement

The personal lab uses clean wall sections and a blue door cropped from ACTG's existing CC0 Top Down Laboratory sheet. No additional asset download or new package is required. Walls now have a full-height face, connected side caps, and a baseboard; the door is seated in the back wall. The glowing floor rectangle is replaced by a doormat.

Equipment rows are four world units apart. Tables and the player use matching depth order inside the personal lab, while the classroom retains its original order. The two cells nearest the back-wall doorway remain clear: 10 usable furniture positions in the initial room and 22 after expansion. Existing furniture in those cells is relocated to the nearest unoccupied legal cell without losing ownership or charging coins. The layout editor marks those cells as the exit and disables placement there.

Georgian instructions, research feedback, equipment descriptions, HUD labels, and quiz copy were revised for consistent terminology and sentence structure. Long mission text, notices, and equipment descriptions now fit their available areas. The notebook also correctly lists NH4+ among cations.

The isolated expansion checks cover saved-layout migration, coin/ownership preservation, reserved cells, door alignment with the wall, and physical clearance from spawn to exit. The full gameplay checks cover travel in both directions after expansion.

Door travel activates on a fresh approach instead of repeatedly while standing in the trigger. This prevents an arrival input held through the cooldown from immediately sending the player back. The isolated checks verify that waiting in the doorway during the cooldown does not teleport, then stepping away and approaching again works.
