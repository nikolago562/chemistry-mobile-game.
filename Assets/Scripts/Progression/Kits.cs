using System;
using System.Linq;
using UnityEngine;

namespace ChemistryGame
{
    public sealed class KitPart
    {
        public string ID, Name, Symbol;
        public Color Color;
        public int Max;
    }

    /// <summary>Something a table can make. <see cref="Counts"/> is the exact recipe, or null when the table judges it another way.</summary>
    public sealed class KitFind
    {
        public string ID, Title, Formula, Why;
        public Color Color;
        public int[] Counts;
    }

    public sealed class KitResult
    {
        public KitFind Find;
        public string Head, Why;
        /// <summary>A funny failure: the stage bursts instead of showing a result.</summary>
        public bool Boom;
    }

    /// <summary>A hands-on table: parts you tap in, a build button, a list of things to discover.</summary>
    public sealed class Kit
    {
        public string ID, Title, Ask, Action, Hint;
        public StationType Station;
        /// <summary>A personal-lab table: no classroom lessons button, its own animated stage.</summary>
        public bool Lab;
        public KitPart[] Parts;
        public KitFind[] Finds;
        public Func<int[], KitResult> Judge;
        public string Key(KitFind find) => "kit:" + ID + ":" + find.ID;
        /// <summary>An exact-recipe match, or null.</summary>
        public KitFind Match(int[] counts) => Finds.FirstOrDefault(f => f.Counts != null && f.Counts.SequenceEqual(counts));
    }

    /// <summary>The three build-and-discover tables: atom forge, molecule kit and pH potions.</summary>
    public static class Kits
    {
        private static KitPart P(string id, string name, string symbol, Color32 color, int max) => new KitPart { ID = id, Name = name, Symbol = symbol, Color = color, Max = max };
        private static KitFind F(string id, string title, string formula, Color32 color, string why, params int[] counts) =>
            new KitFind { ID = id, Title = title, Formula = formula, Color = color, Why = why, Counts = counts.Length > 0 ? counts : null };

        // ------------------------------------------------------------ atom forge: protons, neutrons, electrons

        private static readonly Color32 Proton = new Color32(224, 96, 90, 255), Neutron = new Color32(170, 176, 186, 255), Electron = new Color32(90, 170, 240, 255);
        public static readonly string[] Symbols = { "", "H", "He", "Li", "Be", "B", "C", "N", "O", "F", "Ne", "Na", "Mg" };
        public static readonly string[] Names = { "", "წყალბადი", "ჰელიუმი", "ლითიუმი", "ბერილიუმი", "ბორი", "ნახშირბადი", "აზოტი", "ჟანგბადი", "ფთორი", "ნეონი", "ნატრიუმი", "მაგნიუმი" };
        /// <summary>Neutron counts each element tolerates (stable or long-lived); anything else bursts.</summary>
        private static readonly int[][] Stable = { new int[0], new[] { 0, 1, 2 }, new[] { 1, 2 }, new[] { 3, 4 }, new[] { 5 }, new[] { 5, 6 }, new[] { 6, 7, 8 }, new[] { 7, 8 }, new[] { 8, 9, 10 }, new[] { 10 }, new[] { 10, 11, 12 }, new[] { 12 }, new[] { 12, 13, 14 } };

