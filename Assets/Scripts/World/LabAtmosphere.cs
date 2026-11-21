using System.Collections.Generic;
using UnityEngine;

namespace ChemistryGame
{
    /// <summary>
    /// Time-of-day mood for the whole school: a soft colour grade over the world, window skies
    /// with rain, a ticking wall clock, bubbling glassware, flickering monitors, blinking safety
    /// lights, ventilation fans, floating dust and warm lamp glows that strengthen in the evening.
    /// Everything is cheap sprite animation with point-filtered pixels.
    /// </summary>
    public sealed class LabAtmosphere : MonoBehaviour
    {
        public static LabAtmosphere Instance { get; private set; }
        /// <summary>0 in daylight, 1 in the evening. Lamps and screens read this.</summary>
        public static float Evening { get; private set; }

        private sealed class Pane { public SpriteRenderer Sky; public Vector2 Size; public List<Transform> Drops = new List<Transform>(); public List<SpriteRenderer> Stars = new List<SpriteRenderer>(); }
        private sealed class Glow { public SpriteRenderer Art; public float Base; public bool EveningOnly; public float Phase; }
        private sealed class Mote { public Transform T; public SpriteRenderer Art; public Vector2 Origin, Area; public float Phase; }
        private sealed class BubbleSource { public Vector2 Point; public Transform Parent; public float Next; public int Order; }
        private sealed class Flying { public Transform T; public SpriteRenderer Art; public Vector2 Velocity; public float Life, Age; }

        private LabGame game;
        private SpriteRenderer tint;
        private readonly List<Pane> panes = new List<Pane>();
        private readonly List<Glow> glows = new List<Glow>();
        private readonly List<Mote> motes = new List<Mote>();
        private readonly List<BubbleSource> bubbles = new List<BubbleSource>();
        private readonly List<Flying> flying = new List<Flying>();
        private readonly List<Transform> fans = new List<Transform>();
        private readonly List<SpriteRenderer> lights = new List<SpriteRenderer>();
        private Transform minute, hour;
        private SpriteRenderer monitor, experiment;
        private float savedCheck;

        public void Initialize(LabGame lab)
        {
            Instance = this;
            game = lab;
            var camera = Camera.main;
            var go = new GameObject("დღის განათება", typeof(SpriteRenderer));
            go.transform.SetParent(camera.transform, false);
            go.transform.localPosition = new Vector3(0, 0, 10);
            tint = go.GetComponent<SpriteRenderer>();
            tint.sprite = DecorArt.Square();
            tint.sortingOrder = 9;

            var room = lab.classroom;
            // Classroom windows, clock and props (measured from the classroom wall art).
            AddWindow(room, new Vector2(27.0f, 13.17f), new Vector2(1.84f, 1.68f), false);
            AddWindow(room, new Vector2(52.0f, 13.17f), new Vector2(1.84f, 1.68f), false);
            hour = Hand(room, new Vector2(30.03f, 13.95f), 9);
            minute = Hand(room, new Vector2(30.03f, 13.95f), 13);
            monitor = LaboratoryArt.Draw(room, "მონიტორის ციმციმი", new Vector2(54.49f, 11.33f), DecorArt.Square(), 8);
            monitor.transform.localScale = new Vector3(13, 9, 1);
            AddGlow(room, new Vector2(54.49f, 11.4f), new Color(0.5f, 0.9f, 1f), 1.4f, false);
            AddSafetyLight(room, new Vector2(49.0f, 14.6f));
            AddFan(room, new Vector2(40.2f, 14.6f));
            LaboratoryArt.Draw(room, "ვენტილაცია", new Vector2(40.2f, 14.6f), DecorArt.VentGrille(), 4);
            // Dust only floats in the window light, so it never drifts over the benches.
            AddDust(room, new Vector2(27, 11.2f), new Vector2(4, 2.2f));
            AddDust(room, new Vector2(52, 11.2f), new Vector2(3, 2.2f));
            foreach (var point in new[] { new Vector2(35.3f, 11.4f), new Vector2(36.8f, -3.8f), new Vector2(51.0f, 4.2f), new Vector2(53.3f, -11.8f) })
                AddBubbles(room, point, 8);
            // A harmless background experiment on the teacher's desk: a flask that slowly changes colour.
            experiment = LaboratoryArt.Draw(room, "ფონური ცდა", new Vector2(37.3f, 11.15f), DecorArt.Flask(), 7);
            AddBubbles(room, new Vector2(37.3f, 11.75f), 8);
            Apply(true);
        }

        private void Awake() => Instance = this;

