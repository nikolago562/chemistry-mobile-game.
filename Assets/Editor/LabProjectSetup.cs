using System;
using System.IO;
using System.Linq;
using ChemistryGame;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public static class LabProjectSetup
{
    [Serializable] private class Seed { public QuestionData[] questions; public ElementData[] elements; }
    [Serializable] private class QuestionData
    {
        public string questionID, equationPrompt, correctAnswer, hintText;
        public int reactionCategory, difficultyTier, coinReward;
        public string[] involvedElements;
        public bool isVisualExperiment;
    }
    [Serializable] private class ElementData
    {
        public int atomicNumber, row, column;
        public string symbol, georgianName, fact, group;
    }

    public static void ImportEssentials()
    {
        string package = Directory.GetFiles("Library/PackageCache", "TMP Essential Resources.unitypackage", SearchOption.AllDirectories).First();
        AssetDatabase.importPackageCompleted += _ => EditorApplication.Exit(0);
        AssetDatabase.importPackageFailed += (_, error) => { Debug.LogError(error); EditorApplication.Exit(1); };
        AssetDatabase.ImportPackage(package, false);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

    [MenuItem("ქიმიის ლაბორატორია/სისტემების აგება")]
    public static void Setup()
    {
        foreach (string folder in new[] { "Assets/Data/Questions", "Assets/Data/Discoveries", "Assets/Data/Cosmetics", "Assets/Art/Fonts/TMP" })
            Directory.CreateDirectory(folder);
        AssetDatabase.Refresh();
        var seed = JsonUtility.FromJson<Seed>(File.ReadAllText("Assets/Data/LabContent.json"));
        var questions = seed.questions.Select(CreateQuestion).ToArray();
        var elements = BuildElements(seed.elements);
        var cosmetics = BuildCosmetics();
        var font = BuildFonts();
        BuildAnimator();
        UpdatePlayerPrefab();

        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainLab.unity");
        foreach (var old in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(c => c.GetType().Name == "ExperimentStation"))
            UnityEngine.Object.DestroyImmediate(old.gameObject);
        foreach (var old in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(c => c.GetType().Name == "ChemistryMinigame"))
            UnityEngine.Object.DestroyImmediate(old.gameObject);
        var previous = UnityEngine.Object.FindFirstObjectByType<LabGame>();
        if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
        var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        if (player == null) throw new Exception("Player prefab update did not reach the classroom.");
        var root = new GameObject("ლაბორატორიის სისტემები");
        var lab = root.AddComponent<LabGame>();
        lab.questions = questions;
        lab.cosmetics = cosmetics;
        lab.elements = elements;
        lab.interfaceFont = font;
        lab.player = player;
        lab.classroom = GameObject.Find("MainLab").transform;
        var audio = root.AddComponent<AudioManager>();
        string[] fields = { "footsteps", "uiOpen", "uiClose", "buttonClick", "correctAnswer", "wrongAnswer", "coinPickup", "milestone" };
        string[] clips = { "Footsteps", "UIOpen", "UIClose", "ButtonClick", "CorrectAnswer", "WrongAnswer", "CoinPickup", "Milestone" };
        var serialized = new SerializedObject(audio);
        for (int i = 0; i < fields.Length; i++)
            serialized.FindProperty(fields[i]).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/" + clips[i] + ".wav");
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(lab);
        EditorSceneManager.SaveScene(scene);
        PlayerSettings.productName = "ქიმიის ლაბორატორია";
        AssetDatabase.SaveAssets();
        VisualUpgradeSetup.Run();
        Debug.Log("LAB_SETUP_COMPLETE");
    }

    private static ChemistryQuestionSO CreateQuestion(QuestionData data)
    {
        var q = Asset<ChemistryQuestionSO>("Assets/Data/Questions/" + data.questionID + ".asset");
        q.questionID = data.questionID;
        q.equationPrompt = data.equationPrompt;
        q.correctAnswer = data.correctAnswer;
        q.hintText = data.hintText;
        q.reactionCategory = (ReactionCategory)data.reactionCategory;
        q.difficultyTier = data.difficultyTier;
        q.coinReward = data.coinReward;
        q.involvedElements = data.involvedElements;
        q.isVisualExperiment = data.isVisualExperiment;
        EditorUtility.SetDirty(q);
        return q;
    }

    private static ElementCollectionSO BuildElements(ElementData[] source)
    {
        var collection = Asset<ElementCollectionSO>("Assets/Data/Elements.asset");
        collection.elements = source.Select(data =>
        {
            var question = Asset<ChemistryQuestionSO>("Assets/Data/Discoveries/element_" + data.atomicNumber.ToString("000") + ".asset");
            question.questionID = "element_" + data.atomicNumber;
            question.equationPrompt = data.georgianName + " — ატომური ნომერი " + data.atomicNumber + "\nჩაწერე ელემენტის ქიმიური სიმბოლო.";
            question.correctAnswer = data.symbol;
            question.hintText = "სიმბოლოს პირველი ასოა " + data.symbol[0] + ". " + data.fact;
            question.involvedElements = new[] { data.symbol };
            question.coinReward = 5;
            question.difficultyTier = 1;
            question.reactionCategory = ReactionCategory.ElementDiscovery;
            EditorUtility.SetDirty(question);
            return new ElementCollectionSO.Element
            {
                atomicNumber = data.atomicNumber, symbol = data.symbol, georgianName = data.georgianName,
                fact = data.fact, row = data.row, column = data.column, discoveryQuestion = question,
                color = ElementColor(data.group)
            };
        }).ToArray();
        EditorUtility.SetDirty(collection);
        return collection;
    }

    private static Color ElementColor(string group)
    {
        switch (group)
        {
            case "Noble gas": return new Color32(96, 91, 155, 255);
            case "Nonmetal": return new Color32(50, 115, 95, 255);
            case "Halogen": return new Color32(46, 125, 136, 255);
            case "Alkali metal": return new Color32(168, 93, 65, 255);
            case "Alkaline earth metal": return new Color32(155, 123, 52, 255);
            case "Lanthanide": return new Color32(135, 76, 122, 255);
            case "Actinide": return new Color32(136, 70, 86, 255);
            default: return new Color32(65, 106, 133, 255);
        }
    }

    private static CosmeticItemSO[] BuildCosmetics()
    {
        return new[]
        {
            Cosmetic("goggles_clear", "დამცავი სათვალე", "გამჭვირვალე ლინზები და მუქი ჩარჩო.", CosmeticCategory.SafetyGoggles, 25, Color.white),
            Cosmetic("goggles_gold", "ოქროს სათვალე", "ქარვისფერი დამცავი ლინზები.", CosmeticCategory.SafetyGoggles, 80, new Color(1, 0.8f, 0.4f)),
            Cosmetic("coat_white", "თეთრი ხალათი", "პირველი დღის უსაფრთხო აღჭურვილობა.", CosmeticCategory.LabCoat, 0, Color.white),
            Cosmetic("coat_yellow", "ყვითელი ხალათი", "კაშკაშა ყვითელი ლაბორატორიული სტილი.", CosmeticCategory.LabCoat, 50, new Color(1, 0.83f, 0.2f)),
            Cosmetic("coat_cyber", "ნეონის ხალათი", "ფირუზისფერი მომავლის ლაბორატორიისთვის.", CosmeticCategory.LabCoat, 120, new Color(0.2f, 1, 0.87f)),
            Cosmetic("coat_blue", "სამეფო ლურჯი ხალათი", "ღრმა ლურჯი ფერი ცნობისმოყვარე მკვლევრისთვის.", CosmeticCategory.LabCoat, 80, new Color(0.35f, 0.5f, 1)),
            Cosmetic("hat_violet", "იისფერი ქუდი", "პატარა ქუდი დიდი იდეებისთვის.", CosmeticCategory.HairOrHat, 60, Color.white),
            Cosmetic("weekly_crown", "კვირის ოქროს გვირგვინი", "ექსკლუზიური ჯილდო დასრულებული კვირის პირველი ადგილისთვის.", CosmeticCategory.HairOrHat, 0, Color.white, true),
            Cosmetic("decor_globe", "პლაზმის გლობუსი", "მბზინავი მინიატურა შენს პირად მაგიდაზე.", CosmeticCategory.PersonalDeskDecor, 100, Color.white),
            Cosmetic("decor_flask", "ბუშტებიანი კოლბა", "ფირუზისფერი კოლბა პირადი მაგიდისთვის.", CosmeticCategory.PersonalDeskDecor, 40, Color.white),
            Cosmetic("decor_poster", "პერიოდული ცხრილის პოსტერი", "პატარა ფერადი ცხრილი პირად მაგიდაზე.", CosmeticCategory.PersonalDeskDecor, 30, Color.white)
        };
    }
    private static CosmeticItemSO Cosmetic(string id, string name, string description, CosmeticCategory category, int price, Color color, bool exclusive = false)
    {
        var item = Asset<CosmeticItemSO>("Assets/Data/Cosmetics/" + id + ".asset");
        item.itemID = id; item.displayName = name; item.description = description; item.category = category;
        item.price = price; item.color = color; item.weeklyExclusive = exclusive;
        EditorUtility.SetDirty(item);
        return item;
    }

    private static TMP_FontAsset BuildFonts()
    {
        var latin = FontAsset("NotoSans-Regular");
        var georgian = FontAsset("NotoSansGeorgian-Regular");
        latin.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
        latin.fallbackFontAssetTable.Add(georgian);
        latin.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 +-=?!()[]/.,:;×→↓⁺⁻²³⁴⁵⁶⁷⁸⁹−•…_\n");
        string alphabet = "";
        for (int code = 0x10A0; code <= 0x10FF; code++) alphabet += (char)code;
        for (int code = 0x1C90; code <= 0x1CBF; code++) alphabet += (char)code;
        georgian.TryAddCharacters(alphabet);
        EditorUtility.SetDirty(latin); EditorUtility.SetDirty(georgian);
        return latin;
    }
    private static TMP_FontAsset FontAsset(string name)
    {
        string path = "Assets/Art/Fonts/TMP/" + name + ".asset";
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (font != null) return font;
        var source = AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/Fonts/" + name + ".ttf");
        font = TMP_FontAsset.CreateFontAsset(source, 64, 8, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
        AssetDatabase.CreateAsset(font, path);
        foreach (var atlas in font.atlasTextures) AssetDatabase.AddObjectToAsset(atlas, font);
        AssetDatabase.AddObjectToAsset(font.material, font);
        return font;
    }
    private static void BuildAnimator()
    {
        const string path = "Assets/Animations/PlayerEightWay.controller";
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (existing != null) return;
        var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
        controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
        var machine = controller.layers[0].stateMachine;
        var idle = machine.AddState("Idle");
        var walk = machine.AddState("Walk");
        idle.motion = Tree(controller, "Idle", new[] { "idle_right", "idle_back", "idle_left", "idle_front" });
        walk.motion = Tree(controller, "Walk", new[] { "walk_right", "walk_front", "walk_left", "walk_back" });
        machine.defaultState = idle;
        Transition(idle, walk, AnimatorConditionMode.If);
        Transition(walk, idle, AnimatorConditionMode.IfNot);
    }
    private static BlendTree Tree(AnimatorController controller, string name, string[] clips)
    {
        var tree = new BlendTree { name = name, blendType = BlendTreeType.SimpleDirectional2D, blendParameter = "MoveX", blendParameterY = "MoveY", useAutomaticThresholds = false };
        AssetDatabase.AddObjectToAsset(tree, controller);
        Vector2[] directions = { Vector2.right, Vector2.up, Vector2.left, Vector2.down };
        for (int i = 0; i < clips.Length; i++)
            tree.AddChild(AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/" + clips[i] + ".anim"), directions[i]);
        return tree;
    }
    private static void Transition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode)
    {
        var transition = from.AddTransition(to);
        transition.hasExitTime = false; transition.duration = 0;
        transition.AddCondition(mode, 0, "IsMoving");
    }
    private static void UpdatePlayerPrefab()
    {
        const string path = "Assets/Prefabs/Player.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        var old = root.GetComponent("PlayerMovement");
        if (old != null) UnityEngine.Object.DestroyImmediate(old);
        var circle = root.GetComponent<CircleCollider2D>();
        var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Prefabs/Player.physicsMaterial2D");
        if (circle != null) UnityEngine.Object.DestroyImmediate(circle);
        var box = root.GetComponent<BoxCollider2D>();
        if (box == null) box = root.AddComponent<BoxCollider2D>();
        box.size = new Vector2(0.64f, 0.40f);
        box.offset = new Vector2(0, 1.18f);
        box.edgeRadius = 0.08f;
        box.sharedMaterial = material;
        root.GetComponent<Rigidbody2D>().gravityScale = 0;
        root.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        root.GetComponent<Animator>().runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/PlayerEightWay.controller");
        if (root.GetComponent<PlayerController>() == null) root.AddComponent<PlayerController>();
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/sprites/character-spritesheet.png");
        var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
        importer.isReadable = true;
        importer.SaveAndReimport();
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }
    private static T Asset<T>(string path) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;
        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }
}






