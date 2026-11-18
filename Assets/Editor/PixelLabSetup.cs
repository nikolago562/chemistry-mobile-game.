using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ChemistryGame;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.Tilemaps;

public static class PixelLabSetup
{
    private const string Source = "Assets/Art/Characters/Puny/PunyCharacterBase.png";
    private const string AnimationFolder = "Assets/Animations/PixelStudent/";
    private const string ThemePath = "Assets/Data/PixelTheme.asset";

    public static void Run()
    {
        Directory.CreateDirectory(AnimationFolder);
        Directory.CreateDirectory("Assets/Art/Fonts/FiraGO/TMP");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        ImportLaboratory();
        ImportCharacter();
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Source);
        var clips = BuildAnimations(texture);
        var controller = BuildController(clips);
        var studentMaterial = CharacterMaterial("Student", texture, new Color32(74, 49, 47, 255), false);
        var teacherMaterial = CharacterMaterial("Teacher", texture, new Color32(175, 178, 178, 255), true);
        UpdatePlayer(controller, clips[0, 0], studentMaterial);
        var theme = Theme(texture, teacherMaterial);
        var font = BuildFont();

        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainLab.unity");
        var game = UnityEngine.Object.FindFirstObjectByType<LabGame>();
        game.visualTheme = theme;
        game.interfaceFont = font;
        game.teacherMaterial = teacherMaterial;
        game.teacherSprite = Frame(clips[0, 0]);
        game.player.transform.localPosition = new Vector3(46.5f, 6.8f);
        var layout = game.GetComponent<ChemistryClassroom>();
        if (layout == null) layout = game.gameObject.AddComponent<ChemistryClassroom>();
        layout.storageCupboard = theme.storageCupboard;
        ClearOldDesks(game.classroom);
        UpdateFloor(game.classroom);
        EditorUtility.SetDirty(game);
        EditorUtility.SetDirty(layout);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("PIXEL_LAB_SETUP_PASSED");
    }

    private static void ImportLaboratory()
    {
        foreach (string path in Directory.GetFiles("Assets/Resources/Laboratory", "*.png"))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
            importer.textureType = TextureImporterType.Default;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }
    }
    private static void UpdateFloor(Transform classroom)
    {
        const string path = "Assets/Data/LaboratoryFloor.asset";
        var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = "LaboratoryFloor"; tile.colliderType = Tile.ColliderType.None;
            AssetDatabase.CreateAsset(tile, path);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Laboratory/construction.png");
            var sprite = Sprite.Create(texture, new Rect(96, texture.height - 48, 16, 16), Vector2.one * 0.5f, 16);
            sprite.name = "LaboratoryFloor";
            AssetDatabase.AddObjectToAsset(sprite, tile); tile.sprite = sprite;
        }
        foreach (var map in classroom.GetComponentsInChildren<Tilemap>())
            if (map.name == "Floor")
                foreach (var point in map.cellBounds.allPositionsWithin)
                    if (point.x >= 24 && map.GetTile(point) != null) map.SetTile(point, tile);
    }

    private static void ImportCharacter()
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(Source);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 8;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();
    }

    private static AnimationClip[,] BuildAnimations(Texture2D texture)
    {
        string[] directions = { "S", "SE", "E", "NE", "N", "NW", "W", "SW" };
        var clips = new AnimationClip[2, 8];
        for (int mode = 0; mode < 2; mode++)
        for (int row = 0; row < 8; row++)
        {
            string name = (mode == 0 ? "Idle_" : "Walk_") + directions[row];
            string path = AnimationFolder + name + ".anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                float rate = mode == 0 ? 2 : 8;
                int count = mode == 0 ? 2 : 4;
                clip = new AnimationClip { name = name, frameRate = rate };
                AssetDatabase.CreateAsset(clip, path);
                var keys = new ObjectReferenceKeyframe[count + 1];
                for (int frame = 0; frame < count; frame++)
                {
                    int column = mode == 0 ? frame : frame + 2;
                    var sprite = Sprite.Create(texture, new Rect(column * 32, texture.height - (row + 1) * 32, 32, 32),
                        new Vector2(0.5f, 9f / 32f), 8, 0, SpriteMeshType.FullRect);
                    sprite.name = name + "_" + frame;
                    AssetDatabase.AddObjectToAsset(sprite, clip);
                    keys[frame] = new ObjectReferenceKeyframe { time = frame / rate, value = sprite };
                }
                keys[count] = new ObjectReferenceKeyframe { time = count / rate, value = keys[0].value };
                AnimationUtility.SetObjectReferenceCurve(clip,
                    EditorCurveBinding.PPtrCurve("Sprite", typeof(SpriteRenderer), "m_Sprite"), keys);
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
            }
            clips[mode, row] = clip;
        }
        return clips;
    }

    private static AnimatorController BuildController(AnimationClip[,] clips)
    {
        string path = AnimationFolder + "PixelStudentEightWay.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller != null) return controller;
        controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
        controller.AddParameter(new AnimatorControllerParameter { name = "MoveY", type = AnimatorControllerParameterType.Float, defaultFloat = -1 });
        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
        var machine = controller.layers[0].stateMachine;
        var idle = machine.AddState("Idle");
        var walk = machine.AddState("Walk");
        var states = new[] { idle, walk };
        Vector2[] directions = { Vector2.down, new Vector2(1, -1).normalized, Vector2.right,
            new Vector2(1, 1).normalized, Vector2.up, new Vector2(-1, 1).normalized, Vector2.left, new Vector2(-1, -1).normalized };
        for (int mode = 0; mode < 2; mode++)
        {
            var tree = new BlendTree { name = states[mode].name, blendType = BlendTreeType.SimpleDirectional2D,
                blendParameter = "MoveX", blendParameterY = "MoveY", useAutomaticThresholds = false };
            AssetDatabase.AddObjectToAsset(tree, controller);
            for (int row = 0; row < 8; row++) tree.AddChild(clips[mode, row], directions[row]);
            states[mode].motion = tree;
        }
        machine.defaultState = idle;
        Transition(idle, walk, AnimatorConditionMode.If);
        Transition(walk, idle, AnimatorConditionMode.IfNot);
        return controller;
    }

    private static void Transition(AnimatorState from, AnimatorState to, AnimatorConditionMode condition)
    {
        var transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0;
        transition.AddCondition(condition, 0, "IsMoving");
    }

    private static Material CharacterMaterial(string name, Texture2D texture, Color hair, bool coat)
    {
        string path = "Assets/Art/Characters/Puny/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Chemistry/PixelCharacter")) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }
        material.SetTexture("_MainTex", texture);
        material.SetColor("_HairColor", hair);
        material.SetColor("_ShirtColor", new Color32(88, 120, 163, 255));
        material.SetColor("_CoatColor", new Color32(240, 241, 227, 255));
        material.SetFloat("_HasCoat", coat ? 1 : 0);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void UpdatePlayer(AnimatorController controller, AnimationClip idle, Material material)
    {
        var root = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Player.prefab");
        root.GetComponent<Animator>().runtimeAnimatorController = controller;
        var sprite = root.transform.Find("Sprite");
        sprite.localPosition = Vector3.zero;
        sprite.localScale = Vector3.one;
        sprite.GetComponent<SpriteRenderer>().sprite = Frame(idle);
        sprite.GetComponent<SpriteRenderer>().sharedMaterial = material;
        root.GetComponent<BoxCollider2D>().offset = new Vector2(0, 0.08f);
        root.GetComponent<BoxCollider2D>().size = new Vector2(0.55f, 0.28f);
        PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Player.prefab");
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static Sprite Frame(AnimationClip clip)
    {
        return (Sprite)AnimationUtility.GetObjectReferenceCurve(clip,
            AnimationUtility.GetObjectReferenceCurveBindings(clip).Single())[0].value;
    }

    private static LabThemeSO Theme(Texture2D texture, Material teacherMaterial)
    {
        var theme = AssetDatabase.LoadAssetAtPath<LabThemeSO>(ThemePath);
        if (theme == null)
        {
            theme = ScriptableObject.CreateInstance<LabThemeSO>();
            AssetDatabase.CreateAsset(theme, ThemePath);
            var portrait = Sprite.Create(texture, new Rect(8, texture.height - 24, 16, 16), Vector2.one * 0.5f, 8);
            portrait.name = "PixelPortrait";
            AssetDatabase.AddObjectToAsset(portrait, theme);
            theme.characterPortrait = theme.teacherPortrait = portrait;
            var cupboard = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/tilesets/book_cupboard.png");
            theme.storageCupboard = Sprite.Create(cupboard, new Rect(0, 0, cupboard.width, cupboard.height),
                new Vector2(0.5f, 0), 16);
            theme.storageCupboard.name = "StorageCupboard";
            AssetDatabase.AddObjectToAsset(theme.storageCupboard, theme);
        }
        theme.teacherPortraitMaterial = teacherMaterial;
        theme.button = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Kenney/grey_button03.png");
        EditorUtility.SetDirty(theme);
        return theme;
    }

    private static TMP_FontAsset BuildFont() => LabTypographySetup.BuildFont();

    private static void ClearOldDesks(Transform classroom)
    {
        foreach (var map in classroom.GetComponentsInChildren<Tilemap>())
        {
            foreach (var position in map.cellBounds.allPositionsWithin)
            {
                if (position.x < 24) continue;
                var tile = map.GetTile(position);
                if (tile == null) continue;
                if (tile.name.StartsWith("Classroom_Desk_", StringComparison.Ordinal) ||
                    tile.name.StartsWith("main_desk", StringComparison.Ordinal) ||
                    tile.name.StartsWith("bin_", StringComparison.Ordinal))
                    map.SetTile(position, null);
            }
            // Tile removal is deferred; save the regenerated geometry, not the old desk outlines.
            var tileCollider = map.GetComponent<TilemapCollider2D>();
            if (tileCollider != null)
            {
                tileCollider.enabled = false;
                map.RefreshAllTiles();
                tileCollider.enabled = true;
                tileCollider.ProcessTilemapChanges();
            }
            var composite = map.GetComponent<CompositeCollider2D>();
            if (composite != null) composite.GenerateGeometry();
        }
    }
}