        // ---------- registration (also used by other rooms) ----------

        public void AddWindow(Transform parent, Vector2 local, Vector2 size, bool frame = true)
        {
            var pane = new Pane { Size = size };
            pane.Sky = LaboratoryArt.Draw(parent, "ცა ფანჯარაში", local, DecorArt.Square(), parent == game.classroom ? 1 : -289);
            pane.Sky.transform.localScale = new Vector3(size.x * 16, size.y * 16, 1);
            int order = pane.Sky.sortingOrder + 1;
            for (int i = 0; i < 3; i++)
            {
                var star = LaboratoryArt.Draw(pane.Sky.transform.parent, "ვარსკვლავი", local + new Vector2((i - 1) * size.x * 0.3f, size.y * (0.15f + i % 2 * 0.2f)), DecorArt.Square(), order);
                pane.Stars.Add(star);
            }
            for (int i = 0; i < 6; i++)
            {
                var drop = LaboratoryArt.Draw(parent, "წვიმის წვეთი", local, DecorArt.Square(), order);
                drop.transform.localScale = new Vector3(1, 4, 1);
                drop.color = new Color(0.8f, 0.9f, 1f, 0.75f);
                pane.Drops.Add(drop.transform);
                drop.transform.localPosition = local + new Vector2(Random.Range(-size.x, size.x) * 0.45f, Random.Range(-size.y, size.y) * 0.45f);
            }
            if (frame)
            {
                int w = Mathf.RoundToInt(size.x * 16) + 4, h = Mathf.RoundToInt(size.y * 16) + 4;
                LaboratoryArt.Draw(parent, "ფანჯრის ჩარჩო", local, DecorArt.WindowFrame(w, h), order + 1);
            }
            panes.Add(pane);
            Apply(true);
        }

        public void AddGlow(Transform parent, Vector2 local, Color color, float size, bool eveningOnly)
        {
            var art = LaboratoryArt.Draw(parent, "სინათლე", local, DecorArt.Soft(), 10);
            art.transform.localScale = Vector3.one * size;
            art.color = new Color(color.r, color.g, color.b, 0);
            glows.Add(new Glow { Art = art, Base = 0.35f, EveningOnly = eveningOnly, Phase = Random.value * 10 });
        }

        public SpriteRenderer AddGlowRenderer(Transform parent, Vector2 local, Color color, float size, bool eveningOnly)
        {
            AddGlow(parent, local, color, size, eveningOnly);
            return glows[glows.Count - 1].Art;
        }

        public void AddSafetyLight(Transform parent, Vector2 local)
        {
            var light = LaboratoryArt.Draw(parent, "უსაფრთხოების ნათურა", local, DecorArt.Light(), parent == game.classroom ? 5 : -286);
            lights.Add(light);
            AddGlow(parent, local, new Color(1f, 0.3f, 0.25f), 0.7f, false);
        }

        public void AddFan(Transform parent, Vector2 local)
        {
            var blade = LaboratoryArt.Draw(parent, "ვენტილატორი", local, DecorArt.FanBlade(), parent == game.classroom ? 5 : -286);
            fans.Add(blade.transform);
        }

        public void AddDust(Transform parent, Vector2 centre, Vector2 area)
        {
            for (int i = 0; i < 14; i++)
            {
                var art = LaboratoryArt.Draw(parent, "მტვრის ნაწილაკი", centre, DecorArt.Square(), 10);
                motes.Add(new Mote { T = art.transform, Art = art, Origin = centre + new Vector2(Random.Range(-area.x, area.x) * 0.5f, Random.Range(-area.y, area.y) * 0.5f), Area = area, Phase = Random.value * 100 });
            }
        }

        public void AddBubbles(Transform parent, Vector2 local, int order)
        {
            bubbles.Add(new BubbleSource { Parent = parent, Point = local, Next = Random.value * 2, Order = order });
        }

        /// <summary>Short-lived particle used by ambience and harmless mishaps.</summary>
        public void Puff(Transform parent, Vector2 local, Color color, Vector2 velocity, float life, int order, Sprite sprite = null, float scale = 1)
        {
            // Reuse expired particles: creating and destroying a GameObject per bubble made steady garbage.
            Flying f = null;
            while (spare.Count > 0 && f == null) { f = spare.Pop(); if (f.T == null) f = null; }
            if (f == null)
            {
                var created = LaboratoryArt.Draw(parent, "ნაწილაკი", local, sprite ?? DecorArt.Bubble(), order);
                f = new Flying { T = created.transform, Art = created };
            }
            else
            {
                // Reparenting makes the editor's Hierarchy window rebuild; skip it when unchanged.
                if (f.T.parent != parent) f.T.SetParent(parent, false);
                f.T.localPosition = local;
                f.Art.sprite = sprite ?? DecorArt.Bubble();
                f.Art.sortingOrder = order;
                f.T.gameObject.SetActive(true);
            }
            f.Art.color = color;
            f.T.localScale = Vector3.one * scale;
            f.Velocity = velocity; f.Life = life; f.Age = 0;
            flying.Add(f);
        }
        private readonly Stack<Flying> spare = new Stack<Flying>();

