using ChemistryGame;
using UnityEditor;
using UnityEngine;

// A selected systems object expands many runtime Inspectors (including audio and HUD).
// Their repaint cost dominated the walking capture. Restore the selection after play.
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
            if (Selection.objects.Length != 1 || selected == null || selected.GetComponent<LabGame>() == null) return;
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
