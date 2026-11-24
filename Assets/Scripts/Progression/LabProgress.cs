using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace ChemistryGame
{
    public enum CollectionKind { Mineral, Sample, Photo, Reaction, Fact, Badge, FurnitureSet, Seasonal, Award }

    public sealed class CollectionItem
    {
        public readonly string ID, Name, Description;
        public readonly CollectionKind Kind;
        public readonly Color Color;
        public CollectionItem(CollectionKind kind, string id, string name, string description, Color color)
        { Kind = kind; ID = id; Name = name; Description = description; Color = color; }
    }

    public sealed class MaterialDef
    {
        public readonly string ID, Name, Source;
        public readonly int Price;
        public readonly Color Color;
        public MaterialDef(string id, string name, string source, int price, Color color)
        { ID = id; Name = name; Source = source; Price = price; Color = color; }
    }

    /// <summary>
    /// Reputation, mastery, materials, statistics and collections. Every value lives in the
    /// regular JSON save, so all of it continues across play sessions.
    /// </summary>
    public static class LabProgress
    {
        public static GameSaveData Data => GameSaveManager.Instance.Data;
        public static string Today => SchoolCalendar.DayKey;

        // ---------- generic counters ----------
        public static int Get(List<KeyValue> list, string key) => list.FirstOrDefault(k => k.key == key)?.value ?? 0;
        public static void Add(List<KeyValue> list, string key, int delta)
        {
            var entry = list.FirstOrDefault(k => k.key == key);
            if (entry == null) { entry = new KeyValue { key = key }; list.Add(entry); }
            entry.value = Mathf.Max(0, entry.value + delta);
        }
        public static int Stat(string key) => Get(Data.stats, key);
        public static void CountStat(string key, int delta = 1) => Add(Data.stats, key, delta);

        // ---------- reputation ----------
        public static readonly int[] ReputationSteps = { 0, 30, 80, 160, 280, 450 };
        public static readonly string[] ReputationTitles =
            { "დამწყები ლაბორანტი", "ცნობისმოყვარე მკვლევარი", "სანდო ქიმიკოსი", "ლაბორატორიის ვარსკვლავი", "ახალგაზრდა მეცნიერი", "სკოლის ლეგენდა" };
        public static int ReputationLevel
        {
            get { int level = 0; for (int i = 0; i < ReputationSteps.Length; i++) if (Data.reputation >= ReputationSteps[i]) level = i; return level; }
        }
        public static string ReputationTitle => ReputationTitles[ReputationLevel];
        public static float ReputationProgress
        {
            get
            {
                int level = ReputationLevel;
                if (level >= ReputationSteps.Length - 1) return 1;
                return Mathf.InverseLerp(ReputationSteps[level], ReputationSteps[level + 1], Data.reputation);
            }
        }
        public static void AddReputation(int amount)
        {
            int before = ReputationLevel;
            Data.reputation += Mathf.RoundToInt(amount * (SchoolCalendar.IsExhibitionDay ? 2 : 1));
            if (ReputationLevel > before)
            {
                Collect("badge:rep" + ReputationLevel);
                AudioManager.Instance?.Milestone();
                LabGame.Instance?.HUD?.Notify("ლაბორატორიის რეპუტაცია გაიზარდა! ახალი წოდება: " + ReputationTitle + ". ახალი შესაძლებლობები გაიხსნა.");
            }
        }

        // ---------- mastery ----------
        public static readonly int[] MasterySteps = { 0, 3, 8, 15, 25 };
        public static int MasteryValue(string key) => Get(Data.mastery, key);
        public static int MasteryLevel(string key)
        {
            int value = MasteryValue(key), level = 0;
            for (int i = 0; i < MasterySteps.Length; i++) if (value >= MasterySteps[i]) level = i;
            return level;
        }
        public static string Stars(int level) => "დონე " + level + "/4";
        public static void AddMastery(string key, int amount = 1)
        {
            int before = MasteryLevel(key);
            Add(Data.mastery, key, amount);
            if (MasteryLevel(key) > before && MasteryLevel(key) >= 4) Collect("badge:master");
        }

        // ---------- materials ----------
        public static readonly MaterialDef[] Materials =
        {
            new MaterialDef("cabbage", "წითელი კომბოსტო", "სათბური", 0, new Color32(150, 84, 170, 255)),
            new MaterialDef("lemon", "ლიმონი", "სათბური", 0, new Color32(239, 214, 92, 255)),
            new MaterialDef("flowers", "ყვავილის ფურცლები", "სათბური", 0, new Color32(234, 140, 170, 255)),
            new MaterialDef("citrus", "ციტრუსის ქერქი", "სათბური", 0, new Color32(240, 160, 70, 255)),
            new MaterialDef("rainwater", "წვიმის წყალი", "სათბურის კასრი, წვიმიან დღეს", 0, new Color32(130, 180, 220, 255)),
            new MaterialDef("vinegar", "ძმარი", "სათავსო", 6, new Color32(222, 220, 190, 255)),
            new MaterialDef("soda", "საჭმლის სოდა", "სათავსო", 6, new Color32(240, 240, 240, 255)),
            new MaterialDef("salt", "სუფრის მარილი", "სათავსო", 5, new Color32(230, 236, 240, 255)),
            new MaterialDef("alum", "შაბი", "სათავსო", 8, new Color32(210, 220, 235, 255)),
            new MaterialDef("tonic", "ტონიკი", "სათავსო", 8, new Color32(200, 225, 240, 255)),
            new MaterialDef("zinc", "თუთიის ფირფიტა", "სათავსო", 9, new Color32(170, 176, 186, 255)),
            new MaterialDef("copper", "სპილენძის ფირფიტა", "სათავსო", 9, new Color32(201, 118, 72, 255)),
            new MaterialDef("wire", "სადენი", "სათავსო", 5, new Color32(210, 90, 80, 255)),
            new MaterialDef("tapwater", "ონკანის წყალი", "სათავსო", 3, new Color32(160, 200, 225, 255)),
            new MaterialDef("chalk", "ცარცი", "სათავსო", 4, new Color32(236, 232, 222, 255)),
            new MaterialDef("sugar", "შაქარი", "სათავსო", 3, new Color32(252, 246, 232, 255)),
            new MaterialDef("hcl", "მარილმჟავა (განზავებული)", "სათავსო", 9, new Color32(220, 236, 226, 255)),
            new MaterialDef("naoh", "ნატრიუმის ტუტე (განზავებული)", "სათავსო", 9, new Color32(214, 226, 240, 255)),
            new MaterialDef("nail", "რკინის ლურსმნები", "სათავსო", 4, new Color32(120, 124, 132, 255)),
            new MaterialDef("cuso4", "სპილენძის სულფატი", "სათავსო", 10, new Color32(60, 140, 220, 255)),
            new MaterialDef("peroxide", "წყალბადის ზეჟანგი", "სათავსო", 7, new Color32(230, 240, 246, 255)),
            new MaterialDef("yeast", "საფუარი", "სათავსო", 4, new Color32(214, 186, 140, 255)),
            new MaterialDef("sample", "უცნობი ნიმუში", "ლაბორანტი ქეთევანი", 12, new Color32(190, 170, 140, 255)),
        };
        public static MaterialDef Material(string id) => Materials.FirstOrDefault(m => m.ID == id);
        public static int Has(string material) => Get(Data.materials, material);
        public static void Give(string material, int count = 1)
        {
            Add(Data.materials, material, count);
            if (material == "rainwater") Collect("sample:rain");
            if (material == "tapwater") Collect("sample:tap");
            if (material == "cabbage") Collect("sample:cabbage");
            if (material == "sample") Collect("sample:unknown");
        }
        public static bool Take(string material, int count)
        {
            if (Has(material) < count) return false;
            Add(Data.materials, material, -count);
            return true;
        }

        // ---------- collections ----------
        public static readonly CollectionItem[] Catalog = BuildCatalog();
        private static CollectionItem[] BuildCatalog()
        {
            var list = new List<CollectionItem>();
            void M(string id, string name, string text, Color32 c) => list.Add(new CollectionItem(CollectionKind.Mineral, "mineral:" + id, name, text, c));
            M("quartz", "კვარცი", "SiO2 • ერთ-ერთი ყველაზე გავრცელებული მინერალი დედამიწის ქერქში.", new Color32(232, 232, 240, 255));
            M("pyrite", "პირიტი", "FeS2 • ოქროსფერი ბზინვარების გამო \"სულელების ოქროს\" ეძახიან.", new Color32(214, 186, 92, 255));
            M("halite", "ჰალიტი", "NaCl • ბუნებრივი მარილი, კუბური კრისტალებით.", new Color32(240, 222, 222, 255));
            M("fluorite", "ფლუორიტი", "CaF2 • ზოგი ნიმუში ულტრაიისფერ შუქზე ანათებს.", new Color32(150, 110, 210, 255));
            M("amethyst", "ამეთვისტო", "SiO2 • იისფერი კვარცი; ფერს რკინის მინარევები და ბუნებრივი რადიაცია აძლევს.", new Color32(160, 96, 200, 255));
            M("malachite", "მალაქიტი", "Cu2CO3(OH)2 • მწვანე სპილენძის მინერალი ზოლიანი ნახატით.", new Color32(70, 170, 110, 255));
            M("gypsum", "თაბაშირი", "CaSO4·2H2O • იმდენად რბილია, რომ ფრჩხილითაც იკაწრება.", new Color32(236, 230, 214, 255));
            M("calcite", "კალციტი", "CaCO3 • ძმარზე შიშინებს, რადგან ნახშირორჟანგი გამოიყოფა.", new Color32(242, 210, 170, 255));
            void S(string id, string name, string text, Color32 c) => list.Add(new CollectionItem(CollectionKind.Sample, "sample:" + id, name, text, c));
            S("rain", "წვიმის წყალი", "ოდნავ მჟავეა (pH ≈ 5,6), რადგან ჰაერის ნახშირორჟანგი იხსნება.", new Color32(130, 180, 220, 255));
            S("tap", "ონკანის წყალი", "შეიცავს გახსნილ მინერალებს; სიხისტე რეგიონის მიხედვით იცვლება.", new Color32(160, 200, 225, 255));
            S("cabbage", "კომბოსტოს ინდიკატორი", "ანთოციანები მჟავაში წითლდება, ფუძეში მწვანდება.", new Color32(150, 84, 170, 255));
            S("unknown", "უცნობი ნიმუში", "ქეთევანის საიდუმლო ფხვნილი. ტესტები გაგიმხელს, რა არის.", new Color32(190, 170, 140, 255));
            S("crystal", "შაბის კრისტალი", "ნელა გაციებულ ხსნარში დიდი და სწორი კრისტალები იზრდება.", new Color32(215, 228, 245, 255));
            S("perfume", "ციტრუსის არომატი", "ეთერზეთები წყალზე მსუბუქია და მასში ცუდად იხსნება.", new Color32(245, 180, 90, 255));
            void F(string id, string name, string text) => list.Add(new CollectionItem(CollectionKind.Fact, "fact:" + id, name, text, LabUI.Mint));
            F("water", "წყლის მოლეკულა", "წყლის მოლეკულა კუთხოვანია (≈104,5°), ამიტომ პოლარულია და ბევრ ნივთიერებას ხსნის.");
            F("table", "პერიოდული ცხრილი", "მენდელეევმა 1869 წელს ცხრილში ცარიელი ადგილები დატოვა და ჯერ უცნობი ელემენტები იწინასწარმეტყველა.");
            F("salt", "მარილის კრისტალი", "სუფრის მარილის კრისტალში ნატრიუმისა და ქლორის იონები კუბურ ბადეშია დალაგებული.");
            F("rust", "ჟანგი", "რკინის დაჟანგვას წყალი და ჟანგბადი ერთად სჭირდება.");
            F("ph", "pH შკალა", "pH-ის ერთი ერთეულით შეცვლა წყალბადის იონების კონცენტრაციის ათჯერ ცვლილებას ნიშნავს.");
            F("gold", "ოქრო", "ოქრო ძალიან უმოქმედოა, ამიტომ ათასობით წლის განმავლობაში ბზინვარებას ინარჩუნებს.");
            F("diamond", "ალმასი და გრაფიტი", "ალმასიც და გრაფიტიც სუფთა ნახშირბადია; განსხვავდება მხოლოდ ატომების განლაგება.");
            F("helium", "ჰელიუმი", "ჰელიუმი პირველად მზის სპექტრში აღმოაჩინეს და მერე დედამიწაზე.");
            F("battery", "პირველი ბატარეა", "ალესანდრო ვოლტამ 1800 წელს თუთიისა და სპილენძის დისკებით პირველი ბატარეა ააწყო.");
            F("ice", "ყინული", "ყინული წყალზე მსუბუქია, ამიტომ ტბები ზემოდან იყინება და ქვემოთ სიცოცხლე გრძელდება.");
            F("oxygen", "ჟანგბადი", "ჰაერის დაახლოებით 21% ჟანგბადია, 78% კი აზოტი.");
            F("plants", "ფოტოსინთეზი", "მცენარეები ნახშირორჟანგსა და წყალს სინათლის ენერგიით გლუკოზად და ჟანგბადად აქცევს.");
            F("colour", "ფერის ქიმია", "ფოიერვერკის ფერს ლითონები იძლევა: სტრონციუმი წითელს, ნატრიუმი ყვითელს, სპილენძი მწვანე-ცისფერს.");
            F("catalyst", "კატალიზატორი", "კატალიზატორი რეაქციას აჩქარებს, მაგრამ თავად არ იხარჯება.");
            F("smell", "სუნი", "სუნს მოლეკულები ქმნის, რომლებიც ჰაერში ორთქლდება და ცხვირის რეცეპტორებს ეხება.");
            F("glow", "ფლუორესცენცია", "ტონიკის ქინინი ულტრაიისფერ შუქს შთანთქავს და ცისფრად ანათებს.");
            void B(string id, string name, string text) => list.Add(new CollectionItem(CollectionKind.Badge, "badge:" + id, name, text, LabUI.Gold));
            B("rep1", "ცნობისმოყვარე", "მიაღწიე რეპუტაციის მე-2 დონეს.");
            B("rep2", "სანდო", "მიაღწიე რეპუტაციის მე-3 დონეს.");
            B("rep3", "ვარსკვლავი", "მიაღწიე რეპუტაციის მე-4 დონეს.");
            B("rep4", "მეცნიერი", "მიაღწიე რეპუტაციის მე-5 დონეს.");
            B("rep5", "ლეგენდა", "მიაღწიე რეპუტაციის უმაღლეს დონეს.");
            B("first_project", "პირველი კვლევა", "დაასრულე პირველი კვლევითი პროექტი.");
            B("all_projects", "კვლევების ოსტატი", "დაასრულე ყველა კვლევითი პროექტი.");
            B("master", "ოსტატი", "ნებისმიერ მიმართულებაში მიაღწიე ოსტატობის მაქსიმუმს.");
            B("friend", "კარგი მეგობარი", "ერთ-ერთ კლასელთან გახდი საუკეთესო მეგობარი.");
            B("helper", "დამხმარე", "შეასრულე კლასელების 5 თხოვნა.");
            B("tidy", "მოწესრიგებული", "მიიღე სუფთა ლაბორატორიის შეფასება ინსპექციაზე.");
            B("gardener", "მებაღე", "მორწყე მცენარეები 7 სხვადასხვა დღეს.");
            B("cleaner", "დამლაგებელი", "გაწმინდე 10 უვნებელი ავარია.");
            B("collector", "კოლექციონერი", "შეაგროვე ყველა მინერალი.");
            foreach (var chapter in Curriculum.Chapters) B(chapter.ID, chapter.Badge, "დაასრულე თავი „" + chapter.Title + "“.");
            void A(string id, string name, string text) => list.Add(new CollectionItem(CollectionKind.Award, "award:" + id, name, text, LabUI.Coral));
            A("gold", "ოქროს მედალი", "სამეცნიერო ბაზრობის პირველი ადგილი.");
            A("silver", "ვერცხლის მედალი", "სამეცნიერო ბაზრობის მეორე ადგილი.");
            A("bronze", "ბრინჯაოს მედალი", "სამეცნიერო ბაზრობის მესამე ადგილი.");
            A("ribbon", "გამოფენის ლენტი", "პროექტი გამოფენის დღეს წარადგინე.");
            A("competition", "კლასის ჩემპიონი", "გაიმარჯვე კლასის შეჯიბრში.");
            A("exam", "ექსპერტის თასი", "რთულ გამოცდაზე მიიღე 10-დან 8 ან მეტი ქულა.");
            void Z(string id, string name, string text) => list.Add(new CollectionItem(CollectionKind.Seasonal, "seasonal:" + id, name, text, LabUI.Paper));
            Z("winter", "ფიფქის კრისტალი", "ზამთრის სუვენირი: თითოეულ ფიფქს ექვსი სხივი აქვს.");
            Z("spring", "გაზაფხულის ყვავილი", "გაზაფხულის სუვენირი: ყვავილის ფერს pH-იც ცვლის.");
            Z("summer", "მზის კრისტალი", "ზაფხულის სუვენირი: მზე წყალს აორთქლებს და მარილი რჩება.");
            Z("autumn", "შემოდგომის ფოთოლი", "შემოდგომის სუვენირი: ქლოროფილი იშლება და ყვითელი პიგმენტები ჩანს.");
            void Q(string id, string name, string text) => list.Add(new CollectionItem(CollectionKind.FurnitureSet, "set:" + id, name, text, LabUI.Green));
            Q("nature", "ბუნების კუთხე", "დადგი ოთახის მცენარე, ფიკუსი და აკვარიუმი.");
            Q("study", "მკვლევრის კაბინეტი", "დადგი კვლევების დაფა, ნიმუშების თარო და ფოტოდაფა.");
            Q("cozy", "მყუდრო საღამო", "დადგი სავარძელი, ლამპა და რადიო.");
            Q("museum", "პატარა მუზეუმი", "დადგი ვიტრინა, ჯილდოების თარო და მინერალების ყუთი.");
            foreach (var mix in Mixing.Results) list.Add(new CollectionItem(CollectionKind.Reaction, mix.ID, mix.Title, mix.Note, mix.Color));
            foreach (var project in ResearchProjects.All)
                list.Add(new CollectionItem(CollectionKind.Photo, "photo:" + project.ID, project.Title, "ფოტო ცდის დასრულების მომენტიდან.", project.Color));
            return list.ToArray();
        }
        public static IEnumerable<CollectionItem> Items(CollectionKind kind) => Catalog.Where(c => c.Kind == kind);
        public static bool Owns(string id) => Data.collection.Contains(id);
        public static bool Collect(string id)
        {
            if (string.IsNullOrEmpty(id) || Data.collection.Contains(id)) return false;
            Data.collection.Add(id);
            var item = Catalog.FirstOrDefault(c => c.ID == id);
            if (item != null && item.Kind != CollectionKind.Reaction && LabGame.Instance != null && LabGame.Instance.HUD != null)
            {
                LabGame.Instance.HUD.Notify("კოლექციაში დაემატა: " + item.Name);
                if (item.Kind == CollectionKind.Badge || item.Kind == CollectionKind.Award) UIArt.BadgeUnlocked(item.Name, item.Color);
            }
            if (Items(CollectionKind.Mineral).All(m => Data.collection.Contains(m.ID))) Collect("badge:collector");
            return true;
        }

        public static void OnReactionSolved(ChemistryQuestionSO question)
        {
            if (question == null) return;
            AddMastery("category:" + question.reactionCategory);
            if (!string.IsNullOrEmpty(question.questionID)) Collect("reaction:" + question.questionID);
        }

        /// <summary>Checks furniture sets against the placed decorations.</summary>
        public static void CheckFurnitureSets()
        {
            var placed = new HashSet<string>(Data.labPlacements.Select(p => p.itemID));
            if (placed.Contains("plant") && placed.Contains("ficus") && placed.Contains("aquarium")) Collect("set:nature");
            if (placed.Contains("research") && placed.Contains("samples") && placed.Contains("photos")) Collect("set:study");
            if (placed.Contains("sofa") && placed.Contains("lamp") && placed.Contains("radio")) Collect("set:cozy");
            if (placed.Contains("display") && placed.Contains("trophy") && placed.Contains("mineral_case")) Collect("set:museum");
        }
    }

    public enum DayPart { Morning, Afternoon, Evening }
    public enum Season { Winter, Spring, Summer, Autumn }

    /// <summary>The school calendar: time of day, weather, seasons, themes and recurring events.</summary>
    public static class SchoolCalendar
    {
        /// <summary>Tests (and the atmosphere preview) can pin the clock.</summary>
        public static DateTime? Override;
        public static DateTime Now => Override ?? DateTime.Now;
        private static DateTime keyDate;
        private static string key;
        /// <summary>Today as "yyyy-MM-dd", formatted once per day instead of on every call (it is read many times a frame).</summary>
        public static string DayKey
        {
            get
            {
                var date = Now.Date;
                if (key == null || date != keyDate) { keyDate = date; key = GameSaveManager.Iso(date); }
                return key;
            }
        }

        public static DayPart Part => Now.Hour >= 6 && Now.Hour < 12 ? DayPart.Morning : Now.Hour >= 12 && Now.Hour < 18 ? DayPart.Afternoon : DayPart.Evening;
        public static string PartName => Part == DayPart.Morning ? "დილა" : Part == DayPart.Afternoon ? "შუადღე" : "საღამო";
        public static Season Season => Now.Month == 12 || Now.Month <= 2 ? Season.Winter : Now.Month <= 5 ? Season.Spring : Now.Month <= 8 ? Season.Summer : Season.Autumn;
        public static string SeasonName => new[] { "ზამთარი", "გაზაფხული", "ზაფხული", "შემოდგომა" }[(int)Season];
        public static string SeasonID => new[] { "winter", "spring", "summer", "autumn" }[(int)Season];

        public static bool Raining
        {
            get
            {
                var data = GameSaveManager.Instance.Data;
                string day = DayKey;
                if (data.weatherDay != day)
                {
                    var random = new System.Random(day.GetHashCode() * 31 + 7);
                    data.weatherDay = day;
                    data.weather = random.NextDouble() < 0.4 ? "rain" : "clear";
                }
                return data.weather == "rain";
            }
        }
        public static string WeatherName => Raining ? "წვიმს" : Season == Season.Winter ? "ცივა" : "მოწმენდილია";

        /// <summary>Stores the current part of day and weather in the save when they change.</summary>
        public static bool Refresh()
        {
            var data = GameSaveManager.Instance.Data;
            string part = Part.ToString();
            string weatherBefore = data.weather;
            bool rain = Raining;
            if (data.timeOfDay == part && weatherBefore == data.weather) return false;
            data.timeOfDay = part;
            return true;
        }

        public static readonly string[] Themes =
        {
            "კრისტალების თვე", "ფერების თვე", "წყლის თვე", "ელექტროობის თვე", "მცენარეების ქიმია", "მინერალების თვე",
            "მჟავები და ფუძეები", "არომატების თვე", "სინათლის ქიმია", "უსაფრთხოების თვე", "რეაქციების სიჩქარე", "საიდუმლო ნიმუშები"
        };
        public static readonly string[] ThemeProjects = { "crystals", "colour", "water", "battery", "perfume", "unknown", "indicator", "perfume", "indicator", "unknown", "colour", "unknown" };
        public static string Theme => Themes[(Now.Month - 1) % 12];
        public static string ThemeProject => ThemeProjects[(Now.Month - 1) % 12];

        public static string Week => GameSaveManager.Iso(GameSaveManager.WeekStart(Now.Date));
        public static bool IsWeekend => Now.DayOfWeek == DayOfWeek.Saturday || Now.DayOfWeek == DayOfWeek.Sunday;
        public static bool IsInspectionDay => Now.DayOfWeek == DayOfWeek.Wednesday;
        public static bool IsCompetitionDay => Now.DayOfWeek == DayOfWeek.Tuesday;
        public static bool ResearcherVisiting => Now.DayOfWeek == DayOfWeek.Thursday || Now.DayOfWeek == DayOfWeek.Friday;
        public static bool IsExhibitionDay => Now.Day == 15;
        public static bool IsFairJudging => Now.DayOfWeek == DayOfWeek.Sunday;

        public struct SchoolEvent { public string Title, Text; public Color Color; }
        public static List<SchoolEvent> TodayEvents()
        {
            var list = new List<SchoolEvent>();
            void Add(string title, string text, Color color) => list.Add(new SchoolEvent { Title = title, Text = text, Color = color });
            Add("სამეცნიერო ბაზრობა", "ყოველკვირეული ბაზრობა. დასრულებული პროექტი გამოფენის დარბაზში წარადგინე. შედეგები კვირის ბოლოს ცხადდება.", LabUI.Gold);
            Add("თვის თემა: " + Theme, "ამ თვეში თემატური პროექტი ორმაგ ოსტატობასა და დამატებით რეპუტაციას იძლევა.", LabUI.Mint);
            Add("სეზონი: " + SeasonName, "სკოლა სეზონურადაა მორთული. დაფასთან სეზონური სუვენირი გელოდება.", new Color32(170, 110, 60, 255));
            if (IsInspectionDay) Add("მასწავლებლის ინსპექცია", "დღეს მასწავლებელი ლაბორატორიების სისუფთავეს ამოწმებს. დაასუფთავე ავარიები და მიდი მასთან.", LabUI.Coral);
            if (IsCompetitionDay) Add("კლასის შეჯიბრი", "დღეს ამოხსენი 5 რეაქცია და კლასის ჩემპიონის ჯილდო მიიღე.", LabUI.Coral);
            if (ResearcherVisiting) Add("მოგზაური მკვლევარი", "პროფესორი ლევანი გამოფენის დარბაზშია და იშვიათ მინერალებს ყიდის.", LabUI.Mint);
            if (IsExhibitionDay) Add("გამოფენის დღე", "დღეს მიღებული რეპუტაცია ორმაგდება!", LabUI.Gold);
            if (IsWeekend) Add("შაბათ-კვირის ექსპერიმენტი", "სათბურში დღეს მოსავალი ორმაგია, ცდები კი ორმაგ ოსტატობას იძლევა.", LabUI.Green);
            if (Raining) Add("წვიმიანი დღე", "სათბურის კასრში წვიმის წყალი გროვდება. წყლის ხარისხის კვლევისთვის გამოგადგება.", new Color32(130, 180, 220, 255));
            return list;
        }

        private static readonly string[] Months = { "იანვარი", "თებერვალი", "მარტი", "აპრილი", "მაისი", "ივნისი", "ივლისი", "აგვისტო", "სექტემბერი", "ოქტომბერი", "ნოემბერი", "დეკემბერი" };
        private static readonly string[] Days = { "კვირა", "ორშაბათი", "სამშაბათი", "ოთხშაბათი", "ხუთშაბათი", "პარასკევი", "შაბათი" };
        public static string DateText => Days[(int)Now.DayOfWeek] + ", " + Now.Day + " " + Months[Now.Month - 1];
    }
}