        private Transform Hand(Transform parent, Vector2 local, int length)
        {
            var art = LaboratoryArt.Draw(parent, "საათის ისარი", local, DecorArt.ClockHand(length), 5);
            return art.transform;
        }

        // ---------- per frame ----------

        private void Update()
        {
            float t = Time.time;
            if (Time.unscaledTime > savedCheck)
            {
                savedCheck = Time.unscaledTime + 5;
                if (SchoolCalendar.Refresh()) { Apply(false); GameSaveManager.Instance.Commit(); }
            }
            var camera = Camera.main;
            if (camera != null)
            {
                float height = camera.orthographicSize * 2;
                // Oversized so overview captures with a wider lens are still fully graded.
                tint.transform.localScale = new Vector3(height * camera.aspect * 16 * 4, height * 16 * 4, 1);
            }

            glows.RemoveAll(g => g.Art == null);
            panes.RemoveAll(p => p.Sky == null);
            motes.RemoveAll(m => m.T == null);
            fans.RemoveAll(f => f == null);
            lights.RemoveAll(l => l == null);
            bubbles.RemoveAll(b => b.Parent == null);
            var now = SchoolCalendar.Now;
            if (minute != null)
            {
                minute.localRotation = Quaternion.Euler(0, 0, -(now.Minute + now.Second / 60f) * 6);
                hour.localRotation = Quaternion.Euler(0, 0, -((now.Hour % 12) + now.Minute / 60f) * 30);
            }
            if (monitor != null)
            {
                float flicker = Mathf.PerlinNoise(t * 3, 0.3f) > 0.82f ? 0.05f : 0.16f + Mathf.Sin(t * 40) * 0.01f;
                monitor.color = new Color(0.55f, 0.95f, 1f, flicker);
            }
            if (experiment != null)
                experiment.color = Color.HSVToRGB(Mathf.Repeat(t * 0.03f, 1), 0.45f, 1);
            foreach (var light in lights) light.color = Mathf.Repeat(t, 2.2f) < 0.25f ? new Color(1f, 0.35f, 0.3f) : new Color(0.45f, 0.2f, 0.2f);
            foreach (var fan in fans) fan.localRotation = Quaternion.Euler(0, 0, -t * 400);
            foreach (var glow in glows)
            {
                float strength = glow.EveningOnly ? Evening : 0.12f + Evening * 0.88f;
                if (glow.EveningOnly && glow.Art.transform.parent != null && !glow.Art.transform.parent.gameObject.activeInHierarchy) strength = 0;
                var c = glow.Art.color;
                c.a = glow.Base * strength * (0.9f + Mathf.Sin(t * 2 + glow.Phase) * 0.1f);
                glow.Art.color = c;
            }
            bool daylight = SchoolCalendar.Part != DayPart.Evening && !SchoolCalendar.Raining;
            foreach (var mote in motes)
            {
                float a = daylight ? 0.22f : 0.08f;
                mote.T.localPosition = mote.Origin + new Vector2(Mathf.Sin(t * 0.21f + mote.Phase) * 0.9f, Mathf.Sin(t * 0.13f + mote.Phase * 2) * 0.6f);
                mote.Art.color = new Color(1, 0.95f, 0.8f, a * (0.5f + 0.5f * Mathf.Sin(t * 0.7f + mote.Phase)));
            }
            foreach (var pane in panes)
            {
                bool rain = SchoolCalendar.Raining;
                foreach (var drop in pane.Drops)
                {
                    if (drop == null) continue;
                    drop.gameObject.SetActive(rain);
                    if (!rain) continue;
                    var p = (Vector2)drop.localPosition - (Vector2)pane.Sky.transform.localPosition;
                    p += new Vector2(-0.6f, -5.5f) * Time.deltaTime;
                    if (p.y < -pane.Size.y * 0.45f || p.x < -pane.Size.x * 0.48f)
                        p = new Vector2(Random.Range(-pane.Size.x, pane.Size.x) * 0.48f, pane.Size.y * 0.45f);
                    drop.localPosition = (Vector2)pane.Sky.transform.localPosition + p;
                }
                for (int i = 0; i < pane.Stars.Count; i++)
                    if (pane.Stars[i] != null) pane.Stars[i].color = new Color(1, 1, 0.85f, SchoolCalendar.Part == DayPart.Evening && !rain ? 0.5f + 0.5f * Mathf.Sin(t * 2 + i * 2) : 0);
            }
            foreach (var source in bubbles)
            {
                if (t < source.Next || !source.Parent.gameObject.activeInHierarchy) continue;
                source.Next = t + Random.Range(0.4f, 1.4f);
                Puff(source.Parent, source.Point + new Vector2(Random.Range(-0.15f, 0.15f), 0), new Color(0.85f, 1f, 0.95f, 0.8f), new Vector2(Random.Range(-0.05f, 0.05f), 0.5f), 1.4f, source.Order);
            }
            for (int i = flying.Count - 1; i >= 0; i--)
            {
                var f = flying[i];
                f.Age += Time.deltaTime;
                if (f.T == null || f.Age >= f.Life) { if (f.T != null) { f.T.gameObject.SetActive(false); spare.Push(f); } flying.RemoveAt(i); continue; }
                f.T.localPosition += (Vector3)(f.Velocity * Time.deltaTime) + new Vector3(Mathf.Sin(f.Age * 9) * 0.004f, 0, 0);
                var c = f.Art.color; c.a = Mathf.Clamp01(1 - f.Age / f.Life) * 0.85f; f.Art.color = c;
            }
        }

