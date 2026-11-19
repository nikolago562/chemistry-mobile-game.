using ChemistryGame;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class LabTypographySetup
{
    // FiraGO-Regular carries its own Medium/Bold weight table and NotoSans fallback.
    private const string FontPath = "Assets/Art/Fonts/FiraGO/TMP/FiraGO-Regular.asset";

    public static TMP_FontAsset BuildFont()
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (font == null) throw new System.InvalidOperationException("FiraGO font asset is missing: " + FontPath);
        if (!font.HasCharacter('ა', false, true)) throw new System.InvalidOperationException("FiraGO has no Georgian glyphs.");
        return font;
    }

    [MenuItem("ქიმიის ლაბორატორია/ინტერფეისის შრიფტის განახლება")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Apply typography outside Play mode.");
        var game = Object.FindAnyObjectByType<LabGame>();
        if (game == null) throw new System.InvalidOperationException("Open MainLab before applying typography.");
        Undo.RecordObject(game, "Apply FiraGO interface font");
        game.interfaceFont = BuildFont();
        EditorUtility.SetDirty(game);
        EditorSceneManager.MarkSceneDirty(game.gameObject.scene);
        EditorSceneManager.SaveScene(game.gameObject.scene);
        AssetDatabase.SaveAssets();
    }
}
