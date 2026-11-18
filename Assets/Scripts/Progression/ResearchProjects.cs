using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChemistryGame
{
    public enum StageKind { Gather, Equipment, Experiment, Observe, Wait, Submit }

    public sealed class ProjectStage
    {
        public StageKind Kind;
        public string Title, Text;
        public string Target;           // material, equipment or activity ID
        public int Count = 1;           // materials needed or days to wait
        public string[] Options;        // observation answers
        public int Correct;
        public string Lesson;           // explanation shown after an observation
    }

    public sealed class ResearchProject
    {
        public string ID, Title, Summary, Badge, Unlock;
        public int ReputationLevel, Coins, Reputation, FairBase;
        public Color Color;
        public ProjectStage[] Stages;
    }

    /// <summary>
    /// Long research projects. Each has several saved stages (gather, equipment, experiment,
    /// observe, wait overnight, submit) so it naturally spreads across several play sessions.
    /// Mistakes are counted but never undo progress.
    /// </summary>
    public static class ResearchProjects
    {
        private static ProjectStage Gather(string material, int count, string text) =>
            new ProjectStage { Kind = StageKind.Gather, Title = "მასალების შეგროვება", Target = material, Count = count, Text = text };
        private static ProjectStage Equip(string item, string text) =>
            new ProjectStage { Kind = StageKind.Equipment, Title = "აღჭურვილობის მომზადება", Target = item, Text = text };
        private static ProjectStage Run(string activity, string text) =>
            new ProjectStage { Kind = StageKind.Experiment, Title = "ცდის ჩატარება", Target = activity, Text = text };
        private static ProjectStage Wait(int days, string text) =>
            new ProjectStage { Kind = StageKind.Wait, Title = "მოთმინება", Count = days, Text = text };
        private static ProjectStage Observe(string question, int correct, string lesson, params string[] options) =>
            new ProjectStage { Kind = StageKind.Observe, Title = "დაკვირვების ჩაწერა", Text = question, Options = options, Correct = correct, Lesson = lesson };
        private static ProjectStage Submit() =>
            new ProjectStage { Kind = StageKind.Submit, Title = "შედეგის ჩაბარება", Text = "წაიღე შედეგი მასწავლებელთან ან გამოფენის დარბაზის სტენდთან." };

        public static readonly ResearchProject[] All =
        {
            new ResearchProject
            {
                ID = "crystals", Title = "კრისტალების გაზრდა", Color = new Color32(170, 200, 240, 255),
                Summary = "შაბის ცხელ ნაჯერ ხსნარში ძაფი ჩაუშვი და ნელა გააციე. რაც უფრო ნელა ცივდება, მით დიდი კრისტალები იზრდება.",
                ReputationLevel = 0, Coins = 60, Reputation = 25, FairBase = 60, Badge = "first_project", Unlock = "crystal_lamp",
                Stages = new[]
                {
                    Gather("alum", 2, "სათავსოდან აიღე ორი პაკეტი შაბი."),
                    Gather("tapwater", 1, "ხსნარისთვის ონკანის წყალიც დაგჭირდება."),
                    Equip("thermo", "პირად ლაბორატორიაში დადგი თერმოსტატი."),
                    Run("thermo", "გააცხელე ხსნარი და ნელა გააციე. ტემპერატურა მწვანე ზოლში შეინარჩუნე."),
                    Wait(1, "კრისტალები ღამით იზრდება. ხვალ შეამოწმე!"),
                    Observe("რატომ გაიზარდა ერთი დიდი კრისტალი და არა ბევრი პაწაწინა?", 1,
                        "ნელი გაციებისას ნაკლები ჩანასახი წარმოიქმნება და შაბის ნაწილაკები უკვე არსებულ კრისტალზე ლაგდება.",
                        "იმიტომ, რომ ხსნარი ძალიან სწრაფად გავაციეთ", "იმიტომ, რომ ნელი გაციებისას ნაწილაკები ერთ კრისტალზე ლაგდება", "იმიტომ, რომ შაბი წყალში საერთოდ არ იხსნება"),
                    Submit()
                }
            },
            new ResearchProject
            {
                ID = "colour", Title = "ფერცვალებადი ხსნარი", Color = new Color32(160, 110, 200, 255),
                Summary = "წითელი კომბოსტოს წვენი ბუნებრივი ინდიკატორია. სოდის ხსნარის წვეთ-წვეთად დამატებით ფერი წითლიდან მწვანემდე იცვლება.",
                ReputationLevel = 0, Coins = 55, Reputation = 22, FairBase = 55, Badge = "", Unlock = "glow_jar",
                Stages = new[]
                {
                    Gather("cabbage", 1, "სათბურიდან მოიტანე წითელი კომბოსტო."),
                    Gather("soda", 1, "სათავსოდან აიღე საჭმლის სოდა."),
                    Equip("valve", "დადგი დოზირების სარქველი, რომ ფუძე ზუსტად დაამატო."),
                    Run("valve", "ნელა გახსენი სარქველი და ხსნარი მიზნის ფერამდე მიიყვანე. ძალიან სწრაფად არ დაასხა!"),
                    Observe("სოდის დამატებისას ხსნარი მეწამულიდან მწვანე გახდა. რას ნიშნავს ეს?", 0,
                        "ანთოციანი ფერს pH-ის მიხედვით იცვლის: მჟავაში წითელია, ნეიტრალურში მეწამული, ფუძეში მწვანე-ყვითელი.",
                        "ხსნარი ფუძე გახდა და pH გაიზარდა", "ხსნარი უფრო მჟავე გახდა", "კომბოსტო დაიწვა"),
                    Submit()
                }
            },
            new ResearchProject
            {
                ID = "unknown", Title = "უცნობი ნიმუშის ამოცნობა", Color = new Color32(200, 170, 120, 255),
                Summary = "ქეთევანმა თეთრი ფხვნილი მოიტანა. ტესტების სერიით გაარკვიე, რა არის: მარილი, სოდა, შაქარი, ლიმონმჟავა თუ ცარცი.",
                ReputationLevel = 0, Coins = 65, Reputation = 25, FairBase = 65, Badge = "", Unlock = "",
                Stages = new[]
                {
                    Gather("sample", 1, "უცნობი ნიმუში ლაბორანტ ქეთევანს სთხოვე, ან სათავსოში იყიდე."),
                    Gather("vinegar", 1, "ძმრის ტესტისთვის სათავსოდან ძმარი აიღე."),
                    Equip("analysis", "დადგი ანალიზის სადგური."),
                    Run("analysis", "ჩაატარე ტესტები და ამოიცანი ნიმუში. ყოველი ტესტი ახალ მინიშნებას იძლევა."),
                    Observe("რომელი ტესტი განასხვავებს სოდასა და ცარცს ყველაზე კარგად?", 2,
                        "ორივე ძმარზე შიშინებს, მაგრამ სოდა წყალში იხსნება, ცარცი კი ფსკერზე ილექება.",
                        "ალის ფერი", "ძმრის წვეთი", "წყალში ხსნადობა"),
                    Submit()
                }
            },
            new ResearchProject
            {
                ID = "indicator", Title = "მანათობელი ინდიკატორი", Color = new Color32(110, 200, 230, 255),
                Summary = "ტონიკის ქინინი ულტრაიისფერ შუქზე ცისფრად ანათებს. გაარკვიე, მჟავე თუ ნეიტრალურ ხსნარში ანათებს უფრო კაშკაშა.",
                ReputationLevel = 1, Coins = 80, Reputation = 32, FairBase = 75, Badge = "", Unlock = "",
                Stages = new[]
                {
                    Gather("tonic", 2, "სათავსოდან ორი ბოთლი ტონიკი აიღე."),
                    Gather("lemon", 1, "სათბურიდან ლიმონი მოიტანე, ხსნარი რომ შეამჟავო."),
                    Equip("compare", "დადგი რეაქციების შედარების მაგიდა."),
                    Run("compare", "შეადარე ჭიქები ულტრაიისფერ შუქზე და აირჩიე სწორი ახსნა."),
                    Observe("რატომ ანათებს ქინინი ულტრაიისფერ შუქზე?", 1,
                        "ქინინი შთანთქავს უხილავ ულტრაიისფერ სხივებს და მცირე ენერგიის ხილულ ცისფერ შუქს გამოასხივებს. ეს ფლუორესცენციაა.",
                        "ის თავად ენერგიას აწარმოებს", "ის უხილავ შუქს შთანთქავს და ხილულ ცისფერ შუქს ასხივებს", "მასში ფოსფორია ჩამალული"),
                    Submit()
                }
            },
            new ResearchProject
            {
                ID = "water", Title = "წყლის ხარისხის კვლევა", Color = new Color32(110, 170, 220, 255),
                Summary = "შეადარე წვიმისა და ონკანის წყალი: pH, სიხისტე, სიმღვრივე და ელგამტარობა. წვიმის წყალი მხოლოდ წვიმიან დღეს გროვდება!",
                ReputationLevel = 1, Coins = 85, Reputation = 34, FairBase = 80, Badge = "", Unlock = "",
                Stages = new[]
                {
                    Gather("rainwater", 1, "წვიმიან დღეს სათბურის კასრიდან წვიმის წყალი აიღე."),
                    Gather("tapwater", 1, "სათავსოდან ონკანის წყალი აიღე."),
                    Equip("analysis", "დადგი ანალიზის სადგური."),
                    Run("water", "ჩაატარე წყლის ტესტები და ამოიცანი, რომელი ნიმუშია."),
                    Wait(1, "ნიმუშებმა ერთი ღამე უნდა დაწყნარდეს, რომ ნალექი დაილექოს."),
                    Observe("რატომ არის წვიმის წყალი ოდნავ მჟავე?", 0,
                        "ჰაერის ნახშირორჟანგი წვიმის წვეთებში იხსნება და სუსტ ნახშირმჟავას ქმნის, ამიტომ pH დაახლოებით 5,6-ია.",
                        "ჰაერის ნახშირორჟანგი იხსნება და სუსტ მჟავას ქმნის", "ღრუბლებში მარილია", "წვიმის წყალი სინამდვილეში ფუძეა"),
                    Submit()
                }
            },
            new ResearchProject
            {
                ID = "battery", Title = "მარტივი ბატარეის აწყობა", Color = new Color32(230, 180, 80, 255),
                Summary = "ლიმონში თუთიისა და სპილენძის ფირფიტები ჩაარჭე. ელექტრონები თუთიიდან სადენით სპილენძისკენ გაედინება.",
                ReputationLevel = 2, Coins = 100, Reputation = 40, FairBase = 90, Badge = "", Unlock = "",
                Stages = new[]
                {
                    Gather("lemon", 2, "სათბურიდან ორი ლიმონი მოიტანე."),
                    Gather("zinc", 1, "სათავსოდან თუთიის ფირფიტა აიღე."),
                    Gather("copper", 1, "სათავსოდან სპილენძის ფირფიტა აიღე."),
                    Gather("wire", 1, "და ბოლოს სადენი."),
                    Equip("particles", "დადგი ნაწილაკების დამჭერი, რომ ელექტრონების დინებას დააკვირდე."),
                    Run("particles", "დაიჭირე თუთიიდან გამოსული ელექტრონები და სპილენძისკენ მიმართე."),
                    Run("balance", "დააბალანსე ბატარეის რეაქცია."),
                    Observe("ერთი ლიმონი დაახლოებით 0,9 ვოლტს იძლევა. როგორ მივიღოთ მეტი ძაბვა?", 2,
                        "მიმდევრობით შეერთებული ელემენტების ძაბვა იკრიბება: ორი ლიმონი ≈ 1,8 ვოლტი.",
                        "უფრო დიდი ლიმონი ავიღოთ", "ფირფიტები ერთმანეთს შევახოთ", "რამდენიმე ლიმონი მიმდევრობით შევაერთოთ"),
                    Submit()
                }
            },
            new ResearchProject
            {
                ID = "perfume", Title = "უსაფრთხო არომატის შექმნა", Color = new Color32(240, 150, 120, 255),
                Summary = "ციტრუსის ქერქისა და ყვავილების ორთქლით გამოხდა. ტემპერატურა სწორად უნდა შეინარჩუნო, ნარევი კი ერთი დღე უნდა დაყოვნდეს.",
                ReputationLevel = 2, Coins = 95, Reputation = 38, FairBase = 85, Badge = "", Unlock = "",
                Stages = new[]
                {
                    Gather("citrus", 2, "სათბურიდან ციტრუსის ქერქი მოიტანე."),
                    Gather("flowers", 2, "და ყვავილის ფურცლები."),
                    Equip("thermo", "დადგი თერმოსტატი, რომ ორთქლით გამოხდის ტემპერატურა მართო."),
                    Run("distill", "შეინარჩუნე ორთქლის ტემპერატურა მწვანე ზოლში."),
                    Wait(1, "არომატი ერთ ღამეს უნდა დაყოვნდეს."),
                    Observe("გამოხდილ სითხეში ზეთის თხელი ფენა წყლის თავზე ჩანს. რატომ?", 0,
                        "ეთერზეთები წყალზე მსუბუქია და მასში ცუდად იხსნება, ამიტომ ზემოთ ცალკე ფენად რჩება.",
                        "ზეთი წყალზე მსუბუქია და მასში ცუდად იხსნება", "ზეთი წყალზე მძიმეა", "ზეთი და წყალი ერთმანეთში სრულად იხსნება"),
                    Submit()
                }
            },
        };

        public static ResearchProject Get(string id) => All.FirstOrDefault(p => p.ID == id);
        private static GameSaveData Data => GameSaveManager.Instance.Data;
        public static ProjectProgress State(string id) => Data.projects.FirstOrDefault(p => p.id == id);
        public static bool Unlocked(ResearchProject project) => LabProgress.ReputationLevel >= project.ReputationLevel;
        public static IEnumerable<ProjectProgress> Active => Data.projects.Where(p => !p.completed && Get(p.id) != null);
        public static ProjectStage Stage(ProjectProgress state)
        {
            var project = Get(state.id);
            return project == null || state.stage >= project.Stages.Length ? null : project.Stages[state.stage];
        }

        public static bool Accept(string id)
        {
            var project = Get(id);
            if (project == null || !Unlocked(project)) return false;
            var state = State(id);
            if (state != null && !state.completed) return false;
            if (state == null) { state = new ProjectProgress { id = id }; Data.projects.Add(state); }
            state.completed = false; state.submitted = false; state.stage = 0; state.mistakes = 0; state.readyDay = "";
            state.startedDay = LabProgress.Today; state.notes.Clear();
            GameSaveManager.Instance.Commit();
            return true;
        }

        /// <summary>Human-readable status of the current stage, including what is still missing.</summary>
        public static string Requirement(ProjectProgress state)
        {
            var stage = Stage(state);
            if (stage == null) return "";
            switch (stage.Kind)
            {
                case StageKind.Gather:
                    var material = LabProgress.Material(stage.Target);
                    return material.Name + ": " + Mathf.Min(LabProgress.Has(stage.Target), stage.Count) + " / " + stage.Count + " • წყარო: " + material.Source;
                case StageKind.Equipment:
                    var item = Array.Find(PersonalLab.Catalog, c => c.ID == stage.Target);
                    bool owned = Data.ownedLabItems.Contains(stage.Target);
                    bool placed = Data.labPlacements.Any(p => p.itemID == stage.Target);
                    return item.Name + ": " + (placed ? "დადგმულია (მზადაა)" : owned ? "ნაყიდია, დადგი ოთახში" : "იყიდე პირადი ლაბორატორიის მაღაზიაში (" + item.Price + " ქიმქოინი)");
                case StageKind.Wait:
                    return Ready(state) ? "მზადაა! შეამოწმე შედეგი." : "დაბრუნდი ხვალ. ცდა ღამით გრძელდება.";
                case StageKind.Experiment:
                    return "ცდა ჩაატარე პირად ლაბორატორიაში ან აქედან.";
                default: return "";
            }
        }

        public static bool Ready(ProjectProgress state) =>
            string.IsNullOrEmpty(state.readyDay) || string.CompareOrdinal(LabProgress.Today, state.readyDay) >= 0;

        /// <summary>Completes gather, equipment and wait stages when their requirement is met.</summary>
        public static bool TryAdvance(ProjectProgress state, out string message)
        {
            var stage = Stage(state);
            message = "";
            if (stage == null) return false;
            switch (stage.Kind)
            {
                case StageKind.Gather:
                    if (!LabProgress.Take(stage.Target, stage.Count)) { message = "ჯერ მასალა არ გყოფნის. " + Requirement(state); return false; }
                    message = LabProgress.Material(stage.Target).Name + " გამოყენებულია.";
                    break;
                case StageKind.Equipment:
                    if (!Data.labPlacements.Any(p => p.itemID == stage.Target)) { message = Requirement(state); return false; }
                    LabProgress.AddMastery("equipment:" + stage.Target);
                    message = "აღჭურვილობა მზადაა.";
                    break;
                case StageKind.Wait:
                    if (string.IsNullOrEmpty(state.readyDay))
                    {
                        state.readyDay = GameSaveManager.Iso(SchoolCalendar.Now.Date.AddDays(stage.Count));
                        GameSaveManager.Instance.Commit();
                        message = stage.Text;
                        return false;
                    }
                    if (!Ready(state)) { message = "ჯერ ადრეა. " + stage.Text; return false; }
                    message = "ღამემ თავისი საქმე გააკეთა! შედეგი მზადაა.";
                    break;
                default: return false;
            }
            Next(state);
            return true;
        }

        /// <summary>Called by experiment activities. Failure is harmless and only counted.</summary>
        public static void ExperimentResult(string projectID, bool success)
        {
            var state = State(projectID);
            var stage = state == null ? null : Stage(state);
            if (stage == null || stage.Kind != StageKind.Experiment) return;
            if (!success) { state.mistakes++; GameSaveManager.Instance.Commit(); return; }
            LabProgress.AddMastery("project:" + projectID);
            state.notes.Add(stage.Text);
            Next(state);
        }

        public static bool Answer(ProjectProgress state, int option)
        {
            var stage = Stage(state);
            if (stage == null || stage.Kind != StageKind.Observe) return false;
            if (option != stage.Correct) { state.mistakes++; GameSaveManager.Instance.Commit(); return false; }
            state.notes.Add(stage.Options[stage.Correct]);
            LabProgress.Collect("fact:" + (state.id == "indicator" ? "glow" : state.id == "battery" ? "battery" : state.id == "perfume" ? "smell" : state.id == "colour" ? "ph" : state.id == "crystals" ? "salt" : "water"));
            Next(state);
            return true;
        }

        public static ProjectProgress Submittable => Active.FirstOrDefault(s => Stage(s)?.Kind == StageKind.Submit);

        /// <summary>Hands in a finished project. Returns a reward summary, or null.</summary>
        public static string Submit(ProjectProgress state)
        {
            var project = state == null ? null : Get(state.id);
            if (project == null || Stage(state)?.Kind != StageKind.Submit) return null;
            bool theme = SchoolCalendar.ThemeProject == project.ID;
            int coins = project.Coins + (state.mistakes == 0 ? 15 : 0);
            state.completed = true; state.submitted = true; state.timesCompleted++;
            Data.chemCoins += coins;
            LabProgress.AddReputation(project.Reputation + (theme ? 15 : 0));
            LabProgress.AddMastery("project:" + project.ID, theme ? 4 : 2);
            LabProgress.CountStat("projects");
            LabProgress.Collect("photo:" + project.ID);
            if (project.ID == "crystals") LabProgress.Collect("sample:crystal");
            if (project.ID == "perfume") LabProgress.Collect("sample:perfume");
            if (project.ID == "unknown") LabProgress.Collect("mineral:calcite");
            if (!string.IsNullOrEmpty(project.Badge)) LabProgress.Collect("badge:" + project.Badge);
            LabProgress.Collect("badge:first_project");
            if (All.All(p => State(p.ID)?.timesCompleted > 0)) LabProgress.Collect("badge:all_projects");
            if (!string.IsNullOrEmpty(project.Unlock) && !Data.ownedLabItems.Contains(project.Unlock)) Data.ownedLabItems.Add(project.Unlock);
            if (project.ID == "crystals" && !Data.unlockedCosmeticIDs.Contains("goggles_crystal")) Data.unlockedCosmeticIDs.Add("goggles_crystal");
            GameSaveManager.Instance.Commit();
            AudioManager.Instance?.Milestone();
            return "+" + coins + " ქიმქოინი • +" + (project.Reputation + (theme ? 15 : 0)) + " რეპუტაცია" +
                (state.mistakes == 0 ? " • უშეცდომო ბონუსი" : "") + (theme ? " • თვის თემის ბონუსი" : "") +
                (!string.IsNullOrEmpty(project.Unlock) ? " • ახალი დეკორაცია" : "");
        }

        private static void Next(ProjectProgress state)
        {
            state.stage++;
            state.readyDay = "";
            GameSaveManager.Instance.Commit();
        }

        // ---------- weekly science fair ----------

        public static readonly string[] Rivals = { "ლუკა", "ანა", "ნიკა", "მარიამი", "გიორგი" };

        public static FairEntry ThisWeek => Data.fairEntries.FirstOrDefault(f => f.week == SchoolCalendar.Week);

        public static int RivalScore(string rival, string week)
        {
            var random = new System.Random((week + rival).GetHashCode());
            int baseScore = rival == "ლუკა" ? 85 : 55;
            return baseScore + random.Next(0, 45) + LabProgress.ReputationLevel * 4;
        }

        public static FairEntry EnterFair(string projectID)
        {
            var state = State(projectID);
            var project = Get(projectID);
            if (project == null || state == null || state.timesCompleted == 0 || ThisWeek != null) return null;
            var random = new System.Random((SchoolCalendar.Week + projectID).GetHashCode());
            int score = project.FairBase + LabProgress.MasteryValue("project:" + projectID) * 3 +
                Mathf.Max(0, 20 - state.mistakes * 5) + random.Next(0, 20) + (SchoolCalendar.IsExhibitionDay ? 10 : 0);
            var entry = new FairEntry { week = SchoolCalendar.Week, projectID = projectID, score = score };
            entry.rank = 1 + Rivals.Count(r => RivalScore(r, entry.week) > score);
            Data.fairEntries.Add(entry);
            if (SchoolCalendar.IsExhibitionDay) LabProgress.Collect("award:ribbon");
            LabProgress.AddReputation(8);
            GameSaveManager.Instance.Commit();
            return entry;
        }

        /// <summary>Pays out finished weeks once the week has changed. Returns a message or null.</summary>
        public static string SettleFairs()
        {
            string message = null;
            foreach (var entry in Data.fairEntries.Where(f => !f.settled && f.week != SchoolCalendar.Week))
            {
                entry.settled = true;
                int coins = entry.rank == 1 ? 120 : entry.rank == 2 ? 70 : entry.rank == 3 ? 40 : 15;
                Data.chemCoins += coins;
                LabProgress.AddReputation(entry.rank <= 3 ? 25 - entry.rank * 5 : 4);
                if (entry.rank == 1)
                {
                    LabProgress.Collect("award:gold");
                    if (!Data.unlockedCosmeticIDs.Contains("coat_fair_gold")) Data.unlockedCosmeticIDs.Add("coat_fair_gold");
                    if (!Data.ownedLabItems.Contains("fair_trophy")) Data.ownedLabItems.Add("fair_trophy");
                }
                else if (entry.rank == 2) LabProgress.Collect("award:silver");
                else if (entry.rank == 3) LabProgress.Collect("award:bronze");
                message = "სამეცნიერო ბაზრობის შედეგები: " + entry.rank + " ადგილი! +" + coins + " ქიმქოინი.";
            }
            if (message != null) GameSaveManager.Instance.Commit();
            return message;
        }
    }
}
