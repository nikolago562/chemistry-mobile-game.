using System;
using System.Collections.Generic;
using System.Linq;

namespace ChemistryGame
{
    /// <summary>How a lesson's hands-on part works and how its stage is drawn.</summary>
    public enum PracticeKind { Sort, Atom, Molecule, Equation, Element, Ions, Pairs, Beaker, Drops, Scale, Review }

    public sealed class SortItem { public string Label, Why; public int Bin; }

    /// <summary>One counter-building exercise: the teacher's demo, the guided try or the free try.</summary>
    public sealed class Practice
    {
        public string Goal, Done = "";
        public string[] Counters;
        public int[] Solution, Start, Min, Max;
        /// <summary>Ion charges (Ions), the equation "A + B -> C" (Equation) or the atom symbol (Pairs).</summary>
        public int[] Charges;
        public string Formula = "";
        public float Target;
        /// <summary>Null when solved, otherwise a short note naming what is still wrong.</summary>
        public Func<int[], string> Check;
        /// <summary>Open goals: names what the player actually built (null = use Done).</summary>
        public Func<int[], string> Named;
    }

    public sealed class LessonQuestion
    {
        public string Prompt, Hint;
        /// <summary>Index 0 is the right answer; the UI shuffles the order.</summary>
        public string[] Choices, Why;
    }

    public sealed class Lesson
    {
        public string ID, Title, Why, Note = "", Hint = "", Collect = "";
        public int Chapter, Coins = 10;
        public string[] Teach = new string[0], Elements = new string[0], Bins = new string[0];
        public PracticeKind Kind;
        public SortItem[] Items = new SortItem[0];
        public Practice[] Steps = new Practice[0];
        public LessonQuestion[] Questions = new LessonQuestion[0];
        public bool IsReview => Kind == PracticeKind.Review;
    }

    public sealed class Chapter
    {
        public string ID, Title, Badge, Rank, StationName, Cosmetic = "", LabItem = "";
        public StationType Station;
        public int Coins = 25;
    }

    [Serializable] public sealed class LessonRecord { public string id = ""; public int best; public int attempts; public bool done; }

    /// <summary>
    /// The beginner theory path: eight chapters taught in order before the advanced challenge
    /// floor opens. Everything the player learns is derived from the saved lesson records, so
    /// the current lesson, unlocked stations and tiers all survive a reload.
    /// </summary>
    public static class Curriculum
    {
        public const string LockedText = "ეს დავალება ჯერ არ გახსნილა. ჯერ დაასრულე საჭირო თეორიის გაკვეთილები.";
        public const int TierGoal = 8;
        public static readonly string[] PathNames =
            { "ქიმიის დასაწყისი", "ატომები", "ელემენტები", "მოლეკულები", "ბმები", "რეაქციები", "ხსნარები", "მჟავები და ფუძეები", "რთული გამოწვევები" };
        public static readonly string[] StepNames = { "ახსნა", "ჩვენება", "ვარჯიში მინიშნებით", "დამოუკიდებლად", "ტესტი", "ჯილდო" };

        public static readonly Chapter[] Chapters =
        {
            new Chapter { ID = "ch1", Title = "ქიმიის დასაწყისი", Badge = "მატერიის მკვლევარი", Rank = "მატერიის მკვლევარი", Station = StationType.TutorialStation, StationName = "მასწავლებლის მაგიდა", Cosmetic = "hat_alchemist", LabItem = "poster_safety" },
            new Chapter { ID = "ch2", Title = "ატომები", Badge = "ატომის მცოდნე", Rank = "ატომების მცოდნე", Station = StationType.AtomTable, StationName = "ატომების მაგიდა", Cosmetic = "goggles_atom", LabItem = "atom" },
            new Chapter { ID = "ch3", Title = "ელემენტები", Badge = "ცხრილის მკვლევარი", Rank = "ცხრილის მკვლევარი", Station = StationType.ElementStation, StationName = "ელემენტების სადგური", Cosmetic = "coat_periodic", LabItem = "poster_table" },
            new Chapter { ID = "ch4", Title = "მოლეკულები", Badge = "მოლეკულების მშენებელი", Rank = "მოლეკულების მშენებელი", Station = StationType.MoleculeTable, StationName = "მოლეკულების მაგიდა", LabItem = "molecule" },
            new Chapter { ID = "ch5", Title = "ბმები", Badge = "ბმების ოსტატი", Rank = "ბმების ოსტატი", Station = StationType.MoleculeTable, StationName = "მოლეკულების მაგიდა", LabItem = "plasma" },
            new Chapter { ID = "ch6", Title = "რეაქციები", Badge = "რეაქციების მკვლევარი", Rank = "რეაქციების მკვლევარი", Station = StationType.ReactionTable, StationName = "რეაქციების მაგიდა", Cosmetic = "coat_reaction", LabItem = "balance" },
            new Chapter { ID = "ch7", Title = "ხსნარები", Badge = "ხსნარების სპეციალისტი", Rank = "ხსნარების სპეციალისტი", Station = StationType.SolutionStation, StationName = "ხსნარების სადგური", LabItem = "solution" },
            new Chapter { ID = "ch8", Title = "მჟავები და ფუძეები", Badge = "ახალგაზრდა ქიმიკოსი", Rank = "ახალგაზრდა ქიმიკოსი", Station = StationType.AcidBaseStation, StationName = "მჟავა-ფუძის მაგიდა", LabItem = "ph", Coins = 40 },
        };

        /// <summary>Activities from the rest of the game and the lesson that teaches their theory first.</summary>
        private static readonly Dictionary<string, string> ActivityLessons = new Dictionary<string, string>
        {
            { "atom", "c2_parts" }, { "particles", "c2_parts" }, { "molecule", "c4_molecule" }, { "balance", "c6_conserve" },
            { "compare", "c6_types" }, { "solution", "c7_concentration" }, { "membrane", "c7_concentration" },
            { "chromatography", "c7_separate" }, { "thermo", "c7_separate" }, { "distill", "c7_separate" },
            { "ph", "c8_ph" }, { "analysis", "c8_indicators" }, { "water", "c8_indicators" }, { "gas", "c1_states" }, { "valve", "c1_states" },
        };

        private static GameSaveData Data => GameSaveManager.Instance.Data;

        // ------------------------------------------------------------------ state

        public static readonly Lesson[] Lessons = Build();
        public static Lesson Get(string id) => Lessons.FirstOrDefault(l => l.ID == id);
        public static LessonRecord Record(string id) => Data.lessons.FirstOrDefault(r => r.id == id);
        public static bool Done(string id) => Record(id)?.done == true;
        public static Lesson Next => Lessons.FirstOrDefault(l => !Done(l.ID));
        public static bool Complete => Next == null;
        /// <summary>True when the next lesson is played at this station (each chapter lives at its own bench).</summary>
        public static bool LessonAt(StationType station) =>
            Data.tutorialCompleted && Next != null && ChapterOf(Next).Station == station;
        public static int Percent => Lessons.Count(l => Done(l.ID)) * 100 / Lessons.Length;
        public static Lesson[] Of(int chapter) => Lessons.Where(l => l.Chapter == chapter).ToArray();
        public static Chapter ChapterOf(Lesson lesson) => Chapters[lesson.Chapter - 1];
        public static bool ChapterDone(int chapter) => Of(chapter).All(l => Done(l.ID));
        public static int Number(Lesson lesson) => Array.IndexOf(Of(lesson.Chapter), lesson) + 1;
        public static string Label(Lesson lesson) => lesson.IsReview ? lesson.Title : lesson.Chapter + "." + Number(lesson) + "  " + lesson.Title;

        /// <summary>The path is sequential: a lesson opens when everything before it is done.</summary>
        public static bool Available(Lesson lesson) => lesson != null && (Done(lesson.ID) || lesson == Next);
        public static Lesson Previous(Lesson lesson)
        {
            int index = Array.IndexOf(Lessons, lesson);
            return index > 0 ? Lessons[index - 1] : null;
        }

        public static string Title
        {
            get
            {
                for (int c = Chapters.Length; c >= 1; c--) if (ChapterDone(c)) return Chapters[c - 1].Rank;
                return "დამწყები მოსწავლე";
            }
        }

        /// <summary>What a beginner should do now, in one short line.</summary>
        public static string NextAction
        {
            get
            {
                var next = Next;
                if (next == null) return "ოქროსფრად მბზინავი გამოწვევების მაგიდა გაიხსნა.";
                return "შემდეგი ცდა: „" + next.Title + "“ — " + ChapterOf(next).StationName;
            }
        }

        // ------------------------------------------------------------------ gates

        public static Lesson StationLesson(StationType type) =>
            Chapters.Select((c, i) => (c, i)).Where(p => p.c.Station == type).Select(p => Of(p.i + 1)[0]).FirstOrDefault();

        public static bool StationOpen(StationType type)
        {
            switch (type)
            {
                case StationType.QuizDesk:
                case StationType.LeaderboardBoard: return Complete;
                case StationType.ScienceFairDesk: return TierOpen(4);
                case StationType.AtomTable:
                case StationType.ElementStation:
                case StationType.MoleculeTable:
                case StationType.ReactionTable:
                case StationType.SolutionStation:
                case StationType.AcidBaseStation: return Done(StationLesson(type).ID);
                default: return true;
            }
        }

        public static bool ActivityOpen(string id) => !ActivityLessons.TryGetValue(id ?? "", out var lesson) || Done(lesson);
        public static string Requirement(string activity) =>
            ActivityLessons.TryGetValue(activity ?? "", out var lesson) ? "საჭიროა გაკვეთილი: „" + Get(lesson).Title + "“." : "";
        public static string Requirement(Lesson lesson)
        {
            var before = Previous(lesson);
            return before == null || Done(before.ID) ? "" : "ჯერ დაასრულე: „" + before.Title + "“.";
        }

        // ------------------------------------------------------------------ advanced floor

        public static int TierSolved(int tier) => LabProgress.Stat("tier:" + tier);
        public static bool TierOpen(int tier) => Complete && (tier <= 1 || TierSolved(tier - 1) >= TierGoal);
        public static int HighestTier => Enumerable.Range(1, 4).LastOrDefault(TierOpen);

        /// <summary>Counts a solved advanced question; returns a message when it unlocks the next tier.</summary>
        public static string CountTier(int tier)
        {
            if (tier < 1 || tier > 3) return null;
            LabProgress.CountStat("tier:" + tier);
            if (TierSolved(tier) != TierGoal) return null;
            var save = GameSaveManager.Instance;
            switch (tier + 1)
            {
                case 2: save.Data.chemCoins += 40; save.Commit(); return "გაიხსნა საფეხური 2 • აწყობა! +40 ქიმქოინი.";
                case 3: save.UnlockCosmetic("coat_master"); save.Data.chemCoins += 60; save.Commit(); return "გაიხსნა საფეხური 3 • ოსტატობა! ჯილდო: ოსტატის ხალათი და 60 ქიმქოინი.";
                default:
                    save.UnlockCosmetic("hat_professor");
                    if (!save.Data.ownedLabItems.Contains("gas")) save.Data.ownedLabItems.Add("gas");
                    save.Data.chemCoins += 100; save.Commit();
                    return "გაიხსნა საფეხური 4 • გამოცდა და სამეცნიერო ბაზრობა! ჯილდო: პროფესორის ქუდი, აირის სადგური და 100 ქიმქოინი.";
            }
        }

        // ------------------------------------------------------------------ mistakes and review

        public static string Key(Lesson lesson, int question) => lesson.ID + "|" + question;
        public static IEnumerable<(Lesson lesson, LessonQuestion question)> Mistakes =>
            Data.lessonMistakes.Select(Resolve).Where(m => m.lesson != null && m.question != null);
        public static (Lesson lesson, LessonQuestion question) Resolve(string key)
        {
            var parts = (key ?? "").Split('|');
            var lesson = parts.Length == 2 ? Get(parts[0]) : null;
            int index = parts.Length == 2 && int.TryParse(parts[1], out int n) ? n : -1;
            return (lesson, lesson != null && index >= 0 && index < lesson.Questions.Length ? lesson.Questions[index] : null);
        }

        /// <summary>Five chapter questions for a review, earlier mistakes first.</summary>
        public static string[] ReviewKeys(Lesson review)
        {
            var all = Of(review.Chapter).Where(l => !l.IsReview).SelectMany(l => l.Questions.Select((q, i) => Key(l, i))).ToList();
            var missed = all.Where(Data.lessonMistakes.Contains).ToList();
            var rest = all.Except(missed).OrderBy(_ => UnityEngine.Random.value);
            return missed.Concat(rest).Take(5).ToArray();
        }

        // ------------------------------------------------------------------ completion

        /// <summary>Saves the result, pays the beginner rewards and returns what the player received.</summary>
        public static List<string> Finish(Lesson lesson, string[] keys, ICollection<string> wrong)
        {
            var save = GameSaveManager.Instance;
            var record = Record(lesson.ID);
            if (record == null) { record = new LessonRecord { id = lesson.ID }; Data.lessons.Add(record); }
            bool first = !record.done;
            int score = keys.Count(k => !wrong.Contains(k));
            record.attempts++; record.done = true; record.best = Math.Max(record.best, score);
            foreach (string key in keys)
            {
                if (wrong.Contains(key)) { if (!Data.lessonMistakes.Contains(key)) Data.lessonMistakes.Add(key); }
                else Data.lessonMistakes.Remove(key);
            }
            var rewards = new List<string>();
            int coins = first ? lesson.Coins + score * 2 : 3;
            rewards.Add("+" + coins + " ქიმქოინი" + (first ? "" : " (გამეორება)"));
            if (lesson.Note != "" && !Data.notebookPages.Contains(lesson.ID)) { Data.notebookPages.Add(lesson.ID); rewards.Add("ახალი გვერდი რვეულში: „" + lesson.Title + "“"); }
            var found = lesson.Elements.Where(e => !Data.unlockedElements.Contains(e)).ToList();
            Data.unlockedElements.AddRange(found);
            if (found.Count > 0) rewards.Add("აღმოჩენილი ელემენტები: " + string.Join(", ", found));
            if (lesson.Collect != "" && LabProgress.Collect(lesson.Collect)) rewards.Add("კოლექციაში დაემატა: " + LabProgress.Catalog.FirstOrDefault(c => c.ID == lesson.Collect)?.Name);
            if (first) LabProgress.AddReputation(1);
            if (lesson.IsReview && first)
            {
                var chapter = ChapterOf(lesson);
                coins += chapter.Coins;
                rewards.Add("თავის ჯილდო: +" + chapter.Coins + " ქიმქოინი");
                LabProgress.Collect("badge:" + chapter.ID);
                rewards.Add("ნიშანი: „" + chapter.Badge + "“ • ახალი წოდება: " + chapter.Rank);
                if (chapter.Cosmetic != "" && !Data.unlockedCosmeticIDs.Contains(chapter.Cosmetic))
                {
                    save.UnlockCosmetic(chapter.Cosmetic);
                    rewards.Add("ახალი სამოსი მაღაზიის თაროზე: " + (LabGame.Instance?.cosmetics?.FirstOrDefault(c => c != null && c.itemID == chapter.Cosmetic)?.displayName ?? chapter.Cosmetic));
                }
                if (chapter.LabItem != "" && !Data.ownedLabItems.Contains(chapter.LabItem))
                {
                    Data.ownedLabItems.Add(chapter.LabItem);
                    rewards.Add("ახალი ნივთი შენი ლაბორატორიისთვის: " + PersonalLab.Catalog.FirstOrDefault(c => c.ID == chapter.LabItem)?.Name);
                }
                int next = lesson.Chapter;
                rewards.Add(next < Chapters.Length ? "გაიხსნა თავი: „" + Chapters[next].Title + "“" : "გაიხსნა რთული გამოწვევების მაგიდა და კვირის რეიტინგი!");
            }
            else if (first && ChapterOf(lesson).Station != StationType.TutorialStation && StationLesson(ChapterOf(lesson).Station) == lesson)
                rewards.Add("გაიხსნა კლასის მაგიდა: " + ChapterOf(lesson).StationName);
            save.RecordLesson(coins);
            return rewards;
        }

        // ------------------------------------------------------------------ content helpers

        // A property, not a field: Lessons is built during static initialization, before later fields are set.
        private static string[] AtomParts => new[] { "პროტონები", "ნეიტრონები", "ელექტრონები" };
        private static SortItem I(string label, int bin, string why) => new SortItem { Label = label, Bin = bin, Why = why };
        private static LessonQuestion Q(string prompt, string hint, string right, string why, string wrong1, string why1, string wrong2, string why2) =>
            new LessonQuestion { Prompt = prompt, Hint = hint, Choices = new[] { right, wrong1, wrong2 }, Why = new[] { why, why1, why2 } };
        private static Practice P(string goal, string done, string[] counters, int[] solution, int[] start = null, int[] min = null, int[] max = null) =>
            new Practice { Goal = goal, Done = done, Counters = counters, Solution = solution, Start = start, Min = min, Max = max };
        private static Practice Atom(string goal, string done, int p, int n, int e) => P(goal, done, AtomParts, new[] { p, n, e });
        private static Practice Equation(string goal, string done, string equation, params int[] solution) =>
            new Practice { Goal = goal, Done = done, Formula = equation, Solution = solution, Counters = equation.Replace("->", "+").Split('+').Select(s => s.Trim()).ToArray() };
        private static double DropsPH(int[] v) => ChemistrySandbox.PH(v[0], v[1], 20);
        private static string PHText(int[] v) => "ახლა pH ≈ " + DropsPH(v).ToString("0.0") + ". ";
        private static string AtomicNumber(int[] v, int z) =>
            v[0] != z ? "პროტონები: გაქვს " + v[0] + ". ატომური ნომერი " + z + " ნიშნავს " + z + " პროტონს."
            : v[2] != v[0] ? "ელექტრონები: გაქვს " + v[2] + ". ნეიტრალურ ატომს პროტონების ტოლი, " + z + " ელექტრონი სჭირდება." : null;

