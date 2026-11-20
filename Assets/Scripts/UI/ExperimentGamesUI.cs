using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    /// <summary>
    /// Playful hands-on activities. Each one teaches a concept; a failure produces harmless foam,
    /// smoke, a colour burst, a small spill or wobbling glassware, explains what happened and lets
    /// the player retry straight away. Progress is never lost.
    /// </summary>
    public sealed class ExperimentGamesUI : MonoBehaviour
    {
        public static readonly string[] GameIDs = { "balance", "thermo", "analysis", "valve", "particles", "compare" };
        public static bool IsGame(string id) => Array.IndexOf(GameIDs, id) >= 0 || id == "water" || id == "distill";

        private LabGame game;
        private LabWindow window;
        private RectTransform stage;
        private Transform controls;
        private TMP_Text readout, lesson;
        private string activity, project;
        private Action tick;
        private bool finished;
        public bool IsOpen => window != null;

        public void Initialize(LabGame lab) => game = lab;

        public static string Title(string id) => id switch
        {
            "balance" => "განტოლებების დაბალანსება", "thermo" => "ტემპერატურის მართვა", "distill" => "ორთქლით გამოხდა",
            "analysis" => "უცნობი ნიმუშის ამოცნობა", "water" => "წყლის ხარისხის ტესტები", "valve" => "სარქველი და მანომეტრი",
            "particles" => "ნაწილაკების დაჭერა", "compare" => "რეაქციების შედარება", _ => "ცდა"
        };

        public void Open(string id, string projectID = null)
        {
            if (!Curriculum.ActivityOpen(id)) { LabGame.Instance?.HUD.Notify(Curriculum.LockedText + " " + Curriculum.Requirement(id)); return; }
            if (window != null) window.Close();
            activity = id; project = projectID; finished = false; tick = null;
            window = LabUI.Window(Title(id) + (projectID != null ? " • " + ResearchProjects.Get(projectID)?.Title : ""), 1060, 650);
            window.Closed = () => { window = null; tick = null; };
            var subtitle = LabUI.Text(window.Body, projectID != null ? "კვლევითი პროექტის ცდა • შეცდომა უსაფრთხოა, სცადე რამდენჯერაც გინდა" : "თავისუფალი ცდა • ივარჯიშე და ოსტატობა გაზარდე", 17, LabUI.Muted);
            LabUI.Size(subtitle.gameObject, 26);
            var split = LabUI.Row(window.Body, "ცდა", 22); LabUI.FlexibleHeight(split.gameObject);
            var panel = LabUI.Panel(split.transform, "ცდის მაგიდა", LabUI.Navy);
            LabUI.Size(panel.gameObject, 390, 470, 0); stage = panel.rectTransform;
            panel.gameObject.AddComponent<RectMask2D>();
            var column = LabUI.Column(split.transform, "მართვა", 9); LabUI.Size(column.gameObject, 390, -1, 1); controls = column.transform;
            readout = LabUI.Text(window.Body, "", 19, LabUI.Green); LabUI.Size(readout.gameObject, 30);
            readout.enableAutoSizing = true; readout.fontSizeMin = 13; readout.fontSizeMax = 19;
            lesson = LabUI.Text(window.Body, "", 16, LabUI.Ink); LabUI.Size(lesson.gameObject, 46);
            lesson.enableAutoSizing = true; lesson.fontSizeMin = 12; lesson.fontSizeMax = 16;
            LabProgress.CountStat("activity:" + Key(id));
            LabProgress.CountStat("activity:any");
            GameSaveManager.Instance.Commit();
            Launch(id);
        }

        private static string Key(string id) => id == "water" ? "analysis" : id == "distill" ? "thermo" : id;

        private void Launch(string id)
        {
            switch (id)
            {
                case "balance": Balance(); break;
                case "thermo": Thermo(false); break;
                case "distill": Thermo(true); break;
                case "analysis": Analysis(false); break;
                case "water": Analysis(true); break;
                case "valve": Valve(); break;
                case "particles": Particles(); break;
                default: Compare(); break;
            }
        }

        private void Update()
        {
            if (window == null || finished) return;
            tick?.Invoke();
        }

        // ---------------- shared building blocks ----------------

        private void ClearStage()
        {
            foreach (Transform child in stage) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            foreach (Transform child in controls) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            stage.localRotation = Quaternion.identity;
            finished = false; tick = null; lesson.text = ""; lesson.color = LabUI.Ink;
        }

        private Image Shape(string name, Vector2 point, Vector2 size, Color color, bool round = false)
        {
            var image = LabUI.Panel(stage, name, color);
            if (round) image.sprite = DecorArt.Disc();
            image.rectTransform.anchorMin = image.rectTransform.anchorMax = Vector2.one * 0.5f;
            image.rectTransform.anchoredPosition = point; image.rectTransform.sizeDelta = size;
            return image;
        }

        private TMP_Text StageText(string text, Vector2 point, Vector2 size, float fontSize, Color color)
        {
            var label = LabUI.Text(stage, text, fontSize, color);
            label.alignment = TextAlignmentOptions.Center;
            var rect = label.rectTransform;
            rect.anchorMin = rect.anchorMax = Vector2.one * 0.5f; rect.anchoredPosition = point; rect.sizeDelta = size;
            return label;
        }

        private TMP_Text Note(string text, float height = 48)
        {
            var label = LabUI.Text(controls, text, 17, LabUI.Ink); LabUI.Size(label.gameObject, height);
            label.enableAutoSizing = true; label.fontSizeMin = 12; label.fontSizeMax = 17;
            return label;
        }

        private Button Act(string text, Action pressed, bool secondary = false) =>
            secondary ? LabUI.Secondary(controls, text, pressed, 42) : LabUI.Button(controls, text, pressed, 44);

        // ---------------- results ----------------

        private void Succeed(string message, string explanation)
        {
            if (finished) return;
            finished = true; tick = null;
            readout.text = message; readout.color = LabUI.Green;
            lesson.text = explanation;
            UIFx.Sparkles(stage, LabProgress.ReputationLevel);
            AmbientAudio.Instance?.Sparkle();
            AudioManager.Instance?.Correct();
            LabProgress.AddMastery("equipment:" + Key(activity), SchoolCalendar.IsWeekend ? 2 : 1);
            LabProgress.CountStat("success:" + Key(activity));
            if (project != null) ResearchProjects.ExperimentResult(project, true);
            GameSaveManager.Instance.Commit();
            Footer(true);
        }

        private void Fail(FxKind kind, string message, string explanation)
        {
            if (finished) return;
            finished = true; tick = null;
            readout.text = message; readout.color = LabUI.Coral;
            lesson.text = "რა ვისწავლეთ: " + explanation;
            UIFx.Mishap(stage, kind);
            AudioManager.Instance?.Wrong();
            if (project != null) ResearchProjects.ExperimentResult(project, false);
            game.Mishaps.Spawn(kind);
            Footer(false);
        }

        private void Footer(bool success)
        {
            var row = LabUI.Row(controls, "შედეგი", 8); LabUI.Size(row.gameObject, 46);
            var again = LabUI.Button(row.transform, success ? "კიდევ ერთხელ" : "თავიდან ცდა", () => { ClearStage(); Launch(activity); }, 44);
            LabUI.Size(again.gameObject, 44, -1, 1);
            if (success && project != null)
            {
                var back = LabUI.Secondary(row.transform, "კვლევის დაფა", () => { window.Close(); game.Research.Open(project); }, 44);
                LabUI.Size(back.gameObject, 44, -1, 1);
            }
            else
            {
                var close = LabUI.Secondary(row.transform, "დახურვა", () => window.Close(), 44);
                LabUI.Size(close.gameObject, 44, -1, 1);
            }
        }

        // ---------------- 1. visual equation balancing ----------------

        private sealed class Species { public string Formula; public Dictionary<string, int> Atoms; public bool Product; public int Coefficient = 1; }
        private static readonly (string id, string[] left, string[] right)[] Equations =
        {
            ("water", new[] { "H2", "O2" }, new[] { "H2O" }),
            ("ammonia", new[] { "N2", "H2" }, new[] { "NH3" }),
            ("methane", new[] { "CH4", "O2" }, new[] { "CO2", "H2O" }),
            ("zinc", new[] { "Zn", "HCl" }, new[] { "ZnCl2", "H2" }),
            ("rust", new[] { "Fe", "O2" }, new[] { "Fe2O3" }),
            ("alumina", new[] { "Al", "O2" }, new[] { "Al2O3" }),
        };
        private static readonly Dictionary<string, Color> AtomColors = new Dictionary<string, Color>
        {
            { "H", new Color(0.95f, 0.95f, 0.95f) }, { "O", new Color(0.9f, 0.35f, 0.3f) }, { "N", new Color(0.4f, 0.55f, 0.95f) },
            { "C", new Color(0.35f, 0.35f, 0.38f) }, { "Zn", new Color(0.7f, 0.72f, 0.78f) }, { "Cl", new Color(0.45f, 0.85f, 0.4f) },
            { "Fe", new Color(0.78f, 0.45f, 0.25f) }, { "Al", new Color(0.8f, 0.82f, 0.88f) },
        };

        private static Dictionary<string, int> Parse(string formula)
        {
            var atoms = new Dictionary<string, int>();
            for (int i = 0; i < formula.Length;)
            {
                string symbol = formula[i].ToString(); i++;
                if (i < formula.Length && char.IsLower(formula[i])) { symbol += formula[i]; i++; }
                int count = 0;
                while (i < formula.Length && char.IsDigit(formula[i])) { count = count * 10 + (formula[i] - '0'); i++; }
                atoms[symbol] = atoms.TryGetValue(symbol, out var c) ? c + Math.Max(1, count) : Math.Max(1, count);
            }
            return atoms;
        }

        private void Balance()
        {
            ClearStage();
            var pick = project == "battery" ? Equations[3] : Equations[UnityEngine.Random.Range(0, Equations.Length)];
            var species = pick.left.Select(f => new Species { Formula = f, Atoms = Parse(f) })
                .Concat(pick.right.Select(f => new Species { Formula = f, Atoms = Parse(f), Product = true })).ToList();
            var elements = species.SelectMany(s => s.Atoms.Keys).Distinct().ToList();
            readout.text = "ყოველი ელემენტის ატომები ორივე მხარეს თანაბარი უნდა იყოს.";
            var equation = StageText("", new Vector2(0, 160), new Vector2(450, 40), 24, Color.white);
            var beam = Shape("სასწორი", new Vector2(0, 30), new Vector2(360, 8), LabUI.Gold);
            Shape("საყრდენი", new Vector2(0, -20), new Vector2(14, 90), LabUI.Gold);
            var leftBars = new Dictionary<string, Image>(); var rightBars = new Dictionary<string, Image>();
            var leftCounts = new Dictionary<string, TMP_Text>(); var rightCounts = new Dictionary<string, TMP_Text>();
            for (int i = 0; i < elements.Count; i++)
            {
                float y = -90 - i * 34;
                string element = elements[i];
                Color c = AtomColors.TryGetValue(element, out var col) ? col : Color.white;
                StageText(element, new Vector2(0, y), new Vector2(50, 30), 20, c);
                leftBars[element] = Shape("მარცხენა " + element, new Vector2(-40, y), new Vector2(10, 18), c);
                leftBars[element].rectTransform.pivot = new Vector2(1, 0.5f);
                rightBars[element] = Shape("მარჯვენა " + element, new Vector2(40, y), new Vector2(10, 18), c);
                rightBars[element].rectTransform.pivot = new Vector2(0, 0.5f);
                leftCounts[element] = StageText("", new Vector2(-205, y), new Vector2(40, 30), 17, Color.white);
                rightCounts[element] = StageText("", new Vector2(205, y), new Vector2(40, 30), 17, Color.white);
            }
            void Refresh()
            {
                equation.text = string.Join(" + ", species.Where(s => !s.Product).Select(s => (s.Coefficient > 1 ? s.Coefficient.ToString() : "") + s.Formula)) + "  →  " +
                    string.Join(" + ", species.Where(s => s.Product).Select(s => (s.Coefficient > 1 ? s.Coefficient.ToString() : "") + s.Formula));
                int tilt = 0;
                foreach (var element in elements)
                {
                    int left = species.Where(s => !s.Product).Sum(s => s.Coefficient * (s.Atoms.TryGetValue(element, out var n) ? n : 0));
                    int right = species.Where(s => s.Product).Sum(s => s.Coefficient * (s.Atoms.TryGetValue(element, out var n) ? n : 0));
                    leftBars[element].rectTransform.sizeDelta = new Vector2(Mathf.Min(150, left * 12), 18);
                    rightBars[element].rectTransform.sizeDelta = new Vector2(Mathf.Min(150, right * 12), 18);
                    leftCounts[element].text = left.ToString(); rightCounts[element].text = right.ToString();
                    leftCounts[element].color = rightCounts[element].color = left == right ? LabUI.Mint : LabUI.Coral;
                    tilt += left - right;
                }
                beam.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Clamp(tilt * 3, -14, 14));
            }
            Note("შეცვალე კოეფიციენტები. ინდექსები (პატარა რიცხვები) არ იცვლება!", 40);
            foreach (var s in species)
            {
                var row = LabUI.Row(controls, s.Formula, 8); LabUI.Size(row.gameObject, 40);
                var name = LabUI.Text(row.transform, (s.Product ? "პროდუქტი: " : "რეაგენტი: ") + s.Formula, 17); LabUI.Size(name.gameObject, 40, -1, 1);
                var minus = LabUI.Secondary(row.transform, "−", () => { s.Coefficient = Mathf.Max(1, s.Coefficient - 1); Refresh(); }, 38); LabUI.Size(minus.gameObject, 38, 48, 0);
                var plus = LabUI.Button(row.transform, "+", () => { s.Coefficient = Mathf.Min(9, s.Coefficient + 1); Refresh(); }, 38); LabUI.Size(plus.gameObject, 38, 48, 0);
            }
            Act("შემოწმება", () =>
            {
                var mismatch = elements.FirstOrDefault(e =>
                    species.Where(s => !s.Product).Sum(s => s.Coefficient * (s.Atoms.TryGetValue(e, out var n) ? n : 0)) !=
                    species.Where(s => s.Product).Sum(s => s.Coefficient * (s.Atoms.TryGetValue(e, out var n) ? n : 0)));
                int gcd = species.Select(s => s.Coefficient).Aggregate(Gcd);
                if (mismatch != null) Fail(FxKind.Foam, "ქაფი ამოვიდა! " + mismatch + "-ის ატომები არ ემთხვევა.", "ატომები რეაქციაში არ ქრება და არ ჩნდება. დაითვალე " + mismatch + " ისრის ორივე მხარეს და კოეფიციენტი შეცვალე.");
                else if (gcd > 1) Fail(FxKind.Burst, "ბალანსი სწორია, მაგრამ რიცხვები ზედმეტად დიდია.", "კოეფიციენტები ყველაზე მცირე მთელ რიცხვებამდე უნდა შემცირდეს. ყველა გაყავი " + gcd + "-ზე.");
                else Succeed("დაბალანსებულია! " + equation.text, "მასის შენახვის კანონი: ისრის ორივე მხარეს თითოეული ელემენტის ატომების რაოდენობა ერთნაირია.");
            });
            Refresh();
        }

        private static int Gcd(int a, int b) { while (b != 0) { int t = a % b; a = b; b = t; } return a; }

        // ---------------- 2. temperature control ----------------

        private void Thermo(bool distill)
        {
            ClearStage();
            float temperature = 22, power = 0, inside = 0, elapsed = 0;
            float low = distill ? 92 : 38, high = distill ? 99 : 46, goal = 10;
            readout.text = distill ? "შეინარჩუნე ორთქლის ტემპერატურა 92–99 °C-ზე 10 წამით." : "შეინარჩუნე ხსნარი 38–46 °C-ზე 10 წამით, რომ ნელა გაცივდეს.";
            Shape("თერმომეტრი", new Vector2(-170, 0), new Vector2(26, 300), new Color(0.85f, 0.88f, 0.9f));
            var band = Shape("მწვანე ზოლი", new Vector2(-170, Mathf.Lerp(-150, 150, (low + high) / 2 / 120f)), new Vector2(34, (high - low) / 120f * 300), new Color(0.4f, 0.85f, 0.6f, 0.6f));
            var mercury = Shape("ვერცხლისწყალი", new Vector2(-170, -150), new Vector2(12, 10), LabUI.Coral);
            mercury.rectTransform.pivot = new Vector2(0.5f, 0);
            var degrees = StageText("", new Vector2(-170, 170), new Vector2(120, 30), 20, Color.white);
            Shape("ჭიქა", new Vector2(60, -60), new Vector2(190, 200), new Color(0.75f, 0.88f, 0.95f, 0.35f));
            var liquid = Shape("ხსნარი", new Vector2(60, -110), new Vector2(176, 96), new Color(0.6f, 0.8f, 1f, 0.8f));
            var flame = Shape("სანთურა", new Vector2(60, -185), new Vector2(60, 20), new Color(1f, 0.6f, 0.2f, 0));
            var crystals = new List<Image>();
            var meter = LabUI.Progress(controls, 0, LabUI.Mint, 14);
            var status = Note("გამათბობელი გამორთულია.", 30);
            var row = LabUI.Row(controls, "მართვა", 8); LabUI.Size(row.gameObject, 46);
            LabUI.Size(LabUI.Button(row.transform, "− გაციება", () => power = Mathf.Max(-3, power - 1), 44).gameObject, 44, -1, 1);
            LabUI.Size(LabUI.Button(row.transform, "გაცხელება +", () => power = Mathf.Min(3, power + 1), 44).gameObject, 44, -1, 1);
            Act("გამათბობლის გამორთვა", () => power = 0, true);
            Note(distill ? "ორთქლი ქერქიდან ეთერზეთებს წაიღებს. ძალიან ცხელი ხსნარი ადუღდება და ქაფად ამოვა." : "სწრაფი გაციებისას ბევრი პაწაწინა კრისტალი გაჩნდება. ნელა და თანაბრად!", 60);
            tick = () =>
            {
                float dt = Time.unscaledDeltaTime;
                elapsed += dt;
                float drift = Mathf.PerlinNoise(elapsed * 0.6f, 3) - 0.5f;
                temperature += (power * 4.2f - (temperature - 22) * 0.035f + drift * 3) * dt;
                temperature = Mathf.Clamp(temperature, 0, 120);
                mercury.rectTransform.sizeDelta = new Vector2(12, Mathf.Lerp(4, 300, temperature / 120f));
                degrees.text = Mathf.RoundToInt(temperature) + " °C";
                flame.color = new Color(1f, 0.6f, 0.2f, Mathf.Clamp01(power / 3f));
                liquid.color = Color.Lerp(new Color(0.6f, 0.8f, 1f, 0.8f), new Color(1f, 0.75f, 0.6f, 0.85f), temperature / 110f);
                if (temperature > 70 && UnityEngine.Random.value < dt * 8) UIFx.Bubble(stage, new Vector2(60 + UnityEngine.Random.Range(-70, 70), -150));
                bool ok = temperature >= low && temperature <= high;
                band.color = ok ? new Color(0.4f, 0.95f, 0.6f, 0.8f) : new Color(0.4f, 0.85f, 0.6f, 0.45f);
                if (ok) inside += dt;
                meter.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(inside / goal), 1);
                status.text = power == 0 ? "გამათბობელი გამორთულია." : power > 0 ? "გამათბობლის სიმძლავრე: " + power : "გაგრილება: " + (-power);
                if (!distill && ok && crystals.Count < inside / goal * 14)
                {
                    var crystal = Shape("კრისტალი", new Vector2(60 + UnityEngine.Random.Range(-60, 60), -110 + UnityEngine.Random.Range(-35, 35)), Vector2.one * (6 + crystals.Count), new Color(0.95f, 0.97f, 1f, 0.95f));
                    crystal.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
                    crystals.Add(crystal);
                }
                if (temperature > high + 16) Fail(FxKind.Foam, "ბუყ-ბუყ! ხსნარი ადუღდა და ქაფი ამოვიდა.", "ზედმეტი სითბო სითხეს ადუღებს. მწვანე ზოლს რომ მიუახლოვდები, გამათბობელი შეამცირე.");
                else if (elapsed > 6 && temperature < low - 22) Fail(FxKind.Burst, "ხსნარი ძალიან სწრაფად გაცივდა: ასობით პაწაწინა კრისტალი!", "სწრაფი გაციებისას ბევრი ჩანასახი ერთდროულად ჩნდება. ნელი გაციება დიდ კრისტალებს იძლევა.");
                else if (elapsed > 50) Fail(FxKind.Smoke, "დრო ამოიწურა. ორთქლმა ოთახი აავსო.", "ტემპერატურის მართვა მოთმინებას მოითხოვს: პატარა ცვლილებები და დაკვირვება.");
                else if (inside >= goal)
                    Succeed(distill ? "გამოხდილია! ციტრუსის სასიამოვნო სურნელი დადგა." : "ხსნარი თანაბრად გაცივდა. კრისტალები იზრდება!",
                        distill ? "ორთქლი წყალზე დაბალ ტემპერატურაზე წარიტაცებს ეთერზეთებს. გაციებისას ზეთი წყალზე ცალკე ფენად რჩება." : "ნელი გაციებისას ხსნადობა თანდათან მცირდება და ნაწილაკები არსებულ კრისტალზე ლაგდება.");
            };
        }

        // ---------------- 3. unknown sample identification ----------------

        private sealed class Unknown { public string Name; public string[] Results; }

        private void Analysis(bool water)
        {
            ClearStage();
            string[] tests = water ? new[] { "pH ქაღალდი", "საპნის ქაფი", "სიმღვრივე", "ელგამტარობა" } : new[] { "ალის ფერი", "pH ქაღალდი", "წყალში ხსნადობა", "ძმრის წვეთი" };
            Unknown[] options = water ? new[]
            {
                new Unknown { Name = "წვიმის წყალი", Results = new[] { "pH ≈ 5,6 (ოდნავ მჟავე)", "ბევრი ქაფი (რბილი წყალი)", "გამჭვირვალე", "ძალიან დაბალი" } },
                new Unknown { Name = "ონკანის წყალი", Results = new[] { "pH ≈ 7,2", "საშუალო ქაფი", "გამჭვირვალე", "საშუალო" } },
                new Unknown { Name = "მინერალური წყალი", Results = new[] { "pH ≈ 7", "ცოტა ქაფი (ხისტი წყალი)", "გამჭვირვალე, ბუშტუკებით", "მაღალი" } },
                new Unknown { Name = "გუბის წყალი", Results = new[] { "pH ≈ 7,8", "საშუალო ქაფი", "მღვრიე, ნალექით", "საშუალო" } },
            } : new[]
            {
                new Unknown { Name = "სუფრის მარილი", Results = new[] { "ყვითელ-ნარინჯისფერი (ნატრიუმი)", "pH ≈ 7", "კარგად იხსნება", "შიშინი არ არის" } },
                new Unknown { Name = "საჭმლის სოდა", Results = new[] { "ყვითელ-ნარინჯისფერი (ნატრიუმი)", "pH ≈ 8–9 (ფუძე)", "კარგად იხსნება", "ძლიერად შიშინებს!" } },
                new Unknown { Name = "შაქარი", Results = new[] { "შავდება და კარამელის სუნი დგება", "pH ≈ 7", "კარგად იხსნება", "შიშინი არ არის" } },
                new Unknown { Name = "ლიმონმჟავა", Results = new[] { "შავდება, ფერი არ ჩანს", "pH ≈ 2–3 (მჟავა)", "კარგად იხსნება", "შიშინი არ არის" } },
                new Unknown { Name = "ცარცი", Results = new[] { "აგურისფერ-წითელი (კალციუმი)", "pH ≈ 9", "არ იხსნება, ილექება", "ნელა შიშინებს" } },
            };
            var secret = options[UnityEngine.Random.Range(0, options.Length)];
            readout.text = "ჩაატარე ტესტები, ჩაიწერე შედეგები და ამოიცანი ნიმუში.";
            Shape("სინჯარა", new Vector2(-120, 40), new Vector2(60, 170), new Color(0.8f, 0.9f, 1f, 0.35f));
            var sample = Shape("ნიმუში", new Vector2(-120, -10), new Vector2(50, 60), water ? new Color(0.6f, 0.8f, 0.95f, 0.85f) : new Color(0.95f, 0.95f, 0.92f));
            StageText(water ? "ნიმუში ?" : "ფხვნილი ?", new Vector2(-120, 150), new Vector2(160, 30), 20, Color.white);
            var notes = StageText("ჩანაწერები:\n", new Vector2(90, 20), new Vector2(240, 280), 15, LabUI.Paper);
            notes.alignment = TextAlignmentOptions.TopLeft;
            int used = 0;
            Note("ტესტები:", 24);
            var grid = LabUI.Row(controls, "ტესტები", 6); LabUI.Size(grid.gameObject, 44);
            var grid2 = LabUI.Row(controls, "ტესტები 2", 6); LabUI.Size(grid2.gameObject, 44);
            for (int i = 0; i < tests.Length; i++)
            {
                int index = i;
                var button = LabUI.Secondary(i < 2 ? grid.transform : grid2.transform, tests[i], () => { }, 42);
                LabUI.Size(button.gameObject, 42, -1, 1);
                button.onClick.AddListener(() =>
                {
                    notes.text += "• " + tests[index] + ": " + secret.Results[index] + "\n";
                    button.interactable = false; used++;
                    UIFx.Burst(stage, new Vector2(-120, -10), index == 0 ? new Color(1f, 0.75f, 0.3f) : index == 1 ? new Color(0.6f, 0.4f, 0.9f) : new Color(0.8f, 0.9f, 1f), 10);
                });
            }
            Note("რა არის?", 24);
            foreach (var option in options)
            {
                var choice = option;
                Act(choice.Name, () =>
                {
                    if (used == 0) { lesson.text = "ჯერ ერთი ტესტი მაინც ჩაატარე! მეცნიერი ვარაუდს მონაცემებით ამოწმებს."; lesson.color = LabUI.Coral; return; }
                    if (choice == secret)
                        Succeed("სწორია! ეს " + secret.Name + " იყო.", water ? "წყლის ნიმუშებს pH, სიხისტე, სიმღვრივე და ელგამტარობა განასხვავებს. წვიმის წყალი ოდნავ მჟავეა და თითქმის მარილებს არ შეიცავს." : "ერთი ტესტი იშვიათად კმარა. ალის ფერი ლითონს ამხელს, pH მჟავიანობას, ძმარი კარბონატებს, ხსნადობა კი სოდასა და ცარცს განასხვავებს.");
                    else
                    {
                        sample.color = new Color(1f, 0.4f, 0.7f);
                        int diff = Enumerable.Range(0, tests.Length).First(i => choice.Results[i] != secret.Results[i]);
                        Fail(FxKind.Burst, "ფერი სხვანაირი გამოვიდა! ეს " + choice.Name + " არ არის.", "ტესტი \"" + tests[diff] + "\": " + choice.Name + " აჩვენებდა \"" + choice.Results[diff] + "\", შენი ნიმუში კი \"" + secret.Results[diff] + "\".");
                    }
                }, true);
            }
        }

        // ---------------- 4. valve and gauge ----------------

        private void Valve()
        {
            ClearStage();
            bool colour = project == "colour" || project == null && UnityEngine.Random.value < 0.5f;
            float opening = 0, ph = 6.2f, rush = 0, settled = 0, pressure = 0;
            float low = 8.6f, high = 9.6f;
            readout.text = colour ? "მიიყვანე კომბოსტოს ინდიკატორი ლურჯ-მწვანე ფერამდე (pH 8,6–9,6) და დახურე სარქველი." : "მიიყვანე წნევა მწვანე ზონამდე და იქ შეაჩერე.";
            var gauge = Shape("მანომეტრი", new Vector2(-120, 80), new Vector2(170, 170), new Color(0.9f, 0.9f, 0.85f), true);
            gauge.color = new Color(0.95f, 0.95f, 0.9f, 1);
            if (colour)
            {
                StageText("მიზნის ფერი", new Vector2(110, 170), new Vector2(160, 24), 15, Color.white);
                Shape("მიზანი", new Vector2(110, 140), new Vector2(60, 26), Indicator(9.1f));
            }
            else
            {
                // The success band sits at 55–77 % of the gauge, drawn on the dial as a green tick.
                var zone = Shape("მწვანე ზონა", new Vector2(-120 + Mathf.Sin(31 * Mathf.Deg2Rad) * 72, 80 + Mathf.Cos(31 * Mathf.Deg2Rad) * 72), new Vector2(40, 12), new Color(0.3f, 0.8f, 0.5f));
                zone.rectTransform.localRotation = Quaternion.Euler(0, 0, -31);
            }
            var needle = Shape("ისარი", new Vector2(-120, 80), new Vector2(4, 70), LabUI.Coral);
            needle.rectTransform.pivot = new Vector2(0.5f, 0);
            var reading = StageText("", new Vector2(-120, -20), new Vector2(200, 30), 18, Color.white);
            Shape("მილი", new Vector2(40, 100), new Vector2(150, 12), new Color(0.6f, 0.65f, 0.7f));
            Shape("ჭიქა", new Vector2(110, -70), new Vector2(150, 190), new Color(0.8f, 0.9f, 1f, 0.3f));
            var liquid = Shape("ხსნარი", new Vector2(110, -110), new Vector2(136, 100), Color.white);
            var drop = Shape("წვეთი", new Vector2(110, 70), new Vector2(10, 14), new Color(0.9f, 0.95f, 1f, 0));
            var flow = LabUI.Progress(controls, 0, LabUI.Gold, 12);
            var status = Note("სარქველი დახურულია.", 30);
            var row = LabUI.Row(controls, "სარქველი", 8); LabUI.Size(row.gameObject, 46);
            LabUI.Size(LabUI.Secondary(row.transform, "− დახურვა", () => opening = Mathf.Max(0, opening - 0.2f), 44).gameObject, 44, -1, 1);
            LabUI.Size(LabUI.Button(row.transform, "გახსნა +", () => opening = Mathf.Min(1, opening + 0.2f), 44).gameObject, 44, -1, 1);
            Act("სრულად დახურვა", () => opening = 0, true);
            Note(colour ? "ფერი pH-ს ამხელს: წითელი → იისფერი → ლურჯი → მწვანე → ყვითელი. ბოლოსკენ ნელა!" : "ძალიან ფართოდ გახსნილი სარქველი მილს არხევს და ორთქლს უშვებს.", 60);
            tick = () =>
            {
                float dt = Time.unscaledDeltaTime;
                pressure = Mathf.MoveTowards(pressure, opening, dt * 1.5f);
                needle.rectTransform.localRotation = Quaternion.Euler(0, 0, 100 - pressure * 200);
                flow.rectTransform.anchorMax = new Vector2(pressure, 1);
                if (colour) ph += pressure * 0.55f * dt;
                else ph = 6.2f + pressure * 4.4f;
                float value = colour ? ph : ph;
                liquid.color = Indicator(value);
                drop.color = new Color(0.9f, 0.95f, 1f, pressure > 0.05f ? 0.9f : 0);
                drop.rectTransform.anchoredPosition = new Vector2(110, 70 - Mathf.Repeat(Time.unscaledTime * 160, 110));
                reading.text = colour ? "pH " + value.ToString("0.0") : "წნევა " + (pressure * 100).ToString("0") + "%";
                status.text = opening <= 0 ? "სარქველი დახურულია." : "სარქველი ღიაა: " + Mathf.RoundToInt(opening * 100) + "%";
                rush = pressure > 0.85f ? rush + dt : Mathf.Max(0, rush - dt);
                if (rush > 1.6f) { stage.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.unscaledTime * 40) * 3); }
                bool inZone = value >= low && value <= high;
                bool done = colour ? inZone && pressure < 0.02f : inZone && Mathf.Abs(pressure - opening) < 0.03f;
                settled = done ? settled + dt : 0;
                if (rush > 3f) Fail(FxKind.Spill, "მილი აკანკალდა და ცოტა სითხე დაიღვარა!", "ნაკადის სიჩქარე ზომიერი უნდა იყოს. დიდი წნევა ჭურჭელს არხევს. ნელა გახსენი და დააკვირდი მანომეტრს.");
                else if (colour && value > 11.2f) Fail(FxKind.Burst, "ყვითელი! ზედმეტი ფუძე დაემატა.", "pH 11-ზე მაღლა ინდიკატორი ყვითლდება. მიზანთან ახლოს სარქველი ნაწილობრივ დახურე და წვეთობით დაამატე.");
                else if (settled > 1.5f) Succeed(colour ? "ლამაზი ლურჯ-მწვანე ფერი! pH " + value.ToString("0.0") : "წნევა სტაბილურია მწვანე ზონაში.",
                    colour ? "ანთოციანის მოლეკულა სტრუქტურას pH-ის მიხედვით იცვლის, ამიტომ სხვადასხვა ფერის სინათლეს შთანთქავს." : "მანომეტრი წნევას აჩვენებს. ნაკადის თანდათან მართვით სისტემა უსაფრთხოდ დგება სასურველ მდგომარეობაში.");
            };
        }

        public static Color Indicator(float ph)
        {
            Color[] stops = { new Color(0.85f, 0.15f, 0.25f), new Color(0.9f, 0.35f, 0.6f), new Color(0.6f, 0.3f, 0.75f), new Color(0.45f, 0.35f, 0.8f),
                new Color(0.3f, 0.45f, 0.9f), new Color(0.2f, 0.65f, 0.75f), new Color(0.3f, 0.75f, 0.35f), new Color(0.9f, 0.85f, 0.25f) };
            float t = Mathf.InverseLerp(2, 12, ph) * (stops.Length - 1);
            int i = Mathf.Clamp(Mathf.FloorToInt(t), 0, stops.Length - 2);
            return Color.Lerp(stops[i], stops[i + 1], t - i);
        }

        // ---------------- 5. particle catching ----------------

        private sealed class Particle { public Button Button; public RectTransform Rect; public Vector2 Velocity; public bool Electron; }

        private void Particles()
        {
            ClearStage();
            int caught = 0, goal = 10; float time = 32, spawn = 0;
            readout.text = "დაიჭირე " + goal + " ელექტრონი (e-), სანამ დრო ამოიწურება. თუთიის იონებს (Zn2+) ნუ შეეხები!";
            Shape("თუთია", new Vector2(-200, 0), new Vector2(26, 330), new Color(0.7f, 0.72f, 0.78f));
            Shape("სპილენძი", new Vector2(200, 0), new Vector2(26, 330), new Color(0.85f, 0.5f, 0.3f));
            StageText("Zn", new Vector2(-200, 175), new Vector2(60, 24), 16, Color.white);
            StageText("Cu", new Vector2(200, 175), new Vector2(60, 24), 16, Color.white);
            var wire = Shape("სადენი", new Vector2(0, 175), new Vector2(380, 6), new Color(0.85f, 0.35f, 0.3f));
            var bulb = Shape("ნათურა", new Vector2(0, 175), new Vector2(70, 70), new Color(1f, 0.9f, 0.5f, 0.15f), true);
            bulb.sprite = DecorArt.Soft();
            var count = Note("დაჭერილია: 0 / " + goal, 30);
            var clock = Note("", 30);
            Note("ბატარეაში თუთია ელექტრონებს კარგავს და Zn2+ იონი ხდება. ელექტრონები გარე სადენით სპილენძისკენ მიდის.", 80);
            var particles = new List<Particle>();
            tick = () =>
            {
                float dt = Time.unscaledDeltaTime;
                time -= dt; spawn -= dt;
                clock.text = "დრო: " + Mathf.CeilToInt(Mathf.Max(0, time)) + " წმ";
                if (spawn <= 0 && particles.Count < 12)
                {
                    spawn = 0.45f;
                    bool electron = UnityEngine.Random.value < 0.62f;
                    var image = Shape(electron ? "ელექტრონი" : "იონი", new Vector2(-180, UnityEngine.Random.Range(-150, 150)), electron ? new Vector2(36, 36) : new Vector2(46, 46), electron ? new Color(0.45f, 0.7f, 1f) : new Color(0.85f, 0.45f, 0.45f), true);
                    image.raycastTarget = true;
                    var label = LabUI.Text(image.transform, electron ? "e-" : "Zn2+", electron ? 14 : 12, Color.white); label.alignment = TextAlignmentOptions.Center; LabUI.Fill(label.rectTransform);
                    var particle = new Particle { Button = image.gameObject.AddComponent<Button>(), Rect = image.rectTransform, Electron = electron, Velocity = new Vector2(UnityEngine.Random.Range(40, 90), UnityEngine.Random.Range(-60, 60)) };
                    particle.Button.onClick.AddListener(() =>
                    {
                        if (finished) return;
                        particles.Remove(particle);
                        if (particle.Electron)
                        {
                            caught++; count.text = "დაჭერილია: " + caught + " / " + goal;
                            bulb.color = new Color(1f, 0.9f, 0.5f, Mathf.Lerp(0.15f, 1, caught / (float)goal));
                            UIFx.Burst(stage, particle.Rect.anchoredPosition, new Color(0.6f, 0.85f, 1f), 6);
                        }
                        else { UIFx.Burst(stage, particle.Rect.anchoredPosition, new Color(1f, 0.5f, 0.3f), 10); lesson.text = "ეს თუთიის იონი იყო: ის ლიმონის წვენში რჩება, სადენით არ მიდის."; caught = Mathf.Max(0, caught - 1); count.text = "დაჭერილია: " + caught + " / " + goal; }
                        Destroy(particle.Rect.gameObject);
                        if (caught >= goal) Succeed("ნათურა აინთო! ელექტრონები სპილენძამდე მივიდა.", "ბატარეაში ქიმიური ენერგია ელექტრულად იქცევა: თუთია იჟანგება, ელექტრონები გარე წრედით მოძრაობს და დენს ქმნის.");
                    });
                    particles.Add(particle);
                }
                foreach (var particle in particles.ToArray())
                {
                    var p = particle.Rect.anchoredPosition + particle.Velocity * dt;
                    if (p.y > 160 || p.y < -160) particle.Velocity.y = -particle.Velocity.y;
                    particle.Velocity.y += UnityEngine.Random.Range(-60, 60) * dt;
                    particle.Rect.anchoredPosition = p;
                    if (p.x > 185) { particles.Remove(particle); Destroy(particle.Rect.gameObject); }
                }
                if (time <= 0) Fail(FxKind.Smoke, "დრო ამოიწურა. ნათურა ძლივს ბჟუტავს.", "დენისთვის ელექტრონების მუდმივი ნაკადია საჭირო. დააკვირდი: ცისფერი e- უნდა დაიჭირო, წითელი იონები კი დატოვო.");
            };
        }

        // ---------------- 6. reaction comparison ----------------

        private static readonly (string a, string b, int faster, string why)[] Rounds =
        {
            ("20 °C წყალი + ტაბლეტი", "60 °C წყალი + ტაბლეტი", 1, "ცხელ წყალში ნაწილაკები უფრო სწრაფად მოძრაობს და უფრო ხშირად ეჯახება ერთმანეთს."),
            ("განზავებული ძმარი + სოდა", "კონცენტრირებული ძმარი + სოდა", 1, "მაღალ კონცენტრაციაში მეტი ნაწილაკია და შეჯახებაც ხშირია."),
            ("ცარცის ფხვნილი + ძმარი", "ცარცის ნატეხი + ძმარი", 0, "ფხვნილს უფრო დიდი ზედაპირი აქვს, ამიტომ ძმარი მეტ ნაწილაკს ერთდროულად ეხება."),
            ("წყალბადის ზეჟანგი + კატალიზატორი", "წყალბადის ზეჟანგი კატალიზატორის გარეშე", 0, "კატალიზატორი რეაქციის ენერგეტიკულ ბარიერს ამცირებს და თავად არ იხარჯება."),
        };
        private static readonly (string a, string b, int faster, string why)[] GlowRounds =
        {
            ("ტონიკი + ლიმონის წვენი", "ტონიკი + წყალი", 0, "მჟავე გარემოში ქინინის მოლეკულა უფრო ძლიერად ფლუორესცირებს."),
            ("ტონიკი ულტრაიისფერ შუქზე", "ტონიკი ჩვეულებრივ ნათურაზე", 0, "ფლუორესცენციისთვის საჭიროა უხილავი ულტრაიისფერი სხივი, რომელსაც ქინინი შთანთქავს."),
            ("ტონიკი ულტრაიისფერ შუქზე", "მარილიანი წყალი ულტრაიისფერ შუქზე", 0, "ბრწყინავს ქინინი და არა წყალი. მარილიან წყალში ფლუორესცირებადი მოლეკულა არ არის."),
        };

        private void Compare()
        {
            ClearStage();
            bool glow = project == "indicator";
            var pool = (glow ? GlowRounds : Rounds).OrderBy(_ => UnityEngine.Random.value).Take(3).ToArray();
            int round = 0, correct = 0;
            float rateA = 0, rateB = 0, timerA = 0, timerB = 0;
            readout.text = glow ? "იწინასწარმეტყველე, რომელი ჭიქა ანათებს უფრო კაშკაშა, მერე დააკვირდი." : "იწინასწარმეტყველე, რომელი რეაქცია მიდის უფრო სწრაფად, მერე დააკვირდი.";
            var beakerA = Shape("ჭიქა ა", new Vector2(-110, -40), new Vector2(150, 200), glow ? new Color(0.2f, 0.25f, 0.5f, 0.6f) : new Color(0.8f, 0.9f, 1f, 0.35f));
            var beakerB = Shape("ჭიქა ბ", new Vector2(110, -40), new Vector2(150, 200), glow ? new Color(0.2f, 0.25f, 0.5f, 0.6f) : new Color(0.8f, 0.9f, 1f, 0.35f));
            var labelA = StageText("", new Vector2(-110, 120), new Vector2(200, 60), 15, Color.white);
            var labelB = StageText("", new Vector2(110, 120), new Vector2(200, 60), 15, Color.white);
            var score = Note("", 30);
            var row = LabUI.Row(controls, "პროგნოზი", 8); LabUI.Size(row.gameObject, 46);
            Button chooseA = null, chooseB = null;
            void ShowRound()
            {
                var r = pool[round];
                labelA.text = "ა: " + r.a; labelB.text = "ბ: " + r.b;
                rateA = rateB = 0;
                beakerA.color = beakerB.color = glow ? new Color(0.2f, 0.25f, 0.5f, 0.6f) : new Color(0.8f, 0.9f, 1f, 0.35f);
                score.text = "რაუნდი " + (round + 1) + " / " + pool.Length + " • სწორი: " + correct;
                chooseA.interactable = chooseB.interactable = true;
            }
            void Choose(int pick)
            {
                var r = pool[round];
                chooseA.interactable = chooseB.interactable = false;
                rateA = r.faster == 0 ? 10 : 2.5f; rateB = r.faster == 1 ? 10 : 2.5f;
                if (glow) { (r.faster == 0 ? beakerA : beakerB).color = new Color(0.45f, 0.85f, 1f, 0.95f); }
                bool right = pick == r.faster;
                if (right) correct++;
                else UIFx.Mishap(stage, FxKind.Burst);
                lesson.text = (right ? "სწორი პროგნოზია! " : "დააკვირდი ბუშტუკებს: პროგნოზი არ გამართლდა. ") + r.why;
                lesson.color = right ? LabUI.Green : LabUI.Coral;
                round++;
                Invoke(round < pool.Length ? nameof(NextCompare) : nameof(EndCompare), 2.6f);
                pendingCorrect = correct; pendingNext = ShowRound;
            }
            chooseA = LabUI.Button(row.transform, "ა", () => Choose(0), 44); LabUI.Size(chooseA.gameObject, 44, -1, 1);
            chooseB = LabUI.Button(row.transform, "ბ", () => Choose(1), 44); LabUI.Size(chooseB.gameObject, 44, -1, 1);
            Note("ბუშტუკები ან ნათება პროგნოზის შემდეგ გამოჩნდება. 3-დან მინიმუმ 2 სწორი პროგნოზი გამარჯვებაა.", 60);
            tick = () =>
            {
                float dt = Time.unscaledDeltaTime;
                timerA -= dt * rateA; timerB -= dt * rateB;
                if (rateA > 0 && timerA <= 0) { timerA = 1; UIFx.Bubble(stage, new Vector2(-110 + UnityEngine.Random.Range(-55, 55), -130)); }
                if (rateB > 0 && timerB <= 0) { timerB = 1; UIFx.Bubble(stage, new Vector2(110 + UnityEngine.Random.Range(-55, 55), -130)); }
            };
            comparePool = pool.Length;
            ShowRound();
        }
        private int pendingCorrect, comparePool;
        private Action pendingNext;
        private void NextCompare() { if (window != null && !finished) pendingNext?.Invoke(); }
        private void EndCompare()
        {
            if (window == null || finished) return;
            if (pendingCorrect >= 2) Succeed("კარგი დაკვირვებაა! " + pendingCorrect + " / " + comparePool + " სწორი პროგნოზი.", "რეაქციის სიჩქარეს ტემპერატურა, კონცენტრაცია, ზედაპირის ფართობი და კატალიზატორი ცვლის.");
            else Fail(FxKind.Smoke, "მხოლოდ " + pendingCorrect + " სწორი პროგნოზი. ცოტა ბოლი, ბევრი სწავლა!", "გაიხსენე: მეტი სითბო, მეტი კონცენტრაცია, მეტი ზედაპირი და კატალიზატორი რეაქციას აჩქარებს.");
        }
    }

    public enum FxKind { Foam, Burst, Smoke, Spill, Wobble }

    /// <summary>Small UI particle effects for experiments.</summary>
    public sealed class UIFx : MonoBehaviour
    {
        private Vector2 velocity; private float life, age, grow; private Image image; private RectTransform rect; private float spin;
        private static Image Piece(RectTransform stage, Vector2 point, Vector2 size, Color color, bool round)
        {
            var image = LabUI.Panel(stage, "ეფექტი", color);
            if (round) image.sprite = DecorArt.Soft();
            image.raycastTarget = false;
            image.rectTransform.anchorMin = image.rectTransform.anchorMax = Vector2.one * 0.5f;
            image.rectTransform.anchoredPosition = point; image.rectTransform.sizeDelta = size;
            return image;
        }
        private static void Spawn(RectTransform stage, Vector2 point, Vector2 size, Color color, Vector2 velocity, float life, float grow, bool round, float spin = 0)
        {
            var image = Piece(stage, point, size, color, round);
            var fx = image.gameObject.AddComponent<UIFx>();
            fx.image = image; fx.rect = image.rectTransform; fx.velocity = velocity; fx.life = life; fx.grow = grow; fx.spin = spin;
        }
        public static void Bubble(RectTransform stage, Vector2 point) => Spawn(stage, point, Vector2.one * UnityEngine.Random.Range(6, 12), new Color(0.9f, 1f, 1f, 0.7f), new Vector2(UnityEngine.Random.Range(-6, 6), 90), 1.2f, 2, true);
        public static void Burst(RectTransform stage, Vector2 point, Color color, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var dir = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(60, 180);
                Spawn(stage, point, Vector2.one * UnityEngine.Random.Range(5, 10), color, dir, 0.8f, -4, false, UnityEngine.Random.Range(-200, 200));
            }
        }
        public static void Sparkles(RectTransform stage, int reputationLevel)
        {
            for (int i = 0; i < 26; i++)
            {
                Color color = reputationLevel >= 3 ? Color.HSVToRGB(i / 26f, 0.6f, 1) : reputationLevel >= 2 ? new Color(1f, 0.85f, 0.35f) : new Color(0.7f, 1f, 0.85f);
                Spawn(stage, new Vector2(UnityEngine.Random.Range(-200, 200), UnityEngine.Random.Range(-160, 60)), Vector2.one * UnityEngine.Random.Range(5, 11), color, new Vector2(0, UnityEngine.Random.Range(40, 120)), 1.4f, -3, false, 180);
            }
        }
        public static void Mishap(RectTransform stage, FxKind kind)
        {
            switch (kind)
            {
                case FxKind.Foam:
                    for (int i = 0; i < 30; i++) Spawn(stage, new Vector2(UnityEngine.Random.Range(-120, 160), -120 + UnityEngine.Random.Range(-20, 40)), Vector2.one * UnityEngine.Random.Range(20, 46), new Color(1, 1, 1, 0.9f), new Vector2(UnityEngine.Random.Range(-30, 30), UnityEngine.Random.Range(60, 140)), 2.2f, 18, true);
                    break;
                case FxKind.Burst:
                    for (int i = 0; i < 4; i++) Burst(stage, new Vector2(UnityEngine.Random.Range(-80, 80), UnityEngine.Random.Range(-60, 60)), Color.HSVToRGB(UnityEngine.Random.value, 0.7f, 1), 12);
                    break;
                case FxKind.Smoke:
                    for (int i = 0; i < 20; i++) Spawn(stage, new Vector2(UnityEngine.Random.Range(-100, 100), -80), Vector2.one * UnityEngine.Random.Range(30, 60), new Color(0.75f, 0.75f, 0.8f, 0.6f), new Vector2(UnityEngine.Random.Range(-25, 25), UnityEngine.Random.Range(40, 90)), 2.6f, 25, true);
                    break;
                case FxKind.Spill:
                    Spawn(stage, new Vector2(60, -175), new Vector2(40, 12), new Color(0.6f, 0.8f, 1f, 0.8f), Vector2.zero, 3f, 90, true);
                    goto case FxKind.Wobble;
                case FxKind.Wobble:
                    stage.gameObject.AddComponent<Wobble>();
                    break;
            }
        }
        private void Update()
        {
            age += Time.unscaledDeltaTime;
            if (age >= life) { Destroy(gameObject); return; }
            rect.anchoredPosition += velocity * Time.unscaledDeltaTime;
            rect.sizeDelta = Vector2.Max(Vector2.one * 2, rect.sizeDelta + Vector2.one * grow * Time.unscaledDeltaTime);
            rect.localRotation = Quaternion.Euler(0, 0, rect.localEulerAngles.z + spin * Time.unscaledDeltaTime);
            var c = image.color; c.a = Mathf.Min(c.a, 1 - age / life); image.color = c;
        }
    }

    public sealed class Wobble : MonoBehaviour
    {
        private float age;
        private void Update()
        {
            age += Time.unscaledDeltaTime;
            transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(age * 35) * 4 * (1 - age / 0.9f));
            if (age > 0.9f) { transform.localRotation = Quaternion.identity; Destroy(this); }
        }
    }

    /// <summary>
    /// Harmless leftovers of failed experiments: puddles, foam and dust that wait on the floor
    /// until the player cleans them up. They are saved, so a messy lab stays messy until tidied.
    /// </summary>
    public sealed class SafeMishaps : MonoBehaviour
    {
        private LabGame game;
        private readonly List<GameObject> spills = new List<GameObject>();
        public void Initialize(LabGame lab)
        {
            game = lab;
            int pending = Mathf.Min(GameSaveManager.Instance.Data.pendingSpills, 4);
            GameSaveManager.Instance.Data.pendingSpills = 0;
            for (int i = 0; i < pending; i++) Place(game.Personal.Room, game.Personal.SpawnPosition - game.Personal.RoomOrigin + new Vector2(-3 + i * 2, -2.6f), i % 3);
        }

        public void Spawn(FxKind kind)
        {
            if (GameSaveManager.Instance.Data.pendingSpills >= 4) return;
            var player = (Vector2)game.player.transform.position + new Vector2(UnityEngine.Random.Range(-1.2f, 1.2f), -1.1f);
            bool personal = game.Personal.Inside;
            var parent = personal ? game.Personal.Room : game.Areas.InArea ? null : game.classroom;
            if (parent == null) return;
            Vector2 local = parent.InverseTransformPoint(player);
            Place(parent, local, kind == FxKind.Foam ? 1 : kind == FxKind.Burst ? 2 : 0);
        }

        private void Place(Transform parent, Vector2 local, int look)
        {
            GameSaveManager.Instance.Data.pendingSpills++;
            GameSaveManager.Instance.Commit();
            var art = LaboratoryArt.Draw(parent, "უვნებელი ავარია", local, DecorArt.Puddle(look), parent == game.classroom ? 3 : -298);
            spills.Add(art.gameObject);
            WorldInteractable.Create(art.transform, Vector2.zero, new Vector2(1.6f, 1.2f), 1.6f, () => "[E] დასუფთავება", () => Clean(art.gameObject), null, Glow.Urgent);
        }

        private void Clean(GameObject spill)
        {
            if (spill == null) return;
            spills.Remove(spill);
            var data = GameSaveManager.Instance.Data;
            data.pendingSpills = Mathf.Max(0, data.pendingSpills - 1);
            LabProgress.CountStat("clean");
            if (LabProgress.Stat("clean") >= 10) LabProgress.Collect("badge:cleaner");
            LabProgress.AddReputation(1);
            GameSaveManager.Instance.Commit();
            for (int i = 0; i < 8; i++)
                LabAtmosphere.Instance?.Puff(spill.transform.parent, spill.transform.localPosition, new Color(1f, 1f, 0.8f), UnityEngine.Random.insideUnitCircle * 1.2f, 0.6f, 12);
            AmbientAudio.Instance?.Sparkle();
            game.HUD.Notify("დასუფთავებულია! სუფთა ლაბორატორია ბედნიერი ლაბორატორიაა. +1 რეპუტაცია");
            Destroy(spill);
        }
    }
}