        public static readonly Kit Atoms = new Kit
        {
            ID = "atom", Title = "ატომის სამჭედლო", Station = StationType.AtomTable, Ask = "ააწყვე ატომი", Action = "შექმნა!",
            Hint = "ნეიტრალურ ატომს პროტონებისა და ელექტრონების ტოლი რაოდენობა აქვს. ზედმეტი ნეიტრონი ბირთვს შლის!",
            Parts = new[] { P("p", "პროტონი", "p+", Proton, 12), P("n", "ნეიტრონი", "n", Neutron, 14), P("e", "ელექტრონი", "e-", Electron, 12) },
            Finds = Enumerable.Range(1, 12).Select(z => F(Symbols[z], Names[z], Symbols[z], ElementColor(z), Fact(z), z, DefaultNeutrons(z), z))
                .Concat(new[]
                {
                    F("D", "დეიტერიუმი", "H-2", new Color32(150, 200, 240, 255), "წყალბადი ერთი ნეიტრონით: „მძიმე წყალი“ მისგან კეთდება.", 1, 1, 1),
                    F("T", "ტრიტიუმი", "H-3", new Color32(170, 240, 170, 255), "ორი ნეიტრონით წყალბადი რადიოაქტიურია და სიბნელეში ანათებს საათის ციფერბლატებზე.", 1, 2, 1),
                    F("C14", "ნახშირბად-14", "C-14", new Color32(120, 120, 120, 255), "რადიოაქტიური ნახშირბადით არქეოლოგები ძველი ძვლების ასაკს ითვლიან.", 6, 8, 6),
                    F("O18", "ჟანგბად-18", "O-18", new Color32(230, 120, 120, 255), "მძიმე ჟანგბადი ყინულში მეცნიერებს ძველ ამინდზე უყვება.", 8, 10, 8),
                }).ToArray(),
            Judge = JudgeAtom,
        };
        private static int DefaultNeutrons(int z) => z == 1 ? 0 : z == 2 ? 2 : z == 3 ? 4 : z == 4 ? 5 : z == 5 ? 6 : z == 9 ? 10 : z == 11 ? 12 : z;
        private static Color32 ElementColor(int z) => new[] { new Color32(0, 0, 0, 0), new Color32(240, 240, 250, 255), new Color32(250, 220, 120, 255), new Color32(200, 120, 200, 255), new Color32(170, 200, 140, 255), new Color32(220, 160, 110, 255), new Color32(80, 80, 90, 255), new Color32(110, 140, 230, 255), new Color32(230, 90, 90, 255), new Color32(170, 230, 120, 255), new Color32(250, 120, 80, 255), new Color32(250, 200, 80, 255), new Color32(200, 210, 220, 255) }[z];
        private static string Fact(int z) => new[]
        {
            "", "ყველაზე მსუბუქი და გავრცელებული ელემენტი სამყაროში — ვარსკვლავები მისგან ანათებენ.", "ბუშტებს აფრენს და ხმას აწვრილებს. მზეზე აღმოაჩინეს.",
            "ყველაზე მსუბუქი ლითონი — ტელეფონის ბატარეაშიც არის.", "მსუბუქი და მყარი: კოსმოსურ ტელესკოპებში იყენებენ.", "ბორის ნაერთები შუშას სიცხისგან იცავს.",
            "ცხოვრების საფუძველი: ალმასიცა და ფანქრის გრაფიტიც სუფთა ნახშირბადია.", "ჰაერის 78% აზოტია.", "ვსუნთქავთ — ჰაერის 21%-ია.",
            "კბილის პასტაში ფთორიდია — მინანქარს ამაგრებს.", "ნეონის ნათურები წითლად ანათებს.", "წყალში ჩაგდებისას შიშინებს! სუფრის მარილის ნახევარია.", "ფოიერვერკში თეთრ ნაპერწკლებს იძლევა.",
        }[z];

        private static KitResult JudgeAtom(int[] v)
        {
            int p = v[0], n = v[1], e = v[2];
            var find = Atoms.Match(v);
            if (find != null) return new KitResult { Find = find, Head = find.Title + "!", Why = find.Why };
            if (p == 0) return new KitResult { Head = "ბირთვი ცარიელია.", Why = "ატომს ერთი პროტონი მაინც სჭირდება — პროტონების რიცხვი წყვეტს, რომელი ელემენტია." };
            if (!Stable[p].Contains(n)) return new KitResult { Boom = true, Head = "ბუმ! ბირთვი დაიშალა!", Why = Names[p] + "-ს " + n + " ნეიტრონს ვერ უძლებს. სცადე " + string.Join(" ან ", Stable[p]) + "." };
            if (p != e) return new KitResult { Head = "ეს იონია: მუხტი " + (p > e ? "+" : "−") + Math.Abs(p - e), Why = "ნეიტრალურ ატომს პროტონებისა და ელექტრონების ტოლი რაოდენობა აქვს." };
            return new KitResult { Head = Names[p] + "!", Why = "ეს ელემენტი წიგნში სხვა ნეიტრონებით წერია." };
        }

