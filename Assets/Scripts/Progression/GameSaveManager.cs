using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace ChemistryGame
{
    [Serializable]
    public sealed class GameSaveData
    {
        public int version = 1;
        public bool profileCompleted;
        public string playerName = "";
        public int skinTone;
        public int hairColor;
        public int shirtColor;
        public bool labExpanded;
        public List<string> ownedLabItems = new List<string>();
        public List<LabPlacement> labPlacements = new List<LabPlacement>();
        public int chemCoins;
        public int currentDailyStreak;
        public string lastPlayedDate = "";
        public int weeklyQuizzesSolved;
        public string weekStartDate = "";
        public int totalQuizzesSolved;
        public List<string> unlockedElements = new List<string>();
        public List<string> unlockedCosmeticIDs = new List<string>();
        public string equippedHatOrGogglesID = "";
        public string equippedLabCoatID = "";
        public string equippedDeskDecorID = "";
        public bool tutorialCompleted;
        public bool starterCoatCollected;
        public int dailyQuizzesSolved;
        public string dailyCounterDate = "";
        public List<string> claimedDailyQuests = new List<string>();
        public int bestCombo;
        public string lastWeeklyReward = "";
        public string researchDay = "";
        public List<string> researchClaims = new List<string>();
        public string teacherQuestID = "";
        public int teacherQuestStart;
        public int teacherQuestBest;
        public int teacherQuestsCompleted;
        public bool teacherMet;

        // ---- Long-term life-sim progress (all optional; older saves load with defaults) ----
        public int reputation;
        public List<KeyValue> mastery = new List<KeyValue>();
        public List<KeyValue> stats = new List<KeyValue>();
        public List<KeyValue> materials = new List<KeyValue>();
        public List<ProjectProgress> projects = new List<ProjectProgress>();
        public List<NpcProgress> npcs = new List<NpcProgress>();
        public List<string> collection = new List<string>();
        public List<FairEntry> fairEntries = new List<FairEntry>();
        public List<string> claimedEvents = new List<string>();
        public string timeOfDay = "";
        public string weather = "";
        public string weatherDay = "";
        public int labExpansionLevel;
        public int radioStation;
        public bool radioOn = true;
        public bool lampOn = true;
        public string labPlantsWateredDay = "";
        public string greenhouseWateredDay = "";
        public string greenhouseHarvestDay = "";
        public string storagePickupDay = "";
        public string readingDay = "";
        public int pendingSpills;
        public string lastVisitDay = "";
        public List<string> guideLessons = new List<string>();

        // ---- Beginner curriculum (optional; older saves start at the first lesson) ----
        public List<LessonRecord> lessons = new List<LessonRecord>();
        public List<string> lessonMistakes = new List<string>();
        public List<string> notebookPages = new List<string>();
        /// <summary>Mission progress: finished missions and the world flags their steps check.</summary>
        public List<string> missionsDone = new List<string>();
        public List<string> missionFlags = new List<string>();
    }

    [Serializable] public sealed class KeyValue { public string key = ""; public int value; }

    [Serializable]
    public sealed class ProjectProgress
    {
        public string id = "";
        public int stage;
        public int mistakes;
        public string readyDay = "";
        public bool completed;
        public bool submitted;
        public int timesCompleted;
        public string startedDay = "";
        public List<string> notes = new List<string>();
    }

    [Serializable]
    public sealed class NpcProgress
    {
        public string id = "";
        public int friendship;
        public int story;
        public string lastStoryDay = "";
        public string lastTalkDay = "";
        public string requestID = "";
        public int requestStart;
        public int requestsDone;
        public string lastRequestDay = "";
    }

    [Serializable]
    public sealed class FairEntry
    {
        public string week = "";
        public string projectID = "";
        public int score;
        public int rank;
        public bool settled;
    }

    public sealed class GameSaveManager : MonoBehaviour
    {
        public static GameSaveManager Instance { get; private set; }
        public GameSaveData Data { get; private set; }
        public int Combo { get; private set; }
        public float ComboMultiplier => Combo >= 10 ? 3f : Combo >= 5 ? 2f : Combo >= 3 ? 1.5f : 1f;
        public event Action Changed;
#if UNITY_EDITOR
        public static string TestDirectory;
        private string StorageDirectory => TestDirectory ?? Application.persistentDataPath;
#else
        private string StorageDirectory => Application.persistentDataPath;
#endif
        public string SavePath => Path.Combine(StorageDirectory, "chem-lab.json");
        public string Status { get; private set; } = "";
        private string lastCheckedDay;
        private DateTime lastCheckedDate;

        /// <summary>
        /// Playtest mode: every launch starts like a brand-new player (character creation, coat,
        /// teacher) but with 9999 coins.
        /// shortcut: discards the save on every launch; set to false before release.
        /// </summary>
        public static bool FreshStartEachLaunch = true;
        public const int PlaytestCoins = 9999;

        private void Awake()
        {
            Instance = this;
#if UNITY_EDITOR
            bool testing = TestDirectory != null;
#else
            const bool testing = false;
#endif
            Load(FreshStartEachLaunch && !testing);
            RefreshCalendar(DateTime.UtcNow.Date);
            Commit();
        }

        public void Load(bool fresh = false)
        {
            Data = fresh ? new GameSaveData { chemCoins = PlaytestCoins } : Read(SavePath) ?? Read(SavePath + ".bak") ?? new GameSaveData();
            Data.unlockedElements ??= new List<string>();
            Data.unlockedCosmeticIDs ??= new List<string>();
            Data.claimedDailyQuests ??= new List<string>();
            Data.ownedLabItems ??= new List<string>();
            Data.labPlacements ??= new List<LabPlacement>();
            Data.mastery ??= new List<KeyValue>();
            Data.stats ??= new List<KeyValue>();
            Data.materials ??= new List<KeyValue>();
            Data.projects ??= new List<ProjectProgress>();
            Data.npcs ??= new List<NpcProgress>();
            Data.collection ??= new List<string>();
            Data.fairEntries ??= new List<FairEntry>();
            Data.claimedEvents ??= new List<string>();
            Data.guideLessons ??= new List<string>();
            Data.lessons ??= new List<LessonRecord>();
            Data.lessons.RemoveAll(r => r == null);
            Data.lessonMistakes ??= new List<string>();
            Data.notebookPages ??= new List<string>();
            Data.missionsDone ??= new List<string>();
            Data.missionFlags ??= new List<string>();
            foreach (var project in Data.projects) project.notes ??= new List<string>();
            // Older saves only knew a single one-time expansion.
            if (Data.labExpanded && Data.labExpansionLevel < 1) Data.labExpansionLevel = 1;
            Data.labExpansionLevel = Mathf.Clamp(Data.labExpansionLevel, 0, 3);
            Data.reputation = Mathf.Max(0, Data.reputation);
            Data.researchClaims ??= new List<string>();
            Data.chemCoins = Mathf.Max(0, Data.chemCoins);
            Data.totalQuizzesSolved = Mathf.Max(0, Data.totalQuizzesSolved);
            Data.currentDailyStreak = Mathf.Max(0, Data.currentDailyStreak);
        }

        private static GameSaveData Read(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                string json = File.ReadAllText(path);
                if (!json.TrimStart().StartsWith("{") || !json.TrimEnd().EndsWith("}")) return null;
                return JsonUtility.FromJson<GameSaveData>(json);
            }
            catch (Exception error) when (error is IOException || error is ArgumentException || error is UnauthorizedAccessException)
            {
                Debug.LogWarning("შენახული მონაცემების წაკითხვა ვერ მოხერხდა.");
                return null;
            }
        }

        private void Update()
        {
            // Compare dates, not strings: formatting the day every frame made garbage.
            var today = DateTime.UtcNow.Date;
            if (today == lastCheckedDate) return;
            lastCheckedDate = today;
            if (lastCheckedDay == Iso(today)) return;
            RefreshCalendar(today);
            Commit();
        }

        public static string Iso(DateTime day) => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        private static bool Parse(string text, out DateTime day) =>
            DateTime.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out day);
        public static DateTime WeekStart(DateTime day) => day.Date.AddDays(-(((int)day.DayOfWeek + 6) % 7));

        public void RefreshCalendar(DateTime today)
        {
            lastCheckedDay = Iso(today);
            if (Data.dailyCounterDate != lastCheckedDay)
            {
                Data.dailyCounterDate = lastCheckedDay;
                Data.dailyQuizzesSolved = 0;
                Data.claimedDailyQuests.Clear();
            }

            var week = WeekStart(today);
            if (!Parse(Data.weekStartDate, out var previous))
            {
                Data.weekStartDate = Iso(week);
            }
            else if (week > previous)
            {
                if (Data.weeklyQuizzesSolved > WeeklyLeaderboardManager.TopRivalScore &&
                    Data.lastWeeklyReward != Data.weekStartDate)
                {
                    UnlockCosmetic("weekly_crown");
                    Data.lastWeeklyReward = Data.weekStartDate;
                    Status = "გასული კვირის პირველი ადგილი! ოქროს გვირგვინი შენია.";
                }
                Data.weeklyQuizzesSolved = 0;
                Data.weekStartDate = Iso(week);
            }
        }

        public int RecordCorrect(ChemistryQuestionSO question, DateTime today)
        {
            RefreshCalendar(today.Date);
            Combo++;
            Data.bestCombo = Mathf.Max(Data.bestCombo, Combo);
            int reward = Mathf.RoundToInt(question.coinReward * ComboMultiplier);
            Data.chemCoins += reward;
            Data.totalQuizzesSolved++;
            Data.dailyQuizzesSolved++;
            Data.weeklyQuizzesSolved++;
            foreach (string symbol in question.involvedElements)
                if (!Data.unlockedElements.Contains(symbol)) Data.unlockedElements.Add(symbol);
            LabProgress.OnReactionSolved(question);
            TouchStreak(today);
            Commit();
            return reward;
        }

        /// <summary>Beginner lessons pay small rewards and count toward the daily goal and streak, but not toward combos or rankings.</summary>
        public void RecordLesson(int coins)
        {
            var today = DateTime.UtcNow.Date;
            RefreshCalendar(today);
            Data.chemCoins += Mathf.Max(0, coins);
            Data.dailyQuizzesSolved++;
            TouchStreak(today);
            Commit();
        }

        private void TouchStreak(DateTime today)
        {
            string date = Iso(today);
            if (Data.lastPlayedDate == date) return;
            Data.currentDailyStreak = Parse(Data.lastPlayedDate, out var previous) && previous.Date.AddDays(1) == today.Date
                ? Data.currentDailyStreak + 1 : 1;
            Data.lastPlayedDate = date;
        }

        public void RecordWrong()
        {
            Combo = 0;
            Changed?.Invoke();
        }

        public void UnlockCosmetic(string id)
        {
            if (!Data.unlockedCosmeticIDs.Contains(id)) Data.unlockedCosmeticIDs.Add(id);
        }

        public bool PurchaseAndEquip(CosmeticItemSO item)
        {
            if (item == null) return false;
            if (!Data.unlockedCosmeticIDs.Contains(item.itemID))
            {
                if (item.weeklyExclusive || Data.chemCoins < item.price) return false;
                Data.chemCoins -= item.price;
                UnlockCosmetic(item.itemID);
            }
            switch (item.category)
            {
                case CosmeticCategory.LabCoat: Data.equippedLabCoatID = item.itemID; break;
                case CosmeticCategory.PersonalDeskDecor: Data.equippedDeskDecorID = item.itemID; break;
                default: Data.equippedHatOrGogglesID = item.itemID; break;
            }
            Commit();
            return true;
        }

        public bool ClaimQuest(int goal, int reward)
        {
            RefreshCalendar(DateTime.UtcNow.Date);
            string id = goal.ToString(CultureInfo.InvariantCulture);
            if (Data.dailyQuizzesSolved < goal || Data.claimedDailyQuests.Contains(id)) return false;
            Data.claimedDailyQuests.Add(id);
            Data.chemCoins += reward;
            Commit();
            return true;
        }

        public bool ClaimResearch(string id)
        {
            if (id != "membrane_diffusion" && id != "membrane_pump" && id != "chromatography" && id != "gas_compression") return false;
            string day = Iso(DateTime.UtcNow.Date);
            if (Data.researchDay != day) { Data.researchDay = day; Data.researchClaims.Clear(); }
            if (Data.researchClaims.Contains(id)) return false;
            Data.researchClaims.Add(id);
            Data.chemCoins += 20;
            Commit();
            return true;
        }

        public void Commit()
        {
            try
            {
                Directory.CreateDirectory(StorageDirectory);
                string temporary = SavePath + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(Data, true));
                if (File.Exists(SavePath))
                    File.Replace(temporary, SavePath, SavePath + ".bak");
                else File.Move(temporary, SavePath);
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
            {
                Status = "შენახვა ვერ მოხერხდა. შეამოწმე დისკზე თავისუფალი ადგილი.";
                Debug.LogWarning(Status);
            }
            Changed?.Invoke();
        }

        private void OnApplicationPause(bool paused) { if (paused && Data != null) Commit(); }
        private void OnApplicationQuit() { if (Data != null) Commit(); }
        private void OnDestroy() { if (Instance == this) Instance = null; }
    }
}

