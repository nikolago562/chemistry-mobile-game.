using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChemistryGame
{
    /// <summary>
    /// Performance self-test, only when the game is started with <c>-perf-walk</c>:
    /// alternates 10 s standing and 10 s walking the classroom aisles, records every frame
    /// time and writes the result to <c>perf-walk.txt</c> next to the save, then quits.
    /// Measures a real build, without any editor overhead.
    /// </summary>
    public sealed class PerfWalk : MonoBehaviour
    {
        public static bool Requested => System.Environment.GetCommandLineArgs().Contains("-perf-walk");
        private static readonly Vector2[] Path = System.Array.ConvertAll(new[] { new Vector2(44.75f, 6), new Vector2(44.75f, -3), new Vector2(60, -3), new Vector2(60, -11), new Vector2(30, -11), new Vector2(30, -3), new Vector2(44.75f, -3) }, SchoolLayout.P);
        private LabGame game;
        private bool walking;
        private int waypoint;

        public void Initialize(LabGame lab) { game = lab; StartCoroutine(Run()); }

        private IEnumerator Run()
        {
            yield return new WaitForSecondsRealtime(1.5f);
            game.Profile.NameInput.text = "perf";
            game.Profile.Confirm();
            for (int i = 0; i < 10 && LabUI.AnyOpen; i++) LabUI.CloseTop();
            game.Dialogue.End();
            yield return new WaitForSecondsRealtime(2f);
            var report = new StringBuilder("Build: " + Application.unityVersion + " | " + Screen.width + "x" + Screen.height +
                " | vSync " + QualitySettings.vSyncCount + " | refresh " + Screen.currentResolution.refreshRateRatio.value.ToString("0") + " Hz\n");
            foreach (var phase in new[] { "stand", "walk", "stand", "walk" })
            {
                walking = phase == "walk";
                var times = new List<float>();
                for (float t = 0; t < 10; t += Time.unscaledDeltaTime) { yield return null; times.Add(Time.unscaledDeltaTime * 1000); }
                times.RemoveAt(0);
                float avg = times.Average();
                report.AppendLine($"{phase,-6} avg {avg:0.0} ms ({1000 / avg:0} fps) | p95 {times.OrderBy(x => x).ElementAt((int)(times.Count * 0.95f)):0.0} ms | max {times.Max():0.0} ms | frames over 17 ms: {times.Count(x => x > 17)}/{times.Count}");
            }
            File.WriteAllText(System.IO.Path.Combine(Application.persistentDataPath, "perf-walk.txt"), report.ToString());
            Application.Quit();
        }

        private void FixedUpdate()
        {
            var player = game != null ? game.player : null;
            if (player == null) return;
            player.enabled = !walking;
            var animator = player.GetComponent<Animator>();
            if (!walking) { animator.SetBool("IsMoving", false); return; }
            var body = player.GetComponent<Rigidbody2D>();
            Vector2 goal = game.classroom.TransformPoint(Path[waypoint]);
            Vector2 d = goal - body.position;
            if (d.magnitude < 0.3f) { waypoint = (waypoint + 1) % Path.Length; return; }
            body.linearVelocity = d.normalized * 4.25f;
            animator.SetFloat("MoveX", d.normalized.x); animator.SetFloat("MoveY", d.normalized.y); animator.SetBool("IsMoving", true);
        }
    }
}
