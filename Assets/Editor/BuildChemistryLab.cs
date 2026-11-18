using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildChemistryLab
{
    public static void Run()
    {
        var report = BuildPipeline.BuildPlayer(new[] { "Assets/Scenes/MainLab.unity" },
            "Builds/ChemistryLab/ChemistryLab.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception("Windows build failed.");
        Debug.Log("LAB_WINDOWS_BUILD_PASSED");
    }
}