        /// <summary>Colours for the current part of day. Called when the part of day or weather changes.</summary>
        public void Apply(bool immediate)
        {
            var part = SchoolCalendar.Part;
            bool rain = SchoolCalendar.Raining;
            Evening = part == DayPart.Evening ? 1 : 0;
            Color grade = part == DayPart.Morning ? new Color(1f, 0.85f, 0.6f, 0.07f)
                : part == DayPart.Afternoon ? new Color(1f, 0.95f, 0.85f, 0.0f) : new Color(0.16f, 0.13f, 0.36f, 0.33f);
            if (rain) grade = Color.Lerp(grade, new Color(0.3f, 0.36f, 0.48f, Mathf.Max(grade.a, 0.16f)), 0.55f);
            if (tint != null) tint.color = grade;
            Color sky = part == DayPart.Morning ? new Color(1f, 0.86f, 0.66f) : part == DayPart.Afternoon ? new Color(0.62f, 0.82f, 1f) : new Color(0.14f, 0.12f, 0.32f);
            if (rain) sky = Color.Lerp(sky, new Color(0.5f, 0.56f, 0.64f), 0.6f);
            foreach (var pane in panes) if (pane.Sky != null) pane.Sky.color = sky;
            AmbientAudio.Instance?.SetMood(part, rain);
        }
    }

    /// <summary>
    /// Procedural ambience: room tone, rain on the windows, ventilation, morning birds in the
    /// greenhouse, soft door clicks and the personal lab radio. No audio files are needed.
    /// </summary>
    public sealed class AmbientAudio : MonoBehaviour
    {
        public static AmbientAudio Instance { get; private set; }
        public static readonly string[] Stations = { "ლაბ-ლოფაი", "წყნარი ფორტეპიანო", "ქიმიური ჩიპტიუნი" };
        private AudioSource rain, tone, birds, radio, oneShots, crowd, playful;
        private AudioClip door, splash, sparkle, bell;
        /// <summary>School rhythm in play time: a bell ends each lesson and each break.</summary>
        public const float LessonSeconds = 360, BreakSeconds = 90;
        /// <summary>True during the break between lessons (the school fills with chatter).</summary>
        public static bool Break { get; private set; }
        private float phaseEnds = LessonSeconds;
        private readonly AudioClip[] music = new AudioClip[3];
        private bool raining;
        private DayPart part;
        private LabGame game;
        private int playingStation = -1;

        public void Initialize(LabGame lab)
        {
            Instance = this; game = lab;
            rain = Loop(Rain(8f), 0);
            tone = Loop(Hum(3f), 0);
            birds = Loop(Birds(6f), 0);
            crowd = Loop(Crowd(8f), 0);
            playful = Loop(Playful(), 0);
            radio = Source(); radio.loop = true; radio.volume = 0;
            oneShots = Source(); oneShots.volume = 0.3f;
            door = DoorClip(); splash = Noise(0.35f, 0.2f, 0.9f, true); sparkle = Chime(); bell = Bell();
            Break = false;
            phaseEnds = Time.unscaledTime + LessonSeconds;
        }