        // ------------------------------------------------------------ molecule kit: atoms into molecules

        public static readonly Kit Molecules = new Kit
        {
            ID = "molecule", Title = "მოლეკულების ნაკრები", Station = StationType.MoleculeTable, Ask = "შეაერთე ატომები", Action = "შეერთება!",
            Hint = "ბმები: წყალბადი 1, ჟანგბადი 2, აზოტი 3, ნახშირბადი 4, ქლორი 1, ნატრიუმი 1.",
            Parts = new[] { P("H", "წყალბადი", "H", new Color32(235, 235, 245, 255), 12), P("C", "ნახშირბადი", "C", new Color32(70, 70, 80, 255), 6), P("N", "აზოტი", "N", new Color32(100, 130, 230, 255), 2),
                P("O", "ჟანგბადი", "O", new Color32(230, 80, 80, 255), 6), P("Cl", "ქლორი", "Cl", new Color32(120, 210, 110, 255), 2), P("Na", "ნატრიუმი", "Na", new Color32(170, 120, 230, 255), 2) },
            Finds = new[]
            {
                // counts: H, C, N, O, Cl, Na
                F("H2", "წყალბადის აირი", "H2", new Color32(235, 235, 245, 255), "ყველაზე მსუბუქი აირი — რაკეტების საწვავი.", 2, 0, 0, 0, 0, 0),
                F("O2", "ჟანგბადი", "O2", new Color32(230, 80, 80, 255), "ორმაგი ბმით დაკავშირებული ორი ატომი — ამას ვსუნთქავთ.", 0, 0, 0, 2, 0, 0),
                F("N2", "აზოტი", "N2", new Color32(100, 130, 230, 255), "სამმაგი ბმა ძალიან მტკიცეა, ამიტომაა აზოტი ასე მშვიდი.", 0, 0, 2, 0, 0, 0),
                F("Cl2", "ქლორი", "Cl2", new Color32(120, 210, 110, 255), "მომწვანო აირი — აუზის წყალს ასუფთავებს.", 0, 0, 0, 0, 2, 0),
                F("O3", "ოზონი", "O3", new Color32(140, 170, 240, 255), "სამი ჟანგბადი: მაღლა ცაში მზის მავნე სხივებს გვიჭერს.", 0, 0, 0, 3, 0, 0),
                F("H2O", "წყალი", "H2O", new Color32(120, 190, 240, 255), "ორი წყალბადი და ერთი ჟანგბადი — სიცოცხლის სითხე.", 2, 0, 0, 1, 0, 0),
                F("H2O2", "წყალბადის ზეჟანგი", "H2O2", new Color32(200, 230, 245, 255), "ერთი ჟანგბადით მეტი, ვიდრე წყალში — ჭრილობას ქაფით ასუფთავებს.", 2, 0, 0, 2, 0, 0),
                F("CO2", "ნახშირორჟანგი", "CO2", new Color32(180, 180, 190, 255), "ამოვისუნთქავთ; მცენარეები ისუნთქავენ. გაზიან წყალშიც არის.", 0, 1, 0, 2, 0, 0),
                F("CO", "მხუთავი აირი", "CO", new Color32(110, 110, 120, 255), "უხილავი და საშიში აირი — ამიტომ გვჭირდება ღუმელიან ოთახში განიავება.", 0, 1, 0, 1, 0, 0),
                F("CH4", "მეთანი", "CH4", new Color32(150, 210, 250, 255), "ბუნებრივი აირი, რომლითაც ქურაზე საჭმელს ვამზადებთ.", 4, 1, 0, 0, 0, 0),
                F("NH3", "ამიაკი", "NH3", new Color32(160, 180, 250, 255), "მძაფრი სუნი აქვს; მისგან სასუქს ამზადებენ.", 3, 0, 1, 0, 0, 0),
                F("HCl", "წყალბადის ქლორიდი", "HCl", new Color32(170, 230, 160, 255), "წყალში გახსნილი მარილმჟავაა — კუჭშიც გვაქვს!", 1, 0, 0, 0, 1, 0),
                F("NaCl", "სუფრის მარილი", "NaCl", new Color32(245, 245, 245, 255), "ნატრიუმი ელექტრონს ქლორს აძლევს — იონური ბმა.", 0, 0, 0, 0, 1, 1),
                F("NaOH", "ნატრიუმის ტუტე", "NaOH", new Color32(210, 220, 240, 255), "ძლიერი ფუძე — საპონს მისგან ამზადებენ.", 1, 0, 0, 1, 0, 1),
                F("HCN", "ციანწყალბადი", "HCN", new Color32(200, 230, 200, 255), "ნუშის სუნი და ძალიან შხამიანი — მხოლოდ მოდელში ვაწყობთ!", 1, 1, 1, 0, 0, 0),
                F("CH3OH", "მეთანოლი", "CH3OH", new Color32(200, 220, 250, 255), "მარტივი სპირტი — საწვავად იყენებენ, დალევა არ შეიძლება!", 4, 1, 0, 1, 0, 0),
                F("C2H5OH", "ეთანოლი", "C2H5OH", new Color32(230, 230, 200, 255), "სპირტი, რომლითაც ხელებს ვიწმენდთ.", 6, 2, 0, 1, 0, 0),
                F("C6H12O6", "გლუკოზა", "C6H12O6", new Color32(250, 240, 200, 255), "შაქარი, რომელსაც მცენარეები მზის სინათლით ამზადებენ. დიდი მოლეკულაა!", 12, 6, 0, 6, 0, 0),
            },
            Judge = JudgeMolecule,
        };

