using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ChemistryGame;
using Unity.Profiling;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Profiling;
using UnityEditorInternal;
using UnityEngine;

// Temporary probe: compares frame cost with clear weather and with rain.
public static class RainPerfProbe
{
    private static readonly StringBuilder report = new StringBuilder();
    private static readonly Queue<(string label, Action setup)> phases = new Queue<(string, Action)>();
    private static Sampler sampler;
    private static double due;
    private static bool started;
    private static int phaseStart;
    private static EnterPlayModeOptions previousOptions;

    public static void Run()
    {
        GameSaveManager.TestDirectory = Path.GetFullPath("Logs/rain-probe-profile");
        if (Directory.Exists(GameSaveManager.TestDirectory)) Directory.Delete(GameSaveManager.TestDirectory, true);
        Directory.CreateDirectory(GameSaveManager.TestDirectory);
        previousOptions = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorSceneManager.OpenScene("Assets/Scenes/MainLab.unity");
        EditorApplication.update += Tick;
        due = EditorApplication.timeSinceStartup + 2;
        ProfilerDriver.profileEditor = false;
        ProfilerDriver.enabled = true;
        EditorApplication.isPlaying = true;
    }

    private static void Weather(string w)
    {
        var data = GameSaveManager.Instance.Data;
        data.weatherDay = LabProgress.Today; data.weather = w;
        LabAtmosphere.Instance.Apply(true);
    }

    private static void Tick()
    {
        if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < due) return;
        if (!started)
        {
            started = true;
            var game = UnityEngine.Object.FindFirstObjectByType<LabGame>();
            game.Profile.NameInput.text = "probe";
            game.Profile.Confirm();
            for (int i = 0; i < 10 && LabUI.AnyOpen; i++) LabUI.CloseTop();
            game.Dialogue.End();
            sampler = new GameObject("probe").AddComponent<Sampler>();
            var data = GameSaveManager.Instance.Data;
            data.tutorialCompleted = true; data.reputation = 9999;
            void Hour(int h) => SchoolCalendar.Override = DateTime.Today.AddHours(h);
            phases.Enqueue(("real clear", () => Weather("clear")));
            phases.Enqueue(("real rain", () => Weather("rain")));
            phases.Enqueue(("real clear2", () => Weather("clear")));
            phases.Enqueue(("real rain2", () => Weather("rain")));
            phases.Enqueue(("real clear3", () => Weather("clear")));
            phases.Enqueue(("real rain3", () => Weather("rain")));
            Next();
            return;
        }
        if (!sampler.Done) return;
        report.AppendLine(sampler.Summary);
        report.AppendLine(Spikes(phaseStart, ProfilerDriver.lastFrameIndex));
        if (phases.Count > 0) { Next(); return; }
        File.WriteAllText("Logs/rain-probe.txt", report.ToString());
        Debug.Log("RAIN_PROBE_DONE\n" + report);
        EditorApplication.update -= Tick;
        EditorSettings.enterPlayModeOptions = previousOptions;
        EditorApplication.Exit(0);
    }

    private static void Next()
    {
        var (label, setup) = phases.Dequeue();
        setup();
        phaseStart = ProfilerDriver.lastFrameIndex + 1;
        sampler.Begin(label, 270);
    }

    /// <summary>Self time per profiler marker, averaged over the frames slower than 25 ms.</summary>
    private static string Spikes(int from, int to)
    {
        var self = new Dictionary<string, double>();
        int spikes = 0;
        var kids = new List<int>();
        for (int f = Math.Max(from, ProfilerDriver.firstFrameIndex); f <= to; f++)
            using (var view = ProfilerDriver.GetHierarchyFrameDataView(f, 0, HierarchyFrameDataView.ViewModes.MergeSamplesWithTheSameName, HierarchyFrameDataView.columnSelfTime, false))
            {
                if (!view.valid || view.frameTimeMs < 25) continue;
                spikes++;
                var stack = new Stack<int>();
                stack.Push(view.GetRootItemID());
                while (stack.Count > 0)
                {
                    int id = stack.Pop();
                    float ms = view.GetItemColumnDataAsFloat(id, HierarchyFrameDataView.columnSelfTime);
                    string name = view.GetItemName(id);
                    if (ms > 0.3f) self[name] = (self.TryGetValue(name, out var v) ? v : 0) + ms;
                    view.GetItemChildren(id, kids);
                    foreach (int k in kids) stack.Push(k);
                }
            }
        return "    slow frames " + spikes + ": " + string.Join(", ", self.OrderByDescending(p => p.Value).Take(10).Select(p => p.Key + " " + (p.Value / Math.Max(1, spikes)).ToString("0.0") + "ms"));
    }

    public sealed class Sampler : MonoBehaviour
    {
        public bool Done => left <= 0;
        public string Summary { get; private set; }
        private int left, warm, gcStart;
        private string label;
        private readonly List<double> real = new List<double>(), frame = new List<double>(), scripts = new List<double>(), render = new List<double>(), gc = new List<double>();
        private readonly List<long> batches = new List<long>();
        private ProfilerRecorder mainThread, behaviour, gcAlloc, batchCount;
        private RenderTexture target;
        private Texture2D pixel;
        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        private void Awake()
        {
            mainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1);
            behaviour = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, "BehaviourUpdate", 1);
            gcAlloc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1);
            batchCount = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count", 1);
            target = new RenderTexture(1920, 1080, 24);
            pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        }
        public void Begin(string name, int frames)
        {
            label = name; left = frames; warm = 30; gcStart = System.GC.CollectionCount(0);
            real.Clear(); frame.Clear(); scripts.Clear(); render.Clear(); gc.Clear(); batches.Clear();
        }
        private void LateUpdate()
        {
            if (left <= 0) return;
            var camera = Camera.main;
            camera.targetTexture = target;
            // Reading one pixel back waits for the GPU, so the timing includes GPU work.
            watch.Restart(); camera.Render(); RenderTexture.active = target; pixel.ReadPixels(new Rect(0, 0, 1, 1), 0, 0); RenderTexture.active = null; watch.Stop();
            camera.targetTexture = null;
            if (warm-- > 0) return;
            real.Add(Time.unscaledDeltaTime * 1000); frame.Add(mainThread.LastValue / 1e6); scripts.Add(behaviour.LastValue / 1e6);
            gc.Add(gcAlloc.LastValue / 1024.0); batches.Add(batchCount.LastValue); render.Add(watch.Elapsed.TotalMilliseconds);
            if (--left == 0)
                Summary = $"{label,-12} real {real.Average():0.0} ms ({1000 / real.Average():0} fps, max {real.Max():0}) | main {frame.Average():0.00} ms (p95 {P95(frame):0.00}) | scripts {scripts.Average():0.00} ms | camera render {render.Average():0.00} ms | GC {gc.Average():0.0} KB/frame | max {frame.Max():0.0} ms, >25ms: {frame.Count(f => f > 25)} | GC collections {System.GC.CollectionCount(0) - gcStart}";
        }
        private static double P95(List<double> v) => v.OrderBy(x => x).ElementAt((int)(v.Count * 0.95));
    }
}