        /// <summary>An activity is on screen: a lesson, mini-game, lab activity, quiz or a mission experiment.</summary>
        private bool InActivity() =>
            game.Lessons != null && game.Lessons.IsOpen || game.Games != null && game.Games.IsOpen ||
            game.Challenges != null && game.Challenges.IsOpen || game.Quiz != null && game.Quiz.IsOpen ||
            game.Personal != null && game.Personal.Activities != null && game.Personal.Activities.IsOpen ||
            game.ActOne != null && game.ActOne.Busy;

        /// <summary>Rings the bell and switches between lesson and break (also used by tests).</summary>
        public void RingBell()
        {
            Break = !Break;
            phaseEnds = Time.unscaledTime + (Break ? BreakSeconds : LessonSeconds);
            string area = game.Areas != null ? game.Areas.Current : "classroom";
            bool personal = game.Personal != null && game.Personal.Inside;
            oneShots.PlayOneShot(bell, personal ? 0.25f : area == "hallway" ? 0.9f : 0.6f);
            game.HUD?.Notify(Break ? "ზარი! დასვენებაა." : "ზარი! გაკვეთილი იწყება.");
        }

        private AudioSource Source() { var s = gameObject.AddComponent<AudioSource>(); s.playOnAwake = false; s.spatialBlend = 0; return s; }
        private AudioSource Loop(AudioClip clip, float volume) { var s = Source(); s.clip = clip; s.loop = true; s.volume = volume; s.Play(); return s; }

        public void SetMood(DayPart dayPart, bool isRaining) { part = dayPart; raining = isRaining; }
        public void Door() => oneShots.PlayOneShot(door, 0.8f);
        public void Splash() => oneShots.PlayOneShot(splash, 0.7f);
        public void Sparkle() => oneShots.PlayOneShot(sparkle, 0.6f);

        private void Update()
        {
            string area = game.Areas != null ? game.Areas.Current : "classroom";
            bool personal = game.Personal != null && game.Personal.Inside;
            bool windows = area == "classroom" || area == "hallway" || area == "greenhouse" || area == "reading" || personal;
            float calm = personal ? Mathf.Clamp01(PersonalDecor.Calm / 10f) : 0;
            if (GameSaveManager.Instance.Data.profileCompleted && Time.unscaledTime >= phaseEnds) RingBell();
            // During an activity the world steps back and the cheerful activity tune takes over.
            bool activity = InActivity();
            float duck = activity ? 0.25f : 1;
            Fade(playful, activity ? 0.14f : 0, 0.5f);
            Fade(rain, raining ? (windows ? 0.07f : 0.025f) * (area == "greenhouse" ? 1.5f : 1) * duck : 0);
            Fade(crowd, Break ? (personal ? 0.012f : area == "hallway" ? 0.16f : area == "classroom" ? 0.07f : 0.045f) * duck : 0, 0.15f);
            Fade(tone, (area == "storage" ? 0.09f : area == "classroom" ? 0.05f : 0.03f) * duck);
            Fade(birds, ((part == DayPart.Morning && !raining && (area == "greenhouse" || area == "reading")) ? 0.12f : personal ? calm * 0.05f : 0) * duck);
            var data = GameSaveManager.Instance.Data;
            bool radioPlaying = data.radioOn && data.labPlacements.Exists(p => p.itemID == "radio");
            if (radioPlaying && playingStation != data.radioStation)
            {
                int station = Mathf.Clamp(data.radioStation, 0, Stations.Length - 1);
                music[station] ??= Music(station);
                radio.clip = music[station]; radio.Play(); playingStation = data.radioStation;
            }
            if (!radioPlaying && playingStation >= 0 && radio.volume < 0.01f) { radio.Stop(); playingStation = -1; }
            Fade(radio, radioPlaying ? (personal ? 0.22f : 0.035f) * duck : 0);
        }

        /// <summary>Glides a volume toward its target at <paramref name="speed"/> per second (gentle by default).</summary>
        private static void Fade(AudioSource source, float target, float speed = 0.25f) =>
            source.volume = Mathf.MoveTowards(source.volume, target, Time.unscaledDeltaTime * speed);

        // ---------- loops that must repeat without a seam ----------

        /// <summary>Blends the last <paramref name="overlap"/> samples into the start, so a loop has no click or gap.</summary>
        private static float[] Seamless(float[] data, int overlap)
        {
            int n = data.Length - overlap;
            var loop = new float[n];
            System.Array.Copy(data, loop, n);
            for (int i = 0; i < overlap; i++)
            {
                float k = i / (float)overlap;
                loop[i] = data[i] * k + data[n + i] * (1 - k);
            }
            return loop;
        }