        private static readonly int[] Bonds = { 1, 4, 3, 2, 1, 1 };
        private static KitResult JudgeMolecule(int[] v)
        {
            var find = Molecules.Match(v);
            if (find != null) return new KitResult { Find = find, Head = find.Title + "!", Why = find.Formula + " • " + find.Why };
            int atoms = v.Sum();
            if (atoms == 0) return new KitResult { Head = "ჯერ ატომები დაამატე.", Why = "შეეხე ატომებს მარცხნივ." };
            if (atoms == 1) return new KitResult { Head = "ერთი ატომი მოლეკულა არ არის.", Why = "მოლეკულას ორი ან მეტი ატომი სჭირდება." };
            int hands = v.Select((c, i) => c * Bonds[i]).Sum();
            return new KitResult { Boom = true, Head = "ატომები დაიფანტა!", Why = (hands % 2 == 1 ? "ერთ ატომს ბმა დარჩა თავისუფალი. " : "ასეთი მოლეკულა წიგნში არ არის. ") + Molecules.Hint };
        }

        // ------------------------------------------------------------ pH potions: acid, base and cabbage indicator

        public static readonly Kit Potions = new Kit
        {
            ID = "ph", Title = "pH ნაყენები", Station = StationType.AcidBaseStation, Ask = "დაამატე წვეთები", Action = "მორევა!",
            Hint = "კომბოსტოს წვენი ფერით გეუბნება pH-ს: მჟავაში წითლდება, ფუძეში მწვანდება.",
            Parts = new[] { P("acid", "მჟავის წვეთი", "H+", new Color32(240, 120, 110, 255), 8), P("base", "ფუძის წვეთი", "OH-", new Color32(110, 160, 240, 255), 8), P("cabbage", "კომბოსტოს წვენი", "ინ", new Color32(130, 70, 170, 255), 3) },
            Finds = new[]
            {
                F("red", "წითელი ნაყენი", "pH 1–2", new Color32(220, 40, 70, 255), "ძალიან მჟავე: ბევრი წყალბადის იონი."),
                F("pink", "ვარდისფერი ნაყენი", "pH 3–4", new Color32(235, 100, 150, 255), "მჟავე, როგორც ლიმონი ან ძმარი."),
                F("violet", "იისფერი ნაყენი", "pH 5–7", new Color32(140, 80, 180, 255), "თითქმის ნეიტრალური — წყლის მსგავსი."),
                F("blue", "ლურჯი ნაყენი", "pH 8", new Color32(80, 110, 220, 255), "ოდნავ ტუტე, როგორც საცხობი სოდა."),
                F("green", "მწვანე ნაყენი", "pH 9–11", new Color32(60, 170, 110, 255), "ტუტე: ბევრი ჰიდროქსიდის იონი."),
                F("yellow", "ყვითელი ნაყენი", "pH 12–14", new Color32(230, 210, 60, 255), "ძალიან ტუტე — ინდიკატორი სულ სხვა ფერს იღებს."),
            },
            Judge = JudgePotion,
        };

