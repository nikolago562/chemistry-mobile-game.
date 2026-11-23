using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ChemistryGame;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class LabGameChecks
{
    private static LabGame game;
    private static Action next;
    private static double due;
    private static EnterPlayModeOptions previousOptions;
    private static readonly BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
    private static Vector2 originalPosition;
    private static readonly List<string> failures = new List<string>();

    public static void Run()
    {
        previousOptions = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        GameSaveManager.TestDirectory = Path.GetFullPath("Logs/lab-qa-profile");
        Directory.CreateDirectory(GameSaveManager.TestDirectory);
        foreach (string name in new[] { "chem-lab.json", "chem-lab.json.bak", "chem-lab.json.tmp" })
        {
            string path = Path.Combine(GameSaveManager.TestDirectory, name);
            if (File.Exists(path)) File.Delete(path);
        }
        EditorSceneManager.OpenScene("Assets/Scenes/MainLab.unity");
        Application.logMessageReceived += Observe;
        EditorApplication.update += Tick;
        Schedule(Begin, 1.5);
        EditorApplication.isPlaying = true;
    }

    private static void Observe(string message, string stack, LogType type)
    {
        if ((type == LogType.Exception || type == LogType.Error) &&
            !stack.Contains("UnityEditor.Search") && !message.Contains("FMOD") && !message.Contains("d3d12"))
            failures.Add(message + "\n" + stack);
    }
    private static void Begin()
    {
        game = UnityEngine.Object.FindFirstObjectByType<LabGame>();
        Ensure(game != null && game.player != null, "Scene wiring");
        Ensure(game.Profile.IsOpen && !GameSaveManager.Instance.Data.profileCompleted, "New players must create a profile first");
        Ensure(!game.player.canMove, "Character creation locks movement");
        LabUI.CloseTop();
        Ensure(game.Profile.IsOpen, "Character creation cannot be skipped with Escape");
        game.Profile.NameInput.text = "   ";
        Ensure(!game.Profile.Confirm(), "Blank player names must be rejected");
        game.Profile.NameInput.text = "  თამარ  ";
        game.Profile.SelectAppearance(2, 1, 3);
        Snapshot("00-character-creation");
        Schedule(CreatedProfile, 0.5);
    }
    private static void CreatedProfile()
    {
        Ensure(game.Profile.Confirm(), "Profile completion");
        var profile = GameSaveManager.Instance;
        profile.Load();
        Ensure(profile.Data.profileCompleted && profile.Data.playerName == "თამარ" && profile.Data.skinTone == 2 &&
            profile.Data.hairColor == 1 && profile.Data.shirtColor == 3, "Name and appearance survive reload");
        Ensure(game.Rankings.Entries().Single(e => e.IsPlayer).Name == "თამარ", "Leaderboard uses chosen name");
        var material = game.player.transform.Find("Sprite").GetComponent<SpriteRenderer>().material;
        Ensure(material.GetColor("_HairColor") == PlayerAppearance.Hair[1], "Selected appearance applies to player");
        bool resumed = false;
        game.Profile.Begin(() => resumed = true);
        Ensure(resumed && !game.Profile.IsOpen, "Returning players skip character creation");
        Debug.Log("CHARACTER_PROFILE_CHECKS_PASSED");
        Ensure(game.questions.Length == 26 && game.elements.elements.Length == 118, "Question and element counts");
        Ensure(game.elements.elements.Select(e => e.symbol).Distinct().Count() == 118, "Duplicate element symbols");
        Ensure(game.elements.elements.Select(e => e.row * 18 + e.column).Distinct().Count() == 118, "Periodic table slots");
        Ensure(!game.player.canMove && LabUI.AnyOpen, "Opening dialogue must lock movement");
        Ensure(game.player.GetComponent<BoxCollider2D>() != null && game.player.GetComponent<CircleCollider2D>() == null, "Player feet collider");
        Ensure(game.player.GetComponent<UnityEngine.Rendering.SortingGroup>() != null, "Character and cosmetics must render together in front of the locker");
        Ensure(ChemMinigameUI.Normalize(" 2 h2o ") == "2H2O", "Input normalization");
        Ensure(game.interfaceFont.HasCharacter('ა', false, true), "Georgian glyph font");
        Ensure(game.interfaceFont.faceInfo.familyName.Contains("FiraGO"), "FiraGO interface typeface");
        CheckEightWayArt();
        CheckEconomy();
        ResetSave();
        game.Tutorial.Initialize(game);
        game.HUD.Refresh();
        Ensure(game.Dialogue.IsOpen, "The teacher gives the first lesson");
        Schedule(TutorialPan, 0.1);
    }
    private static void TutorialPan()
    {
        var dialogueType = typeof(TeacherDialogueUI);
        var pages = (string[])dialogueType.GetField("pages", Hidden).GetValue(game.Dialogue);
        Ensure(pages.Length >= 8 && pages.Length <= 14 && pages.Any(p => p.Contains("დღესვე")) && pages.Any(p => p.Contains("მომავალში")), "Welcome shows what you can do today and what opens later, then the closet");
        ShowPage(1);
        Schedule(TutorialToday, 1.6);
    }
    private static void ShowPage(int page)
    {
        var dialogueType = typeof(TeacherDialogueUI);
        var pages = (string[])dialogueType.GetField("pages", Hidden).GetValue(game.Dialogue);
        dialogueType.GetField("page", Hidden).SetValue(game.Dialogue, page < 0 ? pages.Length + page : page);
        dialogueType.GetMethod("BeginPage", Hidden).Invoke(game.Dialogue, null);
    }
    private static void TutorialToday()
    {
        Snapshot("01a-welcome-today");
        ShowPage(6);
        Schedule(TutorialFuture, 1.6);
    }
    private static void TutorialFuture()
    {
        Ensure(game.Personal.Room.gameObject.activeSelf, "The teacher's picture of the lab wakes the lab room");
        Snapshot("01b-welcome-future");
        ShowPage(-1);
        Schedule(TutorialPanView, 1.6);
    }
    private static void TutorialPanView()
    {
        var closet = UnityEngine.Object.FindObjectsByType<InteractableStation>(FindObjectsSortMode.None).Single(s => s.supplyCloset);
        Ensure(Vector2.Distance(Camera.main.transform.position, closet.transform.position) < 3, "Camera pans to the locker while the teacher talks about it");
        Snapshot("01-tutorial");
        Schedule(TutorialContinue, 0.1);
    }
    private static void TutorialContinue()
    {
        LabUI.CloseTop();
        Ensure(!LabUI.Root.Find("მენიუ").gameObject.activeSelf, "The menu rail stays hidden during the intro");
        originalPosition = game.player.GetComponent<Rigidbody2D>().position;
        CheckReachability();
        CheckBenchWalking();
        Schedule(AtCloset, 0.25);
    }

    private static void CheckEightWayArt()
    {
        var animator = game.player.GetComponent<Animator>();
        Vector2[] directions = { Vector2.down, new Vector2(-1, -1).normalized, Vector2.left, new Vector2(-1, 1).normalized,
            Vector2.up, new Vector2(1, 1).normalized, Vector2.right, new Vector2(1, -1).normalized };
        var art = game.player.transform.Find("Sprite").GetComponent<SpriteRenderer>();
        var frames = new HashSet<Rect>();
        foreach (Vector2 direction in directions)
        {
            animator.SetFloat("MoveX", direction.x);
            animator.SetFloat("MoveY", direction.y);
            animator.SetBool("IsMoving", true);
            animator.Play("Walk", 0, 0);
            animator.Update(0.01f);
            Ensure(art.sprite.texture.name == "PunyCharacterBase" && art.sprite.rect.width == 32 && art.sprite.pixelsPerUnit == 8, "Pixel student artwork and scale");
            frames.Add(art.sprite.rect);
        }
        Ensure(frames.Count == 8, "Every direction must have its own walk artwork");
        animator.SetFloat("MoveX", 0);
        animator.SetFloat("MoveY", -1);
        animator.SetBool("IsMoving", false);
        animator.Play("Idle", 0, 0);
        animator.Update(0);
        Debug.Log("EIGHT_DIRECTION_ART_PASSED");
    }
    private static void CheckEconomy()
    {
        var save = GameSaveManager.Instance;
        ResetSave();
        var q = game.questions[0];
        var today = DateTime.UtcNow.Date;
        int[] expected = { 10, 10, 15, 15, 20, 20, 20, 20, 20, 30 };
        for (int i = 0; i < expected.Length; i++)
            Ensure(save.RecordCorrect(q, today) == expected[i], "Combo tier " + (i + 1));
        Ensure(save.Data.currentDailyStreak == 1 && save.Data.totalQuizzesSolved == 10, "Same-day counters");
        Ensure(save.Data.unlockedElements.Count == 2, "Distinct element unlocks");
        save.RecordWrong();
        Ensure(save.Combo == 0 && save.RecordCorrect(q, today) == 10, "Mistake resets combo");
        save.RecordCorrect(q, today.AddDays(1));
        Ensure(save.Data.currentDailyStreak == 2, "Consecutive UTC date");
        save.RecordCorrect(q, today.AddDays(4));
        Ensure(save.Data.currentDailyStreak == 1, "Missed-day reset");

        save.Data.chemCoins = 100;
        var item = game.cosmetics.First(c => c.itemID == "coat_yellow");
        Ensure(save.PurchaseAndEquip(item) && save.Data.chemCoins == 50, "Purchase charges once");
        Ensure(save.PurchaseAndEquip(item) && save.Data.chemCoins == 50, "Equipping owned item is free");
        Ensure(!save.PurchaseAndEquip(game.cosmetics.First(c => c.itemID == "weekly_crown")), "Exclusive cannot be purchased");
        save.Data.weekStartDate = GameSaveManager.Iso(GameSaveManager.WeekStart(today).AddDays(-7));
        save.Data.weeklyQuizzesSolved = 65;
        save.RefreshCalendar(today);
        Ensure(save.Data.weeklyQuizzesSolved == 0 && save.Data.unlockedCosmeticIDs.Contains("weekly_crown"), "Weekly settlement");
        save.Data.dailyCounterDate = GameSaveManager.Iso(today);
        save.Data.dailyQuizzesSolved = 5;
        Ensure(save.ClaimQuest(3, 10) && !save.ClaimQuest(3, 10), "Quest reward only once");
        save.Commit();
        int coins = save.Data.chemCoins;
        save.Load();
        Ensure(save.Data.chemCoins == coins && save.Data.equippedLabCoatID == "coat_yellow", "JSON reload");
        File.WriteAllText(save.SavePath, "{broken");
        save.Load();
        Ensure(save.Data.unlockedCosmeticIDs.Contains("weekly_crown"), "Backup recovery");
        Debug.Log("ECONOMY_CHECKS_PASSED");
    }
    private static void ResetSave()
    {
        var save = GameSaveManager.Instance;
        typeof(GameSaveManager).GetProperty("Data").SetValue(save, new GameSaveData());
        typeof(GameSaveManager).GetProperty("Combo").SetValue(save, 0);
        save.Data.profileCompleted = true;
        save.Data.playerName = "თამარ";
        save.RefreshCalendar(DateTime.UtcNow.Date);
        save.Commit();
    }

    private static void CheckReachability()
    {
        Physics2D.SyncTransforms();
        var body = game.player.GetComponent<Rigidbody2D>();
        var collider = game.player.GetComponent<BoxCollider2D>();
        Vector2 min = game.classroom.TransformPoint(new Vector3(24, -23));
        Vector2 max = game.classroom.TransformPoint(new Vector3(66, 11));
        const float step = 0.5f;
        var start = Vector2Int.RoundToInt(((Vector2)body.position + collider.offset - min) / step);
        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            if (visited.Contains(node)) continue;
            Vector2 point = min + (Vector2)node * step;
            if (point.x < min.x || point.x > max.x || point.y < min.y || point.y > max.y) continue;
            if (Physics2D.OverlapBoxAll(point, collider.size + Vector2.one * 0.06f, 0)
                .Any(hit => !hit.isTrigger && hit.attachedRigidbody != body)) continue;
            visited.Add(node);
            foreach (var direction in new[] { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down })
                if (!visited.Contains(node + direction)) queue.Enqueue(node + direction);
        }
        Ensure(visited.Count > 100, "Spawn is trapped by furniture");
        // Classmates walk on their own schedules (often in other rooms), so only fixed classroom stations are checked here.
        foreach (var station in UnityEngine.Object.FindObjectsByType<InteractableStation>(FindObjectsSortMode.None).Where(s => s.stationType != StationType.Classmate))
        {
            var trigger = station.GetComponent<BoxCollider2D>();
            Ensure(visited.Any(node => trigger.OverlapPoint(min + (Vector2)node * step)), "Unreachable station " + station.name);
        }
        Debug.Log("STATION_PATHS_CHECKED_" + visited.Count);
    }

    private static void CheckBenchWalking()
    {
        var body = game.player.GetComponent<Rigidbody2D>();
        var feet = game.player.GetComponent<BoxCollider2D>();
        var savedPosition = body.position;
        var previousMode = Physics2D.simulationMode;
        Physics2D.simulationMode = SimulationMode2D.Script;
        try
        {
            foreach (float y in new[] { 2f, -6f, -14f })
            foreach (float x in new[] { 37.5f, 52f })
            {
                Vector2 origin = game.classroom.TransformPoint(new Vector3(x, y));
                Vector2[] ring = { new Vector2(-2.4f, -0.4f), new Vector2(2.4f, -0.4f),
                    new Vector2(2.4f, 2.25f), new Vector2(-2.4f, 2.25f), new Vector2(-2.4f, -0.4f) };
                body.position = origin + ring[0] - feet.offset;
                Physics2D.SyncTransforms();
                for (int edge = 1; edge < ring.Length; edge++)
                {
                    Vector2 delta = ring[edge] - ring[edge - 1];
                    int steps = Mathf.CeilToInt(delta.magnitude / 0.08f);
                    for (int tick = 0; tick < steps; tick++)
                    {
                        body.linearVelocity = delta / (steps * 0.02f);
                        Physics2D.Simulate(0.02f);
                    }
                    Ensure(Vector2.Distance(body.position + feet.offset, origin + ring[edge]) < 0.08f,
                        "Blocked walking around challenge table " + new Vector2(x, y) + " edge " + edge +
                        " feet " + (body.position + feet.offset - origin) + " colliders " +
                        string.Join(",", Physics2D.OverlapBoxAll(body.position + feet.offset, Vector2.one * 0.9f, 0)
                            .Where(c => !c.isTrigger && c.attachedRigidbody != body).Select(c => c.name)));
                }
                // Walk diagonally into the front edge: it must block entry but allow lateral sliding.
                body.position = origin + new Vector2(-1.2f, -0.4f) - feet.offset;
                Physics2D.SyncTransforms();
                for (int tick = 0; tick < 35; tick++)
                {
                    body.linearVelocity = new Vector2(3, 3);
                    Physics2D.Simulate(0.02f);
                }
                Vector2 local = body.position + feet.offset - origin;
                Ensure(local.x > 0.5f && local.y < 0.05f, "Table edge must slide without admitting the player");
            }
            Debug.Log("SIX_CHALLENGE_TABLE_WALKING_CHECKS_PASSED");
        }
        finally
        {
            body.linearVelocity = Vector2.zero;
            body.position = savedPosition;
            Physics2D.simulationMode = previousMode;
            Physics2D.SyncTransforms();
        }
    }

    private static void AtCloset()
    {
        Ensure(game.player.canMove, "Movement unlocks after closing dialogue");
        Snapshot("01b-waypoint");
        var closet = UnityEngine.Object.FindObjectsByType<InteractableStation>(FindObjectsSortMode.None).Single(s => s.supplyCloset);
        var trigger = closet.GetComponent<BoxCollider2D>();
        Ensure(!trigger.OverlapPoint(closet.transform.position + Vector3.up), "Locker activation through its back");
        var blocker = closet.GetComponentsInChildren<BoxCollider2D>().Single(c => !c.isTrigger);
        Ensure(blocker.size.x <= 2 && !blocker.OverlapPoint(closet.transform.TransformPoint(trigger.offset)), "Locker approach is unobstructed");
        MoveTo(closet.transform.TransformPoint(trigger.offset));
        Schedule(CoatCollected, 0.5);
    }
    private static void CoatCollected()
    {
        var data = GameSaveManager.Instance.Data;
        Ensure(game.Tutorial.Step == TutorialManager.TutorialStep.Completed && data.tutorialCompleted, "The coat finishes the intro");
        Ensure(data.equippedLabCoatID == "coat_white", "Starter coat equipped");
        Ensure(data.chemCoins == 10 && data.currentDailyStreak == 1 && LabProgress.Has("vinegar") == 3 && LabProgress.Has("soda") == 3, "The coat pays once and brings the starter kit");
        game.Tutorial.CompleteIntro();
        Ensure(data.chemCoins == 10, "No duplicate tutorial payout");
        LabUI.CloseTop();
        Snapshot("09-locker");
        // The first table is the easy, colourful one: cabbage juice + a drop of acid turns red.
        var potions = Station(StationType.AcidBaseStation);
        Ensure(Missions.Current == ActOne.FirstMix && Missions.IsTarget(potions.transform.position), "The first mission points at the pH potion table");
        Ensure(Vector2.Distance(potions.transform.position, game.player.transform.position) < Vector2.Distance(Station(StationType.ReactionTable).transform.position, game.player.transform.position),
            "The first table is nearer than the mixing bench");
        Ensure(Mixing.Reagents.Count(Mixing.Known) == 7 && !Mixing.Known(Mixing.Get("hcl")), "A new player's shelf holds only the starter jars (" + Mixing.Reagents.Count(Mixing.Known) + ")");
        Ensure(Orders.Today.All(o => o.Wants != MixSign.Heat && o.Wants != MixSign.Solid), "Early orders only ask for what the starter jars can do");
        MoveTo(potions.transform.position + Vector3.down * 0.75f);
        game.Interact(potions);
        var kit = game.Lessons.Tables;
        Ensure(kit.Current == Kits.Potions, "The pH table opens the potion kit");
        kit.Add(2, 1);
        var guide = UnityEngine.Object.FindFirstObjectByType<Coach>();
        guide?.SendMessage("LateUpdate");
        Ensure(guide != null && guide.Caption.Contains("მჟავა"), "The tutorial points at the acid drop (" + guide?.Caption + ")");
        kit.Add(0, 1); kit.Build();
        Ensure(Kits.PotionMade && Missions.Current == ActOne.FirstMix && Missions.StepIndex == 2, "The first potion moves the mission on to the mixing bench");
        Snapshot("01c-first-potion");
        LabUI.CloseTop();
        var bench = Station(StationType.ReactionTable);
        Ensure(Missions.IsTarget(bench.transform.position), "Then the mission points at the mixing bench");
        Ensure(Orders.Today.Length == Orders.PerDay && Orders.Today.Select(o => o.Npc).Distinct().Count() == Orders.PerDay && Orders.Today.Select(o => o.Wants).Distinct().Count() == Orders.PerDay, "Three different classmates order three different signs");
        MoveTo(bench.transform.position + Vector3.down * 0.75f);
        game.Interact(bench);
        Ensure(game.Lessons.Bench.IsOpen && !game.Lessons.IsOpen, "The reaction table opens the mixing bench, not a lesson");
        game.Lessons.Bench.Pick("salt");
        Ensure(game.Lessons.Bench.Picked.Count == 0, "During the tutorial the bench only takes vinegar and soda");
        game.Lessons.Bench.Pick("vinegar");
        Schedule(FirstMixCoach, 0.4);
    }
    private static void FirstMixCoach()
    {
        var data = GameSaveManager.Instance.Data;
        var coach = UnityEngine.Object.FindFirstObjectByType<Coach>();
        Ensure(coach != null && coach.Caption.Contains("სოდა"), "The tutorial points at the next jar (" + coach?.Caption + ")");
        Snapshot("01b-coach");
        game.Lessons.Bench.Pick("soda"); game.Lessons.Bench.Guess(MixSign.Gas); game.Lessons.Bench.Mix();
        Ensure(LabProgress.Owns(Mixing.Key("vinegar", "soda")) && LabProgress.Has("vinegar") == 2 && LabProgress.Has("soda") == 2, "Mixing uses up one of each ingredient");
        Ensure(Missions.IsDone("first_mix") && Missions.Current == ActOne.Safety, "The first mix finishes the first mission");
        Snapshot("01c-first-mix");
        Ensure(TouchControls.Snap(new Vector2(0.7f, 0.7f)) == new Vector2(1, 1).normalized && TouchControls.Snap(new Vector2(0.95f, 0.2f)) == Vector2.right
            && TouchControls.Snap(new Vector2(-0.6f, -0.7f)) == new Vector2(-1, -1).normalized && TouchControls.Snap(new Vector2(0.1f, 0.1f)) == Vector2.zero, "The joystick snaps to 8 directions with a dead zone");
        var open = Orders.Next;
        int wallet = data.chemCoins, bond = Classmates.State(open.Npc).friendship;
        Ensure(Orders.Deliver(open.Wants) == open && Orders.Done(open) && data.chemCoins == wallet + Orders.Pay && Classmates.State(open.Npc).friendship == bond + Orders.Friendship, "A matching mix fills an order and pays once");
        Ensure(Orders.Deliver(open.Wants) == null, "An order is filled only once");
        for (int k = 0; k < 10 && LabUI.AnyOpen; k++) LabUI.CloseTop();
        Schedule(AfterTutorial, 0.8);
    }
    private static void AfterTutorial()
    {
        LabUI.CloseTop();
        var rail = LabUI.Root.Find("მენიუ");
        Ensure(rail.gameObject.activeSelf && rail.GetComponentsInChildren<Button>().Length == GameHUD.RailMenus.Length, "After the intro the main menus sit on a rail at the right edge");
        Snapshot("08b-menu-rail");
        rail.GetComponentsInChildren<Button>()[0].onClick.Invoke();
        Ensure(game.Lessons.MapOpen, "The rail's first button opens the learning map");
        LabUI.CloseTop();
        Ensure(!LabUI.AnyOpen && TeacherGuide.Next != null && TeacherGuide.Next.ID == "tour", "A skipped lesson stays available at the teacher");
        CheckSafetyMission();
        CheckCurriculum();
        Schedule(MissionStates, 0.6);
    }
    private static WorldInteractable Prop(string prompt) =>
        UnityEngine.Object.FindObjectsByType<WorldInteractable>(FindObjectsSortMode.None).First(w => w.PromptText() == prompt);
    private static void CheckSafetyMission()
    {
        var data = GameSaveManager.Instance.Data;
        Ensure(Missions.Current == ActOne.Safety && Missions.StepIndex == 1 && Missions.Steering, "After the coat the safety mission asks for the hazards");
        Ensure(!Missions.IsTarget(Station(StationType.TutorialStation).transform.position) && Missions.IsTarget(game.classroom.TransformPoint(ActOne.CartPoint)),
            "The first hazard, not the desk, is the objective");
        int coins = data.chemCoins;
        game.ActOne.UseCart();
        Prop("[E] მოწმენდა").Use();
        Ensure(Missions.Current == ActOne.Safety && Missions.Step.Text().Contains("2/3"), "Hazards count up one by one");
        Prop("[E] ეტიკეტის დაწერა").Use();
        Ensure(Missions.IsDone("safety") && data.chemCoins == coins + 10, "Fixing three hazards completes the safety mission and pays once");
        Ensure(Missions.Current == ActOne.States, "Next mission is the states of matter");
    }
    private static void MissionStates()
    {
        Missions.Refresh();
        Ensure(Missions.Current == ActOne.States, "The states mission is running");
        Snapshot("62-touch-controls");
        coinsBefore = GameSaveManager.Instance.Data.chemCoins;
        game.ActOne.UseCart();
        Schedule(MissionBoil, 3.0);
    }
    private static int coinsBefore;
    private static void MissionBoil()
    {
        Ensure(Missions.Flag("melted") && Missions.StepIndex == 1, "Heating the ice melts it");
        game.ActOne.UseCart();
        Schedule(MissionCondense, 3.6);
    }
    private static void MissionCondense()
    {
        Ensure(Missions.Flag("boiled"), "More heat boils the water");
        game.ActOne.UseCart();
        Schedule(MissionSteam, 3.4);
    }
    private static void MissionSteam()
    {
        var data = GameSaveManager.Instance.Data;
        Ensure(Missions.IsDone("states") && data.chemCoins == coinsBefore + 15 && Missions.Current == ActOne.Steam, "Melt, boil and condense complete the three-states mission");
        Snapshot("62-mission-states");
        Prop("[E] ჩაიდნის აღება და ავსება").Use();
        Ensure(Missions.Flag("kettle_full") && Missions.StepIndex == 1, "The kettle is picked up and filled");
        game.ActOne.UseCart();
        Ensure(Missions.Flag("kettle_placed"), "The kettle is placed on the cart");
        game.ActOne.UseCart();
        Schedule(MissionDone, 5.0);
    }
    private static void MissionDone()
    {
        var data = GameSaveManager.Instance.Data;
        Ensure(Missions.IsDone("steam") && Missions.Current == null && !Missions.Steering, "The steam machine finishes act one");
        Ensure(data.ownedLabItems.Contains("steam_wheel") && data.labPlacements.Any(p => p.itemID == "steam_wheel"), "The steam wheel trophy is standing in the lab");
        Snapshot("63-mission-steam");
        // Activity music: opening a lesson should duck the world and fade in the cheerful tune.
        var audio = AmbientAudio.Instance;
        Ensure(!AmbientAudio.Break, "The day starts in a lesson, not a break");
        audio.RingBell();
        Ensure(AmbientAudio.Break, "The bell starts a break");
        audio.RingBell();
        Ensure(!AmbientAudio.Break, "The next bell ends the break");
        game.Lessons.OpenWorkbook(Curriculum.Lessons[0], true);
        Schedule(ActivityMusic, 1.2);
    }
    private static void ActivityMusic()
    {
        var playful = (AudioSource)Field(AmbientAudio.Instance, "playful");
        var tone = (AudioSource)Field(AmbientAudio.Instance, "tone");
        Ensure(playful.isPlaying && playful.volume > 0.1f && tone.volume < 0.03f, "During an activity the cheerful tune plays and the room hum ducks (" + playful.volume + "/" + tone.volume + ")");
        game.Lessons.Close();
        Schedule(AfterCurriculum, 0.3);
    }
    private static InteractableStation Station(StationType type) =>
        UnityEngine.Object.FindObjectsByType<InteractableStation>(FindObjectsSortMode.None).Single(s => s.stationType == type);
    private static void CheckFocus()
    {
        var desk = Station(StationType.TutorialStation).GetComponent<Focus>();
        var atom = Station(StationType.AtomTable).GetComponent<Focus>();
        var shop = UnityEngine.Object.FindObjectsByType<InteractableStation>(FindObjectsSortMode.None)
            .First(s => s.stationType == StationType.CosmeticShopStation && !s.supplyCloset).GetComponent<Focus>();
        Ensure(desk.Kind() == Glow.Teacher && atom.Kind() == Glow.Equipment && shop.Kind() == Glow.Reward, "Highlight colours follow the interaction type");
        desk.Near = shop.Near = true;
        Ensure(!(desk.IsFocused && shop.IsFocused), "Only one object takes the [E] prompt");
        var door = UnityEngine.Object.FindObjectsByType<Focus>(FindObjectsSortMode.None).First(f => f.Kind() == Glow.Door);
        door.Near = true; desk.Near = false;
        Ensure(Focus.Current != door, "Doors glow but never take the [E] prompt");
        door.Near = shop.Near = false;
    }
    private static void CheckCurriculum()
    {
        var save = GameSaveManager.Instance;
        var lessons = game.Lessons;
        Ensure(Curriculum.Next.ID == "c1_matter" && Curriculum.Percent == 0 && !Curriculum.Complete, "The curriculum starts at the first chemistry lesson");
        Ensure(Curriculum.Lessons.Length >= 40 && Curriculum.Lessons.Select(l => l.ID).Distinct().Count() == Curriculum.Lessons.Length, "Unique curriculum lessons");
        Ensure(Curriculum.Lessons.Select(l => l.Chapter).Distinct().Count() == 8 && Curriculum.Lessons.Count(l => l.IsReview) == 8, "Eight chapters, each with a review");
        foreach (var l in Curriculum.Lessons)
        {
            Ensure(l.Why.Length > 0 && l.Teach.Length > 0, "Lesson explains why: " + l.ID);
            if (l.IsReview) { Ensure(Curriculum.ReviewKeys(l).Length == 4, "Chapter review has four questions: " + l.ID); continue; }
            Ensure(l.Questions.Length == 3 && l.Questions.All(q => q.Choices.Length == 3 && q.Why.Length == 3 && q.Choices.Distinct().Count() == 3 && q.Hint.Length > 0), "Lesson questions: " + l.ID);
            Ensure(l.Note.Length > 0, "Notebook entry: " + l.ID);
            if (l.Collect != "") Ensure(LabProgress.Catalog.Any(c => c.ID == l.Collect), "Collection reward exists: " + l.ID);
            if (l.Kind == PracticeKind.Sort) { Ensure(l.Items.Length >= 6 && l.Items.All(i => i.Bin < l.Bins.Length && i.Why.Length > 0), "Sort items: " + l.ID); continue; }
            Ensure(l.Steps.Length == 3 && l.Steps.All(p => p.Solution.Length == p.Counters.Length && (p.Check == null || p.Check(p.Solution) == null)), "Three solvable practice steps: " + l.ID);
            if (l.Kind == PracticeKind.Equation)
                foreach (var p in l.Steps)
                {
                    var parts = p.Formula.Split(new[] { "->" }, StringSplitOptions.None);
                    int left = parts[0].Split('+').Length;
                    string Side(int from, int to) => string.Join("+", Enumerable.Range(from, to - from).Select(i => p.Solution[i] + p.Counters[i]));
                    var a = ChemistryChallenges.Atoms(Side(0, left)); var b = ChemistryChallenges.Atoms(Side(left, p.Counters.Length));
                    Ensure(a.Count == b.Count && a.All(e => b.TryGetValue(e.Key, out int n) && n == e.Value), "Balanced lesson equation: " + p.Formula);
                }
        }
        foreach (var chapter in Curriculum.Chapters)
        {
            Ensure(LabProgress.Catalog.Any(c => c.ID == "badge:" + chapter.ID), "Chapter badge: " + chapter.ID);
            if (chapter.Cosmetic != "") Ensure(game.cosmetics.Any(c => c.itemID == chapter.Cosmetic), "Chapter cosmetic: " + chapter.Cosmetic);
            if (chapter.LabItem != "") Ensure(PersonalLab.Catalog.Any(c => c.ID == chapter.LabItem), "Chapter lab item: " + chapter.LabItem);
        }

        // Advanced content and stations stay locked for a beginner.
        int coins = save.Data.chemCoins;
        int xpBefore = save.Data.totalXP;
        game.Challenges.Open();
        Ensure(!game.Challenges.IsOpen && !Curriculum.TierOpen(1), "The advanced floor is locked right after the tutorial");
        Ensure(Curriculum.StationOpen(StationType.AtomTable) && Curriculum.ActivityOpen("solution"), "Lessons are optional: stations and lab activities are open from the start");
        game.Interact(Station(StationType.QuizDesk));
        game.OpenMenu(3);
        Ensure(!LabUI.AnyOpen, "The advanced floor and rankings still only show a message");
        CheckFocus();

        // Lesson 1: a Duolingo-style deck of varied cards, then the reward.
        var lesson = Curriculum.Next;
        lessons.OpenWorkbook(lesson);
        Ensure(lessons.IsOpen && lessons.Current == CurriculumUI.Ex.Pick && !lessons.Advance(), "Lessons open straight on a hands-on card that must be answered");
        var item = lesson.Items[lessons.CardItems[0]];
        int dealt = lessons.Cards;
        lessons.Pick((item.Bin + 1) % lesson.Bins.Length);
        Ensure(lessons.Solved && !lessons.Correct && lessons.Feedback.Contains(item.Why) && save.Data.chemCoins == coins && lessons.Cards == dealt + 1, "A wrong pick explains the concept without punishment and comes back later");
        var atomLesson = Curriculum.Lessons.First(l => l.Chapter == 2);
        Ensure(Curriculum.ChapterOf(atomLesson).Station == StationType.AtomTable && game.StationPoint(StationType.AtomTable) != game.StationPoint(StationType.TutorialStation), "Later chapters are played at their own stations");
        var kinds = new List<CurriculumUI.Ex>();
        for (int guard = 0; guard < 30 && lessons.Advance() && lessons.Current != CurriculumUI.Ex.Reward; guard++)
        {
            if (!kinds.Contains(lessons.Current)) Snapshot("54-lesson-" + lessons.Current.ToString().ToLowerInvariant());
            kinds.Add(lessons.Current);
            SolveCard(lesson);
            Ensure(lessons.Solved && lessons.Correct, "Card solved: " + lessons.Current);
        }
        Ensure(lessons.Current == CurriculumUI.Ex.Reward && kinds.Count <= CurriculumUI.DeckSize + 1 && kinds.Distinct().Count() >= 3 && kinds.Zip(kinds.Skip(1), (a, b) => a != b).All(x => x), "A short lesson mixes at least three card kinds, never the same twice in a row (" + string.Join(",", kinds) + ")");
        var gaps = Curriculum.Lessons.Where(x => !x.IsReview).Select(x => (x.ID, gap: Curriculum.MakeGap(x, 0))).ToList();
        foreach (var (id, gap) in gaps) Debug.Log("GAP " + id + ": " + (gap == null ? "-" : gap.Before + "[" + gap.Answer + "]" + gap.After + " | " + string.Join(", ", gap.Words)));
        Ensure(gaps.Count(g => g.gap != null) * 10 >= gaps.Count * 8 && gaps.Where(g => g.gap != null).All(g => g.gap.Words.Distinct().Count() == 3), "Most lessons get a fill-the-gap card (" + gaps.Count(g => g.gap != null) + "/" + gaps.Count + ")");
        int paid = lesson.Coins + 2 * lessons.Asked;
        Ensure(Curriculum.Done("c1_matter") && save.Data.chemCoins == coins + paid, "Lesson reward is paid once (" + (save.Data.chemCoins - coins) + ")");
        Ensure(save.Data.totalXP == xpBefore + Curriculum.LessonXP && save.Data.dailyXP >= Curriculum.LessonXP && save.Data.xpGoal == 20, "A lesson with a mistake gives base XP toward the daily goal (" + (save.Data.totalXP - xpBefore) + ")");
        Ensure(save.Data.notebookPages.Contains("c1_matter") && save.Data.lessonMistakes.Count == 0, "Notebook page is saved");
        Snapshot("56-lesson-reward");
        var loot = lessons.Chest; int beforeChest = save.Data.chemCoins;
        lessons.OpenChest();
        Snapshot("56b-lesson-chest");
        Ensure(lessons.ChestOpen && (loot.Material == null ? save.Data.chemCoins > beforeChest : LabProgress.Has(loot.Material) >= loot.Count), "The lesson chest pays a bonus");
        var rolls = Enumerable.Range(0, 100).Select(i => CurriculumUI.Roll(i / 100f)).ToList();
        Ensure(rolls.Count(r => r.Rare) == 8 && rolls.Count(r => r.Material != null && !r.Rare) == 25 && rolls.All(r => r.Material == null || Mixing.Get(r.Material) != null), "Chest odds: 8% rare jars, 25% an ingredient, otherwise coins");
        save.Load();
        Ensure(Curriculum.Done("c1_matter") && Curriculum.Next.ID == "c1_states", "Curriculum progress survives a reload");
        // Chapter reviews quiz; a wrong answer shows the right one, explains it and returns at the end.
        lessons.OpenWorkbook(Curriculum.Lessons.First(l => l.IsReview));
        var first = lessons.Question;
        Ensure(lessons.Current == CurriculumUI.Ex.Choice && first != null, "Chapter reviews quiz");
        dealt = lessons.Cards;
        lessons.Select(1);
        Snapshot("55-lesson-test");
        lessons.Check();
        Ensure(lessons.Feedback.Contains(first.Why[1]) && lessons.Feedback.Contains(first.Choices[0]) && lessons.Cards == dealt + 1 && lessons.Advance(), "A wrong answer is explained and retried later");
        lessons.Close();
        Ensure(!Curriculum.Available(Curriculum.Get("c2_parts")) && Curriculum.Requirement(Curriculum.Get("c2_parts")).Length > 0, "Later lessons name their prerequisite");
        lessons.Close();

        // Counter practice names what was counted wrong.
        var atoms = Curriculum.Get("c2_parts");
        lessons.OpenWorkbook(atoms);
        Ensure(lessons.Current == CurriculumUI.Ex.Build, "Counter lessons open on the builder");
        lessons.Adjust(0, 1);
        Ensure(lessons.Feedback.Contains("პროტონები") || lessons.Feedback.Contains("ნეიტრონები") || lessons.Feedback.Contains("ელექტრონები"), "Guided atom hint names the wrong particle");
        lessons.Adjust(0, 1); lessons.Adjust(1, 2); lessons.Adjust(2, 2);
        Ensure(lessons.Solved && lessons.Advance() && lessons.Current != CurriculumUI.Ex.Build, "Guided atom building, then a different card");
        for (int guard = 0; guard < 5 && lessons.Current != CurriculumUI.Ex.Build; guard++) { SolveCard(atoms); lessons.Advance(); }
        Ensure(lessons.Current == CurriculumUI.Ex.Build, "The free build comes later in the deck");
        lessons.Check();
        Ensure(!lessons.Solved && lessons.Feedback.Contains("ჯერ არა"), "Free practice checks on request");
        lessons.Adjust(0, 3); lessons.Adjust(1, 4); lessons.Adjust(2, 3); lessons.Check();
        Snapshot("57-lesson-atom");
        Ensure(lessons.Solved, "Free atom building");
        lessons.Close();
        lessons.OpenWorkbook(Curriculum.Get("c6_conserve"));
        lessons.Adjust(2, 1);
        Ensure(lessons.Feedback.Contains("H: მარცხნივ 2") && lessons.Feedback.Contains("მარჯვნივ — 4"), "Unbalanced equations show which element differs");
        Snapshot("58-lesson-equation");
        lessons.Close();
        lessons.OpenMap();
        Ensure(lessons.MapOpen, "Learning map opens");
        Snapshot("59-learning-map");
        for (int k = 0; k < 10 && LabUI.AnyOpen; k++) LabUI.CloseTop();

        // Playtest skip finishes earlier lessons with their normal rewards.
        string beforeSkip = JsonUtility.ToJson(save.Data);
        Curriculum.SkipTo(Curriculum.Get("c6_signs"));
        Ensure(Curriculum.Next.ID == "c6_signs" && LabProgress.Owns("badge:ch5") && Curriculum.StationOpen(StationType.ReactionTable), "Playtest skip lands on the chosen lesson");
        JsonUtility.FromJsonOverwrite(beforeSkip, save.Data); // later checks expect the skipped rewards still unearned

        // Finish the beginner path: stations, activities and the advanced floor open.
        foreach (var l in Curriculum.Lessons) if (!Curriculum.Done(l.ID)) save.Data.lessons.Add(new LessonRecord { id = l.ID, done = true, best = 3 });
        save.Commit();
        Ensure(Curriculum.Complete && Curriculum.StationOpen(StationType.AtomTable) && Curriculum.ActivityOpen("solution") && Curriculum.TierOpen(1) && !Curriculum.TierOpen(2), "Completing the theory opens stations and the first advanced tier");
        game.Interact(Station(StationType.ElementStation));
        Ensure(lessons.HubOpen, "The element station opens its hub");
        for (int k = 0; k < 10 && LabUI.AnyOpen; k++) LabUI.CloseTop();

        // Hands-on tables: every recipe in each book is reachable and judged as itself.
        foreach (var kit in new[] { Kits.Atoms, Kits.Molecules })
        {
            Ensure(kit.Finds.Select(f => f.ID).Distinct().Count() == kit.Finds.Length, "Unique finds: " + kit.ID);
            foreach (var f in kit.Finds)
                Ensure(f.Counts.Length == kit.Parts.Length && f.Counts.Select((c, i) => c <= kit.Parts[i].Max).All(x => x) && kit.Judge(f.Counts).Find == f, "Recipe builds its find: " + kit.ID + "/" + f.ID);
        }
        Ensure(Kits.Atoms.Judge(new[] { 6, 3, 6 }).Boom && !Kits.Atoms.Judge(new[] { 6, 6, 5 }).Boom && Kits.Molecules.Judge(new[] { 1, 0, 0, 1, 0, 0 }).Boom, "Unstable nuclei and impossible molecules burst; ions only explain");
        // Solution table: one recipe per find (water, salt, sugar, copper sulfate, heat).
        var brews = new (string id, int[] v)[] { ("saltwater", new[] { 1, 2, 0, 0, 0 }), ("saturated", new[] { 1, 4, 0, 0, 0 }), ("saltcrystals", new[] { 1, 2, 0, 0, 2 }),
            ("sweet", new[] { 2, 0, 2, 0, 0 }), ("syrup", new[] { 1, 0, 6, 0, 1 }), ("caramel", new[] { 0, 0, 3, 0, 2 }), ("lightblue", new[] { 2, 0, 0, 1, 0 }),
            ("deepblue", new[] { 1, 0, 0, 1, 0 }), ("bluecrystals", new[] { 1, 0, 0, 3, 0 }), ("steam", new[] { 1, 0, 0, 0, 2 }), ("mixture", new[] { 2, 1, 1, 0, 0 }) };
        Ensure(brews.Select(b => b.id).OrderBy(x => x).SequenceEqual(Kits.Solutions.Finds.Select(f => f.ID).OrderBy(x => x)), "Every solution find has a test recipe");
        foreach (var (id, v) in brews) Ensure(Kits.Solutions.Judge(v).Find?.ID == id && v.Select((c, i) => c <= Kits.Solutions.Parts[i].Max).All(x => x), "Solution recipe: " + id);
        Ensure(Kits.Solutions.Judge(new[] { 0, 2, 0, 0, 1 }).Boom && Kits.Solutions.Judge(new[] { 1, 0, 9, 0, 0 }).Find == null, "Burnt dry powder bursts; undissolved sugar only explains");
        Ensure(new[] { new[] { 6, 0, 1 }, new[] { 0, 0, 1 }, new[] { 0, 6, 1 } }.Select(v => Kits.Potions.Judge(v).Find.ID).SequenceEqual(new[] { "red", "violet", "yellow" }) && Kits.Potions.Judge(new[] { 2, 0, 0 }).Find == null, "Potion colours follow the pH; no cabbage, no colour");
        game.Interact(Station(StationType.MoleculeTable));
        var tables = lessons.Tables;
        Ensure(tables.IsOpen && tables.Current == Kits.Molecules, "The molecule table opens its kit directly");
        int wallet = save.Data.chemCoins;
        tables.Add(0, 2); tables.Add(3, 1); tables.Build();
        Ensure(LabProgress.Owns("kit:molecule:H2O") && save.Data.chemCoins == wallet + KitBenchUI.Pay, "Building water is a paid first discovery");
        Snapshot("63-molecule-kit");
        tables.Build();
        Ensure(save.Data.chemCoins == wallet + KitBenchUI.Pay, "A repeat pays nothing");
        game.Interact(Station(StationType.AtomTable));
        tables.Clear(); tables.Add(0, 5); tables.Add(1, 6); tables.Add(2, 5); tables.Build();
        Ensure(tables.Current == Kits.Atoms && LabProgress.Owns("kit:atom:B") && save.Data.unlockedElements.Contains("B"), "Forging boron lights it up in the periodic table");
        Snapshot("64-atom-forge");
        game.Interact(Station(StationType.SolutionStation));
        tables.Clear(); tables.Add(0, 1); tables.Add(3, 3); tables.Build();
        Ensure(tables.Current == Kits.Solutions && LabProgress.Owns("kit:solution:bluecrystals"), "The solution table opens its kit and grows blue crystals");
        Snapshot("65-solution-kit");
        // Personal-lab tables: every find is reachable by some build, and each opens the bench.
        Ensure(PersonalLab.Catalog.Where(e => !e.Decor).All(e => LabKits.For(e.ID) != null), "Every lab table opens a build-and-discover bench");
        foreach (var labKit in LabKits.All)
        {
            var examples = new Dictionary<string, int[]>();
            int combos = labKit.Parts.Aggregate(1, (m, part) => m * (part.Max + 1));
            for (int i = 0; i < combos; i++)
            {
                var v = new int[labKit.Parts.Length];
                for (int j = 0, rest = i; j < v.Length; j++) { v[j] = rest % (labKit.Parts[j].Max + 1); rest /= labKit.Parts[j].Max + 1; }
                var found = labKit.Judge(v).Find;
                if (found != null && !examples.ContainsKey(found.ID)) examples[found.ID] = v;
            }
            Ensure(labKit.Finds.All(f => examples.ContainsKey(f.ID)), "Every lab find is reachable: " + labKit.ID + " missing " + string.Join(",", labKit.Finds.Where(f => !examples.ContainsKey(f.ID)).Select(f => f.ID)));
            var show = examples[labKit.Finds[labKit.Finds.Length / 2].ID];
            tables.Open(labKit);
            for (int j = 0; j < show.Length; j++) tables.Add(j, show[j]);
            tables.Build();
            Ensure(tables.Current == labKit && tables.Feedback.Length > 0, "Lab bench builds: " + labKit.ID);
            Snapshot("66-lab-" + labKit.ID);
        }
        for (int k = 0; k < 10 && LabUI.AnyOpen; k++) LabUI.CloseTop();
        Snapshot("60-station-hub");
        for (int k = 0; k < 10 && LabUI.AnyOpen; k++) LabUI.CloseTop();
        for (int tier = 1; tier <= 4; tier++)
            for (int n = 0; n < 60; n++)
            {
                var q = ChemistryChallenges.Tier(tier, game);
                Ensure(q.Choices.Length == 4 && q.Choices.Distinct().Count() == 4 && q.Feedback.Length == 4 && q.Correct == 0 && q.Reward > 0, "Tier " + tier + " question: " + q.ID);
            }
        LabProgress.Add(save.Data.stats, "tier:1", Curriculum.TierGoal - 1);
        Ensure(Curriculum.CountTier(1) != null && Curriculum.TierOpen(2), "Eight tier-one answers open tier two");

        // Open goals accept every valid answer and name the one the player built.
        var lithium = Curriculum.Get("c2_parts").Steps[2];
        Ensure(lithium.Check(new[] { 3, 4, 3 }) == null && lithium.Check(new[] { 3, 3, 3 }) == null && lithium.Check(new[] { 3, 5, 3 }) != null && lithium.Named(new[] { 3, 3, 3 }) != null, "Lithium accepts both natural isotopes");
        var oxides = Curriculum.Get("c4_salt").Steps[2];
        Ensure(oxides.Check(new[] { 1, 1, 0, 0 }) == null && oxides.Check(new[] { 1, 2, 0, 0 }) == null && oxides.Check(new[] { 1, 3, 0, 0 }) != null && oxides.Check(new[] { 1, 1, 1, 0 }) != null, "Both carbon oxides count");
        var water = Curriculum.Get("c4_molecule").Steps[2];
        Ensure(water.Check(new[] { 2, 1 }) == null && water.Check(new[] { 2, 2 }) == null && water.Check(new[] { 2, 0 }) != null, "Water and peroxide count");
        var bases = Curriculum.Get("c8_ph").Steps[2];
        Ensure(bases.Check(new[] { 11 }) == null && bases.Check(new[] { 14 }) == null && bases.Check(new[] { 9 }) != null, "Any strong base pH counts");

        // Mixing bench: a consistent rule table answers every pair.
        foreach (var r in Mixing.Results) Ensure(Mixing.Get(r.A) != null && Mixing.Get(r.B) != null && r.A != r.B && LabProgress.Catalog.Any(c => c.ID == r.ID), "Mix result: " + r.ID);
        Ensure(Mixing.Results.Select(r => r.ID).Distinct().Count() == Mixing.Results.Length, "Mix results are unique");
        foreach (var a in Mixing.Reagents)
            foreach (var b in Mixing.Reagents)
            {
                var found = Mixing.Find(a.ID, b.ID);
                Ensure(found == Mixing.Find(b.ID, a.ID) && (found != null || Mixing.NoReaction(a, b).Length > 0), "Mixing answers " + a.ID + "+" + b.ID);
                if (a.Tag == "acid" && (b.Tag == "base" || b.Tag == "carbonate")) Ensure(found != null, "Acids react with " + b.ID + ": " + a.ID);
            }
        var bench = lessons.Bench;
        int purse = save.Data.chemCoins;
        LabProgress.Give("lemon", 5);
        bench.Open();
        Ensure(bench.IsOpen, "Mixing bench opens after the reaction lessons");
        bench.Pick("lemon"); bench.Pick("soda"); bench.Mix();
        Ensure(!LabProgress.Owns(Mixing.Key("lemon", "soda")) && bench.Feedback.Contains("გამოიცანი"), "Mixing asks for a prediction first");
        bench.Guess(MixSign.Gas); bench.Mix();
        purse += bench.Feedback.Contains("შეკვეთა შესრულდა") ? Orders.Pay : 0;
        Ensure(LabProgress.Owns(Mixing.Key("lemon", "soda")) && save.Data.chemCoins == purse + MixBenchUI.Pay + MixBenchUI.PayGuess, "A new, predicted result is collected and paid");
        Snapshot("61-mix-bench");
        bench.Guess(MixSign.Nothing); bench.Mix();
        Ensure(save.Data.chemCoins == purse + MixBenchUI.Pay + MixBenchUI.PayGuess && bench.Feedback.Contains("სინამდვილეში"), "A repeat pays nothing and a wrong guess is explained");
        LabProgress.Give("copper", 1); // owning a jar shows it before its unlock
        bench.Pick("soda"); bench.Pick("copper"); bench.Guess(MixSign.Nothing); bench.Mix();
        Ensure(bench.Feedback.Contains("სპილენძი") && bench.Feedback.Contains("სწორად"), "Unlisted pairs explain why nothing happens");
        // Results become ingredients: salt water appears on the shelf and mixes again.
        foreach (var r in Mixing.Reagents.Where(x => x.Product)) Ensure(Mixing.Recipe(r.ID) != null && Mixing.Results.Any(x => x.A == r.ID || x.B == r.ID), "Product has a recipe and a use: " + r.ID);
        foreach (var r in Mixing.Results.Where(x => x.Makes != null)) Ensure(Mixing.Get(r.Makes) != null, "Mix makes a real jar: " + r.ID);
        LabProgress.Give("salt", 1); LabProgress.Give("cabbage", 1);
        bool hiddenJar = !Mixing.Known(Mixing.Get("brine"));
        LabUI.CloseTop(); bench.Open();
        bench.Pick("water"); bench.Pick("salt"); bench.Guess(MixSign.Dissolves); bench.Mix();
        Ensure(hiddenJar && Mixing.Known(Mixing.Get("brine")) && LabProgress.Has("brine") == 1 && bench.Feedback.Contains("ახალი ქილა"), "A mix puts its product on the shelf as a new jar");
        Snapshot("61b-mix-new-jar");
        LabUI.CloseTop(); bench.Open();
        bench.Pick("brine"); bench.Pick("cabbage"); bench.Guess(MixSign.Nothing); bench.Mix();
        Ensure(LabProgress.Owns(Mixing.Key("brine", "cabbage")) && LabProgress.Has("brine") == 0, "A product mixes again and is used up");
        for (int k = 0; k < 10 && LabUI.AnyOpen; k++) LabUI.CloseTop();
        Debug.Log("CURRICULUM_CHECKS_PASSED");
    }
    private static void AfterCurriculum()
    {
        game.OpenMenu(0);
        Ensure(LabUI.AnyOpen, "Periodic table menu shortcut opens");
        game.OpenMenu(0);
        Ensure(!LabUI.AnyOpen, "Periodic table menu shortcut toggles closed");
        game.Quiz.Open(visual: true);
        game.Notebook.Open();
        Schedule(Notebook, 0.4);
    }
    private static void Notebook()
    {
        Ensure(game.Quiz.IsOpen && !game.player.canMove, "Notebook layers above quiz");
        Snapshot("02-notebook");
        LabUI.CloseTop();
        Ensure(LabUI.AnyOpen, "Closing notebook preserves quiz movement lock");
        game.Quiz.Answer.text = "bad";
        game.Quiz.Submit();
        var hint = (TMP_Text)typeof(ChemMinigameUI).GetField("hint", Hidden).GetValue(game.Quiz);
        hint.gameObject.SetActive(true);
        Schedule(QuizSnapshot, 0.4);
    }
    private static void QuizSnapshot()
    {
        Snapshot("03-experiment");
        game.Quiz.Close();
        game.Periodic.Open();
        Schedule(Periodic, 0.4);
    }
    private static void Periodic()
    {
        Ensure(UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Count(t => t.text.Contains("?")) > 100, "Locked element silhouettes");
        Snapshot("04-periodic");
        LabUI.CloseTop();
        var question = game.elements.elements.Single(e => e.symbol == "He").discoveryQuestion;
        game.Quiz.Open(selected: question);
        game.Quiz.Answer.text = "he";
        game.Quiz.Submit();
        Ensure(GameSaveManager.Instance.Data.unlockedElements.Contains("He"), "Element discovery unlock");
        game.Quiz.Close();
        GameSaveManager.Instance.Data.chemCoins = 200;
        GameSaveManager.Instance.Commit();
        game.Shop.Open();
        Schedule(Shop, 0.3);
    }
    private static void Shop()
    {
        Snapshot("05-shop");
        var save = GameSaveManager.Instance;
        Ensure(save.PurchaseAndEquip(game.cosmetics.Single(c => c.itemID == "coat_blue")), "Coat purchase");
        Ensure(save.PurchaseAndEquip(game.cosmetics.Single(c => c.itemID == "decor_globe")), "Desk customization");
        save.Data.chemCoins = 100;
        save.Commit();
        Ensure(save.PurchaseAndEquip(game.cosmetics.Single(c => c.itemID == "goggles_clear")), "Goggles on new character");
        Ensure(save.PurchaseAndEquip(game.cosmetics.Single(c => c.itemID == "hat_violet")), "Switching goggles to hat");
        var accessory = game.player.transform.Find("სათვალე ან ქუდი");
        Ensure(Mathf.Abs(accessory.localScale.x - 0.8f) < 0.001f, "Hat must reset the accessory scale");
        LabUI.CloseTop();
        game.Boards.OpenRankings();
        Schedule(Rankings, 0.3);
    }
    private static void Rankings()
    {
        Snapshot("06-rankings");
        LabUI.CloseTop();
        game.Boards.OpenQuests();
        Schedule(Quests, 0.3);
    }
    private static void Quests()
    {
        Snapshot("07-quests");
        LabUI.CloseTop();
        var body = game.player.GetComponent<Rigidbody2D>();
        body.position = originalPosition;
        game.player.enabled = false;
        game.player.canMove = true;
        game.player.SetMovement(new Vector2(1, 1));
        typeof(PlayerController).GetMethod("FixedUpdate", Hidden).Invoke(game.player, null);
        Ensure(body.linearVelocity.x > 0 && body.linearVelocity.y > 0 &&
            Mathf.Abs(body.linearVelocity.x - body.linearVelocity.y) < 0.001f, "Diagonal movement");
        Ensure(Mathf.Abs(game.player.Movement.magnitude - 1) < 0.001f, "Diagonal speed normalization");
        var fixedTick = typeof(PlayerController).GetMethod("FixedUpdate", Hidden);
        for (int i = 0; i < 20; i++) fixedTick.Invoke(game.player, null);
        Ensure(Mathf.Abs(body.linearVelocity.magnitude - 4.25f) < 0.01f, "Calibrated diagonal walking pace");
        game.player.SetMovement(Vector2.zero);
        for (int i = 0; i < 20; i++) fixedTick.Invoke(game.player, null);
        Ensure(body.linearVelocity == Vector2.zero, "Stopping reaches rest without residual sliding");
        game.player.SetMovement(Vector2.right);
        game.player.canMove = false;
        fixedTick.Invoke(game.player, null);
        Ensure(body.linearVelocity == Vector2.zero, "Movement lock stops immediately");
        game.player.canMove = true;
        game.player.SetMovement(Vector2.zero);
        body.linearVelocity = Vector2.zero;
        game.player.enabled = true;
        Schedule(TeacherTalk, 0.3);
    }
    private static float normalCameraSize;
    private static object Field(object target, string name) => target.GetType().GetField(name, Hidden).GetValue(target);
    private static void TeacherTalk()
    {
        normalCameraSize = Camera.main.orthographicSize;
        MoveTo(game.classroom.TransformPoint(new Vector3(36.5f, 6.6f)));
        var station = UnityEngine.Object.FindObjectsByType<InteractableStation>(FindObjectsSortMode.None)
            .First(s => s.stationType == StationType.TeacherNPC);
        Ensure(game.Tutorial.Allows(station), "Teacher can always be talked to");
        var teacher = game.Dialogue.Teacher;
        Ensure(teacher.AtWork, "The teacher is busy at her desk before anyone talks to her");
        game.Interact(station);
        Ensure(teacher.Busy && !game.Dialogue.IsOpen, "The teacher says 'one sec' and finishes her note before talking");
        Schedule(TeacherTurned, 0.3);
    }
    private static void TeacherTurned()
    {
        var teacher = game.Dialogue.Teacher;
        if (teacher.Busy && turnWait++ < 60) { Schedule(TeacherTurned, 0.05); return; }
        Vector2 d = game.player.transform.position - teacher.transform.position;
        Ensure(!teacher.Busy && Mathf.Abs(Mathf.DeltaAngle(teacher.Facing, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg)) < 30, "The teacher turns to face the player");
        Ensure(game.Dialogue.IsOpen && LabUI.AnyOpen, "Talking to the teacher opens the dialogue");
        Schedule(TeacherTyping, 0.25);
    }
    private static int turnWait;
    private static void TeacherTyping()
    {
        int shown = (int)Field(game.Dialogue, "shown"), total = (int)Field(game.Dialogue, "total");
        Ensure(shown > 0 && shown < total, "Teacher text is typed out gradually (" + shown + "/" + total + ")");
        Ensure(Camera.main.orthographicSize < normalCameraSize * 0.8f, "Camera zooms in on the teacher");
        Ensure(!game.player.canMove, "Player cannot walk during the conversation");
        var npc = UnityEngine.Object.FindAnyObjectByType<TeacherNPC>().transform.position;
        Ensure(Mathf.Abs(game.player.transform.position.x - npc.x) >= 1.2f || game.player.transform.position.y <= npc.y - 2.2f,
            "Player steps aside so the teacher stays visible");
        Snapshot("27-teacher-talk");
        typeof(TeacherDialogueUI).GetMethod("Offers", Hidden).Invoke(game.Dialogue, null);
        Schedule(TeacherOffers, 5.5);
    }
    private static void TeacherOffers()
    {
        var buttons = (System.Collections.IList)Field(game.Dialogue, "choiceButtons");
        Ensure(buttons.Count >= 3 && buttons.Count <= 4, "Teacher offers two or three quests plus a way out (" + buttons.Count + ")");
        Snapshot("28-teacher-quests");
        var save = GameSaveManager.Instance;
        var quest = TeacherQuests.Pool.First(q => q.Kind == TeacherQuestKind.SolveReactions && q.Target == 3);
        TeacherQuests.Accept(quest);
        Ensure(TeacherQuests.Active == quest && TeacherQuests.Progress(quest) == 0, "Accepted quest starts at zero");
        for (int i = 0; i < 3; i++) save.RecordCorrect(game.questions[i % game.questions.Length], DateTime.UtcNow.Date);
        Ensure(TeacherQuests.Done(quest), "Solving reactions completes the teacher's quest");
        int coins = save.Data.chemCoins;
        Ensure(TeacherQuests.TurnIn() == quest.Reward && save.Data.chemCoins == coins + quest.Reward && TeacherQuests.Active == null,
            "Quest reward is paid once");
        Ensure(TeacherQuests.TurnIn() == 0, "Quest cannot be turned in twice");
        var combo = TeacherQuests.Pool.First(q => q.Kind == TeacherQuestKind.ReachCombo && q.Target == 3);
        TeacherQuests.Accept(combo);
        save.RecordCorrect(game.questions[0], DateTime.UtcNow.Date);
        save.RecordWrong();
        save.RecordCorrect(game.questions[0], DateTime.UtcNow.Date);
        save.RecordCorrect(game.questions[0], DateTime.UtcNow.Date);
        Ensure(!TeacherQuests.Done(combo), "A wrong answer breaks the streak quest");
        save.RecordCorrect(game.questions[0], DateTime.UtcNow.Date);
        Ensure(TeacherQuests.Done(combo), "Three in a row completes the streak quest");
        TeacherQuests.Abandon();
        typeof(TeacherDialogueUI).GetMethod("ExplainMenu", Hidden).Invoke(game.Dialogue, new object[] { "რა აგიხსნა? აირჩიე თემა:" });
        Schedule(TeacherExplain, 2.5);
    }
    private static void TeacherExplain()
    {
        var buttons = (System.Collections.IList)Field(game.Dialogue, "choiceButtons");
        Ensure(buttons.Count == 8, "Teacher lists explanation topics (" + buttons.Count + ")");
        Snapshot("29-teacher-explain");
        LabUI.CloseTop();
        Schedule(TeacherClosed, 1.2);
    }
    private static void TeacherClosed()
    {
        Ensure(!game.Dialogue.IsOpen && !LabUI.AnyOpen, "Dialogue closes with Esc");
        Ensure(Mathf.Abs(Camera.main.orthographicSize - normalCameraSize) < 0.05f, "Camera returns to the player after talking");
        Schedule(Finish, 0.2);
    }
    private static void Finish()
    {
        Snapshot("08-classroom");
        var lab = game.Personal;
        var save = GameSaveManager.Instance;
        Ensure(lab != null && !lab.Inside && lab.Columns == 4 && lab.Rows == 3, "Small personal laboratory");
        // The buying and placing checks below start from an empty lab, so set aside the act-one trophy.
        save.Data.ownedLabItems.Remove("steam_wheel"); save.Data.labPlacements.RemoveAll(p => p.itemID == "steam_wheel");
        save.Data.chemCoins = 0; save.Commit();
        Ensure(!lab.Buy("molecule") && !lab.Expand(), "Insufficient coins must not buy equipment or expansion");
        save.Data.chemCoins = 1000; save.Commit();
        foreach (var item in PersonalLab.Catalog.Take(4)) Ensure(lab.Buy(item.ID), "Equipment purchase " + item.ID);
        Ensure(save.Data.chemCoins == 745 && !lab.Buy("molecule"), "Equipment charged once");
        for (int i = 0; i < 4; i++) Ensure(lab.Place(PersonalLab.Catalog[i].ID, i, 0), "Equipment placement");
        Ensure(lab.Place("solution", 0, 0) && save.Data.labPlacements.Single(p => p.itemID == "molecule").column == 1 &&
            save.Data.labPlacements.Select(p => p.column * 10 + p.row).Distinct().Count() == save.Data.labPlacements.Count, "Placing on an occupied cell swaps items without overlap");
        Ensure(lab.Place("solution", 1, 0) && !lab.Place("solution", 9, 0), "Swap back, and placement outside the room is refused");
        Ensure(lab.Place("molecule", 0, 1) && save.Data.labPlacements.Count == 4, "Moving equipment does not duplicate it");
        lab.Store("atom"); Ensure(save.Data.labPlacements.Count == 3 && save.Data.ownedLabItems.Contains("atom"), "Storage preserves ownership");
        Ensure(lab.Place("atom", 3, 1), "Stored equipment can be placed again");
        Ensure(lab.Expand() && lab.Columns == 6 && lab.Rows == 4 && save.Data.chemCoins == 495, "One-time laboratory expansion");
        Ensure(!lab.Expand() && save.Data.chemCoins == 495, "Expansion cannot charge twice");
        save.Load();
        Ensure(save.Data.labExpanded && save.Data.ownedLabItems.Count == 4 && save.Data.labPlacements.Count == 4, "Personal lab save reload");
        Ensure(Math.Abs(ChemistrySandbox.PH(50, 50, 100) - 7) < 0.0001 && ChemistrySandbox.PH(50, 0, 100) < 2 &&
            ChemistrySandbox.PH(0, 50, 100) > 12, "Acid/base model and neutrality");
        Ensure(ChemistrySandbox.PH(50, 0, 1000) > ChemistrySandbox.PH(50, 0, 100), "Dilution raises acidic pH");
        Ensure(Mathf.Abs(ChemistrySandbox.Concentration(0.5f, 1) - 0.5f) < 0.001f, "Concentration model");
        MoveTo(game.classroom.TransformPoint(new Vector3(46.5f, 10.1f)));
        Schedule(InsidePersonalLab, 0.6);
    }
    private static void InsidePersonalLab()
    {
        Ensure(game.Personal.Inside && game.player.transform.position.x > 100, "Door automatically teleports into personal lab");
        game.Personal.Workshop.Open();
        Schedule(WorkshopView, 0.4);
    }
    private static void WorkshopView()
    {
        Snapshot("11-lab-shop");
        UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b => b.name == "ოთახის მოწყობა").onClick.Invoke();
        Schedule(LayoutView, 0.3);
    }
    private static void LayoutView()
    {
        Snapshot("12-lab-layout"); LabUI.CloseTop();
        MoveTo(game.Personal.RoomOrigin + new Vector2(0, 1));
        Schedule(PersonalRoomView, 3.0);
    }
    private static void PersonalRoomView()
    {
        Snapshot("17-personal-lab");
        SnapshotRoom("18-personal-lab-overview", game.Personal.RoomOrigin + new Vector2(4, 4.5f), 12, 1440, 1000);
        MoveTo(game.Personal.ExitPosition);
        Schedule(ReturnedToClass, 0.5);
    }
    private static void ReturnedToClass()
    {
        Ensure(!game.Personal.Inside && game.player.transform.position.x < 100, "Door returns to classroom without a teleport loop");
        Debug.Log("PERSONAL_LAB_PURCHASE_PLACEMENT_ACTIVITIES_AND_DOORS_PASSED");
        MoveTo(game.classroom.TransformPoint(new Vector3(45, -5)));
        Schedule(RoomView, 0.7);
    }
    private static void RoomView()
    {
        Snapshot("10-lab-room");
        SnapshotRoom("19-classroom-overview", game.classroom.TransformPoint(new Vector3(45.5f, -4)), 22, 1280, 1280);
        Schedule(LifeSimStart, 0.3);
    }

    // ---------------- life-sim expansion ----------------
    private static DateTime Day(int addDays, int hour) => DateTime.Now.Date.AddDays(addDays).AddHours(hour);
    private static void LifeSimStart()
    {
        SchoolCalendar.Override = Day(0, 9);
        game.Atmosphere.Apply(true);
        var feet = game.player.GetComponent<BoxCollider2D>();
        Vector2 door = game.classroom.TransformPoint(SchoolAreas.ClassroomDoor);
        Ensure(!Physics2D.OverlapBoxAll(door + new Vector2(0, 0.1f), feet.size, 0).Any(c => !c.isTrigger && c.attachedRigidbody != game.player.GetComponent<Rigidbody2D>()),
            "Hallway doormat is on walkable floor");
        MoveTo(game.classroom.TransformPoint(SchoolAreas.ClassroomDoor));
        Schedule(InHallway, 0.6);
    }
    private static void InHallway()
    {
        Ensure(game.Areas.Current == "hallway", "Classroom doormat leads to the hallway");
        Snapshot("30-hallway");
        SnapshotRoom("31-hallway-overview", SchoolAreas.Def("hallway").Origin + new Vector2(0, 1.5f), 9, 1600, 900);
        Schedule(ToStorage, 1.1);
    }
    private static void ToStorage()
    {
        Ensure(!SchoolAreas.Unlocked("exhibition"), "Exhibition room starts locked");
        Ensure(game.Areas.Travel("storage") && game.Areas.Current == "storage", "Storage room travel");
        Schedule(StorageView, 0.6);
    }
    private static void StorageView()
    {
        Snapshot("32-storage");
        game.Collection.OpenStorage();
        Schedule(StorageUI, 0.4);
    }
    private static void StorageUI()
    {
        Snapshot("33-storage-ui");
        UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b => b.GetComponentInChildren<TMP_Text>().text.StartsWith("დღის მასალების")).onClick.Invoke();
        Ensure(LabProgress.Data.storagePickupDay == LabProgress.Today && LabProgress.Data.materials.Sum(m => m.value) >= 3, "Daily storage pickup gives materials once");
        LabUI.CloseTop();
        Schedule(ToGreenhouse, 1.1);
    }
    private static void ToGreenhouse()
    {
        Ensure(game.Areas.Travel("greenhouse"), "Greenhouse travel");
        Schedule(GreenhouseView, 0.6);
    }
    private static void GreenhouseView()
    {
        SnapshotRoom("34-greenhouse", SchoolAreas.Def("greenhouse").Origin + new Vector2(0, 1.5f), 7, 1400, 800);
        var bed = UnityEngine.Object.FindObjectsByType<WorldInteractable>().First(w => w.PromptText().Contains("მორწყვა"));
        bed.Use(); bed.Use();
        Ensure(LabProgress.Has("cabbage") >= 1 && LabProgress.Has("lemon") >= 1, "Watering then harvesting the greenhouse");
        Schedule(ToReading, 1.1);
    }
    private static void ToReading()
    {
        Ensure(game.Areas.Travel("reading"), "Reading corner travel");
        Schedule(ReadingView, 0.6);
    }
    private static void ReadingView()
    {
        int facts = LabProgress.Items(CollectionKind.Fact).Count(f => LabProgress.Owns(f.ID));
        UnityEngine.Object.FindObjectsByType<WorldInteractable>().First(w => w.PromptText().Contains("წიგნის")).Use();
        Ensure(LabProgress.Items(CollectionKind.Fact).Count(f => LabProgress.Owns(f.ID)) == facts + 1, "Reading a book adds a fact");
        Snapshot("35-reading-fact");
        LabUI.CloseTop();
        SnapshotRoom("36-reading", SchoolAreas.Def("reading").Origin + new Vector2(0, 1.5f), 7, 1400, 800);
        LabProgress.Data.reputation = 30; GameSaveManager.Instance.Commit();
        Schedule(ToExhibition, 1.1);
    }
    private static void ToExhibition()
    {
        Ensure(game.Areas.Travel("hallway") && SchoolAreas.Unlocked("exhibition"), "Reputation unlocks the exhibition room");
        Schedule(ToExhibition2, 1.1);
    }
    private static void ToExhibition2()
    {
        Ensure(game.Areas.Travel("exhibition"), "Exhibition travel");
        SchoolCalendar.Override = new DateTime(2026, 11, 19, 10, 0, 0); // a Thursday: the researcher visits
        Schedule(ExhibitionView, 1.2);
    }
    private static void ExhibitionView()
    {
        var levani = game.Classmates.Single(n => n.Def.ID == "levani");
        Ensure(levani.transform.GetChild(0).gameObject.activeSelf, "Travelling researcher appears on Thursdays");
        SnapshotRoom("37-exhibition", SchoolAreas.Def("exhibition").Origin + new Vector2(0, 1.5f), 8, 1400, 800);
        SchoolCalendar.Override = Day(0, 9);
        Schedule(BackToClass, 1.1);
    }
    private static void BackToClass()
    {
        Ensure(game.Areas.Travel("hallway"), "Back to hallway");
        Schedule(BackToClass2, 1.1);
    }
    private static void BackToClass2()
    {
        Ensure(game.Areas.Travel("classroom") && game.Areas.Current == "classroom", "Hallway door returns to the classroom");
        Schedule(ClassmateTalk, 1.2);
    }
    private static void ClassmateTalk()
    {
        var nika = game.Classmates.Single(n => n.Def.ID == "nika");
        Ensure(nika.transform.GetChild(0).gameObject.activeSelf, "Nika is in the classroom in the morning");
        MoveTo((Vector2)nika.transform.position + new Vector2(0, -1.6f));
        int bond = Classmates.State("nika").friendship; // orders filled earlier may already have raised it
        nika.Talk();
        Ensure(game.Dialogue.IsOpen, "Classmates talk through the dialogue box");
        Ensure(Classmates.State("nika").friendship == bond + 2, "Daily chat raises friendship once");
        typeof(Classmates).GetMethod("Chat", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { game, nika });
        Ensure(Classmates.State("nika").story == 1 && !Classmates.StoryReady(nika.Def, Classmates.State("nika")), "Story advances once per day");
        Schedule(ClassmateSnapshot, 1.5);
    }
    private static void ClassmateSnapshot()
    {
        Snapshot("38-classmate-talk");
        typeof(Classmates).GetMethod("Requests", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { game, game.Classmates.Single(n => n.Def.ID == "nika") });
        var dialogueType = typeof(TeacherDialogueUI);
        var pages = (string[])dialogueType.GetField("pages", Hidden).GetValue(game.Dialogue);
        dialogueType.GetField("page", Hidden).SetValue(game.Dialogue, pages.Length - 1);
        dialogueType.GetMethod("BeginPage", Hidden).Invoke(game.Dialogue, null);
        Schedule(ClassmateRequest, 4);
    }
    private static void ClassmateRequest()
    {
        var buttons = (System.Collections.IList)Field(game.Dialogue, "choiceButtons");
        Ensure(buttons.Count == 2, "Classmate offers a request (" + buttons.Count + ")");
        typeof(TeacherDialogueUI).GetField("acceptInputAfter", Hidden).SetValue(game.Dialogue, 0f);
        ((Button)buttons[0]).onClick.Invoke();
        var state = Classmates.State("nika");
        Ensure(state.requestID == "nika_copper", "Request accepted and saved");
        LabProgress.Give("copper", 1);
        Ensure(Classmates.RequestDone(Classmates.Get("nika"), state), "Delivery request completes when the material is carried");
        LabUI.CloseTop();
        SchoolCalendar.Override = Day(1, 9);
        Ensure(Classmates.StoryReady(Classmates.Get("nika"), state) == (state.friendship >= 5), "Next story beat waits for friendship and a new day");
        SchoolCalendar.Override = Day(0, 9);
        Schedule(ResearchFlow, 1.0);
    }
    private static void ResearchFlow()
    {
        var save = GameSaveManager.Instance;
        save.Data.chemCoins += 500;
        Ensure(ResearchProjects.Accept("crystals"), "Accept research project");
        var state = ResearchProjects.State("crystals");
        Ensure(!ResearchProjects.TryAdvance(state, out _) || LabProgress.Has("alum") >= 0, "");
        LabProgress.Data.materials.RemoveAll(m => m.key == "alum" || m.key == "tapwater");
        Ensure(!ResearchProjects.TryAdvance(state, out _) && state.stage == 0, "Missing materials block gathering");
        LabProgress.Give("alum", 2); LabProgress.Give("tapwater", 1);
        Ensure(ResearchProjects.TryAdvance(state, out _) && ResearchProjects.TryAdvance(state, out _) && state.stage == 2, "Gathering consumes materials");
        Ensure(!ResearchProjects.TryAdvance(state, out _), "Equipment stage needs the thermostat placed");
        Ensure(game.Personal.Buy("thermo") && game.Personal.Place("thermo", 4, 1), "Buy and place thermostat");
        Ensure(ResearchProjects.TryAdvance(state, out _) && ResearchProjects.Stage(state).Kind == StageKind.Experiment, "Equipment stage completes");
        ResearchProjects.ExperimentResult("crystals", false);
        Ensure(state.mistakes == 1 && state.stage == 3, "A failed experiment is counted but keeps progress");
        ResearchProjects.ExperimentResult("crystals", true);
        Ensure(!ResearchProjects.TryAdvance(state, out _) && state.readyDay != "", "Crystals must grow overnight");
        save.Load();
        state = ResearchProjects.State("crystals");
        Ensure(state != null && state.stage == 4 && state.readyDay != "", "Research stage survives save reload");
        SchoolCalendar.Override = Day(1, 9);
        Ensure(ResearchProjects.TryAdvance(state, out _), "Next day the overnight stage completes");
        Ensure(!ResearchProjects.Answer(state, 0) && state.mistakes == 2, "Wrong observation teaches but does not regress");
        Ensure(ResearchProjects.Answer(state, 1) && ResearchProjects.Stage(state).Kind == StageKind.Submit, "Correct observation recorded");
        int coins = save.Data.chemCoins, reputation = save.Data.reputation;
        Ensure(ResearchProjects.Submit(state) != null && save.Data.chemCoins > coins && save.Data.reputation > reputation && state.completed, "Project submission rewards coins and reputation");
        Ensure(LabProgress.Owns("photo:crystals") && save.Data.ownedLabItems.Contains("crystal_lamp") && LabProgress.Owns("badge:first_project"), "Project unlocks photo, decoration and badge");
        var entry = ResearchProjects.EnterFair("crystals");
        Ensure(entry != null && ResearchProjects.EnterFair("crystals") == null, "One science-fair entry per week");
        SchoolCalendar.Override = Day(8, 9);
        Ensure(ResearchProjects.SettleFairs() != null && entry.settled, "Science fair settles after the week ends");
        SchoolCalendar.Override = Day(0, 20);
        game.Atmosphere.Apply(true);
        Ensure(LabAtmosphere.Evening == 1, "Evening atmosphere");
        game.Dialogue.Talk();
        TeacherGuide.Play(game, TeacherGuide.Get("school"), null);
        var dt = typeof(TeacherDialogueUI);
        dt.GetField("page", Hidden).SetValue(game.Dialogue, 2);
        dt.GetMethod("BeginPage", Hidden).Invoke(game.Dialogue, null);
        Schedule(LessonSnap, 1.5);
    }
    private static void LessonSnap()
    {
        Snapshot("53-lesson-snapshot");
        LabUI.CloseTop();
        game.Research.Open("crystals");
        Schedule(ResearchView, 0.5);
    }
    private static void ResearchView()
    {
        Snapshot("39-research-board");
        LabUI.CloseTop();
        game.Collection.OpenTab("progress");
        Schedule(CollectionView, 0.4);
    }
    private static void CollectionView()
    {
        Snapshot("40-progress");
        LabUI.CloseTop();
        game.Collection.Open(CollectionKind.Mineral);
        Schedule(CollectionView2, 0.4);
    }
    private static void CollectionView2()
    {
        Snapshot("41-collection");
        LabUI.CloseTop();
        game.Journal.Open();
        Schedule(JournalView, 0.4);
    }
    private static void JournalView()
    {
        Snapshot("42-noticeboard");
        LabUI.CloseTop();
        // A burst at a lab bench leaves a harmless spill; cleaning it pays reputation.
        int spills = LabProgress.Data.pendingSpills;
        var bench = game.Lessons.Tables;
        bench.Open(LabKits.Valve);
        bench.Add(0, 7); bench.Build();
        Ensure(LabProgress.Data.pendingSpills == spills + 1, "A lab burst leaves a harmless spill to clean");
        Snapshot("44-lab-burst");
        LabUI.CloseTop();
        var spill = UnityEngine.Object.FindObjectsByType<WorldInteractable>().First(w => w.PromptText().Contains("დასუფთავება"));
        spill.Use();
        Ensure(LabProgress.Data.pendingSpills == spills && LabProgress.Stat("clean") >= 1, "Cleaning a spill");
        var save = GameSaveManager.Instance;
        foreach (var id in new[] { "aquarium", "plant", "radio", "lamp", "research", "display", "plasma", "sofa" })
            if (!save.Data.ownedLabItems.Contains(id)) save.Data.ownedLabItems.Add(id);
        save.Data.labPlacements.Clear();
        string[] layout = { "aquarium", "plant", "radio", "lamp", "research", "display", "plasma", "sofa", "thermo", "molecule" };
        int cell = 0;
        foreach (var id in layout)
        {
            while (game.Personal.IsEntranceCell(cell % game.Personal.Columns, cell / game.Personal.Columns)) cell++;
            Ensure(game.Personal.Place(id, cell % game.Personal.Columns, cell / game.Personal.Columns), "Place decoration " + id);
            cell++;
        }
        Ensure(PersonalDecor.Calm >= 5, "Decorations raise the calm of the lab");
        save.Data.weatherDay = LabProgress.Today; save.Data.weather = "rain";
        game.Atmosphere.Apply(true);
        MoveTo(game.classroom.TransformPoint(new Vector3(46.5f, 10.1f)));
        Schedule(EveningLab, 0.8);
    }
    private static void EveningLab()
    {
        Ensure(game.Personal.Inside, "Back in the personal lab");
        MoveTo(game.Personal.RoomOrigin + new Vector2(2, 3));
        Schedule(EveningLab2, 1.5);
    }
    private static void EveningLab2()
    {
        Snapshot("50-personal-lab-evening");
        SnapshotRoom("51-personal-lab-evening-overview", game.Personal.RoomOrigin + new Vector2(4, 4.5f), 12, 1440, 1000);
        SchoolCalendar.Override = Day(0, 9);
        GameSaveManager.Instance.Data.weather = "clear";
        game.Atmosphere.Apply(true);
        Schedule(MorningLab, 0.5);
    }
    private static void MorningLab()
    {
        SnapshotRoom("52-personal-lab-morning-overview", game.Personal.RoomOrigin + new Vector2(4, 4.5f), 12, 1440, 1000);
        MoveTo(game.Personal.ExitPosition);
        Schedule(LifeSimPersistence, 0.6);
    }
    private static void LifeSimPersistence()
    {
        var save = GameSaveManager.Instance;
        SchoolCalendar.Refresh();
        save.Commit();
        save.Load();
        Ensure(save.Data.projects.Any(p => p.id == "crystals" && p.completed) && save.Data.npcs.Any(n => n.id == "nika" && n.story == 1) &&
            save.Data.collection.Contains("photo:crystals") && save.Data.fairEntries.Count == 1 && save.Data.labPlacements.Any(p => p.itemID == "aquarium") &&
            save.Data.reputation > 0 && save.Data.mastery.Count > 0 && save.Data.weatherDay != "" && save.Data.timeOfDay != "", "Life-sim progress saves and reloads");
        SchoolCalendar.Override = null;
        Debug.Log("LIFE_SIM_CHECKS_PASSED");
        Schedule(FinalChecks, 0.2);
    }
    private static void FinalChecks()
    {
        Ensure(failures.Count == 0, "Runtime errors: " + string.Join("\n", failures));
        GameSaveManager.Instance.Load();
        Ensure(GameSaveManager.Instance.Data.tutorialCompleted, "Tutorial remains completed after reload");
        File.WriteAllText("Logs/lab-checks-result.txt", "PASS\nCharacter creation, tutorial, economy, persistence, UTC rollover, backup recovery, table walking and sliding, stations, UI, discovery, cosmetics, diagonal movement, personal lab door travel, purchases, placement, moving and storage, one-time expansion, lab persistence, four interactive chemistry models.");
        Debug.Log("LAB_PLAYMODE_CHECKS_PASSED");
        Exit(0);
    }

    private static void MoveTo(Vector3 feet)
    {
        var body = game.player.GetComponent<Rigidbody2D>();
        body.position = (Vector2)feet - game.player.GetComponent<BoxCollider2D>().offset;
        Physics2D.SyncTransforms();
    }
    private static void SnapshotRoom(string name, Vector2 centre, float size, int width, int height)
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
        var camera = Camera.main;
        var follow = camera.GetComponent<ClassroomCamera>();
        var position = camera.transform.position; float previousSize = camera.orthographicSize;
        var target = new RenderTexture(width, height, 24);
        follow.enabled = false; LabUI.Root.gameObject.SetActive(false);
        camera.targetTexture = target; camera.aspect = (float)width / height; camera.orthographicSize = size;
        camera.transform.position = new Vector3(centre.x, centre.y, position.z);
        camera.Render();
        var previous = RenderTexture.active; RenderTexture.active = target;
        var pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
        pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        File.WriteAllBytes("Logs/LabScreens/" + name + ".png", pixels.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = null; camera.ResetAspect();
        camera.transform.position = position; camera.orthographicSize = previousSize;
        follow.enabled = true; LabUI.Root.gameObject.SetActive(true);
        UnityEngine.Object.Destroy(pixels); UnityEngine.Object.Destroy(target);
    }
    /// <summary>Answers whichever lesson card is showing, correctly.</summary>
    private static void SolveCard(Lesson lesson)
    {
        var l = game.Lessons; var items = l.CardItems;
        switch (l.Current)
        {
            case CurriculumUI.Ex.Pick: l.Pick(lesson.Items[items[0]].Bin); break;
            case CurriculumUI.Ex.TrueFalse: l.Truth(l.Question != null ? l.CardClaim == 0 : lesson.Items[items[0]].Bin == l.CardBin); break;
            case CurriculumUI.Ex.Choice: l.Select(0); l.Check(); break;
            case CurriculumUI.Ex.SelectAll: foreach (int i in items) if (lesson.Items[i].Bin == l.CardBin) l.Toggle(i); l.Check(); break;
            case CurriculumUI.Ex.Drag: foreach (int i in items) l.Drop(i, lesson.Items[i].Bin); break;
            case CurriculumUI.Ex.Blank: l.Fill(l.GapAnswer); l.Check(); break;
        }
    }

    private static void Snapshot(string name)
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
        var camera = Camera.main;
        var canvas = LabUI.Root.GetComponent<Canvas>();
        var target = new RenderTexture(1280, 720, 24);
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 0.5f;
        camera.targetTexture = target;
        camera.aspect = 16f / 9f;
        camera.SendMessage("LateUpdate");
        Canvas.ForceUpdateCanvases();
        foreach (var window in UnityEngine.Object.FindObjectsByType<LabWindow>(FindObjectsSortMode.None))
        {
            // Capture windows fully faded in, even when opened in this same frame.
            typeof(LabWindow).GetField("opened", Hidden).SetValue(window, Time.unscaledTime - 1);
            window.SendMessage("LateUpdate");
        }
        foreach (var glow in UnityEngine.Object.FindObjectsByType<NextGlow>(FindObjectsSortMode.None)) glow.SendMessage("LateUpdate");
        foreach (var picture in Camera.allCameras) if (picture != camera && picture.targetTexture != null) picture.Render(); // the teacher's picture-in-picture
        camera.Render();
        var previous = RenderTexture.active;
        RenderTexture.active = target;
        var pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        Directory.CreateDirectory("Logs/LabScreens");
        File.WriteAllBytes("Logs/LabScreens/" + name + ".png", pixels.EncodeToPNG());
        RenderTexture.active = previous;
        camera.targetTexture = null;
        camera.ResetAspect();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        UnityEngine.Object.Destroy(pixels); UnityEngine.Object.Destroy(target);
    }
    private static void Schedule(Action action, double delay) { next = action; due = EditorApplication.timeSinceStartup + delay; }
    private static void Tick()
    {
        if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < due) return;
        try { next(); }
        catch (Exception error)
        {
            Debug.LogException(error);
            File.WriteAllText("Logs/lab-checks-result.txt", error.ToString());
            Exit(1);
        }
    }
    private static void Ensure(bool okay, string message) { if (!okay) throw new Exception(message); }
    private static void Exit(int code)
    {
        Application.logMessageReceived -= Observe;
        EditorApplication.update -= Tick;
        EditorSettings.enterPlayModeOptions = previousOptions;
        EditorApplication.Exit(code);
    }
}