        /// <summary>
        /// Soft rain: deep filtered noise that swells slowly, a faint hiss and occasional quiet
        /// droplets. Replaces bright white noise that sounded harsh at volume.
        /// </summary>
        private static AudioClip Rain(float seconds)
        {
            var random = new System.Random(7);
            int overlap = Rate / 2, n = Mathf.RoundToInt(seconds * Rate) + overlap;
            var data = new float[n];
            float low1 = 0, low2 = 0, hiss = 0;
            for (int i = 0; i < n; i++)
            {
                float white = (float)(random.NextDouble() * 2 - 1);
                low1 += (white - low1) * 0.08f;
                low2 += (low1 - low2) * 0.12f;
                hiss += (white - hiss) * 0.35f;
                // A slow swell, two cycles per loop so the ends meet.
                float swell = 0.8f + 0.2f * Mathf.Sin(2 * Mathf.PI * 2 * i / (float)(n - overlap));
                data[i] = (low2 * 2.4f + hiss * 0.06f) * swell;
            }
            // About five soft droplets a second: short, quiet, high "plips".
            int drops = Mathf.RoundToInt(seconds * 5);
            for (int d = 0; d < drops; d++)
            {
                int start = random.Next(0, n - Rate / 10);
                float f = 1800 + (float)random.NextDouble() * 1600, gain = 0.025f + (float)random.NextDouble() * 0.03f;
                for (int i = 0; i < Rate / 12 && start + i < n; i++)
                {
                    float t = i / (float)Rate;
                    data[start + i] += Mathf.Sin(2 * Mathf.PI * f * t * (1 - t * 3)) * Mathf.Exp(-t * 70) * gain;
                }
            }
            return Make("წვიმა", Seamless(data, overlap));
        }

        /// <summary>
        /// Break-time chatter: several murmuring voices (a buzzing "voice" filtered soft, cut into
        /// syllables), with breath noise, overlapping like a crowded hallway.
        /// </summary>
        private static AudioClip Crowd(float seconds)
        {
            var random = new System.Random(21);
            int overlap = Rate / 2, n = Mathf.RoundToInt(seconds * Rate) + overlap;
            var data = new float[n];
            for (int v = 0; v < 9; v++)
            {
                float f0 = 110 + (float)random.NextDouble() * 150, pan = 0.6f + (float)random.NextDouble() * 0.6f;
                double phase = 0;
                float low = 0, breath = 0, envelope = 0, target = 0;
                int nextSyllable = random.Next(0, Rate / 2);
                for (int i = 0; i < n; i++)
                {
                    if (i >= nextSyllable)
                    {
                        // A syllable (on) or a short pause (off), at a talking rhythm of about 4-6 per second.
                        bool talking = random.NextDouble() < 0.72;
                        target = talking ? 0.5f + (float)random.NextDouble() * 0.5f : 0;
                        nextSyllable = i + (int)(Rate * (talking ? 0.09 + random.NextDouble() * 0.16 : 0.08 + random.NextDouble() * 0.35));
                    }
                    envelope += (target - envelope) * 0.004f;
                    float t = i / (float)Rate;
                    phase += f0 * (1 + 0.06f * Mathf.Sin(t * 5.3f + v)) / Rate;
                    float buzz = (float)(phase % 1.0) * 2 - 1;
                    low += (buzz - low) * 0.06f;
                    breath += ((float)(random.NextDouble() * 2 - 1) - breath) * 0.2f;
                    data[i] += (low * 0.9f + breath * 0.25f) * envelope * pan;
                }
            }
            float peak = 0.0001f;
            foreach (float s in data) peak = Mathf.Max(peak, Mathf.Abs(s));
            for (int i = 0; i < n; i++) data[i] = data[i] / peak * 0.6f;
            return Make("დასვენება", Seamless(data, overlap));
        }

        /// <summary>An electric school bell: a fast clapper trill on bright metal partials, about two seconds.</summary>
        private static AudioClip Bell()
        {
            int n = Mathf.RoundToInt(2.4f * Rate);
            var data = new float[n];
            float[] partials = { 950, 2280, 3610, 5100 }, gains = { 1f, 0.55f, 0.3f, 0.15f };
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float strike = Mathf.Exp(-((t * 19) % 1) * 4);
                float tail = t < 1.9f ? 1 : Mathf.Exp(-(t - 1.9f) * 8);
                float s = 0;
                for (int p = 0; p < partials.Length; p++) s += Mathf.Sin(2 * Mathf.PI * partials[p] * t) * gains[p];
                data[i] = s * (0.35f + 0.65f * strike) * tail * 0.22f;
            }
            return Make("ზარი", data);
        }

