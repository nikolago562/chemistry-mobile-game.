using System.Collections.Generic;
using ChemistryGame;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public static class LabTypographySetup
{
    private const string FontPath = "Assets/Art/Fonts/NotoSerifGeorgian/NotebookGeorgian.asset";

    public static TMP_FontAsset BuildFont()
    {
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (existing != null) return existing;
        var source = AssetDatabase.LoadAssetAtPath<Font>(
            "Assets/Art/Fonts/NotoSerifGeorgian/NotoSerifGeorgian-Variable.ttf");
        if (source == null) throw new System.InvalidOperationException("Notebook Georgian font source is missing.");
        var font = TMP_FontAsset.CreateFontAsset(source, 80, 8, GlyphRenderMode.SDFAA,
            1024, 1024, AtlasPopulationMode.Dynamic, false);
        font.name = "Notebook Georgian";
        font.fallbackFontAssetTable = new List<TMP_FontAsset>
        {
            AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/TMP/NotoSans-Regular.asset"),
            AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/FiraGO/TMP/FiraGO-Regular.asset")
        };
        string characters = "";
        for (int c = 32; c < 127; c++) characters += (char)c;
        for (int c = 0x10A0; c <= 0x10FF; c++) characters += (char)c;
        for (int c = 0x1C90; c <= 0x1CBF; c++) characters += (char)c;
        characters += "→←↑↓×−•…⁺⁻²³⁴⁵⁶⁷⁸⁹±°≤≥✓";
        font.TryAddCharacters(characters, out string missing);
        for (char c = '\u10D0'; c <= '\u10F6'; c++)
            if (!font.HasCharacter(c)) throw new System.InvalidOperationException("Missing Georgian glyph: " + c);
        font.atlasPopulationMode = AtlasPopulationMode.Static;
        AssetDatabase.CreateAsset(font, FontPath);
        foreach (var atlas in font.atlasTextures) AssetDatabase.AddObjectToAsset(atlas, font);
        AssetDatabase.AddObjectToAsset(font.material, font);
        EditorUtility.SetDirty(font);
        AssetDatabase.SaveAssets();
        return font;
    }

    [MenuItem("ქიმიის ლაბორატორია/ინტერფეისის შრიფტის განახლება")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Apply typography outside Play mode.");
        var game = Object.FindAnyObjectByType<LabGame>();
        if (game == null) throw new System.InvalidOperationException("Open MainLab before applying typography.");
        Undo.RecordObject(game, "Apply notebook Georgian font");
        game.interfaceFont = BuildFont();
        EditorUtility.SetDirty(game);
        EditorSceneManager.MarkSceneDirty(game.gameObject.scene);
        EditorSceneManager.SaveScene(game.gameObject.scene);
        AssetDatabase.SaveAssets();
    }
}
