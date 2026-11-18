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
        Ensure(game.interfaceFont.faceInfo.familyName.Contains("Noto Serif Georgian"), "Notebook Georgian typeface");
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
        Ensure(pages.Length >= 5, "Welcome lesson covers the basics");
        dialogueType.GetField("page", Hidden).SetValue(game.Dialogue, 3);
        dialogueType.GetMethod("BeginPage", Hidden).Invoke(game.Dialogue, null);
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
        Ensure(game.Tutorial.Step == TutorialManager.TutorialStep.FirstReaction, "Supply closet trigger progression");
        Ensure(GameSaveManager.Instance.Data.equippedLabCoatID == "coat_white", "Starter coat equipped");
        LabUI.CloseTop();
        Snapshot("09-locker");
        var station = UnityEngine.Object.FindObjectsByType<InteractableStation>(FindObjectsSortMode.None)
            .Single(s => s.stationType == StationType.TutorialStation);
        MoveTo(station.transform.position + Vector3.down * 0.75f);
        game.Interact(station);
        Schedule(FirstQuiz, 0.5);
    }
    private static void FirstQuiz()
    {
        Ensure(game.Quiz.Current.correctAnswer == "2H2O" && !game.player.canMove, "Guided reaction and UI movement lock");
        game.Quiz.Answer.text = "wrong";
        game.Quiz.Answer.onSubmit.Invoke(game.Quiz.Answer.text);
        Ensure(GameSaveManager.Instance.Combo == 0 && game.Quiz.IsOpen, "Incorrect guided answer");
        game.Quiz.Answer.text = " 2 h2o ";
        game.Quiz.Answer.onSubmit.Invoke(game.Quiz.Answer.text);
        int coins = GameSaveManager.Instance.Data.chemCoins;
        game.Quiz.Submit();
        Ensure(GameSaveManager.Instance.Data.chemCoins == coins && coins == 10, "No duplicate quiz payout");
        Ensure(GameSaveManager.Instance.Data.tutorialCompleted && GameSaveManager.Instance.Data.currentDailyStreak == 1, "Tutorial persistence");
        Schedule(AfterTutorial, 1.9);
    }
    private static void AfterTutorial()
    {
        Ensure(!game.Quiz.IsOpen, "Quiz success auto-close");
        Ensure(game.Dialogue.IsOpen, "After the first reaction the teacher starts the classroom tour");
        LabUI.CloseTop();
        Ensure(!LabUI.AnyOpen && TeacherGuide.Next != null && TeacherGuide.Next.ID == "tour", "A skipped lesson stays available at the teacher");
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
        MoveTo(game.classroom.TransformPoint(new Vector3(32f, 8.7f)));
        var station = UnityEngine.Object.FindObjectsByType<InteractableStation>(FindObjectsSortMode.None)
            .First(s => s.stationType == StationType.TeacherNPC);
        Ensure(game.Tutorial.Allows(station), "Teacher can always be talked to");
        game.Interact(station);
        Ensure(game.Dialogue.IsOpen && LabUI.AnyOpen, "Talking to the teacher opens the dialogue");
        Schedule(TeacherTyping, 0.8);
    }
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
        Ensure(Mathf.Abs(ChemistrySandbox.Concentration(0.5f, 1) - 0.5f) < 0.001f &&
            ChemistrySandbox.MoleculeMatches(0, 2, 1, 0, 0) && !ChemistrySandbox.MoleculeMatches(0, 1, 1, 0, 0), "Concentration and molecule models");
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
        game.Personal.Activities.Open("molecule");
        AdjustCounter("წყალბადი • H", 2); AdjustCounter("ჟანგბადი • O", 1);
        Schedule(MoleculeView, 0.3);
    }
    private static void AdjustCounter(string label, int times)
    {
        var button = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b => b.name == "+" && b.transform.parent.name == label);
        for (int i = 0; i < times; i++) button.onClick.Invoke();
    }
    private static void MoleculeView()
    {
        Ensure(UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Any(t => t.text == "მოლეკულა აწყობილია!"), "Interactive water molecule assembly");
        Snapshot("13-molecule"); LabUI.CloseTop(); game.Personal.Activities.Open("solution");
        UnityEngine.Object.FindObjectsByType<Slider>(FindObjectsSortMode.None).Single(s => s.name == "ნივთიერება").value = 0.5f;
        Schedule(SolutionView, 0.3);
    }
    private static void SolutionView()
    {
        Snapshot("14-concentration"); LabUI.CloseTop(); game.Personal.Activities.Open("ph");
        foreach (var slider in UnityEngine.Object.FindObjectsByType<Slider>(FindObjectsSortMode.None))
            if (slider.name != "დამატებული წყალი") slider.value = 50;
        Schedule(PHView, 0.3);
    }
    private static void PHView()
    {
        Snapshot("15-neutralization"); LabUI.CloseTop(); game.Personal.Activities.Open("atom");
        AdjustCounter("პროტონები", 7); AdjustCounter("ნეიტრონები", 8); AdjustCounter("ელექტრონები", 7);
        Schedule(AtomView, 0.3);
    }
    private static void AtomView()
    {
        Snapshot("16-atom"); LabUI.CloseTop();
        Ensure(GameSaveManager.Instance.Data.chemCoins == 495, "Sandbox play neither charges nor farms coins");
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
        nika.Talk();
        Ensure(game.Dialogue.IsOpen, "Classmates talk through the dialogue box");
        Ensure(Classmates.State("nika").friendship == 2, "Daily chat raises friendship once");
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
        game.Games.Open("balance");
        Schedule(GameBalance, 0.4);
    }
    private static void GameBalance()
    {
        Snapshot("43-balance");
        int spills = LabProgress.Data.pendingSpills;
        UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b => b.GetComponentInChildren<TMP_Text>().text == "შემოწმება").onClick.Invoke();
        Ensure(LabProgress.Data.pendingSpills == spills + 1, "Failure leaves a harmless spill to clean");
        Schedule(GameFail, 0.3);
    }
    private static void GameFail()
    {
        Snapshot("44-balance-fail");
        LabUI.CloseTop();
        var spill = UnityEngine.Object.FindObjectsByType<WorldInteractable>().First(w => w.PromptText().Contains("დასუფთავება"));
        int spills = LabProgress.Data.pendingSpills;
        spill.Use();
        Ensure(LabProgress.Data.pendingSpills == spills - 1 && LabProgress.Stat("clean") >= 1, "Cleaning a spill");
        foreach (var id in new[] { "thermo", "analysis", "valve", "particles", "compare" }) Ensure(ExperimentGamesUI.IsGame(id), id);
        game.Games.Open("thermo");
        Schedule(GameThermo, 0.6);
    }
    private static void GameThermo() { Snapshot("45-thermo"); game.Games.Open("analysis"); Schedule(GameAnalysis, 0.4); }
    private static void GameAnalysis()
    {
        UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b => b.GetComponentInChildren<TMP_Text>().text == "pH ქაღალდი").onClick.Invoke();
        Snapshot("46-analysis"); game.Games.Open("valve"); Schedule(GameValve, 0.6);
    }
    private static void GameValve() { Snapshot("47-valve"); game.Games.Open("particles"); Schedule(GameParticles, 1.6); }
    private static void GameParticles() { Snapshot("48-particles"); game.Games.Open("compare"); Schedule(GameCompare, 0.4); }
    private static void GameCompare()
    {
        Snapshot("49-compare"); LabUI.CloseTop();
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
            window.SendMessage("LateUpdate");
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