        /// <summary>
        /// A cheerful mini-game loop: 120 bpm in C major (C, G, Am, F), plucky melody, bouncy bass,
        /// soft kick and hats. Notes that ring past the end wrap to the start, so it loops cleanly.
        /// </summary>
        private static AudioClip Playful()
        {
            const float bpm = 120;
            float beat = 60f / bpm;
            int bars = 8, n = Mathf.RoundToInt(bars * 4 * beat * Rate);
            var data = new float[n];
            var random = new System.Random(12);
            int[][] chords = { new[] { 0, 4, 7 }, new[] { 7, 11, 14 }, new[] { 9, 12, 16 }, new[] { 5, 9, 12 } };
            float root = 261.63f;
            void Add(int start, int length, System.Func<float, float> wave)
            {
                for (int i = 0; i < length; i++) data[(start + i) % n] += wave(i / (float)Rate);
            }
            for (int bar = 0; bar < bars; bar++)
            {
                var chord = chords[bar % 4];
                int barStart = Mathf.RoundToInt(bar * 4 * beat * Rate);
                for (int step = 0; step < 8; step++)
                {
                    int start = barStart + Mathf.RoundToInt(step * 0.5f * beat * Rate);
                    // Melody: chord tones mostly, an octave up, with a few rests for bounce.
                    if (step != 3 && step != 7 || random.NextDouble() < 0.4)
                    {
                        int note = chord[random.Next(chord.Length)] + 12 + (random.NextDouble() < 0.2 ? 2 : 0);
                        float f = root * Mathf.Pow(2, note / 12f);
                        Add(start, Mathf.RoundToInt(0.3f * Rate), t => ((f * t) % 1 < 0.35f ? 1 : -1) * Mathf.Exp(-t * 11) * 0.07f
                            + Mathf.Sin(2 * Mathf.PI * f * t) * Mathf.Exp(-t * 8) * 0.06f);
                    }
                    // Hats on every half beat, a little louder off the beat.
                    float hat = step % 2 == 1 ? 0.035f : 0.018f;
                    Add(start, Rate / 30, t => (float)(random.NextDouble() * 2 - 1) * Mathf.Exp(-t * 160) * hat);
                }
                for (int b = 0; b < 4; b++)
                {
                    int start = barStart + Mathf.RoundToInt(b * beat * Rate);
                    // Bouncy bass on the root and fifth.
                    float bass = root / 2 * Mathf.Pow(2, (chord[0] + (b % 2 == 1 ? 7 : 0)) / 12f);
                    Add(start, Mathf.RoundToInt(0.4f * Rate), t => Mathf.Sin(2 * Mathf.PI * bass * t) * Mathf.Exp(-t * 6) * 0.16f);
                    // Soft kick on beats one and three.
                    if (b % 2 == 0) Add(start, Rate / 6, t => Mathf.Sin(2 * Mathf.PI * (50 + 70 * Mathf.Exp(-t * 30)) * t) * Mathf.Exp(-t * 18) * 0.25f);
                }
            }
            float peak = 0.0001f;
            foreach (float s in data) peak = Mathf.Max(peak, Mathf.Abs(s));
            for (int i = 0; i < n; i++) data[i] = data[i] / peak * 0.8f;
            return Make("მხიარული მელოდია", data);
        }

        // ---------- synthesis ----------
        private const int Rate = 22050;
        private static AudioClip Make(string name, float[] data) { var clip = AudioClip.Create(name, data.Length, 1, Rate, false); clip.SetData(data, 0); return clip; }

        private static AudioClip Noise(float seconds, float smooth, float gain, bool decay = false)
        {
            var random = new System.Random(4);
            int n = Mathf.RoundToInt(seconds * Rate);
            var data = new float[n];
            float last = 0;
            for (int i = 0; i < n; i++)
            {
                float white = (float)(random.NextDouble() * 2 - 1);
                last = Mathf.Lerp(last, white, 1 - smooth);
                float env = decay ? Mathf.Exp(-i / (float)Rate * 9) : 1;
                // Crossfade the loop ends so the rain has no audible seam.
                float edge = decay ? 1 : Mathf.Clamp01(Mathf.Min(i, n - i) / (Rate * 0.05f));
                data[i] = last * gain * env * edge;
            }
            return Make("ხმაური", data);
        }