        // ------------------------------------------------------------ solution table: dissolve, saturate, heat

        public static readonly Kit Solutions = new Kit
        {
            ID = "solution", Title = "ხსნარების ქვაბი", Station = StationType.SolutionStation, Ask = "მოამზადე ხსნარი", Action = "მორევა!",
            Hint = "ერთ ჭიქა ცივ წყალში 2 კოვზი მარილი, 4 შაქარი ან 1 სპილენძის სულფატი იხსნება. სითბო მეტს ხსნის — ან წყალს აორთქლებს!",
            Parts = new[] { P("water", "წყლის ჭიქა", "H2O", new Color32(120, 190, 240, 255), 4), P("salt", "მარილის კოვზი", "NaCl", new Color32(245, 245, 245, 255), 10),
                P("sugar", "შაქრის კოვზი", "შაქ", new Color32(250, 236, 200, 255), 12), P("cuso4", "სპილენძის სულფატი", "Cu", new Color32(60, 140, 220, 255), 6), P("heat", "სანთურა", "°C", new Color32(240, 140, 60, 255), 2) },
            Finds = new[]
            {
                F("saltwater", "მარილიანი წყალი", "NaCl + H2O", new Color32(200, 225, 240, 255), "მარილი გაქრა თვალიდან, მაგრამ წყალში დარჩა — დააგემოვნე და გაიგებ."),
                F("saturated", "ნაჯერი ხსნარი", "ზედმეტი მარილი", new Color32(225, 232, 238, 255), "წყალმა მეტი ვეღარ გახსნა: ზედმეტი მარილი ფსკერზე დაილექა."),
                F("saltcrystals", "მარილის კრისტალები", "აორთქლება", new Color32(240, 240, 250, 255), "სითბომ წყალი აორთქლა და მარილი კუბურ კრისტალებად დარჩა — ასე იღებენ ზღვის მარილს."),
                F("sweet", "ტკბილი წყალი", "შაქარი + H2O", new Color32(240, 236, 220, 255), "შაქარი წყალში ადვილად იხსნება."),
                F("syrup", "სიროფი", "ბევრი შაქარი", new Color32(235, 200, 120, 255), "ცხელ წყალში შაქარი ბევრად მეტი იხსნება — ასე კეთდება სიროფი."),
                F("caramel", "კარამელი", "შაქარი + სიცხე", new Color32(190, 110, 40, 255), "წყლის გარეშე გაცხელებული შაქარი იშლება და ყავისფერ კარამელად იქცევა. ეს უკვე ქიმიური ცვლილებაა!"),
                F("lightblue", "ცისფერი ხსნარი", "განზავებული CuSO4", new Color32(150, 200, 240, 255), "ცოტა ნივთიერება ბევრ წყალში — განზავებული ხსნარი ღია ფერისაა."),
                F("deepblue", "მუქი ლურჯი ხსნარი", "კონცენტრირებული CuSO4", new Color32(30, 90, 200, 255), "ბევრი ნივთიერება ცოტა წყალში — კონცენტრირებული ხსნარი მუქია."),
                F("bluecrystals", "ლურჯი კრისტალები", "CuSO4 · 5H2O", new Color32(40, 110, 230, 255), "ცივ წყალს ამდენი ვერ ეტევა: ზედმეტი სპილენძის სულფატი ლამაზ ლურჯ კრისტალებად გამოიყო."),
                F("steam", "ორთქლი", "H2O (აირი)", new Color32(230, 236, 240, 255), "100 °C-ზე წყალი დუღს და ორთქლად იქცევა — მატერიის სხვა მდგომარეობა."),
                F("mixture", "ფერადი ნარევი", "რამდენიმე ნივთიერება", new Color32(170, 200, 200, 255), "ერთ წყალში რამდენიმე ნივთიერებაც იხსნება — ეს ნარევია."),
            },
            Judge = JudgeSolution,
        };

