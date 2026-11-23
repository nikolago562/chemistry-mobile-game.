using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChemistryGame
{
    /// <summary>
    /// The personal lab's equipment as build-and-discover tables (same bench as the classroom kits).
    /// Atom, molecule, pH and solution share the classroom kits and books; the rest are lab-only.
    /// </summary>
    public static class LabKits
    {
        private static KitPart P(string id, string name, string symbol, Color32 color, int max) => new KitPart { ID = id, Name = name, Symbol = symbol, Color = color, Max = max };
        private static KitFind F(string id, string title, string formula, Color32 color, string why, params int[] counts) =>
            new KitFind { ID = id, Title = title, Formula = formula, Color = color, Why = why, Counts = counts.Length > 0 ? counts : null };
        private static KitResult Got(Kit kit, string id, string extra = "") { var f = kit.Finds.First(x => x.ID == id); return new KitResult { Find = f, Head = f.Title + "!", Why = extra + f.Why }; }
        private static KitResult Say(string head, string why, bool boom = false) => new KitResult { Head = head, Why = why, Boom = boom };

        /// <summary>The table an equipment id (or a research target) opens.</summary>
        public static Kit For(string id) => id switch
        {
            "atom" => Kits.Atoms, "molecule" => Kits.Molecules, "ph" => Kits.Potions, "solution" => Kits.Solutions,
            "water" => Analysis, "distill" => Thermo,
            _ => All.FirstOrDefault(k => k.ID == id),
        };
        public static Kit[] All => new[] { Balance, Thermo, Analysis, Valve, Battery, Speed, Membrane, Paper, Gas };

        // ------------------------------------------------------------ reaction scale: balance equations by picking ratios

        public static readonly Kit Balance = new Kit
        {
            ID = "balance", Lab = true, Title = "რეაქციების სასწორი", Ask = "შეარჩიე მორეაგირეები", Action = "რეაქცია!",
            Hint = "ატომები არ ქრება: რამდენიც შევა, იმდენი უნდა გამოვიდეს. შეარჩიე სწორი თანაფარდობა.",
            Parts = new[] { P("H2", "წყალბადი", "H2", new Color32(235, 235, 245, 255), 6), P("O2", "ჟანგბადი", "O2", new Color32(230, 80, 80, 255), 4),
                P("N2", "აზოტი", "N2", new Color32(100, 130, 230, 255), 2), P("CH4", "მეთანი", "CH4", new Color32(150, 210, 250, 255), 2), P("C", "ნახშირი", "C", new Color32(70, 70, 80, 255), 4) },
            Finds = new[]
            {
                // counts: H2, O2, N2, CH4, C
                F("water", "წყალი", "2H2 + O2 → 2H2O", new Color32(120, 190, 240, 255), "ორი წყალბადის მოლეკულა ერთ ჟანგბადზე — რაკეტის ძრავაც ასე მუშაობს.", 2, 1, 0, 0, 0),
                F("ammonia", "ამიაკი", "3H2 + N2 → 2NH3", new Color32(170, 150, 240, 255), "ასე ამზადებენ სასუქს — კაცობრიობის ნახევარი ამ რეაქციით იკვებება.", 3, 0, 1, 0, 0),
                F("methane", "მეთანის წვა", "CH4 + 2O2 → CO2 + 2H2O", new Color32(250, 170, 70, 255), "ქურის ალი: ბუნებრივი აირი იწვის და წყალი და ნახშირორჟანგი ჩნდება.", 0, 2, 0, 1, 0),
                F("co2", "ნახშირის წვა", "C + O2 → CO2", new Color32(180, 180, 190, 255), "მაყალში ნახშირი ჟანგბადს იერთებს და ნახშირორჟანგად იქცევა.", 0, 1, 0, 0, 1),
                F("co", "მხუთავი აირი", "2C + O2 → 2CO", new Color32(120, 120, 130, 255), "ჟანგბადი რომ აკლია, ნახშირი CO-ს ქმნის — ამიტომ სჭირდება ღუმელს ჰაერი.", 0, 1, 0, 0, 2),
                F("no", "აზოტის ოქსიდი", "N2 + O2 → 2NO", new Color32(240, 150, 80, 255), "ელვა ჰაერს ისე აცხელებს, რომ აზოტი და ჟანგბადი ერთმანეთს უერთდება.", 0, 1, 1, 0, 0),
            },
            Judge = JudgeBalance,
        };

        /// <summary>How many times the recipe fits exactly into the counts (0 = not a whole multiple).</summary>
        public static int Times(int[] counts, int[] recipe)
        {
            int k = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                if (recipe[i] == 0) { if (counts[i] != 0) return 0; continue; }
                if (counts[i] % recipe[i] != 0) return 0;
                int m = counts[i] / recipe[i];
                if (k == 0) k = m; else if (k != m) return 0;
            }
            return k;
        }

        private static KitResult JudgeBalance(int[] v)
        {
            if (v.Sum() == 0) return Say("ჯერ მოლეკულები დაამატე.", "შეეხე მოლეკულებს მარცხნივ.");
            if (v.Count(c => c > 0) == 1) return Say("ერთი ნივთიერება თავისით არ რეაგირებს.", "დაამატე პარტნიორი — მაგალითად ჟანგბადი.");
            foreach (var f in Balance.Finds)
            {
                int k = Times(v, f.Counts);
                if (k > 0) return Got(Balance, f.ID, k > 1 ? "ესეც ბალანსშია — " + k + "-ჯერ მეტი! " : "");
            }
            // Same ingredients as a recipe but the wrong ratio: it reacts and something is left over.
            foreach (var f in Balance.Finds)
            {
                if (!v.Select((c, i) => c > 0 == f.Counts[i] > 0).All(x => x)) continue;
                int k = v.Select((c, i) => f.Counts[i] == 0 ? int.MaxValue : c / f.Counts[i]).Min();
                var left = v.Select((c, i) => c - k * f.Counts[i]).Select((c, i) => c > 0 ? c + " " + Balance.Parts[i].Symbol : null).Where(t => t != null);
                return Say(k == 0 ? "ჯერ არ კმარა." : "დარჩა ზედმეტი: " + string.Join(", ", left), "რეცეპტი: " + f.Formula + ". თანაფარდობა ზუსტად უნდა დაემთხვეს.");
            }
            return v[1] > 0 ? Say("ბუმ! ნარევი აფეთქდა!", "ასეთი ნარევისთვის რეცეპტი არ ვიცით. სცადე ორი ნივთიერება.", true)
                : Say("არაფერი მოხდა.", "ეს ნივთიერებები ერთმანეთთან არ რეაგირებს. ჟანგბადი ან წყალბადი სცადე.");
        }

        // ------------------------------------------------------------ thermostat: heat and cool, watch states change

        public static readonly Kit Thermo = new Kit
        {
            ID = "thermo", Lab = true, Title = "თერმოსტატი", Ask = "გააცხელე ან გააცივე", Action = "ჩართვა!",
            Hint = "ყოველი სანთურა +20 °C, ყინულის აბაზანა −15 °C. ოთახში 20 °C-ია. ერთ ჯერზე ერთი ნივთიერება!",
            Parts = new[] { P("water", "წყალი", "H2O", new Color32(120, 190, 240, 255), 1), P("ice", "ყინული", "ყინ", new Color32(220, 240, 250, 255), 1),
                P("wax", "სანთელი", "ცვ", new Color32(245, 230, 190, 255), 1), P("choco", "შოკოლადი", "შო", new Color32(120, 70, 40, 255), 1),
                P("salt", "მარილი", "NaCl", new Color32(245, 245, 245, 255), 2), P("flame", "სანთურა", "+20", new Color32(240, 140, 60, 255), 5),
                P("cold", "ყინულის აბაზანა", "−15", new Color32(120, 190, 230, 255), 3) },
            Finds = new[]
            {
                F("melt", "დნობა", "ყინული → წყალი, 0 °C", new Color32(150, 210, 245, 255), "ყინული 0 °C-ზე ზემოთ დნება: მყარი სითხედ იქცევა."),
                F("freeze", "გაყინვა", "წყალი → ყინული", new Color32(210, 235, 250, 255), "0 °C-ზე ქვემოთ წყლის მოლეკულები ჩერდება და კრისტალად ლაგდება."),
                F("boil", "დუღილი", "წყალი → ორთქლი, 100 °C", new Color32(230, 236, 240, 255), "100 °C-ზე წყალი დუღს — ბუშტები ორთქლია, არა ჰაერი!"),
                F("distill", "გამოხდა", "ორთქლი → სუფთა წყალი", new Color32(170, 220, 250, 255), "ორთქლად მხოლოდ წყალი ადის, მარილი ქვაბში რჩება. ასე იღებენ ზღვის წყლიდან სასმელს."),
                F("roadsalt", "მარილი ყინულზე", "ყინული + NaCl", new Color32(235, 245, 250, 255), "მარილი ყინულს ცივშიც ადნობს — ზამთარში გზებს ასე ასუფთავებენ."),
                F("wax", "ცვილის დნობა", "~60 °C", new Color32(245, 225, 170, 255), "სანთლის ცვილი 60 °C-მდე რბილდება და გამჭვირვალე სითხედ დნება."),
                F("choco", "შოკოლადის დნობა", "~34 °C", new Color32(140, 80, 45, 255), "შოკოლადი პირში იმიტომ დნება, რომ მისი კაკაოს კარაქი 34 °C-ზე დნება."),
            },
            Judge = JudgeThermo,
        };

        public static int Temperature(int[] v) => 20 + 20 * v[5] - 15 * v[6];
        private static KitResult JudgeThermo(int[] v)
        {
            int t = Temperature(v), water = v[0], ice = v[1], wax = v[2], choco = v[3], salt = v[4];
            int things = water + ice + wax + choco;
            if (things == 0) return Say("ჭიქა ცარიელია.", "ჩადე წყალი, ყინული, სანთელი ან შოკოლადი.");
            if (things > 1) return Say("ერთ ჯერზე ერთი ნივთიერება!", "ასე ზუსტად დაინახავ, რომელი რა ტემპერატურაზე იცვლება.");
            string now = "ახლა " + t + " °C. ";
            if (choco > 0) return t >= 80 ? Say("ფუ! შოკოლადი დაიწვა!", now + "ძალიან ცხელია — შოკოლადს ნაზი სითბო უყვარს.", true) : t >= 30 ? Got(Thermo, "choco", now) : Say("შოკოლადი მყარია.", now + "ოდნავ გაათბე — 34 °C საკმარისია.");
            if (wax > 0) return t >= 60 ? Got(Thermo, "wax", now) : Say("სანთელი მყარია.", now + "ცვილს ~60 °C სჭირდება.");
            if (ice > 0)
            {
                if (salt > 0 && v[5] == 0) return Got(Thermo, "roadsalt", now);
                return t > 20 ? Got(Thermo, "melt", now) : Say("ყინული ჯერ მყარია.", now + "გაათბე — ყინული 0 °C-ზე ზემოთ დნება.");
            }
            if (t <= 0) return salt > 0 ? Say("მარილიანი წყალი არ გაიყინა!", now + "მარილი გაყინვის ტემპერატურას ამცირებს. მარილის გარეშე სცადე.") : Got(Thermo, "freeze", now);
            if (t >= 100) return Got(Thermo, salt > 0 ? "distill" : "boil", now);
            return Say("წყალი თბება.", now + "100 °C-მდე გაათბე ან 0 °C-მდე გააცივე.");
        }

        // ------------------------------------------------------------ flame test: each metal colours the flame

        public static readonly Kit Analysis = new Kit
        {
            ID = "analysis", Lab = true, Title = "ალის ტესტი", Ask = "შეიტანე ნიმუში ალში", Action = "ტესტი!",
            Hint = "ყოველი ლითონი ალს თავის ფერად ღებავს — ფეიერვერკიც ასე მუშაობს. ერთ ჯერზე ერთი ნიმუში!",
            Parts = new[] { P("na", "ნატრიუმის მარილი", "Na", new Color32(255, 210, 60, 255), 1), P("k", "კალიუმის მარილი", "K", new Color32(190, 140, 240, 255), 1),
                P("cu", "სპილენძის მარილი", "Cu", new Color32(60, 210, 140, 255), 1), P("sr", "სტრონციუმის მარილი", "Sr", new Color32(240, 50, 50, 255), 1),
                P("li", "ლითიუმის მარილი", "Li", new Color32(220, 40, 110, 255), 1), P("ca", "კალციუმის მარილი", "Ca", new Color32(250, 140, 40, 255), 1),
                P("burner", "სანთურა", "ალი", new Color32(90, 140, 240, 255), 1) },
            Finds = new[]
            {
                F("na", "ყვითელი ალი", "ნატრიუმი", new Color32(255, 210, 60, 255), "ქუჩის ძველი ნარინჯისფერი ფარნები ნატრიუმით ანათებს."),
                F("k", "იისფერი ალი", "კალიუმი", new Color32(190, 140, 240, 255), "კალიუმი ალს ღია იისფრად ღებავს. ბანანშიც ბევრია!"),
                F("cu", "მწვანე ალი", "სპილენძი", new Color32(60, 210, 140, 255), "ფეიერვერკის მწვანე ფერი სპილენძის ნაერთებისაა."),
                F("sr", "წითელი ალი", "სტრონციუმი", new Color32(240, 50, 50, 255), "წითელი სასიგნალო რაკეტები სტრონციუმით ანათებს."),
                F("li", "ჟოლოსფერი ალი", "ლითიუმი", new Color32(220, 40, 110, 255), "ლითიუმი ალს მუქ ვარდისფრად ღებავს."),
                F("ca", "ნარინჯისფერი ალი", "კალციუმი", new Color32(250, 140, 40, 255), "კალციუმი ცარცშიც არის და კვერცხის ნაჭუჭშიც."),
            },
            Judge = JudgeFlame,
        };

        private static KitResult JudgeFlame(int[] v)
        {
            if (v[6] == 0) return Say("ჯერ სანთურა აანთე.", "ალის გარეშე ფერს ვერ დაინახავ.");
            var salts = Enumerable.Range(0, 6).Where(i => v[i] > 0).ToList();
            if (salts.Count == 0) return Say("ლურჯი ალი.", "სუფთა აირი ლურჯად იწვის. ახლა რომელიმე მარილი შეიტანე.");
            if (salts.Count > 1) return salts.Contains(0) ? Say("ყვითელმა ყველაფერი დაფარა!", "ნატრიუმი ისე კაშკაშებს, რომ სხვა ფერებს მალავს — ამიტომ ტესტს ერთ ნიმუშზე ვატარებთ.")
                : Say("ფერები აირია.", "ორი ლითონი ერთად ფერს ურევს. ერთი ნიმუში სცადე.");
            return Got(Analysis, Analysis.Parts[salts[0]].ID);
        }

        // ------------------------------------------------------------ balloon valve: light and heavy gases

        public static readonly Kit Valve = new Kit
        {
            ID = "valve", Lab = true, Title = "ბუშტების სარქველი", Ask = "გაბერე ბუშტი", Action = "გაშვება!",
            Hint = "ბუშტი მაღლა მიფრინავს, თუ შიგნით აირი ჰაერზე მსუბუქია. 6 ჭყლეტაზე მეტს ვერ გაუძლებს!",
            Parts = new[] { P("he", "ჰელიუმი", "He", new Color32(250, 170, 200, 255), 7), P("air", "ჰაერი", "ჰაერი", new Color32(250, 230, 140, 255), 7),
                P("co2", "ნახშირორჟანგი", "CO2", new Color32(170, 175, 185, 255), 7), P("h2", "წყალბადი", "H2", new Color32(235, 235, 245, 255), 7) },
            Finds = new[]
            {
                F("he", "ჰელიუმის ბუშტი", "He: 4 გ/მოლი", new Color32(250, 170, 200, 255), "ჰელიუმი ჰაერზე 7-ჯერ მსუბუქია — ბუშტი ცაში მიფრინავს."),
                F("h2", "წყალბადის ბუშტი", "H2: 2 გ/მოლი", new Color32(235, 235, 245, 255), "ყველაზე მსუბუქი აირია, მაგრამ ცეცხლს ეკიდება — ამიტომ ბუშტებში ჰელიუმს ვიყენებთ."),
                F("air", "ჰაერის ბუშტი", "ჰაერი: 29 გ/მოლი", new Color32(250, 230, 140, 255), "შიგნითაც ჰაერია, ოღონდ რეზინიც აქვს — ამიტომ ნელა ეშვება."),
                F("co2", "მძიმე ბუშტი", "CO2: 44 გ/მოლი", new Color32(170, 175, 185, 255), "ნახშირორჟანგი ჰაერზე მძიმეა — ბუშტი ქვასავით ვარდება."),
                F("hover", "მოლივლივე ბუშტი", "ნარევი ≈ ჰაერი", new Color32(240, 200, 170, 255), "მსუბუქი და მძიმე აირის ნარევი თითქმის ჰაერისოდენაა — ბუშტი ერთ ადგილზე ტივტივებს."),
            },
            Judge = JudgeBalloon,
        };

        private static readonly float[] Molar = { 4, 29, 44, 2 };
        /// <summary>Average molar mass of the gas inside (0 when empty).</summary>
        public static float Mass(int[] v) => v.Sum() == 0 ? 0 : v.Select((c, i) => c * Molar[i]).Sum() / v.Sum();
        /// <summary>Where the released balloon goes: +1 up, 0 hover, negative falls.</summary>
        public static float Lift(int[] v) { float m = Mass(v); return m < 20 ? 1 : m <= 27 ? 0 : m <= 35 ? -0.4f : -1; }
        private static KitResult JudgeBalloon(int[] v)
        {
            int total = v.Sum();
            if (total == 0) return Say("ბუშტი ცარიელია.", "შეეხე აირს და გაბერე.");
            if (total > 6) return Say("ტკაც! ბუშტი გასკდა!", "რეზინი მეტს ვერ უძლებს. 6 ჭყლეტა საკმარისია.", true);
            int kinds = v.Count(c => c > 0);
            if (kinds == 1) return Got(Valve, Valve.Parts[System.Array.FindIndex(v, c => c > 0)].ID);
            float lift = Lift(v);
            if (lift == 0) return Got(Valve, "hover", "საშუალო მასა ≈ " + Mass(v).ToString("0") + " გ/მოლი. ");
            return Say(lift > 0 ? "ნარევი ბუშტი მაღლა მიფრინავს." : "ნარევი ბუშტი ეშვება.", "საშუალო მასა ≈ " + Mass(v).ToString("0") + " გ/მოლი (ჰაერი 29). სცადე ისე, რომ ზუსტად ჰაერში იტივტივოს.");
        }

        // ------------------------------------------------------------ fruit battery: electrons from zinc to copper

        public static readonly Kit Battery = new Kit
        {
            ID = "particles", Lab = true, Title = "ლიმონის ბატარეა", Ask = "ააწყვე ბატარეა", Action = "ჩართვა!",
            Hint = "ბატარეას ორი სხვადასხვა ლითონი, მჟავე წვენი და სადენი სჭირდება. ნათურას ~3 უჯრედი უნდა.",
            Parts = new[] { P("lemon", "ლიმონი", "ლიმ", new Color32(245, 220, 70, 255), 4), P("potato", "კარტოფილი", "კარ", new Color32(200, 160, 100, 255), 4),
                P("zinc", "თუთიის ფირფიტა", "Zn", new Color32(170, 176, 186, 255), 1), P("copper", "სპილენძის ფირფიტა", "Cu", new Color32(201, 118, 72, 255), 1),
                P("wire", "სადენი", "სად", new Color32(210, 90, 80, 255), 1), P("led", "ნათურა", "ნათ", new Color32(250, 240, 160, 255), 1) },
            Finds = new[]
            {
                F("lemon", "ლიმონის ბატარეა", "~0.9 ვოლტი", new Color32(245, 220, 70, 255), "ლიმონის მჟავა ელექტროლიტია: თუთიიდან სპილენძისკენ ელექტრონები მიედინება."),
                F("potato", "კარტოფილის ბატარეა", "~0.8 ვოლტი", new Color32(200, 160, 100, 255), "კარტოფილის წვენიც ელექტროლიტია — მოხარშული კიდევ უკეთ მუშაობს!"),
                F("stack", "ბატარეების ჯაჭვი", "ძაბვები იკრიბება", new Color32(250, 200, 80, 255), "მიმდევრობით შეერთებული უჯრედების ძაბვა იკრიბება — ასეა ფანრის ბატარეებშიც."),
                F("led", "ნათურა აინთო!", "~2.5 ვოლტი", new Color32(255, 240, 120, 255), "სამი უჯრედი ერთად საკმარისია პატარა ნათურისთვის. ხილით აანთე სინათლე!"),
            },
            Judge = JudgeBattery,
        };

        private static KitResult JudgeBattery(int[] v)
        {
            int cells = v[0] + v[1];
            if (cells == 0) return Say("ელექტროლიტი არ არის.", "დაამატე ლიმონი ან კარტოფილი — მათი წვენი დენს ატარებს.");
            if (v[2] == 0 || v[3] == 0) return Say("დენი არ გადის.", "ორი სხვადასხვა ლითონი სჭირდება: თუთია ელექტრონებს გასცემს, სპილენძი — იღებს.");
            if (v[4] == 0) return Say("წრედი გახსნილია.", "ელექტრონებს გზა სჭირდება — დაამატე სადენი.");
            string volts = "ძაბვა ≈ " + (cells * 0.85f).ToString("0.0") + " ვოლტი. ";
            if (v[5] > 0) return cells >= 3 ? Got(Battery, "led", volts) : Say("ნათურა ჯერ არ ანთია.", volts + "ნათურას მეტი სჭირდება — დაამატე უჯრედები.");
            if (cells > 1) return Got(Battery, "stack", volts);
            return Got(Battery, v[0] > 0 ? "lemon" : "potato", volts);
        }

        // ------------------------------------------------------------ reaction speed: surface, heat, catalyst

        public static readonly Kit Speed = new Kit
        {
            ID = "compare", Lab = true, Title = "რეაქციის სიჩქარე", Ask = "შეარჩიე და შეურიე", Action = "შერევა!",
            Hint = "წვრილი ფხვნილი, სითბო და კატალიზატორი რეაქციას აჩქარებს. ერთი ცვლილება — ერთი ცდა!",
            Parts = new[] { P("vinegar", "ძმარი", "ძმ", new Color32(236, 228, 196, 255), 1), P("lump", "ცარცის ნატეხი", "ნატ", new Color32(236, 232, 222, 255), 1),
                P("powder", "ცარცის ფხვნილი", "ფხვ", new Color32(250, 248, 240, 255), 1), P("warm", "თბილი აბაზანა", "°C", new Color32(240, 150, 80, 255), 1),
                P("peroxide", "წყალბადის ზეჟანგი", "H2O2", new Color32(200, 230, 245, 255), 1), P("yeast", "საფუარი", "საფ", new Color32(214, 186, 140, 255), 1) },
            Finds = new[]
            {
                F("slow", "ნელი შიშინი", "ნატეხი + მჟავა", new Color32(220, 225, 200, 255), "დიდ ნატეხს ზედაპირი პატარა აქვს — მჟავა მას მხოლოდ გარედან ჭამს."),
                F("fast", "სწრაფი შიშინი", "ფხვნილი + მჟავა", new Color32(240, 240, 220, 255), "ფხვნილს უზარმაზარი ზედაპირი აქვს — ყველა მარცვალი ერთდროულად რეაგირებს."),
                F("warmlump", "თბილი ნატეხი", "ნატეხი + მჟავა + სითბო", new Color32(245, 220, 180, 255), "სითბომ ნატეხიც დააჩქარა, მაგრამ ფხვნილს მაინც ვერ ეწევა."),
                F("hot", "ქაფიანი ამოხეთქვა", "ფხვნილი + მჟავა + სითბო", new Color32(250, 200, 140, 255), "სითბოში ნაწილაკები სწრაფად მოძრაობს და ხშირად ეჯახება ერთმანეთს."),
                F("catalyst", "სპილოს კბილის პასტა", "H2O2 + საფუარი", new Color32(250, 250, 255, 255), "საფუარის ფერმენტი ზეჟანგს წამებში შლის: წყალი და ჟანგბადის ქაფი!"),
            },
            Judge = JudgeSpeed,
        };

        private static KitResult JudgeSpeed(int[] v)
        {
            bool acid = v[0] > 0, lump = v[1] > 0, powder = v[2] > 0, warm = v[3] > 0, peroxide = v[4] > 0, yeast = v[5] > 0;
            if (v.Sum() == 0) return Say("კოლბა ცარიელია.", "შეარჩიე ნივთიერებები მარცხნივ.");
            if (peroxide && yeast && !acid && !lump && !powder) return Got(Speed, "catalyst");
            if (peroxide && !acid && !lump && !powder) return Say("თითქმის არაფერი.", "ზეჟანგი თავისით ძალიან ნელა იშლება — კატალიზატორი სცადე.");
            if (acid && !peroxide && !yeast && (lump != powder)) return Got(Speed, lump ? (warm ? "warmlump" : "slow") : (warm ? "hot" : "fast"));
            if (acid && lump && powder) return Say("ორივე ცარცი ერთად!", "სიჩქარის შესადარებლად ერთ ჯერზე ერთი ფორმა გამოსცადე.");
            if (acid) return Say("ძმარი მარტო მშვიდია.", "დაამატე ცარცი — ნატეხი ან ფხვნილი.");
            return Say("არაფერი მომხდარა.", "ცარცს მჟავა სჭირდება, ზეჟანგს — კატალიზატორი.");
        }

        // ------------------------------------------------------------ membrane tank: diffusion and osmosis

        public static readonly Kit Membrane = new Kit
        {
            ID = "membrane", Lab = true, Title = "მემბრანის აკვარიუმი", Ask = "ჩადე წყალში", Action = "დაკვირვება!",
            Hint = "წყალი მემბრანაში იქ გადადის, სადაც მარილი ან შაქარი მეტია. ნაწილაკები თავისით ნაწილდება თანაბრად.",
            Parts = new[] { P("water", "წყალი", "H2O", new Color32(120, 190, 240, 255), 1), P("salt", "მარილის კოვზი", "NaCl", new Color32(245, 245, 245, 255), 3),
                P("dye", "საღებავის წვეთი", "საღ", new Color32(150, 70, 190, 255), 1), P("raisin", "ქიშმიში", "ქიშ", new Color32(110, 60, 90, 255), 1),
                P("potato", "კარტოფილის ნაჭერი", "კარ", new Color32(235, 215, 150, 255), 1) },
            Finds = new[]
            {
                F("diffusion", "დიფუზია", "ნაწილაკები ნაწილდება", new Color32(150, 70, 190, 255), "საღებავის ნაწილაკები თავისით მოძრაობს და ნელ-ნელა მთელ წყალს ღებავს."),
                F("swell", "ქიშმიში გაიბერა", "ოსმოსი: წყალი შედის", new Color32(160, 90, 130, 255), "ქიშმიშის შიგნით შაქარია: წყალი გარსში შიგნით შედის — ეს ოსმოსია."),
                F("firm", "მკვრივი კარტოფილი", "სუფთა წყალი შედის", new Color32(240, 225, 160, 255), "სუფთა წყალი უჯრედებში შედის და კარტოფილი იჭიმება."),
                F("limp", "დაჭკნარი კარტოფილი", "წყალი გამოდის", new Color32(210, 195, 140, 255), "მარილიან წყალში წყალი კარტოფილიდან გამოდის და ის რბილდება."),
            },
            Judge = JudgeMembrane,
        };

        private static KitResult JudgeMembrane(int[] v)
        {
            if (v[0] == 0) return Say("აკვარიუმი ცარიელია.", "ჯერ წყალი ჩაასხი.");
            bool raisin = v[3] > 0, potato = v[4] > 0;
            if (raisin && potato) return Say("ორივე ერთად!", "ერთ ჯერზე ერთი ნიმუში ჩადე, რომ ცვლილება გაარჩიო.");
            if (raisin) return v[1] == 0 ? Got(Membrane, "swell") : Say("ქიშმიში არ გაიბერა.", "მარილიან წყალში წყალი ქიშმიშში ვეღარ შედის. სუფთა წყალი სცადე.");
            if (potato) return v[1] >= 2 ? Got(Membrane, "limp") : v[1] == 0 ? Got(Membrane, "firm") : Say("კარტოფილი თითქმის არ შეიცვალა.", "ცოტა მარილია. დაამატე კიდევ ან სულ ამოიღე.");
            if (v[2] > 0) return Got(Membrane, "diffusion");
            return Say("სუფთა წყალია.", v[1] > 0 ? "მარილი გაიხსნა. ახლა ქიშმიში ან კარტოფილი ჩადე." : "დაამატე საღებავი, ქიშმიში ან კარტოფილი.");
        }

        // ------------------------------------------------------------ paper chromatography: hidden colours in inks

        public static readonly Kit Paper = new Kit
        {
            ID = "chromatography", Lab = true, Title = "ფერების დეტექტივი", Ask = "მარკერი და გამხსნელი", Action = "ქაღალდზე!",
            Hint = "გამხსნელი ქაღალდზე ადის და ფერებს თან მიაქვს: მსუბუქი პიგმენტი უფრო მაღლა ადის.",
            Parts = new[] { P("black", "შავი მარკერი", "შავ", new Color32(40, 40, 50, 255), 1), P("green", "მწვანე მარკერი", "მწვ", new Color32(40, 150, 80, 255), 1),
                P("brown", "ყავისფერი მარკერი", "ყავ", new Color32(130, 80, 40, 255), 1), P("red", "წითელი მარკერი", "წით", new Color32(220, 40, 50, 255), 1),
                P("permanent", "მუდმივი მარკერი", "მუდ", new Color32(60, 40, 120, 255), 1), P("water", "წყალი", "H2O", new Color32(120, 190, 240, 255), 1),
                P("alcohol", "სპირტი", "სპ", new Color32(220, 235, 245, 255), 1) },
            Finds = new[]
            {
                F("black", "შავის საიდუმლო", "ლურჯი + წითელი + ყვითელი", new Color32(40, 40, 50, 255), "შავი მარკერი სინამდვილეში სამი ფერის ნარევია!"),
                F("green", "მწვანის საიდუმლო", "ლურჯი + ყვითელი", new Color32(40, 150, 80, 255), "მწვანე ორი პიგმენტისგანაა — ყვითელი უფრო მაღლა ადის."),
                F("brown", "ყავისფრის საიდუმლო", "წითელი + ყვითელი + ლურჯი", new Color32(130, 80, 40, 255), "ყავისფერი ბევრი ფერის ნარევია, თითქმის როგორც შავი."),
                F("red", "სუფთა წითელი", "ერთი პიგმენტი", new Color32(220, 40, 50, 255), "წითელი მარკერი ერთი პიგმენტია — ზოლიც ერთია. ეს სუფთა ნივთიერებაა."),
                F("permanent", "მუდმივი მელანი", "იისფერი + ლურჯი", new Color32(60, 40, 120, 255), "მუდმივი მელანი წყალში არ იხსნება, სპირტში კი იხსნება და იყოფა."),
            },
            Judge = JudgePaper,
        };

        /// <summary>Pigment bands per find: colour and how far up the paper it climbs (0..1).</summary>
        public static readonly Dictionary<string, (Color color, float height)[]> Bands = new Dictionary<string, (Color, float)[]>
        {
            { "black", new[] { (new Color(0.25f, 0.35f, 0.85f), 0.3f), (new Color(0.85f, 0.2f, 0.25f), 0.55f), (new Color(0.95f, 0.8f, 0.2f), 0.85f) } },
            { "green", new[] { (new Color(0.25f, 0.35f, 0.85f), 0.4f), (new Color(0.95f, 0.8f, 0.2f), 0.85f) } },
            { "brown", new[] { (new Color(0.25f, 0.35f, 0.85f), 0.25f), (new Color(0.85f, 0.2f, 0.25f), 0.5f), (new Color(0.95f, 0.8f, 0.2f), 0.8f) } },
            { "red", new[] { (new Color(0.86f, 0.16f, 0.2f), 0.65f) } },
            { "permanent", new[] { (new Color(0.5f, 0.2f, 0.65f), 0.45f), (new Color(0.25f, 0.35f, 0.85f), 0.75f) } },
        };

        private static KitResult JudgePaper(int[] v)
        {
            var markers = Enumerable.Range(0, 5).Where(i => v[i] > 0).ToList();
            if (markers.Count == 0) return Say("ქაღალდზე წერტილი არ არის.", "ჯერ მარკერი აირჩიე.");
            if (markers.Count > 1) return Say("ერთი მარკერი აირჩიე.", "ერთ ზოლზე ერთი წერტილი — თორემ ფერები აირევა.");
            if (v[5] + v[6] == 0) return Say("ქაღალდი მშრალია.", "დაამატე გამხსნელი: წყალი ან სპირტი.");
            if (v[5] > 0 && v[6] > 0) return Say("ორი გამხსნელი ერთად!", "ერთი აირჩიე, რომ შედეგი გაიგო.");
            if (markers[0] == 4 && v[5] > 0) return Say("მელანი ადგილზე დარჩა.", "მუდმივი მელანი წყალში არ იხსნება — სპირტი სცადე.");
            return Got(Paper, Paper.Parts[markers[0]].ID);
        }

        // ------------------------------------------------------------ gas pressure: particles, heat and squeezing

        public static readonly Kit Gas = new Kit
        {
            ID = "gas", Lab = true, Title = "აირის წნევა", Ask = "შეცვალე პირობები", Action = "გაზომვა!",
            Hint = "წნევა იზრდება, როცა ნაწილაკები მეტია, უფრო ცხელია ან ნაკლებ ადგილზეა. 4 ატმოსფეროზე მეტს ჭურჭელი ვერ უძლებს!",
            Parts = new[] { P("n", "აირის ნაწილაკი", "n", new Color32(240, 200, 90, 255), 10), P("heat", "სანთურა", "+100K", new Color32(240, 140, 60, 255), 3),
                P("cold", "ყინული", "−60K", new Color32(120, 190, 230, 255), 2), P("piston", "დგუშის ჩაწევა", "V−", new Color32(150, 156, 170, 255), 3) },
            Finds = new[]
            {
                F("vacuum", "ვაკუუმი", "P = 0", new Color32(40, 40, 60, 255), "ნაწილაკები არ არის — წნევაც ნულია. კოსმოსი თითქმის ასეთია."),
                F("boyle", "ბოილის კანონი", "ნაკლები V → მეტი P", new Color32(150, 156, 170, 255), "მოცულობა შემცირდა და ნაწილაკები კედლებს უფრო ხშირად ეჯახება."),
                F("hot", "გაცხელებული აირი", "მეტი T → მეტი P", new Color32(240, 140, 60, 255), "ცხელი ნაწილაკები სწრაფად დარბის და კედლებს ძლიერად აწვება."),
                F("cold", "გაცივებული აირი", "ნაკლები T → ნაკლები P", new Color32(120, 190, 230, 255), "ცივში ნაწილაკები ნელდება — ამიტომ იკუმშება ბუშტი მაცივარში."),
                F("more", "მეტი ნაწილაკი", "მეტი n → მეტი P", new Color32(240, 200, 90, 255), "საბურავს რომ ჰაერს ვუმატებთ, წნევა იზრდება."),
            },
            Judge = JudgeGas,
        };

        public static float Kelvin(int[] v) => 300 + 100 * v[1] - 60 * v[2];
        public static float Volume(int[] v) => 1 - 0.2f * v[3];
        /// <summary>Pressure in atmospheres: 4 particles at room temperature in the full box make 1 atm.</summary>
        public static float Pressure(int[] v) => v[0] / 4f * (Kelvin(v) / 300f) / Volume(v);
        private static KitResult JudgeGas(int[] v)
        {
            float p = Pressure(v);
            string now = "P = " + p.ToString("0.00") + " ატმ. ";
            if (v[0] == 0) return Got(Gas, "vacuum");
            if (p > 4) return Say("ბუმ! სახურავი აფრინდა!", now + "რეალურ ჭურჭელს ამიტომ აქვს უსაფრთხოების სარქველი.", true);
            bool heat = v[1] > 0, cold = v[2] > 0, squeeze = v[3] > 0;
            if (squeeze && !heat && !cold) return Got(Gas, "boyle", now);
            if (heat && !squeeze && !cold) return Got(Gas, "hot", now);
            if (cold && !heat && !squeeze) return Got(Gas, "cold", now);
            if (!heat && !cold && !squeeze) return v[0] >= 6 ? Got(Gas, "more", now) : Say("ჩვეულებრივი წნევა.", now + "შეცვალე ერთი რამ: ნაწილაკები, სითბო ან მოცულობა.");
            return Say("რამდენიმე ცვლილება ერთად.", now + "კანონის დასანახად ერთ ჯერზე ერთი რამ შეცვალე.");
        }
    }
}