        private static AudioClip Hum(float seconds)
        {
            int n = Mathf.RoundToInt(seconds * Rate);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                data[i] = (Mathf.Sin(2 * Mathf.PI * 100 * t) * 0.3f + Mathf.Sin(2 * Mathf.PI * 150 * t) * 0.12f) * 0.4f;
            }
            return Make("ოთახის გუგუნი", data);
        }

        private static AudioClip Birds(float seconds)
        {
            int n = Mathf.RoundToInt(seconds * Rate);
            var data = new float[n];
            var random = new System.Random(11);
            for (int chirp = 0; chirp < 7; chirp++)
            {
                int start = random.Next(0, n - Rate / 3);
                float f0 = 2400 + (float)random.NextDouble() * 1400;
                double phase = 0;
                for (int i = 0; i < Rate / 6; i++)
                {
                    float t = i / (float)Rate;
                    phase += (f0 + Mathf.Sin(t * 70) * 500) / Rate;
                    data[start + i] += Mathf.Sin((float)(phase * 2 * Mathf.PI)) * Mathf.Sin(Mathf.PI * i / (Rate / 6f)) * 0.3f;
                }
            }
            return Make("ჩიტები", data);
        }

        private static AudioClip DoorClip()
        {
            int n = Rate / 3;
            var data = new float[n];
            var random = new System.Random(2);
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                float click = t < 0.02f ? (float)(random.NextDouble() * 2 - 1) * (1 - t / 0.02f) : 0;
                float thud = Mathf.Sin(2 * Mathf.PI * 90 * t) * Mathf.Exp(-t * 18) * 0.6f;
                data[i] = (click * 0.5f + thud) * 0.6f;
            }
            return Make("კარი", data);
        }

        private static AudioClip Chime()
        {
            int n = Rate / 2;
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Rate;
                data[i] = (Mathf.Sin(2 * Mathf.PI * 1318 * t) + Mathf.Sin(2 * Mathf.PI * 1976 * t) * 0.5f) * Mathf.Exp(-t * 6) * 0.25f;
            }
            return Make("ზარი", data);
        }

        /// <summary>Gentle looping tunes for the radio: three moods built from simple chords.</summary>
        private static AudioClip Music(int station)
        {
            float bpm = station == 0 ? 76 : station == 1 ? 64 : 112;
            float beat = 60f / bpm;
            int bars = 8, n = Mathf.RoundToInt(bars * 4 * beat * Rate);
            var data = new float[n];
            int[][] chords = { new[] { 0, 4, 7, 11 }, new[] { 9, 12, 16, 19 }, new[] { 5, 9, 12, 16 }, new[] { 7, 11, 14, 17 } };
            float root = station == 2 ? 261.6f : 220f;
            for (int bar = 0; bar < bars; bar++)
            {
                var chord = chords[bar % 4];
                for (int step = 0; step < 8; step++)
                {
                    int note = chord[(station == 1 ? step % 4 : (step * 3) % 4)] + (step >= 4 && station != 1 ? 12 : 0);
                    float f = root * Mathf.Pow(2, note / 12f);
                    int start = Mathf.RoundToInt((bar * 4 + step * 0.5f) * beat * Rate);
                    int length = Mathf.RoundToInt(beat * (station == 1 ? 1.4f : 0.5f) * Rate);
                    for (int i = 0; i < length && start + i < n; i++)
                    {
                        float t = i / (float)Rate;
                        float wave = station == 2 ? (Mathf.Repeat(f * t, 1) < 0.5f ? 1 : -1) * 0.5f : Mathf.Sin(2 * Mathf.PI * f * t) + Mathf.Sin(4 * Mathf.PI * f * t) * 0.2f;
                        data[start + i] += wave * Mathf.Exp(-t * (station == 1 ? 2.5f : 6)) * 0.12f;
                    }
                }
                // Soft bass note per bar.
                float bass = root / 2 * Mathf.Pow(2, chord[0] / 12f);
                int s0 = Mathf.RoundToInt(bar * 4 * beat * Rate), len = Mathf.RoundToInt(4 * beat * Rate);
                for (int i = 0; i < len && s0 + i < n; i++)
                {
                    float t = i / (float)Rate;
                    data[s0 + i] += Mathf.Sin(2 * Mathf.PI * bass * t) * Mathf.Exp(-t * 0.8f) * 0.1f;
                }
            }
            if (station == 0)
            {
                var random = new System.Random(5);
                for (int i = 0; i < n; i++) data[i] += (float)(random.NextDouble() * 2 - 1) * 0.008f; // vinyl hiss
            }
            return Make(Stations[station], data);
        }
    }
}
