using ChemistryGame;
using UnityEditor;
using UnityEngine;

// A selected scene object keeps its Inspector repainting during play. With the AudioManager
// selected, the footstep AudioSource repaints it every frame while walking (5-8 ms of editor
// GUI and ~150 KB of garbage per frame, halving FPS). Deselect on Play, restore after.
[InitializeOnLoad]
internal static class LabPlayModePerformance
{
    private const string SelectionKey = "ChemistryGame.PlayModeSelection";

    static LabPlayModePerformance()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            var selected = Selection.activeGameObject;
            // Only scene objects; a selected asset (script, sprite) doesn't repaint during play.
            if (Selection.objects.Length != 1 || selected == null || !selected.scene.IsValid()) return;
            SessionState.SetString(SelectionKey, GlobalObjectId.GetGlobalObjectIdSlow(selected).ToString());
            Selection.activeObject = null;
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            string saved = SessionState.GetString(SelectionKey, "");
            SessionState.EraseString(SelectionKey);
            if (Selection.activeObject == null && GlobalObjectId.TryParse(saved, out var id))
                Selection.activeObject = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
        }
    }
}