        private static KitResult Solution(string id) { var f = Solutions.Finds.First(x => x.ID == id); return new KitResult { Find = f, Head = f.Title + "!", Why = f.Why }; }
        /// <summary>How much of each solute one cup of water holds: salt 2, sugar 4 (8 when heated), copper sulfate 1 (2 when heated).</summary>
        public static int Capacity(int part, int water, int heat) => water * (part == 1 ? 2 : part == 2 ? (heat > 0 ? 8 : 4) : heat > 0 ? 2 : 1);
        private static KitResult JudgeSolution(int[] v)
        {
            int w = v[0], s = v[1], g = v[2], c = v[3], h = v[4], solutes = (s > 0 ? 1 : 0) + (g > 0 ? 1 : 0) + (c > 0 ? 1 : 0);
            if (v.Sum() == 0) return new KitResult { Head = "ქვაბი ცარიელია.", Why = "დაამატე წყალი და რამე გასახსნელი." };
            if (w == 0)
            {
                if (g > 0 && s == 0 && c == 0 && h == 2) return Solution("caramel");
                if (h > 0) return new KitResult { Boom = true, Head = "ფუ! მშრალი ფხვნილი დაიწვა!", Why = "ბოლი ავიდა. წყალი დაამატე — ან სცადე მხოლოდ შაქრის ძლიერ გაცხელება." };
                return new KitResult { Head = "მშრალი ფხვნილია.", Why = "გასახსნელად წყალი სჭირდება." };
            }
            if (solutes == 0) return h == 2 ? Solution("steam") : new KitResult { Head = "უბრალოდ წყალია.", Why = h == 1 ? "თბილია, მაგრამ ჯერ არ დუღს — კიდევ გაათბე." : "დაამატე მარილი, შაქარი ან სპილენძის სულფატი." };
            if (solutes > 1) return Solution("mixture");
            if (s > 0)
            {
                if (h == 2) return Solution("saltcrystals");
                return s <= Capacity(1, w, h) ? Solution("saltwater") : Solution("saturated");
            }
            if (g > 0)
            {
                if (g > Capacity(2, w, h)) return new KitResult { Head = "შაქარი ფსკერზე დარჩა.", Why = "ცივ წყალს ამდენი ვერ ეტევა — გაათბე, ან წყალი დაამატე." };
                return g >= 3 * w ? Solution("syrup") : Solution("sweet");
            }
            if (c > Capacity(3, w, h)) return h == 0 ? Solution("bluecrystals") : new KitResult { Head = "ლურჯი ფხვნილი ფსკერზეა.", Why = "ცხელ წყალსაც აქვს ზღვარი. დაამატე წყალი." };
            return 2 * c <= w ? Solution("lightblue") : Solution("deepblue");
        }

        /// <summary>The potion's pH from its drops (strong acid and base, 0.1 mol/L, 20 mL of water).</summary>
        public static double PotionPH(int[] v) => ChemistrySandbox.PH(v[0], v[1], 20);
        private static KitResult JudgePotion(int[] v)
        {
            if (v[2] == 0) return new KitResult { Head = "უფერო სითხეა.", Why = "დაამატე კომბოსტოს წვენი — მის გარეშე pH-ს ვერ დაინახავ." };
            double ph = PotionPH(v);
            int band = ph < 2.5 ? 0 : ph < 4.5 ? 1 : ph < 7.5 ? 2 : ph < 8.5 ? 3 : ph < 11.5 ? 4 : 5;
            var find = Potions.Finds[band];
            return new KitResult { Find = find, Head = find.Title + "!", Why = "pH ≈ " + ph.ToString("0.0") + " • " + find.Why };
        }
    }
}
