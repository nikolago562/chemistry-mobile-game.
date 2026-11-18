# გრაფიკის წყაროები

## პერსონაჟები

სტუდენტი და მასწავლებელი იყენებენ Shade-ის [Puny Characters](https://opengameart.org/content/puny-characters) პაკეტის საბაზისო პერსონაჟს. ლიცენზია — **CC0**. წყარო და ლიცენზია ინახება **Assets/Art/Characters/Puny/**-ში.

პერსონაჟი დაახლოებით 16×16 პიქსელის სივრცეშია დახატული; ანიმაციის უჯრებია 32×32. გამოიყენება რვა მიმართულების დამოუკიდებელი დგომა და სიარული. თმისა და ტანსაცმლის ფერები თამაშის შადერით იცვლება.

## ქართული შრიფტი

ინტერფეისის ძირითადი შრიფტია [FiraGO](https://github.com/bBoxType/FiraGO), Regular, Medium და Bold. ქართული ნაწილის დიზაინერები არიან აკაკი რაზმაძე და ანია მაინერსი. ლიცენზია — **SIL Open Font License**; ასლი ინახება **Assets/Art/Fonts/FiraGO/OFL.txt**-ში. იშვიათი სიმბოლოების სათადარიგო შრიფტია Noto Sans, ასევე OFL ლიცენზიით.

## ინტერფეისი

ღილაკის საფუძველია [Kenney UI Pack](https://kenney.nl/assets/ui-pack), ავტორი — Kenney Vleugels, ლიცენზია — **CC0**. ლიცენზია ინახება **Assets/Art/UI/Kenney/LICENSE.txt**-ში. ფერები, განლაგება, ანიმაციები და დამრგვალებული პანელები ამ პროექტისთვისაა შექმნილი.

## საკლასო ოთახი

კედლები, იატაკი და საწყისი კარადის გრაფიკა Godot-ის პროექტიდანაა გადმოტანილი. ექვსი სამუშაო მაგიდა, მასწავლებლის დახლი, ნიჟარა, სინჯარები და კოლბები პროექტისთვისაა შექმნილი პიქსელური გრაფიკით. ხალათის კარადას აქვს ცალკე მყარი საზღვარი და წინიდან მისასვლელი ზონა.

## ახალი ლაბორატორიის გრაფიკა

ორივე ოთახში გამოყენებულია ACTG-ის [Top Down Laboratory](https://actg.itch.io/top-down-laboratory) პაკეტის სუფთა იატაკი, მეტალის სამუშაო მაგიდები, კარი, კარადა, ხელსაწყო და მინის ჭურჭელი. ლიცენზია — **CC0 1.0 Universal**. უცვლელი წყაროს ფურცლები და სრული ლიცენზია დევს **Assets/Resources/Laboratory/**-ში. შერჩეული ფრაგმენტები იჭრება Unity-ში; დაზიანებული და საშინელებათა დეკორაციები გამოყენებული არ არის.

პირადი ოთახის კედლის პანელები, განლაგება და ცდების დიაგრამები შექმნილია ამ პროექტისთვის.

## თავისუფალი ქიმიური ცდები

მოლეკულების აწყობა, კონცენტრაცია, მჟავებისა და ფუძეების კვლევა და ატომების აწყობა ორიგინალური Unity მინიაქტივობებია. შთაგონების წყაროა [PhET-ის ქიმიის კოლექცია](https://phet.colorado.edu/en/simulations/filter?subjects=chemistry&type=html), განსაკუთრებით [Build a Molecule](https://phet.colorado.edu/en/simulations/build-a-molecule), [Concentration](https://phet.colorado.edu/en/simulations/concentration) და [pH Scale: Basics](https://phet.colorado.edu/en/simulations/ph-scale-basics). PhET-ის პროგრამული კოდი, გრაფიკა და ბრენდინგი გადმოტანილი არ არის.

## Notebook typography and pixel icons

The primary Georgian interface font is Noto Serif Georgian from Google Fonts (SIL Open Font License), in `Assets/Art/Fonts/NotoSerifGeorgian/`. Its fixed 1024px SDF atlas is preloaded with Georgian and Latin glyphs, with existing fonts providing additional symbols. `LabTypographySetup.Apply` updates the current lab scene; the pixel setup also uses this font.

The ten UI icons are original 16x16 pixel patterns in `LabIcons.cs`: brass coin, flame, glass flask, atom, notebook, coat, trophy, quest clipboard, lock, and checkmark. They use point filtering and a shared ink/brass/paper/reagent palette.

## Original interactive research expansion

The membrane, chromatography and gas-piston activities are original Unity code and UI graphics in `LabResearchUI.cs`. No third-party simulation assets or code were imported. Concept references and the educational limits of the models are documented in `GAMEPLAY_EXPANSION.md`. The 16 general questions and per-choice feedback are original Georgian educational copy. Equation choices reuse the project's authored 26 reaction assets and compute atom-count feedback locally.

## Personal lab walls and door

The raised wall paint, lower panels, cap, baseboard and blue door use clean crops from the existing ACTG **Top Down Laboratory** CC0 sheets (`Assets/Resources/Laboratory/construction.png` and `doors.png`). The source remains [ACTG on itch.io](https://actg.itch.io/top-down-laboratory). No Kenney/dungeon artwork was imported; those alternatives were reviewed, and the existing laboratory set was retained for visual consistency.