        private static Lesson Review(int chapter) => new Lesson
        {
            ID = "c" + chapter + "_review", Chapter = chapter, Kind = PracticeKind.Review, Coins = 15,
            Title = "შემაჯამებელი ტესტი: " + PathNames[chapter - 1],
            Why = "გამეორება ცოდნას ამაგრებს. შემდეგი თავი ამ თავზე დაშენდება.",
            Teach = new[] { "ხუთი კითხვაა მთელი თავიდან. შეცდომა არაუშავს: ყოველ ჯერზე აგიხსნი და თავიდან სცდი." },
        };

        private static Lesson[] Build()
        {
            var list = new List<Lesson>
            {
                // ================================================= 1. Chemistry and matter
                new Lesson
                {
                    ID = "c1_matter", Chapter = 1, Kind = PracticeKind.Sort, Title = "რა არის ქიმია და მატერია",
                    Why = "ეს ყველაფრის საფუძველია: შემდეგ გაკვეთილებზე სწორედ მატერიის აგებულებას და ცვლილებებს შევისწავლით.",
                    Teach = new[]
                    {
                        "ქიმია მეცნიერებაა ნივთიერებებზე: რისგან შედგება ისინი, როგორ იქცევიან და როგორ გარდაიქმნებიან.",
                        "მატერია ყველაფერია, რასაც მასა აქვს და ადგილს იკავებს: წყალი, ქვა, ჰაერიც კი.",
                        "სინათლე, ხმა და სითბო მატერია არ არის. ეს ენერგიის სახეებია.",
                    },
                    Bins = new[] { "მატერია", "არ არის მატერია" }, Hint = "იკითხე: აქვს მას მასა და იკავებს ადგილს?",
                    Items = new[]
                    {
                        I("წყალი", 0, "წყალს მასა აქვს და ჭიქაში ადგილს იკავებს."),
                        I("სინათლე", 1, "სინათლე ენერგიაა, მას მასა არ აქვს."),
                        I("ჰაერი", 0, "ჰაერი უხილავია, მაგრამ მასა აქვს: ბუშტს ავსებს."),
                        I("ხმა", 1, "ხმა რხევაა, რომელიც მატერიაში ვრცელდება. თავად ის მატერია არ არის."),
                        I("ქვა", 0, "ქვას მასა აქვს და ადგილს იკავებს."),
                        I("სითბო", 1, "სითბო ენერგიის გადაცემაა და არა ნივთიერება."),
                        I("ორთქლი", 0, "ორთქლი აირადი წყალია, ამიტომ მატერიაა."),
                    },
                    Questions = new[]
                    {
                        Q("რა არის მატერია?", "იფიქრე მასასა და ადგილზე.", "ყველაფერი, რასაც მასა აქვს და ადგილს იკავებს", "სწორია: მასა და მოცულობა მატერიის ნიშნებია.",
                            "მხოლოდ მყარი საგნები", "აირიც და სითხეც მატერიაა.", "ყველაფერი, რასაც ვხედავთ", "ჰაერს ვერ ვხედავთ, მაგრამ მატერიაა. სინათლეს ვხედავთ, მაგრამ მატერია არ არის."),
                        Q("რას სწავლობს ქიმია?", "გაიხსენე სიტყვა „ნივთიერება“.", "ნივთიერებებს და მათ ცვლილებებს", "ქიმია ნივთიერებების შემადგენლობასა და გარდაქმნებს სწავლობს.",
                            "მხოლოდ ცოცხალ ორგანიზმებს", "ეს უფრო ბიოლოგიაა.", "ვარსკვლავების მოძრაობას", "ეს ასტრონომიაა."),
                        Q("რომელია მატერია?", "რომელს აქვს მასა?", "ჰაერი ბუშტში", "ჰაერს მასა აქვს და ბუშტს ავსებს.",
                            "ნათურის შუქი", "შუქი ენერგიაა.", "ზარის ხმა", "ხმა ენერგიის გადაცემაა."),
                    },
                    Note = "ქიმია — მეცნიერება ნივთიერებებსა და მათ გარდაქმნებზე.\nმატერია — ყველაფერი, რასაც მასა აქვს და ადგილს იკავებს.\nსინათლე, ხმა და სითბო ენერგიაა და არა მატერია.",
                },
                new Lesson
                {
                    ID = "c1_states", Chapter = 1, Kind = PracticeKind.Sort, Title = "მყარი, თხევადი და აირადი",
                    Why = "მდგომარეობების ცოდნა დაგეხმარება, გაიგო დნობა, დუღილი და ისიც, რატომ ავსებს აირი ნებისმიერ ჭურჭელს.",
                    Teach = new[]
                    {
                        "მატერია ჩვეულებრივ სამ მდგომარეობაშია: მყარი, თხევადი და აირადი.",
                        "მყარს საკუთარი ფორმა აქვს. სითხე ჭურჭლის ფორმას იღებს, მაგრამ მოცულობას ინარჩუნებს.",
                        "აირი მთელ ჭურჭელს ავსებს. მის ნაწილაკებს შორის დიდი მანძილია და ისინი სწრაფად მოძრაობენ.",
                        "ერთი ნივთიერება სხვადასხვა მდგომარეობაში შეიძლება იყოს: ყინული, წყალი და ორთქლი.",
                    },
                    Bins = new[] { "მყარი", "თხევადი", "აირადი" }, Hint = "ფორმას ინარჩუნებს — მყარი; იღვრება — თხევადი; ყველგან ვრცელდება — აირადი.",
                    Items = new[]
                    {
                        I("ყინული", 0, "ყინული მყარი წყალია: ფორმას ინარჩუნებს."),
                        I("ორთქლი", 2, "ორთქლი აირადი წყალია."),
                        I("რძე", 1, "რძე ჭიქის ფორმას იღებს, ესე იგი სითხეა."),
                        I("ქვა", 0, "ქვას საკუთარი ფორმა აქვს."),
                        I("ჟანგბადი", 2, "ჟანგბადი ჰაერის ერთ-ერთი აირია."),
                        I("ზეთი", 1, "ზეთი იღვრება და ჭურჭლის ფორმას იღებს."),
                        I("მარილი", 0, "მარილის მარცვლები პატარა მყარი კრისტალებია."),
                        I("ნახშირორჟანგი", 2, "ნახშირორჟანგი აირია. გაზიან წყალში ბუშტუკებს ქმნის."),
                    },
                    Questions = new[]
                    {
                        Q("რომელი მდგომარეობა იღებს ჭურჭლის ფორმას, მაგრამ მოცულობას ინარჩუნებს?", "წარმოიდგინე წყალი ჭიქაში.", "თხევადი", "სითხე ჭურჭლის ფორმას იღებს, მოცულობა კი უცვლელია.",
                            "მყარი", "მყარს საკუთარი ფორმა აქვს.", "აირადი", "აირი მთელ ჭურჭელს ავსებს და მოცულობასაც იცვლის."),
                        Q("რა ხდება, როცა ყინული დნება?", "იცვლება მდგომარეობა და არა ნივთიერება.", "მყარი წყალი თხევად წყლად იქცევა", "დნობა მყარიდან თხევადზე გადასვლაა.",
                            "წყალი აირად იქცევა", "ეს აორთქლებაა.", "წყალი ახალ ნივთიერებად იქცევა", "დნობისას წყალი ისევ წყალია."),
                        Q("რატომ ავსებს აირი მთელ ოთახს?", "იფიქრე ნაწილაკების მოძრაობაზე.", "ნაწილაკები თავისუფლად და სწრაფად მოძრაობს", "აირის ნაწილაკებს შორის დიდი მანძილია და ისინი ყველა მიმართულებით მოძრაობს.",
                            "ნაწილაკები ერთმანეთზეა მიმაგრებული", "ასეა მყარ სხეულში.", "აირს მასა არ აქვს", "აირს მასა აქვს."),
                    },
                    Note = "მყარი — საკუთარი ფორმა და მოცულობა.\nთხევადი — ჭურჭლის ფორმა, მუდმივი მოცულობა.\nაირადი — ავსებს მთელ ჭურჭელს.\nდნობა: მყარი → თხევადი • აორთქლება: თხევადი → აირადი.",
                },
                new Lesson
                {
                    ID = "c1_objects", Chapter = 1, Kind = PracticeKind.Sort, Title = "საგანი და მასალა",
                    Why = "ქიმიკოსი საგნის ფორმას კი არ აკვირდება, არამედ მასალას, რისგანაც ის არის გაკეთებული.",
                    Teach = new[]
                    {
                        "საგანი კონკრეტული ნივთია: ჭიქა, სკამი, მონეტა.",
                        "მასალა ის ნივთიერებაა, რისგანაც საგანი მზადდება: მინა, ხე, სპილენძი.",
                        "ერთი მასალისგან ბევრი სხვადასხვა საგანი კეთდება. მინისგან ჭიქაც მზადდება და ფანჯარაც.",
                    },
                    Bins = new[] { "საგანი", "მასალა" }, Hint = "იკითხე: ეს ნივთია, თუ ის, რისგანაც ნივთს აკეთებენ?",
                    Items = new[]
                    {
                        I("ჭიქა", 0, "ჭიქა საგანია. ის მინისგან ან პლასტმასისგან კეთდება."),
                        I("მინა", 1, "მინა მასალაა: მისგან ჭიქა და ფანჯარა მზადდება."),
                        I("ლურსმანი", 0, "ლურსმანი საგანია, რკინისგან გაკეთებული."),
                        I("რკინა", 1, "რკინა მასალაა და ქიმიური ელემენტიც."),
                        I("სკამი", 0, "სკამი საგანია."),
                        I("ხე", 1, "ხე მასალაა: მისგან სკამი და მაგიდა კეთდება."),
                        I("მონეტა", 0, "მონეტა საგანია, ხშირად ლითონების ნარევისგან გაკეთებული."),
                        I("ბამბა", 1, "ბამბა მასალაა: მისგან მაისური კეთდება."),
                    },
                    Questions = new[]
                    {
                        Q("რომელია მასალა?", "რისგან კეთდება ნივთი?", "სპილენძი", "სპილენძი მასალაა: მისგან მავთული და მონეტები მზადდება.",
                            "მავთული", "მავთული საგანია.", "ქვაბი", "ქვაბი საგანია, ლითონისგან გაკეთებული."),
                        Q("მინის ჭიქა გატყდა. რა შეიცვალა?", "ნამსხვრევიც მინაა.", "საგნის ფორმა, მასალა კი იგივე დარჩა", "ნამსხვრევები ისევ მინაა. შეიცვალა მხოლოდ საგანი.",
                            "მასალა სხვა ნივთიერებად იქცა", "მინა მინად დარჩა.", "მინა გაქრა", "მატერია არ ქრება."),
                        Q("რატომ უნდა იცოდეს ქიმიკოსმა მასალა?", "იფიქრე მასალის თვისებებზე.", "მასალა განსაზღვრავს, როგორ იქცევა ნივთი", "მაგალითად, რკინა ჟანგდება, მინა კი არა.",
                            "მასალას მნიშვნელობა არ აქვს", "მასალის თვისებები ძალიან მნიშვნელოვანია.", "მხოლოდ ფერისთვის", "ფერი მხოლოდ ერთი თვისებაა."),
                    },
                    Note = "საგანი — კონკრეტული ნივთი (ჭიქა).\nმასალა — ის, რისგანაც ნივთი კეთდება (მინა).\nქიმია მასალების თვისებებს და ცვლილებებს სწავლობს.",
                },
                new Lesson
                {
                    ID = "c1_safety", Chapter = 1, Kind = PracticeKind.Sort, Title = "ლაბორატორიის უსაფრთხოება",
                    Why = "ეს წესები ყველა შემდეგ ცდაში დაგჭირდება. უსაფრთხოდ მუშაობა ნამდვილი ქიმიკოსის პირველი უნარია.",
                    Teach = new[]
                    {
                        "ლაბორატორიაში ყოველთვის ატარე ხალათი და დამცავი სათვალე. გრძელი თმა შეიკარი.",
                        "ნივთიერებას არასდროს გასინჯავ და პირდაპირ არ დაყნოსავ. სუნი ხელის მოძრაობით შენკენ მიიზიდე.",
                        "ლაბორატორიაში არ ვჭამთ და არ ვსვამთ.",
                        "თუ რამე დაიღვარა ან გატყდა, მაშინვე მასწავლებელს უთხარი.",
                    },
                    Bins = new[] { "უსაფრთხოა", "სახიფათოა" }, Hint = "იკითხე: იცავს ეს თვალებს, კანს და სხეულს?",
                    Items = new[]
                    {
                        I("დამცავი სათვალის ტარება", 0, "სათვალე თვალებს შხეფებისგან იცავს."),
                        I("ნივთიერების გასინჯვა", 1, "ლაბორატორიაში არაფერს ვსინჯავთ: ნივთიერება შეიძლება შხამიანი იყოს."),
                        I("გრძელი თმის შეკვრა", 0, "შეკრული თმა ალსა და ხელსაწყოებს არ ეხება."),
                        I("გაცხელებულ სინჯარაში ჩახედვა", 1, "გაცხელებული სითხე შეიძლება ამოისხას."),
                        I("დაღვრაზე მასწავლებლისთვის თქმა", 0, "მასწავლებელმა იცის, როგორ გაიწმინდოს ის უსაფრთხოდ."),
                        I("მაგიდასთან სენდვიჩის ჭამა", 1, "საკვებს ქიმიური ნივთიერება შეიძლება მოხვდეს."),
                        I("სუნის ხელით მიზიდვა", 0, "ასე მხოლოდ ცოტა ორთქლს ყნოსავ და უსაფრთხოა."),
                    },
                    Questions = new[]
                    {
                        Q("როგორ უნდა დაყნოსო ნივთიერება?", "პირდაპირ — არა.", "ხელის მოძრაობით სუნი შენკენ მიიზიდო", "ასე მხოლოდ ცოტა ორთქლი მოგხვდება.",
                            "ცხვირი სინჯარასთან მიიტანო", "ძლიერი ორთქლი შეიძლება საშიში იყოს.", "ჯერ გასინჯო", "ლაბორატორიაში არაფერს ვსინჯავთ."),
                        Q("რა უნდა გააკეთო, თუ ხსნარი დაიღვარა?", "ვის უნდა უთხრა?", "მაშინვე მასწავლებელს ვუთხრა", "მასწავლებელმა იცის, როგორ გაიწმინდოს ის უსაფრთხოდ.",
                            "დავმალო", "დამალული დაღვრა სხვებისთვის საშიშია.", "ხელით მოვწმინდო", "ზოგი ნივთიერება კანს აზიანებს."),
                        Q("რატომ ვატარებთ დამცავ სათვალეს?", "იფიქრე შხეფებზე.", "თვალებს შხეფებისგან იცავს", "თვალები ძალიან მგრძნობიარეა.",
                            "უკეთ რომ დავინახოთ", "სათვალე მხედველობას არ აუმჯობესებს.", "ლამაზად რომ გამოვიყურებოდეთ", "სათვალე უსაფრთხოებისთვისაა."),
                    },
                    Note = "ხალათი და სათვალე — ყოველთვის.\nარ ვსინჯავთ, პირდაპირ არ ვყნოსავთ.\nარ ვჭამთ და არ ვსვამთ.\nდაღვრა ან ტეხვა — მაშინვე მასწავლებელს.",
                },
                new Lesson
                {
                    ID = "c1_change", Chapter = 1, Kind = PracticeKind.Sort, Title = "ფიზიკური და ქიმიური ცვლილება",
                    Why = "ამ განსხვავებით დაიწყება რეაქციების თავი: ქიმიური ცვლილება სწორედ ქიმიური რეაქციაა.",
                    Teach = new[]
                    {
                        "ფიზიკური ცვლილებისას ნივთიერება იგივე რჩება: იცვლება ფორმა ან მდგომარეობა. ყინული დნება, მაგრამ ისევ წყალია.",
                        "ქიმიური ცვლილებისას ახალი ნივთიერება წარმოიქმნება. ხე იწვის და ნაცრად და აირებად იქცევა.",
                        "ქიმიური ცვლილების ნიშნებია: ახალი ფერი, აირის ბუშტუკები, სუნი, სითბო ან სინათლე.",
                    },
                    Bins = new[] { "ფიზიკური", "ქიმიური" }, Hint = "წარმოიქმნა ახალი ნივთიერება? თუ კი, ქიმიური ცვლილებაა.",
                    Items = new[]
                    {
                        I("ყინულის დნობა", 0, "წყალი წყლად რჩება, მხოლოდ მდგომარეობა იცვლება."),
                        I("ხის წვა", 1, "წარმოიქმნება ნაცარი, ნახშირორჟანგი და წყლის ორთქლი."),
                        I("ქაღალდის დაჭრა", 0, "პატარა ნაჭრებიც ქაღალდია."),
                        I("რკინის დაჟანგვა", 1, "ჟანგი ახალი ნივთიერებაა."),
                        I("შაქრის გახსნა წყალში", 0, "შაქარი არ ქრება: წყლის აორთქლების შემდეგ ისევ მიიღებ."),
                        I("პურის გამოცხობა", 1, "ცომში ახალი ნივთიერებები წარმოიქმნება და უკან ვეღარ დააბრუნებ."),
                        I("წყლის დუღილი", 0, "ორთქლიც წყალია."),
                        I("კვერცხის შეწვა", 1, "ცილა იცვლება და ახალ ნივთიერებად იქცევა."),
                    },
                    Questions = new[]
                    {
                        Q("რომელია ქიმიური ცვლილება?", "ეძებე ახალი ნივთიერება.", "ასანთის ანთება", "წვისას ახალი ნივთიერებები, სითბო და სინათლე წარმოიქმნება.",
                            "მინის გატეხვა", "ნამსხვრევები ისევ მინაა.", "კარაქის დნობა", "დამდნარი კარაქიც კარაქია."),
                        Q("რომელია ქიმიური ცვლილების ნიშანი?", "რა ჩნდება ახალი?", "აირის ბუშტუკები და ფერის შეცვლა", "ეს ახალ ნივთიერებაზე მიუთითებს.",
                            "მხოლოდ ფორმის შეცვლა", "ფორმის შეცვლა ფიზიკური ცვლილებაა.", "ნივთის გადაადგილება", "გადაადგილება ნივთიერებას არ ცვლის."),
                        Q("წყლის გაყინვა რა ცვლილებაა?", "ყინული რა ნივთიერებაა?", "ფიზიკური", "ყინულიც წყალია. შეიცვალა მხოლოდ მდგომარეობა.",
                            "ქიმიური", "ახალი ნივთიერება არ წარმოიქმნა.", "არანაირი", "მდგომარეობა შეიცვალა, ესეც ცვლილებაა."),
                    },
                    Note = "ფიზიკური ცვლილება — ნივთიერება იგივეა (დნობა, დაჭრა, გახსნა).\nქიმიური ცვლილება — ახალი ნივთიერება (წვა, ჟანგი, გამოცხობა).\nნიშნები: ფერი, აირი, სუნი, სითბო, სინათლე.",
                },
                new Lesson
                {
                    ID = "c1_history", Chapter = 1, Kind = PracticeKind.Sort, Title = "ალქიმიიდან ქიმიამდე",
                    Why = "ისტორია გვიჩვენებს, რატომ ვენდობით ცდასა და გაზომვას. ეს ყველა შემდეგი გაკვეთილის მეთოდია.",
                    Teach = new[]
                    {
                        "ძველად ალქიმიკოსები ცდილობდნენ, ტყვია ოქროდ ექციათ და „ფილოსოფიური ქვა“ ეპოვათ.",
                        "ოქრო ვერ მიიღეს, მაგრამ შექმნეს ხელსაწყოები: გამოსახდელი აპარატი, ჭურჭელი და გაცხელების ხერხები.",
                        "XVII საუკუნეში რობერტ ბოილმა თქვა: ყველა მოსაზრება ცდით უნდა შემოწმდეს.",
                        "ანტუან ლავუაზიემ ნივთიერებები აწონა და აჩვენა, რომ რეაქციისას მასა არ იკარგება. ასე დაიბადა თანამედროვე ქიმია.",
                    },
                    Bins = new[] { "ალქიმია", "თანამედროვე ქიმია" }, Hint = "ჯადო და საიდუმლო — ალქიმია; გაზომვა და ცდა — ქიმია.",
                    Items = new[]
                    {
                        I("ტყვიის ოქროდ ქცევის მცდელობა", 0, "ეს ალქიმიკოსების მთავარი ოცნება იყო."),
                        I("ზუსტი აწონვა და გაზომვა", 1, "ლავუაზიეს შემდეგ გაზომვა ქიმიის საფუძველია."),
                        I("ფილოსოფიური ქვის ძიება", 0, "ალქიმიკოსებს ჯადოსნური ქვის არსებობის სჯეროდათ."),
                        I("ცდის განმეორება და შემოწმება", 1, "მეცნიერული მეთოდი თანამედროვე ქიმიას ეკუთვნის."),
                        I("საიდუმლო ნიშნებით ჩანაწერები", 0, "ალქიმიკოსები ცოდნას მალავდნენ."),
                        I("ელემენტების ცხრილი", 1, "პერიოდული ცხრილი 1869 წელს შეიქმნა."),
                    },
                    Questions = new[]
                    {
                        Q("რას ცდილობდნენ ალქიმიკოსები?", "ძვირფასი ლითონი.", "ტყვიის ოქროდ ქცევას", "ეს მათი ცნობილი მიზანი იყო. ჩვეულებრივი ქიმიით ეს შეუძლებელია.",
                            "ელექტროენერგიის მიღებას", "ეს ბევრად გვიან გახდა შესაძლებელი.", "პერიოდული ცხრილის შედგენას", "ცხრილი მენდელეევმა 1869 წელს შექმნა."),
                        Q("რა აჩვენა ლავუაზიემ აწონვით?", "შეადარე მასა რეაქციამდე და რეაქციის შემდეგ.", "რეაქციისას მასა არ იკარგება", "ეს მასის შენახვის კანონია.",
                            "მასა ყოველთვის ორმაგდება", "მასა იგივე რჩება.", "ჰაერს მასა არ აქვს", "ჰაერს მასა აქვს."),
                        Q("რა სასარგებლო დაგვიტოვეს ალქიმიკოსებმა?", "იფიქრე ლაბორატორიულ ჭურჭელზე.", "ხელსაწყოები და ხერხები, მაგალითად გამოხდა", "მათი ხელსაწყოები შემდეგ ქიმიკოსებმა გამოიყენეს.",
                            "ოქროს ქარხანა", "ოქრო ვერ მიიღეს.", "ატომის მოდელი", "ატომის მოდელები XIX–XX საუკუნეებში შეიქმნა."),
                    },
                    Note = "ალქიმია — ტყვიის ოქროდ ქცევის მცდელობა, მაგრამ სასარგებლო ხელსაწყოები.\nბოილი (XVII ს.) — ცდით შემოწმება.\nლავუაზიე (XVIII ს.) — აწონვა; მასა არ იკარგება.\nქიმია = ცდა + გაზომვა.",
                },
                Review(1),

                // ================================================= 2. Atoms
                new Lesson
                {
                    ID = "c2_parts", Chapter = 2, Kind = PracticeKind.Atom, Title = "ატომი და მისი ნაწილები", Elements = new[] { "H", "He", "Li" },
                    Why = "ატომებით აიგება ყველაფერი: ელემენტები, მოლეკულები და რეაქციები. ამ ნაწილაკებს ყველა მომდევნო თავში გამოვიყენებთ.",
                    Teach = new[]
                    {
                        "ყველა ნივთიერება პაწაწინა ნაწილაკებისგან, ატომებისგან შედგება.",
                        "ატომის ცენტრში ბირთვია. მასშია პროტონები (+) და ნეიტრონები, რომლებსაც მუხტი არ აქვს.",
                        "ბირთვის გარშემო ელექტრონები (−) მოძრაობენ.",
                        "ნეიტრალურ ატომში პროტონებისა და ელექტრონების რაოდენობა ტოლია.",
                    },
                    Steps = new[]
                    {
                        Atom("ნახე, როგორ ავაწყობ წყალბადს: 1 პროტონი და 1 ელექტრონი.", "წყალბადის ატომი ყველაზე მარტივია: ერთი პროტონი და ერთი ელექტრონი.", 1, 0, 1),
                        Atom("ააწყვე ჰელიუმი: 2 პროტონი, 2 ნეიტრონი და 2 ელექტრონი.", "ჰელიუმი მზადაა! პროტონები და ელექტრონები ტოლია, ამიტომ ატომი ნეიტრალურია.", 2, 2, 2),
                        new Practice { Goal = "ახლა თავად: ააწყვე ლითიუმის ნეიტრალური ატომი — 3 პროტონი. ბუნებაში ლითიუმს 3 ან 4 ნეიტრონი აქვს: აირჩიე შენ.", Done = "ლითიუმ-7: 3 პროტონი, 4 ნეიტრონი და 3 ელექტრონი. ბუნებაში ასეთი ლითიუმი ყველაზე ხშირია.", Counters = AtomParts, Solution = new[] { 3, 4, 3 },
                            Check = v => AtomicNumber(v, 3) ?? (v[1] == 3 || v[1] == 4 ? null : "ნეიტრონები: გაქვს " + v[1] + ". ლითიუმის ბირთვში 3 ან 4 ნეიტრონია."),
                            Named = v => v[1] == 3 ? "ლითიუმ-6: 3 პროტონი, 3 ნეიტრონი და 3 ელექტრონი. ესეც ნამდვილი ლითიუმია, უბრალოდ უფრო იშვიათი." : null },
                    },
                    Questions = new[]
                    {
                        Q("რომელ ნაწილაკს აქვს დადებითი მუხტი?", "ის ბირთვშია.", "პროტონს", "პროტონი დადებითია (+).",
                            "ელექტრონს", "ელექტრონი უარყოფითია (−).", "ნეიტრონს", "ნეიტრონს მუხტი არ აქვს."),
                        Q("სად მოძრაობენ ელექტრონები?", "ბირთვში ისინი არ არიან.", "ბირთვის გარშემო", "ელექტრონები ბირთვის გარშემო შრეებში მოძრაობენ.",
                            "ბირთვის შიგნით", "ბირთვში პროტონები და ნეიტრონებია.", "ატომის გარეთ, ჰაერში", "ელექტრონები ატომს ეკუთვნის."),
                        Q("ნეიტრალურ ატომში 6 პროტონია. რამდენი ელექტრონია?", "ნეიტრალური ნიშნავს, რომ მუხტები ტოლია.", "6", "6 დადებითი და 6 უარყოფითი მუხტი ერთმანეთს აბათილებს.",
                            "12", "მაშინ ატომი უარყოფითი იქნებოდა.", "0", "მაშინ ატომი დადებითი იქნებოდა."),
                    },
                    Note = "ატომი: ბირთვი + ელექტრონები.\nპროტონი (+) და ნეიტრონი (0) — ბირთვში.\nელექტრონი (−) — ბირთვის გარშემო.\nნეიტრალური ატომი: პროტონები = ელექტრონები.",
                },
                new Lesson
                {
                    ID = "c2_number", Chapter = 2, Kind = PracticeKind.Atom, Title = "ატომური ნომერი", Elements = new[] { "C", "N", "O" },
                    Why = "ატომური ნომრით პერიოდულ ცხრილში ელემენტს იპოვი. ეს შემდეგ თავში დაგჭირდება.",
                    Teach = new[]
                    {
                        "ატომური ნომერი პროტონების რაოდენობაა.",
                        "სწორედ პროტონები განსაზღვრავს ელემენტს: 1 პროტონი — წყალბადი, 6 — ნახშირბადი, 8 — ჟანგბადი.",
                        "თუ ატომს ერთ პროტონს დაუმატებ, სულ სხვა ელემენტი მიიღება.",
                    },
                    Steps = new[]
                    {
                        Atom("ატომური ნომერი 6 ნიშნავს 6 პროტონს. ნეიტრალური ატომისთვის 6 ელექტრონსაც დავამატებ.", "6 პროტონი — ეს ნახშირბადია, ნეიტრონების რაოდენობის მიუხედავად.", 6, 6, 6),
                        new Practice { Goal = "ააწყვე ნეიტრალური ატომი, რომლის ატომური ნომერია 8. ნეიტრონები ამჯერად არ ითვლება.", Done = "8 პროტონი — ეს ჟანგბადია!", Counters = AtomParts, Solution = new[] { 8, 8, 8 }, Check = v => AtomicNumber(v, 8) },
                        new Practice { Goal = "ააწყვე ნეიტრალური ატომი, რომლის ატომური ნომერია 7.", Done = "7 პროტონი — ეს აზოტია, ჰაერის მთავარი აირი.", Counters = AtomParts, Solution = new[] { 7, 7, 7 }, Check = v => AtomicNumber(v, 7) },
                    },
                    Questions = new[]
                    {
                        Q("რას გვიჩვენებს ატომური ნომერი?", "იფიქრე ბირთვის დადებით ნაწილაკებზე.", "პროტონების რაოდენობას", "ატომური ნომერი პროტონების რაოდენობის ტოლია.",
                            "ნეიტრონების რაოდენობას", "ერთი ელემენტის ატომებში ნეიტრონების რაოდენობა შეიძლება განსხვავდებოდეს.", "ატომის ზომას", "ზომა სხვა რამეზეა დამოკიდებული."),
                        Q("ატომს 8 პროტონი აქვს. რომელი ელემენტია?", "ის ჰაერის 21%-ია.", "ჟანგბადი", "ჟანგბადის ატომური ნომერია 8.",
                            "ნახშირბადი", "ნახშირბადს 6 პროტონი აქვს.", "წყალბადი", "წყალბადს 1 პროტონი აქვს."),
                        Q("რა მოხდება, თუ ატომს ერთ პროტონს დავუმატებთ?", "ელემენტს პროტონები განსაზღვრავს.", "სხვა ელემენტი მიიღება", "პროტონების ახალი რაოდენობა ახალ ატომურ ნომერს ნიშნავს.",
                            "იგივე ელემენტი დარჩება", "ელემენტი პროტონებით განისაზღვრება.", "ატომი გაქრება", "ატომი არ ქრება."),
                    },
                    Note = "ატომური ნომერი (Z) = პროტონების რაოდენობა.\nZ ელემენტის „პასპორტია“: H — 1, He — 2, C — 6, N — 7, O — 8.",
                },
                new Lesson
                {
                    ID = "c2_isotopes", Chapter = 2, Kind = PracticeKind.Atom, Title = "მასური რიცხვი და იზოტოპები",
                    Why = "მასური რიცხვით ატომის მასას შეაფასებ. იზოტოპები მეცნიერებს ნაპოვნი ნივთების ასაკის დადგენაშიც ეხმარება.",
                    Teach = new[]
                    {
                        "მასური რიცხვი = პროტონები + ნეიტრონები. ელექტრონები ძალიან მსუბუქია და არ ითვლება.",
                        "ნახშირბად-12-ს 6 პროტონი და 6 ნეიტრონი აქვს: 6 + 6 = 12.",
                        "იზოტოპები ერთი ელემენტის ატომებია, ნეიტრონების განსხვავებული რაოდენობით. ნახშირბად-14-ს 8 ნეიტრონი აქვს.",
                        "იზოტოპები ქიმიურად თითქმის ერთნაირად იქცევა, მაგრამ მათი მასა განსხვავდება.",
                    },
                    Steps = new[]
                    {
                        Atom("ნახშირბად-12: 6 პროტონი + 6 ნეიტრონი = 12.", "მასური რიცხვი 12-ია, რადგან ბირთვში 12 ნაწილაკია.", 6, 6, 6),
                        Atom("ააწყვე ნახშირბად-14. პროტონები იგივე რჩება, მასური რიცხვი კი 14 უნდა იყოს.", "6 + 8 = 14. ეს ნახშირბადის იზოტოპია!", 6, 8, 6),
                        Atom("ააწყვე დეიტერიუმი — წყალბად-2-ის ნეიტრალური ატომი.", "1 პროტონი + 1 ნეიტრონი = 2. ეს წყალბადის მძიმე იზოტოპია.", 1, 1, 1),
                    },
                    Questions = new[]
                    {
                        Q("რას უდრის მასური რიცხვი?", "დათვალე ბირთვის ნაწილაკები.", "პროტონებს + ნეიტრონებს", "ბირთვის ყველა ნაწილაკი ითვლება.",
                            "პროტონებს + ელექტრონებს", "ელექტრონები თითქმის უწონოა და ბირთვში არ არიან.", "მხოლოდ ნეიტრონებს", "პროტონებიც ითვლება."),
                        Q("რით განსხვავდება ერთი ელემენტის იზოტოპები?", "პროტონები იგივეა.", "ნეიტრონების რაოდენობით", "პროტონები ერთნაირია, ნეიტრონები — სხვადასხვა.",
                            "პროტონების რაოდენობით", "მაშინ სხვა ელემენტი იქნებოდა.", "ქიმიური სიმბოლოთი", "იზოტოპებს ერთი სიმბოლო აქვს."),
                        Q("ატომს 11 პროტონი და 12 ნეიტრონი აქვს. რა არის მისი მასური რიცხვი?", "შეკრიბე.", "23", "11 + 12 = 23.",
                            "11", "ეს ატომური ნომერია.", "1", "გამოკლება არ გვჭირდება."),
                    },
                    Note = "მასური რიცხვი (A) = პროტონები + ნეიტრონები.\nიზოტოპი — იგივე პროტონები, ნეიტრონების სხვა რაოდენობა.\nC-12: 6p + 6n • C-14: 6p + 8n.",
                },
                new Lesson
                {
                    ID = "c2_history", Chapter = 2, Kind = PracticeKind.Sort, Title = "როგორ აღმოაჩინეს ატომი",
                    Why = "ეს ისტორია გვიჩვენებს, როგორ იცვლება მეცნიერული მოდელი ახალი ცდებით. შენც ასე იმუშავებ.",
                    Teach = new[]
                    {
                        "დემოკრიტემ ძველ საბერძნეთში თქვა: ყველაფერი პაწაწინა, გაუყოფელი ნაწილაკებისგან, „ატომოსებისგან“ შედგება.",
                        "1897 წელს ჯ. ჯ. ტომსონმა ელექტრონი აღმოაჩინა და ატომი ქიშმიშიან ფუნთუშას შეადარა.",
                        "1911 წელს ერნესტ რეზერფორდმა ოქროს თხელ ფურცელზე ცდით ბირთვი აღმოაჩინა.",
                        "ნილს ბორმა ელექტრონები შრეებად დაალაგა, 1932 წელს კი ჯეიმზ ჩედვიკმა ნეიტრონი იპოვა.",
                    },
                    Bins = new[] { "ტომსონი", "რეზერფორდი", "ჩედვიკი" }, Hint = "თანმიმდევრობა: ელექტრონი (1897) → ბირთვი (1911) → ნეიტრონი (1932).",
                    Items = new[]
                    {
                        I("ელექტრონის აღმოჩენა", 0, "ტომსონმა ელექტრონი 1897 წელს აღმოაჩინა."),
                        I("ოქროს ფურცლის ცდა", 1, "რეზერფორდის ნაწილაკების უმეტესობამ ფურცელში გაიარა, ზოგი კი უკან აირეკლა."),
                        I("ნეიტრონის აღმოჩენა", 2, "ჩედვიკმა ნეიტრონი 1932 წელს აღმოაჩინა."),
                        I("ქიშმიშიანი ფუნთუშის მოდელი", 0, "ტომსონის მოდელში ელექტრონები დადებით „ცომშია“ ჩაფლული."),
                        I("პატარა, მკვრივი ბირთვი", 1, "რეზერფორდმა დაასკვნა, რომ ატომის ცენტრში პატარა ბირთვია."),
                        I("მუხტის არმქონე ნაწილაკი ბირთვში", 2, "ეს ნეიტრონია, ჩედვიკის აღმოჩენა."),
                    },
                    Questions = new[]
                    {
                        Q("ვინ აღმოაჩინა ატომის ბირთვი?", "გაიხსენე ოქროს ფურცელი.", "ერნესტ რეზერფორდმა", "1911 წლის ცდამ ბირთვის არსებობა აჩვენა.",
                            "დემოკრიტემ", "დემოკრიტემ მხოლოდ ატომის იდეა გამოთქვა.", "ლავუაზიემ", "ლავუაზიე მასის შენახვის კანონით არის ცნობილი."),
                        Q("ოქროს ფურცლის ცდაში ზოგი ნაწილაკი უკან აირეკლა. რა დაასკვნეს?", "რაღაც პატარა და მკვრივი.", "ატომის ცენტრში მკვრივი ბირთვია", "მხოლოდ მკვრივ ბირთვს შეეძლო ნაწილაკების უკან მობრუნება.",
                            "ატომი სულ ცარიელია", "მაშინ არცერთი ნაწილაკი არ აირეკლებოდა.", "ოქრო მაგნიტია", "ოქრო მაგნიტი არ არის."),
                        Q("რატომ შეიცვალა ატომის მოდელი რამდენჯერმე?", "იფიქრე ცდებზე.", "ახალმა ცდებმა ახალი ფაქტები აჩვენა", "მეცნიერება ცდების მიხედვით აუმჯობესებს მოდელებს.",
                            "მეცნიერებს ახალი სახელები მოსწონდათ", "მოდელს ფაქტები ცვლის.", "ატომები დროთა განმავლობაში იცვლება", "ატომები არ შეცვლილა, ჩვენი ცოდნა შეიცვალა."),
                    },
                    Note = "დემოკრიტე — ატომის იდეა.\nტომსონი (1897) — ელექტრონი.\nრეზერფორდი (1911) — ბირთვი.\nბორი (1913) — შრეები • ჩედვიკი (1932) — ნეიტრონი.",
                },
                Review(2),

                // ================================================= 3. Elements and the periodic table
                new Lesson
                {
                    ID = "c3_symbols", Chapter = 3, Kind = PracticeKind.Sort, Title = "ელემენტი, სახელი და სიმბოლო", Elements = new[] { "Na", "Fe", "Au", "Cu", "Ca" },
                    Why = "სიმბოლოები ქიმიის ანბანია. მათ გარეშე ფორმულებსა და რეაქციებს ვერ წაიკითხავ.",
                    Teach = new[]
                    {
                        "ელემენტი ნივთიერებაა, რომლის ყველა ატომს პროტონების ერთნაირი რაოდენობა აქვს.",
                        "ყველა ელემენტს სიმბოლო აქვს: ერთი დიდი ასო, ან დიდი და პატარა ასო. H — წყალბადი, O — ჟანგბადი, Ca — კალციუმი.",
                        "ზოგი სიმბოლო ლათინური სახელიდან მოდის: Na — Natrium (ნატრიუმი), Fe — Ferrum (რკინა), Au — Aurum (ოქრო).",
                        "ამ გაკვეთილის შემდეგ პერიოდულ ცხრილში [Tab] ელემენტების აღმოჩენას შეძლებ.",
                    },
                    Bins = new[] { "ჟანგბადი", "ნატრიუმი", "რკინა" }, Hint = "სიმბოლოს პირველი ასო დიდია, მეორე — პატარა.",
                    Items = new[]
                    {
                        I("O", 0, "O ჟანგბადის სიმბოლოა."),
                        I("Na", 1, "Na ლათინური სიტყვიდან Natrium მოდის."),
                        I("Fe", 2, "Fe ლათინური სიტყვიდან Ferrum მოდის."),
                        I("ჰაერის დაახლოებით 21%", 0, "ჰაერის ყოველი მეხუთე მოლეკულა ჟანგბადია."),
                        I("სუფრის მარილის ნაწილი", 1, "სუფრის მარილი NaCl-ია."),
                        I("ლურსმნისა და ხიდის ლითონი", 2, "რკინა ფოლადის მთავარი ნაწილია."),
                    },
                    Questions = new[]
                    {
                        Q("რომელია ოქროს სიმბოლო?", "ლათინურად — Aurum.", "Au", "Au ლათინური სიტყვიდან Aurum მოდის.",
                            "Go", "ასეთი სიმბოლო არ არსებობს.", "O", "O ჟანგბადია."),
                        Q("რომელი ჩანაწერია სწორი?", "მეორე ასო პატარაა.", "Ca", "სიმბოლოში პირველი ასო დიდია, მეორე — პატარა.",
                            "CA", "სიმბოლოში ორი დიდი ასო არ იწერება.", "ca", "პირველი ასო ყოველთვის დიდია."),
                        Q("რა არის ელემენტი?", "იფიქრე პროტონებზე.", "ნივთიერება, რომლის ყველა ატომს ერთი ატომური ნომერი აქვს", "ელემენტს პროტონების რაოდენობა განსაზღვრავს.",
                            "ნებისმიერი ნარევი", "ნარევი რამდენიმე ნივთიერებისგან შედგება.", "მხოლოდ ლითონი", "არალითონებიც ელემენტებია."),
                    },
                    Note = "ელემენტი — ერთი ტიპის ატომები.\nსიმბოლო: H, O, C, Ca (პირველი ასო დიდია).\nლათინური სახელები: Na — Natrium, Fe — Ferrum, Au — Aurum, Cu — Cuprum.",
                },
                new Lesson
                {
                    ID = "c3_table", Chapter = 3, Kind = PracticeKind.Element, Title = "ატომური ნომერი პერიოდულ ცხრილში", Collect = "fact:table",
                    Elements = new[] { "Be", "B", "F", "Ne", "Mg", "Al", "Si", "P", "S", "Cl", "Ar", "K" },
                    Why = "ცხრილში ელემენტის მოძებნა ყოველდღიური უნარია: ყოველ ფორმულასა და რეაქციაში დაგჭირდება.",
                    Teach = new[]
                    {
                        "პერიოდულ ცხრილში ელემენტები ატომური ნომრის ზრდის მიხედვით ლაგდება: H — 1, He — 2, Li — 3...",
                        "უჯრაში წერია ატომური ნომერი, სიმბოლო და სახელი.",
                        "დიმიტრი მენდელეევმა ცხრილი 1869 წელს შექმნა და ჯერ უცნობი ელემენტებისთვის ცარიელი ადგილებიც დატოვა.",
                        "ამ გაკვეთილით პირველი 20 ელემენტი შენს კოლექციაში დაემატება.",
                    },
                    Steps = new[]
                    {
                        P("ნახე: ნომერი 6 — ნახშირბადი (C).", "ნახშირბადი მე-6 უჯრაშია, რადგან 6 პროტონი აქვს.", new[] { "ატომური ნომერი" }, new[] { 6 }, new[] { 1 }, new[] { 1 }, new[] { 30 }),
                        P("იპოვე ნატრიუმი (Na). ის მე-11 უჯრაშია.", "ნატრიუმი იპოვე! 11 პროტონი — 11-ე ადგილი.", new[] { "ატომური ნომერი" }, new[] { 11 }, new[] { 1 }, new[] { 1 }, new[] { 30 }),
                        P("იპოვე კალციუმი (Ca). მინიშნება: ის კალიუმის (19) შემდეგაა.", "კალციუმი — ნომერი 20. ძვლები და ცარცი მას შეიცავს.", new[] { "ატომური ნომერი" }, new[] { 20 }, new[] { 1 }, new[] { 1 }, new[] { 30 }),
                    },
                    Questions = new[]
                    {
                        Q("რის მიხედვით ლაგდება ელემენტები ცხრილში?", "ატომის „პასპორტი“.", "ატომური ნომრის მიხედვით", "ყოველ შემდეგ ელემენტს ერთი პროტონით მეტი აქვს.",
                            "ანბანის მიხედვით", "ანბანს აქ მნიშვნელობა არ აქვს.", "ფერის მიხედვით", "ფერები მხოლოდ დასახმარებლადაა."),
                        Q("ვინ შექმნა პერიოდული ცხრილი 1869 წელს?", "რუსი ქიმიკოსი.", "დიმიტრი მენდელეევმა", "მან უცნობი ელემენტების თვისებებიც იწინასწარმეტყველა.",
                            "ერნესტ რეზერფორდმა", "რეზერფორდმა ბირთვი აღმოაჩინა.", "რობერტ ბოილმა", "ბოილი ცდის მეთოდით არის ცნობილი."),
                        Q("რომელი ელემენტია ჟანგბადის (8) შემდეგ?", "8 + 1.", "ფთორი (9)", "შემდეგ უჯრაში ატომური ნომერი 9-ია — ფთორი.",
                            "აზოტი (7)", "აზოტი ჟანგბადამდეა.", "ნახშირბადი (6)", "ნახშირბადი კიდევ უფრო ადრეა."),
                    },
                    Note = "ცხრილი ატომური ნომრის მიხედვით ლაგდება.\nუჯრა: ნომერი • სიმბოლო • სახელი.\nმენდელეევი, 1869 — ცარიელი ადგილები მომავალი აღმოჩენებისთვის.",
                },
                new Lesson
                {
                    ID = "c3_metals", Chapter = 3, Kind = PracticeKind.Sort, Title = "ლითონები და არალითონები",
                    Why = "ლითონია თუ არალითონი — ამით გაიგებ, როგორ ბმას შექმნის ელემენტი. ეს ბმების თავში დაგჭირდება.",
                    Teach = new[]
                    {
                        "ლითონები ბზინავს, სითბოსა და ელექტროდენს კარგად ატარებს და იჭედება: რკინა, სპილენძი, ოქრო.",
                        "არალითონები ხშირად მყიფეა ან აირია და დენს ცუდად ატარებს: ჟანგბადი, გოგირდი, ქლორი.",
                        "ცხრილში ლითონები მარცხნივ და შუაშია, არალითონები — მარჯვენა ზედა ნაწილში. წყალბადი არალითონია, თუმცა მარცხნივ დგას.",
                    },
                    Bins = new[] { "ლითონი", "არალითონი" }, Hint = "ბზინავს და დენს ატარებს — ლითონი.",
                    Items = new[]
                    {
                        I("რკინა", 0, "რკინა მყარი, ბზინვარე ლითონია."),
                        I("ჟანგბადი", 1, "ჟანგბადი არალითონური აირია."),
                        I("სპილენძი", 0, "სპილენძი დენს კარგად ატარებს, ამიტომ მისგან მავთულებს აკეთებენ."),
                        I("გოგირდი", 1, "გოგირდი ყვითელი, მყიფე არალითონია."),
                        I("ოქრო", 0, "ოქრო ბზინვარე, ადვილად საჭედი ლითონია."),
                        I("ქლორი", 1, "ქლორი მომწვანო არალითონური აირია."),
                        I("ნატრიუმი", 0, "ნატრიუმი რბილია, მაგრამ ჭრილზე ბზინავს — ლითონია."),
                        I("ნახშირბადი", 1, "ნახშირბადი (გრაფიტი, ალმასი) არალითონია."),
                    },
                    Questions = new[]
                    {
                        Q("რომელი თვისება ახასიათებს ლითონებს?", "გაიხსენე მავთული.", "ელექტროდენს კარგად ატარებს", "ამიტომ მავთულებს სპილენძისგან აკეთებენ.",
                            "ყოველთვის აირია", "ლითონები, ვერცხლისწყლის გარდა, ოთახის ტემპერატურაზე მყარია.", "ყოველთვის მყიფეა", "ლითონები იჭედება და იღუნება."),
                        Q("რომელია არალითონი?", "მას ვსუნთქავთ.", "ჟანგბადი", "ჟანგბადი არალითონური აირია.",
                            "რკინა", "რკინა ლითონია.", "ალუმინი", "ალუმინი ლითონია."),
                        Q("ცხრილის რომელ ნაწილშია არალითონები?", "მარჯვნივ და მაღლა.", "მარჯვენა ზედა ნაწილში", "წყალბადის გარდა არალითონები მარჯვნივ, ზემოთაა.",
                            "მარცხენა ქვედა ნაწილში", "იქ ლითონებია.", "მხოლოდ შუაში", "შუაში გარდამავალი ლითონებია."),
                    },
                    Note = "ლითონი — ბზინავს, ატარებს დენსა და სითბოს, იჭედება (Fe, Cu, Au, Na).\nარალითონი — ხშირად მყიფე ან აირია, დენს ცუდად ატარებს (O, S, Cl, C).\nცხრილში: ლითონები მარცხნივ, არალითონები მარჯვნივ, ზემოთ.",
                },
                new Lesson
                {
                    ID = "c3_groups", Chapter = 3, Kind = PracticeKind.Sort, Title = "ჯგუფები და პერიოდები", Collect = "fact:helium",
                    Why = "ჯგუფის ცოდნით ელემენტის ქცევას ცდის გარეშეც გამოიცნობ. ეს ბმებისა და რეაქციების თავებში ძალიან დაგეხმარება.",
                    Teach = new[]
                    {
                        "ცხრილის ჰორიზონტალური მწკრივი პერიოდია, ვერტიკალური სვეტი კი — ჯგუფი.",
                        "ერთი ჯგუფის ელემენტებს გარე შრეზე ელექტრონების ერთნაირი რაოდენობა აქვს, ამიტომ ისინი მსგავსად იქცევა.",
                        "1-ლი ჯგუფის ლითიუმი, ნატრიუმი და კალიუმი წყალთან აქტიურად რეაგირებს. მე-18 ჯგუფის კეთილშობილი აირები თითქმის არ რეაგირებს.",
                        "ამიერიდან პერიოდულ ცხრილში ელემენტის ჯგუფსა და პერიოდსაც დაინახავ.",
                    },
                    Bins = new[] { "1-ლი ჯგუფი", "მე-17 ჯგუფი", "მე-18 ჯგუფი" }, Hint = "1-ლი — აქტიური ლითონები; მე-17 — ჰალოგენები; მე-18 — კეთილშობილი აირები.",
                    Items = new[]
                    {
                        I("Li • ლითიუმი", 0, "ლითიუმი ტუტე ლითონია, 1-ლ ჯგუფში."),
                        I("F • ფთორი", 1, "ფთორი ჰალოგენია."),
                        I("He • ჰელიუმი", 2, "ჰელიუმი კეთილშობილი აირია."),
                        I("Na • ნატრიუმი", 0, "ნატრიუმი ლითიუმის ქვეშაა."),
                        I("Cl • ქლორი", 1, "ქლორი ჰალოგენია, ფთორის ქვეშ."),
                        I("Ne • ნეონი", 2, "ნეონი კეთილშობილი აირია."),
                        I("K • კალიუმი", 0, "კალიუმი ნატრიუმის ქვეშაა."),
                        I("Ar • არგონი", 2, "არგონი კეთილშობილი აირია."),
                    },
                    Questions = new[]
                    {
                        Q("რა არის ჯგუფი?", "სვეტი.", "ცხრილის ვერტიკალური სვეტი", "ჯგუფის ელემენტები ერთმანეთის ქვეშ დგას.",
                            "ჰორიზონტალური მწკრივი", "მწკრივი პერიოდია.", "ერთი ფერის უჯრები", "ფერი მხოლოდ დამხმარეა."),
                        Q("რატომ იქცევა ერთი ჯგუფის ელემენტები მსგავსად?", "იფიქრე გარე შრეზე.", "გარე შრეზე ელექტრონების რაოდენობა ერთნაირია", "რეაქციაში ძირითადად გარე ელექტრონები მონაწილეობს.",
                            "მასა ერთნაირი აქვთ", "ჯგუფში მასა ძალიან განსხვავდება.", "ერთ წელს აღმოაჩინეს", "აღმოჩენის თარიღი ქცევაზე არ მოქმედებს."),
                        Q("ნატრიუმი წყალთან აქტიურად რეაგირებს. რომელი ელემენტი მოიქცევა მსგავსად?", "მოძებნე იგივე ჯგუფში.", "კალიუმი", "კალიუმიც 1-ლ ჯგუფშია.",
                            "ნეონი", "ნეონი კეთილშობილი აირია და თითქმის არ რეაგირებს.", "ჟანგბადი", "ჟანგბადი სხვა ჯგუფშია."),
                    },
                    Note = "პერიოდი — ჰორიზონტალური მწკრივი.\nჯგუფი — ვერტიკალური სვეტი; მსგავსი თვისებები.\n1-ლი ჯგუფი: Li, Na, K — აქტიური ლითონები.\nმე-17: F, Cl — ჰალოგენები • მე-18: He, Ne, Ar — კეთილშობილი აირები.",
                },
                Review(3),

                // ================================================= 4. Molecules and formulas
                new Lesson
                {
                    ID = "c4_molecule", Chapter = 4, Kind = PracticeKind.Molecule, Title = "მოლეკულა",
                    Why = "მოლეკულები ის „სამშენებლო ბლოკებია“, რომლებიც რეაქციებში ერთმანეთს ხვდება.",
                    Teach = new[]
                    {
                        "მოლეკულა ორი ან მეტი ერთმანეთთან დაკავშირებული ატომია.",
                        "მოლეკულა შეიძლება ერთი ელემენტის ატომებისგან შედგებოდეს: ჰაერში ჟანგბადი O2-ის სახითაა — ორი ჟანგბადის ატომი.",
                        "ან სხვადასხვა ელემენტისგან: წყლის მოლეკულაში წყალბადიცაა და ჟანგბადიც.",
                    },
                    Steps = new[]
                    {
                        P("ავაწყობ ჟანგბადის მოლეკულას: ორი O ატომი.", "O2 — ჟანგბადის მოლეკულა, რომელსაც ვსუნთქავთ.", new[] { "H", "O" }, new[] { 0, 2 }),
                        P("ააწყვე წყალბადის მოლეკულა H2.", "H2 მზადაა: ორი წყალბადის ატომი ერთად.", new[] { "H", "O" }, new[] { 2, 0 }),
                        new Practice { Goal = "ააწყვე მოლეკულა, რომელშიც ორი სხვადასხვა ელემენტია: წყალბადიც და ჟანგბადიც. ერთზე მეტი პასუხი არსებობს!", Done = "H2O — წყალი! ორი სხვადასხვა ელემენტი ერთ მოლეკულაში.", Counters = new[] { "H", "O" }, Solution = new[] { 2, 1 },
                            Check = v => v[0] == 0 || v[1] == 0 ? "ახლა მხოლოდ ერთი ელემენტია. დაამატე მეორე სახის ატომიც."
                                : v[0] == 2 && (v[1] == 1 || v[1] == 2) ? null : "ასეთი მოლეკულა არ არსებობს. მინიშნება: წყალში 2 წყალბადი და 1 ჟანგბადია.",
                            Named = v => v[1] == 2 ? "H2O2 — წყალბადის ზეჟანგი! ერთი ჟანგბადით მეტი და უკვე სულ სხვა ნივთიერებაა: ჭრილობის დასამუშავებლად იყენებენ." : null },
                    },
                    Questions = new[]
                    {
                        Q("რა არის მოლეკულა?", "ატომები ერთად.", "ერთმანეთთან დაკავშირებული ატომების ჯგუფი", "მოლეკულაში ატომები ბმებითაა დაკავშირებული.",
                            "ერთი თავისუფალი ელექტრონი", "ელექტრონი ატომის ნაწილია.", "ნებისმიერი ნარევი", "ნარევში ნივთიერებები ბმით არ არის დაკავშირებული."),
                        Q("რამდენი ატომია O2 მოლეკულაში?", "შეხედე პატარა რიცხვს.", "2", "ორი ჟანგბადის ატომი.",
                            "1", "ერთი ატომი მოლეკულა არ არის.", "3", "სამი ატომი ოზონშია, O3-ში."),
                        Q("შეიძლება მოლეკულა ერთი ელემენტისგან შედგებოდეს?", "გაიხსენე O2.", "დიახ, მაგალითად O2 და H2", "ორივე ერთი ელემენტის ატომებისგანაა.",
                            "არა, არასდროს", "O2 ამის მაგალითია.", "მხოლოდ ლითონებში", "O2 და H2 არალითონებისგანაა."),
                    },
                    Note = "მოლეკულა — დაკავშირებული ატომები.\nერთი ელემენტისგან: O2, H2, N2.\nრამდენიმე ელემენტისგან: H2O, CO2.",
                },
                new Lesson
                {
                    ID = "c4_compound", Chapter = 4, Kind = PracticeKind.Sort, Title = "ელემენტი და ნაერთი", Collect = "fact:salt",
                    Why = "ეს განსხვავება დაგეხმარება, ფორმულაში დაინახო, ერთი ელემენტია თუ რამდენიმე.",
                    Teach = new[]
                    {
                        "ნაერთი ნივთიერებაა, რომელშიც ორი ან მეტი სხვადასხვა ელემენტი ქიმიურადაა დაკავშირებული: H2O, CO2, NaCl.",
                        "ნაერთის თვისებები მისი ელემენტებისგან განსხვავდება. ნატრიუმი აქტიური ლითონია, ქლორი შხამიანი აირია, NaCl კი სუფრის მარილი!",
                        "ფორმულაში დიდი ასოები დათვალე: ერთი სახის სიმბოლო — ელემენტია, ორი და მეტი — ნაერთი.",
                    },
                    Bins = new[] { "ელემენტი", "ნაერთი" }, Hint = "დათვალე სხვადასხვა დიდი ასო ფორმულაში.",
                    Items = new[]
                    {
                        I("O2", 0, "მასში მხოლოდ ჟანგბადია."),
                        I("H2O", 1, "წყალბადი და ჟანგბადი — ნაერთი."),
                        I("Fe", 0, "რკინა ელემენტია."),
                        I("CO2", 1, "ნახშირბადი და ჟანგბადი — ნაერთი."),
                        I("NaCl", 1, "ნატრიუმი და ქლორი — ნაერთი."),
                        I("N2", 0, "მასში მხოლოდ აზოტია."),
                        I("He", 0, "ჰელიუმი ელემენტია."),
                        I("CH4", 1, "ნახშირბადი და წყალბადი — ნაერთი."),
                    },
                    Questions = new[]
                    {
                        Q("რომელია ნაერთი?", "ორი სხვადასხვა ელემენტი.", "CO2", "მასში ნახშირბადი და ჟანგბადია.",
                            "O2", "მასში მხოლოდ ჟანგბადია.", "Cu", "სპილენძი ელემენტია."),
                        Q("რისგან შედგება NaCl?", "დათვალე დიდი ასოები.", "ნატრიუმისა და ქლორისგან", "Na და Cl — ორი ელემენტი.",
                            "მხოლოდ ნატრიუმისგან", "ფორმულაში Cl-იც არის.", "აზოტისა და ნახშირბადისგან", "N და C ამ ფორმულაში არ არის."),
                        Q("ქლორი შხამიანია. რატომ არ არის მარილი შხამიანი?", "ნაერთს საკუთარი თვისებები აქვს.", "ნაერთს ახალი თვისებები აქვს", "ქიმიური ბმა ნივთიერებას სულ სხვა თვისებებს აძლევს.",
                            "მარილში ქლორი არ არის", "Cl მარილის ნაწილია.", "ქლორი მარილში დაიწვა", "წვა აქ არ ხდება."),
                    },
                    Note = "ელემენტი — ერთი ტიპის ატომები (Fe, O2).\nნაერთი — ორი ან მეტი ელემენტი, დაკავშირებული ბმით (H2O, NaCl).\nნაერთს ახალი თვისებები აქვს.",
                },
                new Lesson
                {
                    ID = "c4_formula", Chapter = 4, Kind = PracticeKind.Molecule, Title = "ქიმიური ფორმულა და ინდექსი",
                    Why = "ფორმულის წაკითხვა რეაქციებისა და განტოლებების საფუძველია: ზუსტად გეცოდინება, რამდენი ატომი გაქვს.",
                    Teach = new[]
                    {
                        "ფორმულა გვიჩვენებს, რომელი ატომები და რამდენი შედის მოლეკულაში.",
                        "სიმბოლოს შემდეგ პატარა რიცხვი, ინდექსი, ატომების რაოდენობაა. H2O: 2 წყალბადი, 1 ჟანგბადი.",
                        "თუ ინდექსი არ წერია, ესე იგი 1-ია. CO2: 1 ნახშირბადი და 2 ჟანგბადი.",
                        "ინდექსის შეცვლა ნივთიერებას ცვლის: H2O წყალია, H2O2 კი — წყალბადის ზეჟანგი.",
                    },
                    Steps = new[]
                    {
                        P("CO2: ერთი C და ორი O.", "C-ს შემდეგ ინდექსი არ წერია, ამიტომ ნახშირბადი ერთია.", new[] { "C", "O", "H" }, new[] { 1, 2, 0 }),
                        P("წაიკითხე ფორმულა H2O და ააწყვე ის.", "H2O: H-ის შემდეგ 2, O-ს შემდეგ არაფერი, ანუ 1.", new[] { "C", "O", "H" }, new[] { 0, 1, 2 }),
                        P("წაიკითხე მეთანის ფორმულა CH4 და ააწყვე ის.", "CH4: ერთი ნახშირბადი და ოთხი წყალბადი. ეს ბუნებრივი აირია.", new[] { "C", "O", "H" }, new[] { 1, 0, 4 }),
                    },
                    Questions = new[]
                    {
                        Q("რამდენი წყალბადის ატომია H2O-ში?", "შეხედე H-ის შემდეგ რიცხვს.", "2", "ინდექსი 2 H-ის შემდეგ წერია.",
                            "1", "ერთია ჟანგბადის ატომი.", "3", "სულ 3 ატომია, წყალბადი კი 2."),
                        Q("რას ნიშნავს CO2-ში ციფრი 2?", "რომელ სიმბოლოს მოსდევს?", "ჟანგბადის ორ ატომს", "ინდექსი მის წინ მდგარ სიმბოლოს ეკუთვნის.",
                            "ნახშირბადის ორ ატომს", "C-ს შემდეგ ციფრი არ წერია, ესე იგი 1-ია.", "ორ მოლეკულას", "მოლეკულების რაოდენობას ფორმულის წინ დაწერილი დიდი რიცხვი გვიჩვენებს."),
                        Q("რა მოხდება, თუ H2O-ს H2O2-ით შევცვლით?", "ინდექსი ნივთიერებას ცვლის.", "სხვა ნივთიერება მიიღება", "H2O2 წყალბადის ზეჟანგია და არა წყალი.",
                            "მეტი წყალი გვექნება", "მეტი წყლისთვის კოეფიციენტი გვჭირდება.", "არაფერი შეიცვლება", "ინდექსის შეცვლა ფორმულას ცვლის."),
                    },
                    Note = "ფორმულა: სიმბოლოები + ინდექსები.\nინდექსი — ატომების რაოდენობა (H2O: H 2, O 1).\nინდექსის გარეშე = 1.\nსხვა ინდექსი = სხვა ნივთიერება.",
                },
                new Lesson
                {
                    ID = "c4_salt", Chapter = 4, Kind = PracticeKind.Molecule, Title = "H2O, O2, CO2 და NaCl", Collect = "fact:water",
                    Why = "ეს ოთხი ფორმულა ყველაზე ხშირად შეგხვდება რეაქციებში, ხსნარებსა და მჟავა-ფუძის ცდებში.",
                    Teach = new[]
                    {
                        "H2O — წყალი. O2 — ჟანგბადი, რომელსაც ვსუნთქავთ. CO2 — ნახშირორჟანგი, რომელსაც ამოვისუნთქავთ.",
                        "NaCl — სუფრის მარილი. ის ნატრიუმისა და ქლორის თანაბარი რაოდენობისგან შედგება.",
                        "მარილი კრისტალია და არა ცალკეული მოლეკულები, მაგრამ ფორმულა მაინც თანაფარდობას გვიჩვენებს: 1 Na : 1 Cl.",
                    },
                    Steps = new[]
                    {
                        P("სუფრის მარილი: ერთი Na და ერთი Cl.", "NaCl — თანაფარდობა 1 : 1.", new[] { "C", "O", "Na", "Cl" }, new[] { 0, 0, 1, 1 }),
                        P("ააწყვე ნახშირორჟანგი CO2.", "CO2 მზადაა: ამ აირს ამოვისუნთქავთ.", new[] { "C", "O", "Na", "Cl" }, new[] { 1, 2, 0, 0 }),
                        new Practice { Goal = "ააწყვე ნაერთი მხოლოდ ნახშირბადისა და ჟანგბადისგან. ორი სწორი პასუხი არსებობს!", Done = "CO — ნახშირჟანგი, ანუ მხუთავი აირი. ერთი ჟანგბადით ნაკლები და ძალიან საშიშია: ამიტომ ღუმელიან ოთახს ანიავებენ.", Counters = new[] { "C", "O", "Na", "Cl" }, Solution = new[] { 1, 1, 0, 0 },
                            Check = v => v[2] + v[3] > 0 ? "ნატრიუმი და ქლორი ამჯერად არ გვჭირდება." : v[0] != 1 ? "ერთი ნახშირბადის ატომი საკმარისია."
                                : v[1] == 1 || v[1] == 2 ? null : "ნახშირბადს 1 ან 2 ჟანგბადი შეიძლება მიუერთდეს.",
                            Named = v => v[1] == 2 ? "CO2 — ნახშირორჟანგი, სწორია. მეორე პასუხიც არსებობს: სცადე ერთი ჟანგბადით." : null },
                    },
                    Questions = new[]
                    {
                        Q("რომელი ფორმულაა სუფრის მარილი?", "ნატრიუმი და ქლორი.", "NaCl", "Na — ნატრიუმი, Cl — ქლორი.",
                            "CO2", "ეს ნახშირორჟანგია.", "H2O", "ეს წყალია."),
                        Q("რომელ აირს ამოვისუნთქავთ?", "ნახშირბადი + ჟანგბადი.", "CO2", "სხეული CO2-ს გამოყოფს.",
                            "O2", "O2-ს ჩავისუნთქავთ.", "NaCl", "მარილი აირი არ არის."),
                        Q("რამდენი ატომია CO2-ის ერთ მოლეკულაში?", "1 + 2.", "3", "1 ნახშირბადი + 2 ჟანგბადი = 3.",
                            "2", "ჟანგბადი 2-ია, მაგრამ ნახშირბადიც ითვლება.", "4", "მხოლოდ 3 ატომია."),
                    },
                    Note = "H2O — წყალი • O2 — ჟანგბადი\nCO2 — ნახშირორჟანგი • NaCl — სუფრის მარილი (1 Na : 1 Cl).",
                },
                Review(4),

                // ================================================= 5. Chemical bonds
                new Lesson
                {
                    ID = "c5_why", Chapter = 5, Kind = PracticeKind.Sort, Title = "რატომ ერთდება ატომები",
                    Why = "ბმის მიზეზის ცოდნით გაიგებ, რატომ წარმოიქმნება ზოგი ნაერთი და ზოგი — არა.",
                    Teach = new[]
                    {
                        "ატომის ქცევას ძირითადად გარე შრის ელექტრონები განსაზღვრავს.",
                        "კეთილშობილ აირებს (He, Ne, Ar) გარე შრე სავსე აქვთ, ამიტომ ისინი თითქმის არ რეაგირებს.",
                        "სხვა ატომებს გარე შრე არასრული აქვთ. ბმისას ისინი ელექტრონებს იღებენ, გასცემენ ან იზიარებენ და უფრო სტაბილურები ხდებიან.",
                    },
                    Bins = new[] { "გარე შრე სავსეა", "ბმას შექმნის" }, Hint = "კეთილშობილი აირები სტაბილურია, სხვები ბმას ქმნიან.",
                    Items = new[]
                    {
                        I("He • ჰელიუმი", 0, "ჰელიუმი კეთილშობილი აირია."),
                        I("Na • ნატრიუმი", 1, "ნატრიუმს ერთი ზედმეტი გარე ელექტრონი აქვს."),
                        I("Ne • ნეონი", 0, "ნეონის გარე შრე სავსეა."),
                        I("Cl • ქლორი", 1, "ქლორს ერთი ელექტრონი აკლია."),
                        I("O • ჟანგბადი", 1, "ჟანგბადს ორი ელექტრონი აკლია."),
                        I("Ar • არგონი", 0, "არგონი კეთილშობილი აირია."),
                        I("H • წყალბადი", 1, "წყალბადს ერთი ელექტრონი აკლია."),
                        I("C • ნახშირბადი", 1, "ნახშირბადი ოთხ ბმას ქმნის."),
                    },
                    Questions = new[]
                    {
                        Q("რატომ არ რეაგირებს ნეონი თითქმის არაფერთან?", "გარე შრე.", "მისი გარე შრე სავსეა", "სავსე შრე სტაბილურია.",
                            "ის ძალიან მძიმეა", "ნეონი მსუბუქი აირია.", "მას ელექტრონები არ აქვს", "ნეონს 10 ელექტრონი აქვს."),
                        Q("რას ცდილობს ატომი ბმის შექმნით?", "სტაბილურობა.", "გარე შრის შევსებას", "ელექტრონების მიღება, გაცემა ან გაზიარება გარე შრეს ავსებს.",
                            "პროტონების დაკარგვას", "ქიმიურ ბმაში პროტონები არ იცვლება.", "სხვა ელემენტად ქცევას", "ელემენტი უცვლელი რჩება."),
                        Q("რომელი ნაწილაკები მონაწილეობს ქიმიურ ბმაში?", "გარე შრე.", "გარე ელექტრონები", "ბმა გარე ელექტრონებით იქმნება.",
                            "ნეიტრონები", "ნეიტრონები ბირთვშია.", "მთელი ბირთვი", "ბირთვი ბმაში არ ერთვება."),
                    },
                    Note = "ბმაში გარე ელექტრონები მონაწილეობს.\nსავსე გარე შრე = სტაბილური ატომი (He, Ne, Ar).\nსხვა ატომები ელექტრონს იღებენ, გასცემენ ან იზიარებენ.",
                },
                new Lesson
                {
                    ID = "c5_ionic", Chapter = 5, Kind = PracticeKind.Ions, Title = "იონური ბმა", Elements = new[] { "Mg" },
                    Why = "იონური ბმა მარილების ფორმულებს ხსნის. მას ხსნარებისა და მჟავა-ფუძის თავებშიც შეხვდები.",
                    Teach = new[]
                    {
                        "ნატრიუმი თავის ერთ გარე ელექტრონს ქლორს გადასცემს.",
                        "ნატრიუმი Na⁺ იონად (დადებითი), ქლორი კი Cl⁻ იონად (უარყოფითი) იქცევა.",
                        "საწინააღმდეგო მუხტები ერთმანეთს იზიდავს — ეს იონური ბმაა. ის ხშირად ლითონსა და არალითონს შორის წარმოიქმნება.",
                        "ნაერთის მუხტების ჯამი ნულია: Mg²⁺-ს ორი Cl⁻ სჭირდება — MgCl2.",
                    },
                    Steps = new[]
                    {
                        new Practice { Goal = "Na⁺ + Cl⁻: +1 და −1 ერთმანეთს აბათილებს.", Done = "NaCl — ჯამური მუხტი 0.", Counters = new[] { "Na⁺", "Cl⁻" }, Charges = new[] { 1, -1 }, Solution = new[] { 1, 1 } },
                        new Practice { Goal = "ააწყვე ნეიტრალური მაგნიუმის ქლორიდი: მუხტების ჯამი ნული უნდა იყოს.", Done = "MgCl2: +2 − 1 − 1 = 0.", Counters = new[] { "Mg²⁺", "Cl⁻" }, Charges = new[] { 2, -1 }, Solution = new[] { 1, 2 } },
                        new Practice { Goal = "ააწყვე კალციუმის ოქსიდი იონების უმცირესი რაოდენობით.", Done = "CaO: +2 − 2 = 0.", Counters = new[] { "Ca²⁺", "O²⁻" }, Charges = new[] { 2, -2 }, Solution = new[] { 1, 1 } },
                    },
                    Questions = new[]
                    {
                        Q("რა ხდება იონური ბმის წარმოქმნისას?", "გადაცემა.", "ელექტრონი ერთი ატომიდან მეორეზე გადადის", "ასე წარმოიქმნება დადებითი და უარყოფითი იონები.",
                            "ატომები ბირთვებს ცვლიან", "ბირთვი არ იცვლება.", "ელექტრონები ქრება", "ელექტრონები მხოლოდ გადადის."),
                        Q("ნატრიუმმა ერთი ელექტრონი გასცა. რა იონი მიიღო?", "ელექტრონი უარყოფითია.", "Na⁺", "ერთი უარყოფითი მუხტის დაკარგვის შემდეგ +1 რჩება.",
                            "Na⁻", "უარყოფითი იონი ელექტრონის მიღებით წარმოიქმნება.", "Na²⁺", "ამისთვის ორი ელექტრონი უნდა გაეცა."),
                        Q("რამდენი Cl⁻ სჭირდება ერთ Mg²⁺ იონს?", "+2 და −1.", "2", "+2 − 1 − 1 = 0, ამიტომ MgCl2.",
                            "1", "მაშინ ჯამი +1 დარჩებოდა.", "3", "მაშინ ჯამი −1 იქნებოდა."),
                    },
                    Note = "იონური ბმა — ელექტრონის გადაცემა (ლითონი → არალითონი).\nNa → Na⁺, Cl → Cl⁻; საწინააღმდეგო მუხტები იზიდავს.\nმუხტების ჯამი = 0: NaCl, MgCl2, CaO.",
                },
                new Lesson
                {
                    ID = "c5_covalent", Chapter = 5, Kind = PracticeKind.Pairs, Title = "კოვალენტური ბმა",
                    Why = "კოვალენტური ბმით არის აგებული წყალი, ჟანგბადი, ნახშირორჟანგი და სიცოცხლის თითქმის ყველა მოლეკულა.",
                    Teach = new[]
                    {
                        "არალითონები ელექტრონებს ერთმანეთს არ გადასცემენ — ისინი მათ იზიარებენ.",
                        "ერთი საერთო ელექტრონული წყვილი ერთმაგი ბმაა: H–H. ორი წყვილი ორმაგი ბმაა: O=O.",
                        "აზოტის მოლეკულაში N≡N სამმაგი ბმაა — სამი საერთო წყვილი.",
                    },
                    Steps = new[]
                    {
                        new Practice { Goal = "H2: თითოეულ წყალბადს ერთი ელექტრონი აკლია, ამიტომ ისინი ერთ წყვილს იზიარებენ.", Done = "H–H: ერთი საერთო წყვილი.", Counters = new[] { "საერთო წყვილები" }, Formula = "H", Solution = new[] { 1 } },
                        new Practice { Goal = "ჟანგბადის თითოეულ ატომს ორი ელექტრონი აკლია. რამდენი წყვილი უნდა გაიზიარონ?", Done = "O=O: ორი საერთო წყვილი, ორმაგი ბმა.", Counters = new[] { "საერთო წყვილები" }, Formula = "O", Solution = new[] { 2 } },
                        new Practice { Goal = "აზოტის ატომს სამი ელექტრონი აკლია. ააწყვე N2-ის ბმა.", Done = "N≡N: სამმაგი ბმა, ძალიან მტკიცე.", Counters = new[] { "საერთო წყვილები" }, Formula = "N", Solution = new[] { 3 } },
                    },
                    Questions = new[]
                    {
                        Q("რა ხდება კოვალენტური ბმისას?", "გაზიარება.", "ატომები ელექტრონებს იზიარებენ", "საერთო წყვილი ორივე ატომს ეკუთვნის.",
                            "ერთი ატომი მეორეს ელექტრონს მთლიანად აძლევს", "ეს იონური ბმაა.", "პროტონები გადადის", "პროტონები ბირთვში რჩება."),
                        Q("რამდენი საერთო წყვილია O=O ბმაში?", "ორი ხაზი.", "2", "ორმაგი ბმა ორ წყვილს ნიშნავს.",
                            "1", "ერთი წყვილი ერთმაგი ბმაა.", "3", "სამი წყვილი სამმაგი ბმაა."),
                        Q("რომელ ელემენტებს შორის იქმნება ხშირად კოვალენტური ბმა?", "გაიხსენე წყალი, H2O.", "არალითონებს შორის", "მაგალითად, წყალბადსა და ჟანგბადს შორის.",
                            "ლითონსა და არალითონს შორის", "ამ შემთხვევაში ბმა ხშირად იონურია.", "მხოლოდ კეთილშობილ აირებს შორის", "ისინი თითქმის არ ქმნიან ბმებს."),
                    },
                    Note = "კოვალენტური ბმა — ელექტრონული წყვილების გაზიარება (არალითონები).\nერთმაგი H–H • ორმაგი O=O • სამმაგი N≡N.",
                },
                Review(5),

                // ================================================= 6. Chemical reactions
                new Lesson
                {
                    ID = "c6_reactants", Chapter = 6, Kind = PracticeKind.Sort, Title = "რეაგენტები და პროდუქტები",
                    Why = "ყველა განტოლება ასე იკითხება: მარცხნივ — რაც გვქონდა, მარჯვნივ — რაც მივიღეთ.",
                    Teach = new[]
                    {
                        "ქიმიური რეაქციისას ნივთიერებები ახალ ნივთიერებებად გარდაიქმნება.",
                        "საწყის ნივთიერებებს რეაგენტები ჰქვია. ისინი ისრის მარცხნივ იწერება.",
                        "მიღებულ ნივთიერებებს პროდუქტები ჰქვია. ისინი ისრის მარჯვნივ იწერება. ისარი ნიშნავს: „წარმოქმნის“.",
                        "მაგალითად: C + O2 → CO2. ნახშირბადი და ჟანგბადი ნახშირორჟანგს წარმოქმნის.",
                    },
                    Bins = new[] { "რეაგენტი", "პროდუქტი" }, Hint = "ისრის მარცხნივ — რეაგენტი, მარჯვნივ — პროდუქტი.",
                    Items = new[]
                    {
                        I("C + O2 → CO2:  C", 0, "C ისრის მარცხნივაა."),
                        I("C + O2 → CO2:  CO2", 1, "CO2 ისრის მარჯვნივაა."),
                        I("2H2 + O2 → 2H2O:  H2", 0, "H2 ისრის მარცხნივაა."),
                        I("2H2 + O2 → 2H2O:  H2O", 1, "წყალი რეაქციის პროდუქტია."),
                        I("Zn + 2HCl → ZnCl2 + H2:  HCl", 0, "HCl ისრის მარცხნივაა."),
                        I("Zn + 2HCl → ZnCl2 + H2:  H2", 1, "H2 ისრის მარჯვნივაა: ის აირად გამოიყოფა."),
                    },
                    Questions = new[]
                    {
                        Q("C + O2 → CO2. რომელია პროდუქტი?", "ისრის მარჯვნივ.", "CO2", "CO2 ისრის მარჯვნივაა.",
                            "C", "C რეაგენტია.", "O2", "O2 რეაგენტია."),
                        Q("რას ნიშნავს განტოლებაში ისარი →?", "რეაგენტებიდან პროდუქტებისკენ.", "„წარმოქმნის“", "ისარი რეაქციის მიმართულებას გვიჩვენებს.",
                            "„ტოლია“", "ისარი ტოლობის ნიშანი არ არის.", "„გამოკლება“", "ეს მათემატიკური ნიშანი არ არის."),
                        Q("სად იწერება რეაგენტები?", "საწყისი ნივთიერებები.", "ისრის მარცხნივ", "რეაგენტები მარცხნივაა, პროდუქტები — მარჯვნივ.",
                            "ისრის მარჯვნივ", "იქ პროდუქტებია.", "ისრის ზემოთ", "ისრის ზემოთ ზოგჯერ პირობებს წერენ, მაგალითად ტემპერატურას."),
                    },
                    Note = "რეაგენტები → პროდუქტები.\nC + O2 → CO2.\nისარი = „წარმოქმნის“.",
                },
                new Lesson
                {
                    ID = "c6_signs", Chapter = 6, Kind = PracticeKind.Sort, Title = "ქიმიური რეაქციის ნიშნები",
                    Why = "ნიშნებით ლაბორატორიაში დაადგენ, მოხდა თუ არა რეაქცია.",
                    Teach = new[]
                    {
                        "რეაქცია ნიშნებით ჩანს: ფერის შეცვლა, აირის ბუშტუკები, ნალექი, სითბო, სინათლე ან სუნი.",
                        "ნალექი ხსნარში წარმოქმნილი მყარი ნაწილაკებია — სითხე იმღვრევა.",
                        "ფრთხილად: დნობა და დუღილი ცვლილებაა, მაგრამ რეაქცია არა. საჭიროა ახალი ნივთიერება.",
                    },
                    Bins = new[] { "რეაქციის ნიშანი", "არ არის რეაქციის ნიშანი" }, Hint = "იკითხე: გაჩნდა ახალი ნივთიერება?",
                    Items = new[]
                    {
                        I("ძმარში სოდის ჩაყრისას ბუშტუკები", 0, "გამოიყოფა ახალი აირი — ნახშირორჟანგი."),
                        I("წყლის გადასხმა ერთი ჭიქიდან მეორეში", 1, "ნივთიერება იგივე დარჩა."),
                        I("ხსნარში თეთრი ნალექის გაჩენა", 0, "ნალექი ახალი მყარი ნივთიერებაა."),
                        I("ყინულის დნობა", 1, "ეს მდგომარეობის ცვლილებაა."),
                        I("ფერის მოულოდნელი შეცვლა", 0, "ახალი ფერი ხშირად ახალ ნივთიერებას ნიშნავს."),
                        I("სანთლის ალი და სითბო", 0, "წვა სითბოსა და სინათლეს გამოყოფს."),
                        I("შაქრის გახსნა ჩაიში", 1, "შაქარი ქიმიურად არ შეცვლილა."),
                    },
                    Questions = new[]
                    {
                        Q("რომელი ნიშანი მიუთითებს რეაქციაზე?", "ახალი აირი.", "ბუშტუკები ორი სითხის შერევისას", "ახალი აირი ახალ ნივთიერებას ნიშნავს.",
                            "წყლის დუღილი", "ორთქლი ისევ წყალია.", "ქაღალდის დახევა", "ეს ფიზიკური ცვლილებაა."),
                        Q("რა არის ნალექი?", "სითხე იმღვრევა.", "ხსნარში წარმოქმნილი მყარი ნივთიერება", "ორი ხსნარის შერევისას შეიძლება უხსნადი მყარი წარმოიქმნას.",
                            "აორთქლებული წყალი", "ეს ორთქლია.", "ნებისმიერი ფერადი ხსნარი", "ფერი ჯერ კიდევ ნალექი არ არის."),
                        Q("რატომ არ არის დნობა ქიმიური რეაქცია?", "ახალი ნივთიერება?", "ახალი ნივთიერება არ წარმოიქმნება", "დამდნარი ყინული ისევ წყალია.",
                            "იმიტომ, რომ სითბო სჭირდება", "ბევრ რეაქციასაც სითბო სჭირდება.", "იმიტომ, რომ სწრაფად ხდება", "სიჩქარე აქ მთავარი არ არის."),
                    },
                    Note = "რეაქციის ნიშნები: ფერი • აირი • ნალექი • სითბო • სინათლე • სუნი.\nმთავარია, რომ ახალი ნივთიერება წარმოიქმნას.",
                },
                new Lesson
                {
                    ID = "c6_conserve", Chapter = 6, Kind = PracticeKind.Equation, Title = "ატომების შენახვა და კოეფიციენტები",
                    Why = "ეს დაბალანსების საფუძველია. რთულ გამოწვევებში ყველა განტოლებაში დაგჭირდება.",
                    Teach = new[]
                    {
                        "რეაქციისას ატომები არ ქრება და არ ჩნდება — ისინი მხოლოდ ახლებურად ლაგდება.",
                        "ამიტომ ისრის ორივე მხარეს თითოეული ელემენტის ატომების რაოდენობა ტოლი უნდა იყოს.",
                        "ატომების გასათანაბრებლად ფორმულის წინ დიდ რიცხვს, კოეფიციენტს ვწერთ. ინდექსებს არ ვცვლით!",
                        "2H2 + O2 → 2H2O ნიშნავს: 2 წყალბადისა და 1 ჟანგბადის მოლეკულა 2 წყლის მოლეკულას იძლევა. თანაფარდობაა 2 : 1 : 2.",
                    },
                    Steps = new[]
                    {
                        Equation("მარჯვნივ 2 HCl დავწერე, რომ წყალბადიც და ქლორიც გათანაბრდეს.", "ორივე მხარეს 2 H და 2 Cl-ია.", "H2 + Cl2 -> HCl", 1, 1, 2),
                        Equation("დააბალანსე წყლის მიღების განტოლება. დაიწყე ჟანგბადით.", "4 H და 2 O ორივე მხარეს: 2H2 + O2 → 2H2O.", "H2 + O2 -> H2O", 2, 1, 2),
                        Equation("დააბალანსე ამიაკის მიღების განტოლება.", "N2 + 3H2 → 2NH3: 2 N და 6 H ორივე მხარეს.", "N2 + H2 -> NH3", 1, 3, 2),
                    },
                    Questions = new[]
                    {
                        Q("რა უნდა იყოს ტოლი დაბალანსებული განტოლების ორივე მხარეს?", "ატომები არ ქრება.", "თითოეული ელემენტის ატომების რაოდენობა", "ატომები ინახება.",
                            "მოლეკულების რაოდენობა", "2H2 + O2 → 2H2O-ში მარცხნივ 3 მოლეკულაა, მარჯვნივ კი — 2.", "ფორმულების ფერი", "ფერი აქ არაფერ შუაშია."),
                        Q("განტოლების დასაბალანსებლად რას ვცვლით?", "დიდი რიცხვი ფორმულის წინ.", "კოეფიციენტებს", "კოეფიციენტი მოლეკულების რაოდენობას ცვლის და არა ნივთიერებას.",
                            "ინდექსებს", "ინდექსის შეცვლა სხვა ნივთიერებას ქმნის.", "სიმბოლოებს", "სხვა სიმბოლო სხვა ელემენტს ნიშნავს."),
                        Q("2H2 + O2 → 2H2O. რამდენი O2 მოლეკულაა საჭირო 4 H2 მოლეკულისთვის?", "თანაფარდობა 2 : 1.", "2", "H2 : O2 = 2 : 1, ამიტომ 4 : 2.",
                            "4", "ეს ორჯერ მეტია, ვიდრე საჭიროა.", "1", "ერთი O2 მხოლოდ ორ H2-ს ეყოფა."),
                    },
                    Note = "ატომები ინახება: მარცხნივ = მარჯვნივ.\nკოეფიციენტი (დიდი რიცხვი წინ) — მოლეკულების რაოდენობა.\nინდექსს არ ვცვლით!\n2H2 + O2 → 2H2O • თანაფარდობა 2 : 1 : 2.",
                },
                new Lesson
                {
                    ID = "c6_types", Chapter = 6, Kind = PracticeKind.Sort, Title = "შეერთება, დაშლა და წვა",
                    Why = "რეაქციის ტიპის ცოდნით პროდუქტებს წინასწარ გამოიცნობ. რთულ გამოწვევებშიც ტიპის ამოცნობის კითხვები გელოდება.",
                    Teach = new[]
                    {
                        "შეერთება: ორი ან მეტი ნივთიერება ერთ პროდუქტს ქმნის. N2 + 3H2 → 2NH3.",
                        "დაშლა: ერთი ნივთიერება რამდენიმე ნივთიერებად იშლება. 2H2O2 → 2H2O + O2.",
                        "წვა: საწვავი ჟანგბადთან რეაგირებს და სითბოსა და სინათლეს გამოყოფს. CH4 + 2O2 → CO2 + 2H2O.",
                    },
                    Bins = new[] { "შეერთება", "დაშლა", "წვა" }, Hint = "დათვალე რეაგენტები და პროდუქტები. O2 და CO2 + H2O — წვის ნიშანია.",
                    Items = new[]
                    {
                        I("N2 + 3H2 → 2NH3", 0, "ორი რეაგენტი, ერთი პროდუქტი."),
                        I("CaCO3 → CaO + CO2", 1, "ერთი ნივთიერება ორად იშლება."),
                        I("CH4 + 2O2 → CO2 + 2H2O", 2, "საწვავი ჟანგბადში იწვის; მიიღება CO2 და H2O."),
                        I("2Na + Cl2 → 2NaCl", 0, "ორი ელემენტისგან ერთი ნაერთი."),
                        I("2H2O2 → 2H2O + O2", 1, "წყალბადის ზეჟანგი იშლება."),
                        I("C3H8 + 5O2 → 3CO2 + 4H2O", 2, "ეს პროპანის წვაა."),
                    },
                    Questions = new[]
                    {
                        Q("რა ტიპისაა CaCO3 → CaO + CO2?", "რამდენი რეაგენტია?", "დაშლა", "ერთი ნივთიერება ორად იშლება.",
                            "შეერთება", "შეერთებისას პროდუქტი ერთია.", "წვა", "რეაგენტებში ჟანგბადი არ არის."),
                        Q("რა მიიღება ნახშირწყალბადის სრული წვისას?", "C → ?, H → ?", "CO2 და H2O", "ნახშირბადი CO2-ად, წყალბადი კი H2O-ად იქცევა.",
                            "მხოლოდ O2", "ჟანგბადი იხარჯება.", "NaCl", "ნატრიუმი და ქლორი აქ არ არის."),
                        Q("რა ტიპისაა 2Na + Cl2 → 2NaCl?", "ერთი პროდუქტი.", "შეერთება", "ორი ელემენტი ერთ ნაერთად ერთდება.",
                            "დაშლა", "დაშლაში ერთი რეაგენტია.", "წვა", "აქ ჟანგბადი არ მონაწილეობს."),
                    },
                    Note = "შეერთება: A + B → AB\nდაშლა: AB → A + B\nწვა: საწვავი + O2 → CO2 + H2O (+ სითბო, სინათლე).",
                },
                new Lesson
                {
                    ID = "c6_acidbase", Chapter = 6, Kind = PracticeKind.Sort, Title = "მჟავა ხვდება ფუძეს",
                    Why = "ეს რეაქცია ხსნის, რატომ აქრობს სოდა მჟავიანობას. მჟავებისა და ფუძეების თავში მას დაწვრილებით შევისწავლით.",
                    Teach = new[]
                    {
                        "მჟავები, მაგალითად HCl, წყალში H⁺ იონებს გამოყოფს. ფუძეები, მაგალითად NaOH, OH⁻ იონებს იძლევა.",
                        "როცა ისინი ერთმანეთს ხვდება, H⁺ და OH⁻ წყლად ერთდება: H⁺ + OH⁻ → H2O.",
                        "დარჩენილი იონებისგან მარილი მიიღება: HCl + NaOH → NaCl + H2O. ამას ნეიტრალიზაცია ჰქვია.",
                    },
                    Bins = new[] { "მჟავა", "ფუძე", "პროდუქტი" }, Hint = "H წინ — ხშირად მჟავა; OH ბოლოში — ფუძე.",
                    Items = new[]
                    {
                        I("HCl", 0, "ჰიდროქლორმჟავა H⁺-ს გამოყოფს."),
                        I("NaOH", 1, "ნატრიუმის ჰიდროქსიდი OH⁻-ს გამოყოფს."),
                        I("NaCl", 2, "მარილი ნეიტრალიზაციის პროდუქტია."),
                        I("H2O", 2, "H⁺ და OH⁻ წყალს ქმნის."),
                        I("H2SO4", 0, "გოგირდმჟავა ძლიერი მჟავაა."),
                        I("KOH", 1, "კალიუმის ჰიდროქსიდი ფუძეა."),
                    },
                    Questions = new[]
                    {
                        Q("რა მიიღება მჟავისა და ფუძის რეაქციით?", "HCl + NaOH.", "მარილი და წყალი", "HCl + NaOH → NaCl + H2O.",
                            "მხოლოდ ჟანგბადი", "ჟანგბადი არ წარმოიქმნება.", "უფრო ძლიერი მჟავა", "ისინი ერთმანეთს ანეიტრალებს."),
                        Q("რომელი იონები ქმნის წყალს ნეიტრალიზაციისას?", "მჟავისა და ფუძის იონები.", "H⁺ და OH⁻", "H⁺ + OH⁻ → H2O.",
                            "Na⁺ და Cl⁻", "ისინი მარილს ქმნის.", "O²⁻ და Cl⁻", "ორივე უარყოფითია და ერთმანეთს არ იზიდავს."),
                        Q("რომელია ფუძე?", "ფორმულაში OH.", "NaOH", "ის OH⁻ იონებს იძლევა.",
                            "HCl", "HCl მჟავაა.", "NaCl", "NaCl მარილია."),
                    },
                    Note = "მჟავა → H⁺ (HCl, H2SO4) • ფუძე → OH⁻ (NaOH, KOH).\nნეიტრალიზაცია: მჟავა + ფუძე → მარილი + წყალი.\nH⁺ + OH⁻ → H2O.",
                },
                Review(6),

                // ================================================= 7. Mixtures and solutions
                new Lesson
                {
                    ID = "c7_mixtures", Chapter = 7, Kind = PracticeKind.Sort, Title = "სუფთა ნივთიერება და ნარევი", Collect = "fact:oxygen",
                    Why = "ბუნებაში ნივთიერებები თითქმის ყოველთვის ნარევშია. ეს ხსნარებისა და დაყოფის მეთოდების საფუძველია.",
                    Teach = new[]
                    {
                        "სუფთა ნივთიერებას მუდმივი შემადგენლობა აქვს: სუფთა წყალი, ოქრო, ჟანგბადი.",
                        "ნარევში რამდენიმე ნივთიერებაა შერეული ქიმიური ბმის გარეშე: ჰაერი, ზღვის წყალი, სალათი.",
                        "ნარევის ნაწილები თავის თვისებებს ინარჩუნებს, ამიტომ მათი ერთმანეთისგან დაყოფა შეიძლება.",
                    },
                    Bins = new[] { "სუფთა ნივთიერება", "ნარევი" }, Hint = "ერთი ნივთიერებაა, თუ რამდენიმე?",
                    Items = new[]
                    {
                        I("გამოხდილი წყალი", 0, "ეს სუფთა H2O-ა."),
                        I("ჰაერი", 1, "აზოტი, ჟანგბადი და სხვა აირები."),
                        I("ზღვის წყალი", 1, "წყალი და გახსნილი მარილები."),
                        I("ოქრო", 0, "ოქრო ელემენტია."),
                        I("სალათი", 1, "ბოსტნეულის ნაჭრები ერთმანეთში აურიე."),
                        I("ჟანგბადი O2", 0, "ერთი ნივთიერება."),
                        I("ქვიშა და რკინის ნაქლიბი", 1, "ორივე ნაწილი ჩანს და მაგნიტით გამოიყოფა."),
                        I("შაქარი", 0, "სუფთა შაქარი ერთი ნივთიერებაა."),
                    },
                    Questions = new[]
                    {
                        Q("რომელია ნარევი?", "რამდენი ნივთიერებაა?", "ჰაერი", "ჰაერი რამდენიმე აირის ნარევია.",
                            "ოქრო", "ოქრო ელემენტია.", "გამოხდილი წყალი", "ეს სუფთა H2O-ა."),
                        Q("რით განსხვავდება ნარევი ნაერთისგან?", "ბმა.", "ნარევში ნივთიერებები ბმით არ არის დაკავშირებული", "ამიტომ ნარევის ფიზიკურად დაყოფა შეიძლება.",
                            "ნარევი ყოველთვის სითხეა", "ქვიშისა და რკინის ნარევი მყარია.", "ნაერთის დაყოფა უფრო ადვილია", "პირიქით: ნაერთი ქიმიურად უნდა დაიშალოს."),
                        Q("რა არის ზღვის წყალი?", "წყალი + მარილი.", "ნარევი", "მასში წყალი და გახსნილი მარილებია.",
                            "სუფთა ნივთიერება", "მასში ერთზე მეტი ნივთიერებაა.", "ელემენტი", "წყალიც ნაერთია და მარილიც."),
                    },
                    Note = "სუფთა ნივთიერება — მუდმივი შემადგენლობა (H2O, Au, O2).\nნარევი — რამდენიმე ნივთიერება ბმის გარეშე (ჰაერი, ზღვის წყალი).\nნარევი ფიზიკურად იყოფა.",
                },
                new Lesson
                {
                    ID = "c7_dissolve", Chapter = 7, Kind = PracticeKind.Sort, Title = "გახსნა და ხსნადობა",
                    Why = "ბევრი რეაქცია ხსნარებში მიმდინარეობს, ჩვენს სხეულშიც კი. მომდევნო გაკვეთილზე კონცენტრაციას ზუსტად გამოვთვლით.",
                    Teach = new[]
                    {
                        "ხსნარი ერთგვაროვანი ნარევია. მაგალითად, შაქარი წყალში.",
                        "რასაც ხსნიან, ის გასახსნელი ნივთიერებაა (შაქარი). რაშიც ხსნიან — გამხსნელი (წყალი).",
                        "ხსნადობა გვიჩვენებს, რამდენი ნივთიერება იხსნება გამხსნელში. მარილი წყალში კარგად იხსნება, ქვიშა — არა.",
                        "ცხელ წყალში მყარი ნივთიერება ხშირად უფრო მეტი და უფრო სწრაფად იხსნება.",
                    },
                    Bins = new[] { "გამხსნელი", "წყალში იხსნება", "წყალში არ იხსნება" }, Hint = "წარმოიდგინე ჭიქა წყალი: გაქრება ნივთიერება წყალში, თუ ფსკერზე დარჩება?",
                    Items = new[]
                    {
                        I("წყალი", 0, "წყალი ყველაზე გავრცელებული გამხსნელია."),
                        I("შაქარი", 1, "შაქარი წყალში იხსნება და მას ატკბობს."),
                        I("ქვიშა", 2, "ქვიშა ფსკერზე ილექება."),
                        I("მარილი", 1, "მარილი წყალში კარგად იხსნება."),
                        I("ზეთი", 2, "ზეთი წყლის ზედაპირზე ცალკე ფენას ქმნის."),
                        I("სპირტი", 0, "სპირტიც გამხსნელია, მაგალითად სუნამოებში."),
                        I("ცარცის ფხვნილი", 2, "ცარცი წყალში თითქმის არ იხსნება."),
                    },
                    Questions = new[]
                    {
                        Q("შაქრის ხსნარში რა არის გამხსნელი?", "რაშიც ხსნიან.", "წყალი", "შაქარი წყალში იხსნება.",
                            "შაქარი", "შაქარი გასახსნელი ნივთიერებაა.", "ჭიქა", "ჭიქა ჭურჭელია."),
                        Q("როგორ დააჩქარებ შაქრის გახსნას?", "ტემპერატურა და მორევა.", "თბილი წყლით და მორევით", "ნაწილაკები უფრო სწრაფად მოძრაობს და ერთმანეთში ერევა.",
                            "ცივი წყლით", "ცივ წყალში შაქარი უფრო ნელა იხსნება.", "ჭიქის დახურვით", "დახურვა გახსნას არ აჩქარებს."),
                        Q("რას ნიშნავს: „ქვიშა წყალში არ იხსნება“?", "ფსკერი.", "ქვიშის ნაწილაკები ცალკე რჩება და ილექება", "ერთგვაროვანი ხსნარი არ წარმოიქმნება.",
                            "ქვიშა ქრება", "ქვიშა ფსკერზე ჩანს.", "ქვიშა წყლად იქცევა", "ახალი ნივთიერება არ წარმოიქმნება."),
                    },
                    Note = "ხსნარი = გასახსნელი ნივთიერება + გამხსნელი.\nშაქარი (გასახსნელი) + წყალი (გამხსნელი).\nხსნადობა — რამდენი იხსნება; სითბო მას ხშირად ზრდის.",
                },
                new Lesson
                {
                    ID = "c7_concentration", Chapter = 7, Kind = PracticeKind.Beaker, Title = "კონცენტრაცია და განზავება",
                    Why = "კონცენტრაციით ზუსტად ამზადებენ წამლებსა და ლაბორატორიულ ხსნარებს. ამ გაკვეთილის შემდეგ ხსნარების სადგურიც გაიხსნება.",
                    Teach = new[]
                    {
                        "კონცენტრაცია გვიჩვენებს, რამდენი ნივთიერებაა ხსნარის ყოველ ლიტრში.",
                        "ფორმულა: C = n / V. n — ნივთიერების რაოდენობა მოლებში, V — მოცულობა ლიტრებში. ერთეულია მოლ/ლ.",
                        "განზავება: წყლის დამატებისას ნივთიერება იგივე რჩება, მოცულობა იზრდება, კონცენტრაცია კი მცირდება.",
                    },
                    Steps = new[]
                    {
                        new Practice { Goal = "0.5 მოლი ნივთიერება 1 ლიტრში: C = 0.5 / 1 = 0.5 მოლ/ლ.", Done = "C = 0.50 მოლ/ლ.", Counters = new[] { "ნივთიერება (× 0.1 მოლი)", "მოცულობა (× 0.1 ლ)" }, Target = 0.5f, Solution = new[] { 5, 10 }, Start = new[] { 0, 10 }, Min = new[] { 0, 1 }, Max = new[] { 10, 20 } },
                        new Practice { Goal = "მიიღე 1.0 მოლ/ლ ხსნარი. შეცვალე ნივთიერება ან მოცულობა.", Done = "C = 1.00 მოლ/ლ! ნაკლებ მოცულობაში იგივე ნივთიერება უფრო კონცენტრირებულია.", Counters = new[] { "ნივთიერება (× 0.1 მოლი)", "მოცულობა (× 0.1 ლ)" }, Target = 1f, Solution = new[] { 5, 5 }, Start = new[] { 5, 10 }, Min = new[] { 0, 1 }, Max = new[] { 10, 20 } },
                        new Practice { Goal = "განაზავე 0.5 მოლ/ლ ხსნარი 0.25 მოლ/ლ-მდე. ნივთიერებას ნუ შეცვლი.", Done = "წყალი დაამატე და კონცენტრაცია განახევრდა: 0.25 მოლ/ლ.", Counters = new[] { "ნივთიერება (× 0.1 მოლი)", "მოცულობა (× 0.1 ლ)" }, Target = 0.25f, Solution = new[] { 5, 20 }, Start = new[] { 5, 10 }, Min = new[] { 5, 1 }, Max = new[] { 5, 20 } },
                    },
                    Questions = new[]
                    {
                        Q("რას უდრის კონცენტრაცია?", "ნივთიერება გაყოფილი მოცულობაზე.", "C = n / V", "მოლები გაყოფილი ლიტრებზე.",
                            "C = n × V", "აქ გამრავლება არ გვჭირდება.", "C = V / n", "პირიქით არის."),
                        Q("1 მოლი ნივთიერება 2 ლიტრ ხსნარშია. რა არის კონცენტრაცია?", "1 / 2.", "0.5 მოლ/ლ", "1 / 2 = 0.5.",
                            "2 მოლ/ლ", "ეს V / n-ია.", "1 მოლ/ლ", "მოცულობა 2 ლიტრია და არა 1."),
                        Q("ხსნარს წყალი დავამატეთ. რა მოუვა კონცენტრაციას?", "ნივთიერება იგივეა, მოცულობა — მეტი.", "შემცირდება", "იგივე n უფრო დიდ V-ზე ნაწილდება.",
                            "გაიზრდება", "წყალი ნივთიერებას არ ამატებს.", "არ შეიცვლება", "მოცულობა ხომ შეიცვალა."),
                    },
                    Note = "კონცენტრაცია: C = n / V (მოლ/ლ).\nმეტი ნივთიერება → მეტი C • მეტი წყალი → ნაკლები C.\nგანზავება ნივთიერების რაოდენობას არ ცვლის.",
                },
                new Lesson
                {
                    ID = "c7_separate", Chapter = 7, Kind = PracticeKind.Sort, Title = "ფილტრაცია და აორთქლება",
                    Why = "ასე იღებენ სუფთა წყალს, ზღვის წყლიდან მარილს და ასე ასუფთავებენ ცდის პროდუქტებს.",
                    Teach = new[]
                    {
                        "ფილტრაცია უხსნად მყარს სითხისგან ყოფს: ქვიშა ფილტრზე რჩება, წყალი კი გაედინება.",
                        "აორთქლება გახსნილ მყარს ყოფს: წყალი ორთქლდება, მარილი რჩება.",
                        "მაგნიტით რკინის ნაქლიბს გამოყოფ, გამოხდით კი სითხეებს — დუღილის ტემპერატურის მიხედვით.",
                    },
                    Bins = new[] { "ფილტრაცია", "აორთქლება", "მაგნიტი" }, Hint = "უხსნადი მყარი — ფილტრი; გახსნილი მყარი — აორთქლება; რკინა — მაგნიტი.",
                    Items = new[]
                    {
                        I("ქვიშა წყლიდან", 0, "ქვიშა არ იხსნება და ფილტრზე რჩება."),
                        I("მარილი ზღვის წყლიდან", 1, "წყალი ორთქლდება, მარილი რჩება."),
                        I("რკინის ნაქლიბი ქვიშიდან", 2, "რკინა მაგნიტს ეკვრის."),
                        I("ჩაის ფოთლები ჩაიდან", 0, "ფოთლები საწურზე რჩება."),
                        I("შაქარი შაქრის ხსნარიდან", 1, "აორთქლების შემდეგ შაქრის კრისტალები რჩება."),
                        I("ფოლადის ჭანჭიკები ნახერხიდან", 2, "ფოლადი რკინას შეიცავს და მაგნიტს ეკვრის."),
                    },
                    Questions = new[]
                    {
                        Q("როგორ გამოყოფ ქვიშას წყლისგან?", "უხსნადი მყარი.", "ფილტრაციით", "ქვიშა არ იხსნება და ფილტრზე რჩება.",
                            "აორთქლებით", "ასეც შეიძლება, მაგრამ წყალი დაიკარგება. ფილტრაცია მარტივი და სწრაფია.", "მაგნიტით", "ქვიშა მაგნიტს არ ეკვრის."),
                        Q("როგორ მიიღებ მარილს ზღვის წყლიდან?", "მარილი გახსნილია.", "აორთქლებით", "წყალი ორთქლდება და მარილი რჩება.",
                            "ფილტრაციით", "გახსნილი მარილი ფილტრში გადის.", "მაგნიტით", "მარილი მაგნიტს არ ეკვრის."),
                        Q("რატომ ვერ აჩერებს ფილტრი გახსნილ მარილს?", "იფიქრე ნაწილაკების ზომაზე.", "გახსნილი ნაწილაკები ძალიან პატარაა", "ისინი ფილტრის ფორებში წყალთან ერთად გადის.",
                            "მარილი ფილტრს ხსნის", "ფილტრი მთელი რჩება.", "მარილი აირია", "მარილი მყარია."),
                    },
                    Note = "ფილტრაცია — უხსნადი მყარი სითხიდან (ქვიშა).\nაორთქლება — გახსნილი მყარი (მარილი).\nმაგნიტი — რკინა • გამოხდა — სითხეები დუღილის ტემპერატურით.",
                },
                Review(7),

                // ================================================= 8. Acids, bases and pH
                new Lesson
                {
                    ID = "c8_acids", Chapter = 8, Kind = PracticeKind.Sort, Title = "მჟავები და ფუძეები",
                    Why = "მჟავები და ფუძეები ყველგანაა: ლიმონში, საპონში, კუჭში. მათი ამოცნობა უსაფრთხო მუშაობისთვის აუცილებელია.",
                    Teach = new[]
                    {
                        "მჟავებს მჟავე გემო აქვს (ლაბორატორიაში არასდროს გასინჯო!) და წყალში H⁺ იონებს გამოყოფს: ლიმონის წვენი, ძმარი.",
                        "ფუძეები შეხებისას ხშირად საპნისებრია და წყალში OH⁻ იონებს გამოყოფს: საპონი, სოდის ხსნარი.",
                        "ძლიერი მჟავები და ფუძეები კანს წვავს. ამიტომ სათვალე და ხელთათმანები სავალდებულოა.",
                    },
                    Bins = new[] { "მჟავა", "ფუძე" }, Hint = "მჟავე გემო — მჟავა; საპნისებრი და სარეცხი — ფუძე.",
                    Items = new[]
                    {
                        I("ლიმონის წვენი", 0, "ლიმონის წვენში ლიმონმჟავაა."),
                        I("საპნიანი წყალი", 1, "საპონი ფუძეა."),
                        I("ძმარი", 0, "ძმარში ძმარმჟავაა."),
                        I("სასმელი სოდის ხსნარი", 1, "სოდის ხსნარი სუსტი ფუძეა."),
                        I("კუჭის წვენი", 0, "კუჭის წვენში ჰიდროქლორმჟავაა."),
                        I("ამიაკის ხსნარი", 1, "ამიაკის ხსნარი ფუძეა."),
                        I("გაზიანი სასმელი", 0, "გახსნილი CO2 სუსტ მჟავას ქმნის."),
                    },
                    Questions = new[]
                    {
                        Q("რომელი იონები ახასიათებს მჟავებს წყალში?", "H.", "H⁺", "მჟავა წყალში წყალბადის იონებს გამოყოფს.",
                            "OH⁻", "OH⁻ ფუძეებს ახასიათებს.", "Na⁺", "Na⁺ ბევრ მარილსა და ფუძეშია."),
                        Q("რომელია ფუძე?", "სარეცხი.", "საპნიანი წყალი", "საპონი ფუძეა.",
                            "ძმარი", "ძმარი მჟავაა.", "ლიმონის წვენი", "ლიმონის წვენი მჟავაა."),
                        Q("როგორ ამოიცნობ მჟავას ლაბორატორიაში?", "უსაფრთხოება!", "ინდიკატორით", "ინდიკატორი ფერს იცვლის — სინჯვა საჭირო არ არის.",
                            "გასინჯვით", "ლაბორატორიაში არაფერს ვსინჯავთ!", "ხელზე დასხმით", "ეს საშიშია."),
                    },
                    Note = "მჟავა: H⁺, მჟავე (ლიმონი, ძმარი, HCl).\nფუძე: OH⁻, საპნისებრი (საპონი, სოდა, NaOH).\nარასდროს გასინჯო — ამოიცანი ინდიკატორით.",
                },
                new Lesson
                {
                    ID = "c8_ph", Chapter = 8, Kind = PracticeKind.Scale, Title = "pH შკალა", Collect = "fact:ph",
                    Why = "pH ხსნარის მჟავიანობას რიცხვით გამოხატავს. ნეიტრალიზაციის ცდაში სწორედ ამ რიცხვს მიჰყვები.",
                    Teach = new[]
                    {
                        "pH შკალა 0-დან 14-მდეა.",
                        "7 — ნეიტრალურია (სუფთა წყალი). 7-ზე ნაკლები — მჟავა. 7-ზე მეტი — ფუძე.",
                        "რაც უფრო შორსაა 7-დან, მით უფრო ძლიერია: pH 1 ძალიან მჟავეა, pH 13 — ძალიან ტუტე.",
                    },
                    Steps = new[]
                    {
                        P("სუფთა წყალი: pH 7, ნეიტრალური.", "შკალის შუა — ნეიტრალური ხსნარი.", new[] { "pH" }, new[] { 7 }, new[] { 0 }, new[] { 0 }, new[] { 14 }),
                        P("აჩვენე ლიმონის წვენის მიახლოებითი pH — 2.", "pH 2 — ძლიერად მჟავე, ამიტომაა ლიმონი ასე მჟავე.", new[] { "pH" }, new[] { 2 }, new[] { 7 }, new[] { 0 }, new[] { 14 }),
                        new Practice { Goal = "აჩვენე ნებისმიერი ძლიერი ფუძის pH.", Done = "pH 12 — ძლიერი ფუძე, მაგალითად ჭურჭლის სარეცხი საშუალება.", Counters = new[] { "pH" }, Solution = new[] { 12 }, Start = new[] { 7 }, Min = new[] { 0 }, Max = new[] { 14 },
                            Check = v => v[0] >= 11 ? null : v[0] > 7 ? "pH " + v[0] + " — სუსტი ფუძეა. ძლიერი ფუძე შკალის ბოლოსკენაა." : "ეს ფუძე არ არის: ფუძის pH 7-ზე მეტია.",
                            Named = v => v[0] == 11 ? "pH 11 — ნიშადურის სპირტი, ძლიერი ფუძე." : v[0] == 13 ? "pH 13 — მილის საწმენდი, ძალიან ძლიერი ფუძე." : v[0] == 14 ? "pH 14 — შკალის ბოლო: კაუსტიკური სოდის ხსნარი." : null },
                    },
                    Questions = new[]
                    {
                        Q("რა pH აქვს ნეიტრალურ ხსნარს?", "შკალის შუა.", "7", "სუფთა წყლის pH დაახლოებით 7-ია.",
                            "0", "0 ძალიან ძლიერი მჟავაა.", "14", "14 ძალიან ძლიერი ფუძეა."),
                        Q("pH 3 ხსნარი რა არის?", "7-ზე ნაკლები.", "მჟავა", "7-ზე ნაკლები — მჟავა.",
                            "ფუძე", "ფუძის pH 7-ზე მეტია.", "ნეიტრალური", "ნეიტრალური ხსნარის pH 7-ია."),
                        Q("რომელი უფრო ძლიერი ფუძეა?", "7-დან შორს.", "pH 13", "რაც უფრო მაღალია pH 7-ზე, მით უფრო ტუტეა ხსნარი.",
                            "pH 8", "8 სუსტი ფუძეა.", "pH 6", "6 სუსტი მჟავაა."),
                    },
                    Note = "pH: 0 ← მჟავა — 7 ნეიტრალური — ფუძე → 14.\nრაც უფრო შორსაა 7-დან, მით უფრო ძლიერია ხსნარი.",
                },
                new Lesson
                {
                    ID = "c8_indicators", Chapter = 8, Kind = PracticeKind.Drops, Title = "ინდიკატორები", Collect = "sample:cabbage",
                    Why = "ინდიკატორით pH-ს თვალით დაინახავ. ანალიზის ცდებში ის ხშირად დაგჭირდება.",
                    Teach = new[]
                    {
                        "ინდიკატორი ნივთიერებაა, რომელიც მჟავასა და ფუძეში სხვადასხვა ფერს იღებს.",
                        "წითელი კომბოსტოს წვენი მჟავაში წითლდება, ნეიტრალურში იისფერია, ფუძეში კი მწვანე-ყვითელი ხდება.",
                        "ლაკმუსის ქაღალდი მჟავაში წითლდება, ფუძეში — ლურჯდება.",
                    },
                    Steps = new[]
                    {
                        new Practice { Goal = "სამი წვეთი მჟავა ინდიკატორს აწითლებს.", Done = "წითელი ფერი — მჟავე ხსნარი.", Counters = new[] { "მჟავის წვეთები", "ფუძის წვეთები" }, Solution = new[] { 3, 0 }, Max = new[] { 10, 10 } },
                        new Practice { Goal = "გაამწვანე ინდიკატორი: ხსნარი ტუტე უნდა გახდეს (pH 9-ზე მეტი).", Done = "მწვანე ფერი — ტუტე ხსნარი!", Counters = new[] { "მჟავის წვეთები", "ფუძის წვეთები" }, Solution = new[] { 0, 3 }, Max = new[] { 10, 10 },
                            Check = v => DropsPH(v) > 9 ? null : PHText(v) + (v[0] > 0 ? "ხსნარში მჟავაა — ჯერ მოაკელი მჟავის წვეთები." : "დაამატე ფუძის წვეთები.") },
                        new Practice { Goal = "ხსნარში 3 წვეთი მჟავაა. ფუძით გადაიყვანე ის ტუტე მხარეს — მწვანე ფერამდე.", Done = "ფუძემ მჟავა გაანეიტრალა და ჭარბი ფუძე ხსნარს ამწვანებს.", Counters = new[] { "მჟავის წვეთები", "ფუძის წვეთები" }, Solution = new[] { 3, 6 }, Start = new[] { 3, 0 }, Min = new[] { 3, 0 }, Max = new[] { 3, 10 },
                            Check = v => DropsPH(v) > 9 ? null : PHText(v) + (DropsPH(v) < 7 ? "ჯერ კიდევ მჟავეა: ფუძე დაამატე." : "თითქმის ნეიტრალურია: კიდევ ცოტა ფუძე.") },
                    },
                    Questions = new[]
                    {
                        Q("რა არის ინდიკატორი?", "ფერი.", "ნივთიერება, რომელიც pH-ის მიხედვით ფერს იცვლის", "ფერით ვიგებთ, მჟავაა ხსნარი თუ ფუძე.",
                            "ძლიერი მჟავა", "ინდიკატორი მხოლოდ ფერს გვიჩვენებს.", "საზომი ჭიქა", "ჭიქა ჭურჭელია."),
                        Q("ლაკმუსის ქაღალდი გაწითლდა. რა არის ხსნარი?", "წითელი — მჟავა.", "მჟავა", "ლაკმუსი მჟავაში წითლდება.",
                            "ფუძე", "ფუძეში ლაკმუსი ლურჯდება.", "სუფთა წყალი", "ნეიტრალურ ხსნარში ფერი თითქმის არ იცვლება."),
                        Q("რა ფერს იღებს კომბოსტოს ინდიკატორი ფუძეში?", "საპონში.", "მწვანე-ყვითელს", "ფუძეში კომბოსტოს პიგმენტები მწვანდება.",
                            "წითელს", "წითელი ფერი მჟავაშია.", "იისფერს", "იისფერი ნეიტრალურ ხსნართან ახლოსაა."),
                    },
                    Note = "ინდიკატორი — ფერით გვიჩვენებს pH-ს.\nკომბოსტო: მჟავა — წითელი, ნეიტრალური — იისფერი, ფუძე — მწვანე-ყვითელი.\nლაკმუსი: მჟავა — წითელი, ფუძე — ლურჯი.",
                },
                new Lesson
                {
                    ID = "c8_neutral", Chapter = 8, Kind = PracticeKind.Drops, Title = "ნეიტრალიზაცია და უსაფრთხოება", Collect = "fact:catalyst",
                    Why = "ნეიტრალიზაციით დაღვრილ მჟავას უსაფრთხოდ აუვნებელყოფენ. ესაა რთული გამოწვევების მჟავა-ფუძის ამოცანების საფუძველი.",
                    Teach = new[]
                    {
                        "თუ მჟავასა და ფუძეს თანაბარი რაოდენობით შევურევთ, H⁺ და OH⁻ წყლად ერთდება და pH 7-ს უახლოვდება.",
                        "ფუძე წვეთ-წვეთად დაამატე და ყოველი წვეთის შემდეგ ინდიკატორს დააკვირდი.",
                        "უსაფრთხოება: მჟავის განზავებისას მჟავა წყალს დაუმატე და არა პირიქით. ატარე სათვალე და ხელთათმანები.",
                    },
                    Steps = new[]
                    {
                        new Practice { Goal = "ორი წვეთი მჟავა და ორი წვეთი ფუძე: pH 7.", Done = "H⁺ + OH⁻ → H2O. ხსნარი ნეიტრალურია.", Counters = new[] { "მჟავის წვეთები", "ფუძის წვეთები" }, Solution = new[] { 2, 2 }, Max = new[] { 10, 10 } },
                        new Practice { Goal = "4 წვეთ მჟავას ფუძე დაუმატე და ზუსტად ნეიტრალური ხსნარი მიიღე.", Done = "4 + 4: ნეიტრალიზაცია დასრულდა!", Counters = new[] { "მჟავის წვეთები", "ფუძის წვეთები" }, Solution = new[] { 4, 4 }, Start = new[] { 4, 0 }, Min = new[] { 4, 0 }, Max = new[] { 4, 10 },
                            Check = v => Math.Abs(DropsPH(v) - 7) < 0.5 ? null : PHText(v) + (DropsPH(v) < 7 ? "ჯერ კიდევ მჟავეა: კიდევ ერთი წვეთი ფუძე." : "ფუძე ზედმეტია: ერთი წვეთი მოაკელი.") },
                        new Practice { Goal = "გაანეიტრალე 6 წვეთი მჟავა.", Done = "ზუსტი ნეიტრალიზაცია: 6 წვეთი მჟავა და 6 წვეთი ფუძე.", Counters = new[] { "მჟავის წვეთები", "ფუძის წვეთები" }, Solution = new[] { 6, 6 }, Start = new[] { 6, 0 }, Min = new[] { 6, 0 }, Max = new[] { 6, 10 },
                            Check = v => Math.Abs(DropsPH(v) - 7) < 0.5 ? null : PHText(v) + (DropsPH(v) < 7 ? "ხსნარი ჯერ მჟავეა." : "ხსნარი უკვე ტუტეა.") },
                    },
                    Questions = new[]
                    {
                        Q("ნეიტრალიზაციის შემდეგ pH რომელ რიცხვს უახლოვდება?", "ნეიტრალური.", "7", "H⁺ და OH⁻ წყლად იქცა.",
                            "0", "0 ძლიერი მჟავაა.", "14", "14 ძლიერი ფუძეა."),
                        Q("როგორ უნდა განაზავო მჟავა უსაფრთხოდ?", "რა რას ემატება?", "მჟავა ნელა დაუმატო წყალს", "ასე სითბო თანდათან ნაწილდება და შხეფები არ ამოდის.",
                            "წყალი დაასხა მჟავას", "ამან შეიძლება ცხელი მჟავა ამოისხას.", "ორივე ერთად სწრაფად შეურიო", "სწრაფი შერევა საშიშია."),
                        Q("რატომ ვამატებთ ფუძეს წვეთ-წვეთად?", "სიზუსტე.", "რომ ნეიტრალურ წერტილს არ გავცდეთ", "ერთი ზედმეტი წვეთი ხსნარს ტუტეს ხდის.",
                            "რომ ფუძე დავზოგოთ", "მთავარი სიზუსტეა.", "რომ ხსნარი გაცხელდეს", "გაცხელება მიზანი არ არის."),
                    },
                    Note = "ნეიტრალიზაცია: მჟავა + ფუძე → მარილი + წყალი; pH → 7.\nფუძე — წვეთ-წვეთად, ინდიკატორის კონტროლით.\nმჟავა წყალს ემატება და არა პირიქით!",
                },
                Review(8),
            };
            return list.ToArray();
        }
    }
}
