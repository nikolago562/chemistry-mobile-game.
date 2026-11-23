using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    /// <summary>
    /// A build-and-discover table in the mixing bench's style: big part tiles on the left (tap to
    /// add, the small − to take one back), a live stage in the middle, the recipe so far and one big
    /// action button on the right. New finds pop a "NEW!" card and fill the table's book; failures
    /// burst on the stage instead of scolding.
    /// </summary>
    public sealed partial class KitBenchUI : MonoBehaviour
    {
        public const int Pay = 6, XP = 5;
        private LabGame game;
        private Kit kit;
        private LabWindow window;
        private RectTransform stage;
        private Transform parts;
        private TMP_Text headline, detail, recipe;
        private Button build, book;
        private NextGlow next;
        private int[] counts;
        private Coroutine anim;
        private string last = "", project;
        private Coroutine loop;
        /// <summary>The last good result, so lab stages can show what happened (null until the next build).</summary>
        private KitResult shown;

        public bool IsOpen => window != null;
        private readonly Dictionary<int, RectTransform> tiles = new Dictionary<int, RectTransform>();
        /// <summary>Where part <paramref name="i"/> and the action button are, for the tutorial pointer.</summary>
        public RectTransform PartSpot(int i) => window != null && tiles.TryGetValue(i, out var t) && t != null ? t : null;
        public RectTransform BuildSpot => window != null && build != null ? (RectTransform)build.transform : null;
        public int Count(int i) => window != null && i >= 0 && i < counts.Length ? counts[i] : 0;
        public Kit Current => window != null ? kit : null;
        public string Feedback => last;
        public void Initialize(LabGame lab) => game = lab;

        /// <summary>Opens a table; <paramref name="projectID"/> is a research project waiting for a result here.</summary>
        public void Open(Kit table, string projectID = null)
        {
            if (table == null) return;
            if (window != null) window.Close();
            kit = table; counts = new int[kit.Parts.Length]; last = ""; shown = null; project = projectID;
            var research = projectID != null ? ResearchProjects.Get(projectID) : null;
            window = LabUI.Window(kit.Title + (research != null ? " • " + research.Title : ""), 1260, 712);
            window.Closed = () => { StopAllCoroutines(); window = null; anim = null; loop = null; };

            // Top strip: the book as a wide progress bar (the table's goal) and the optional lessons.
            next = NextGlow.On(window);
            var top = LabUI.Row(window.Body, "ზედა ზოლი", 10); LabUI.Size(top.gameObject, 58);
            book = LabUI.Button(top.transform, "", Book, 58); book.GetComponent<Image>().color = new Color32(120, 86, 160, 255);
            LabUI.Size(book.gameObject, 58, -1, 1);
            var lessons = LabUI.Button(top.transform, "გაკვეთილები", () => { window.Close(); game.Lessons.OpenStation(kit.Station, kit.Title); }, 58);
            lessons.GetComponent<Image>().color = LabUI.Navy; LabUI.Size(lessons.gameObject, 58, 170, 0);
            lessons.gameObject.SetActive(!kit.Lab);

            var middle = LabUI.Row(window.Body, "მაგიდა", 14); LabUI.FlexibleHeight(middle.gameObject); middle.childForceExpandHeight = true;
            var shelf = LabUI.Object(middle.transform, "ნაწილები", typeof(GridLayoutGroup));
            var grid = shelf.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(196, 128); grid.spacing = new Vector2(8, 8);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 2;
            LabUI.Size(shelf, 418, 400, 0); parts = shelf.transform;
            // Seven-part lab tables use three narrower columns so every tile fits.
            if (kit.Parts.Length > 6) { grid.cellSize = new Vector2(128, 128); grid.constraintCount = 3; }

            var panel = LabUI.Panel(middle.transform, "სცენა", LabUI.Navy); LabUI.Size(panel.gameObject, 418, 330, 0);
            panel.gameObject.AddComponent<RectMask2D>(); stage = panel.rectTransform;

            var right = LabUI.Column(middle.transform, "აწყობა", 10); LabUI.Size(right.gameObject, -1, -1, 1);
            var ask = LabUI.Text(right.transform, kit.Ask, 20, LabUI.Ink); ask.fontStyle = FontStyles.Bold; LabUI.Size(ask.gameObject, 28);
            var card = LabUI.Panel(right.transform, "რეცეპტი", LabUI.Paper); LabUI.Size(card.gameObject, 150);
            recipe = LabUI.Text(card.transform, "", 34, LabUI.Green); recipe.alignment = TextAlignmentOptions.Center; recipe.fontStyle = FontStyles.Bold;
            recipe.enableAutoSizing = true; recipe.fontSizeMin = 16; recipe.fontSizeMax = 34; LabUI.Fill(recipe.rectTransform, 12);
            build = LabUI.Button(right.transform, kit.Action, Build, 100); build.GetComponent<Image>().color = new Color32(224, 160, 60, 255);
            LabUI.Size(build.gameObject, 100);
            var clear = LabUI.Secondary(right.transform, "გასუფთავება", Clear, 60); LabUI.Size(clear.gameObject, 60);

            var result = LabUI.Card(window.Body, "შედეგი", LabUI.Soft, 8); LabUI.Size(result.gameObject, 84);
            result.GetComponent<VerticalLayoutGroup>().spacing = 2;
            headline = LabUI.Text(result.transform, "შეეხე ნაწილებს მარცხნივ, მერე — „" + kit.Action + "“", 26, LabUI.Green); headline.fontStyle = FontStyles.Bold;
            headline.enableAutoSizing = true; headline.fontSizeMin = 18; headline.fontSizeMax = 26; LabUI.Size(headline.gameObject, 32);
            detail = LabUI.Text(result.transform, kit.Hint, 19, LabUI.Ink); detail.enableAutoSizing = true; detail.fontSizeMin = 14; detail.fontSizeMax = 19; LabUI.Size(detail.gameObject, 38);
            Refresh();
        }

        public void Add(int part, int delta)
        {
            if (window == null || part < 0 || part >= counts.Length) return;
            int before = counts[part];
            counts[part] = Mathf.Clamp(counts[part] + delta, 0, kit.Parts[part].Max);
            shown = null;
            AudioManager.Instance?.Click();
            Refresh();
            if (counts[part] <= before) return;
            Juice.Bounce(PartSpot(part));
            Juice.Fly(PartSpot(part), stage, kit.Parts[part].Color, () => { if (stage != null) Juice.Bounce(stage); }, 1 + 0.08f * Mathf.Min(counts.Sum(), 8));
        }

        public void Clear() { if (window == null) return; counts = new int[kit.Parts.Length]; shown = null; Refresh(); }

        private string ProjectTarget => project == null ? null : ResearchProjects.State(project) is ProjectProgress state ? ResearchProjects.Stage(state)?.Target : null;

        public void Build()
        {
            if (window == null) return;
            var result = kit.Judge(counts);
            bool fresh = result.Find != null && LabProgress.Collect(kit.Key(result.Find));
            if (fresh)
            {
                GameSaveManager.Instance.Data.chemCoins += Pay;
                LabProgress.AddReputation(1);
                GameSaveManager.Instance.AddXP(XP);
                // Atoms you forge light up their square in the periodic table.
                if (kit == Kits.Atoms && !GameSaveManager.Instance.Data.unlockedElements.Contains(Kits.Symbols[counts[0]])) GameSaveManager.Instance.Data.unlockedElements.Add(Kits.Symbols[counts[0]]);
                GameSaveManager.Instance.Commit();
            }
            if (result.Find != null)
            {
                LabProgress.AddMastery("equipment:" + kit.ID, SchoolCalendar.IsWeekend ? 2 : 1);
                // A waiting research project counts any find here (distilling needs the distill find).
                if (project != null && (ProjectTarget != "distill" || result.Find.ID == "distill"))
                {
                    ResearchProjects.ExperimentResult(project, true);
                    game.HUD.Notify("პროექტის ცდა შესრულდა! ნახე კვლევების დაფა.");
                    project = null;
                }
            }
            else if (result.Boom && project != null) ResearchProjects.ExperimentResult(project, false);
            last = result.Head + "\n" + result.Why + (result.Find != null && !fresh ? " (უკვე აღმოჩენილია)" : "");
            headline.text = result.Head; headline.color = result.Boom ? LabUI.Coral : LabUI.Green;
            detail.text = result.Why + (result.Find != null && !fresh ? "  (უკვე წიგნშია)" : "");
            shown = result.Boom ? null : result;
            if (kit.Lab) { if (result.Boom) StopLoop(); else Draw(); }
            if (result.Boom) { AudioManager.Instance?.Wrong(); Play(Burst()); if (kit.Lab) game.Mishaps?.Spawn(FxKind.Smoke); }
            else if (result.Find != null) { if (fresh) AmbientAudio.Instance?.Sparkle(); else AudioManager.Instance?.Correct(); Play(Glow(result.Find.Color)); }
            if (fresh) { StartCoroutine(NewCard(result.Find)); Juice.Confetti(stage, 4, 160); }
            else if (result.Find != null) Juice.Sparkle(stage, 12);
            if (fresh) Missions.Refresh(); // the first potion is a story step
            Refresh(false);
        }

        // ------------------------------------------------------------ layout

        private void Refresh(bool redraw = true)
        {
            Ui.Clear(parts);
            for (int i = 0; i < kit.Parts.Length; i++) PartTile(i);
            int found = kit.Finds.Count(f => LabProgress.Owns(kit.Key(f)));
            NextGlow.Progress(book, project != null ? "პროექტი: " + (ProjectTarget == "distill" ? "გამოხადე მარილიანი წყალი" : "აღმოაჩინე რამე ამ მაგიდაზე")
                : "წიგნი " + found + "/" + kit.Finds.Length, found, kit.Finds.Length);
            // The big button and the gold ring say what comes next: parts first, then the table's action.
            bool empty = counts.Sum() == 0;
            build.interactable = !empty;
            build.GetComponentInChildren<TMP_Text>().text = empty ? "აირჩიე ნაწილები" : kit.Action;
            next.Target = empty ? (RectTransform)parts : (RectTransform)build.transform;
            recipe.text = Recipe();
            if (redraw) Draw();
        }

        private string Recipe()
        {
            if (kit.Lab) return LabRecipe();
            if (kit == Kits.Solutions)
            {
                string brew = string.Join("\n", kit.Parts.Take(4).Select((p, i) => counts[i] == 0 ? null : p.Name + " ×" + counts[i]).Where(t => t != null));
                if (counts[4] > 0) brew += (brew == "" ? "" : "\n") + (counts[4] == 2 ? "დუღილი" : "თბილი");
                return brew == "" ? "—" : "<size=22>" + brew + "</size>";
            }
            if (kit == Kits.Potions) return counts[2] == 0 ? "უფერო" : "pH ≈ " + Kits.PotionPH(counts).ToString("0.0");
            if (kit == Kits.Atoms) return counts[0] == 0 ? "—" : Kits.Names[counts[0]] + "\n<size=22>" + counts[0] + "p  " + counts[1] + "n  " + counts[2] + "e</size>";
            var text = string.Concat(kit.Parts.Select((p, i) => counts[i] == 0 ? "" : p.Symbol + (counts[i] > 1 ? counts[i].ToString() : "")));
            return text == "" ? "—" : text;
        }

        private void PartTile(int i)
        {
            var part = kit.Parts[i];
            var tile = LabUI.Button(parts, "", () => Add(i, 1), 128);
            tiles[i] = (RectTransform)tile.transform;
            tile.GetComponent<Image>().color = counts[i] > 0 ? LabUI.Mint : LabUI.Paper;
            var ball = LabUI.Panel(tile.transform, "ნიშანი", part.Color); ball.sprite = Disc;
            bool narrow = kit.Parts.Length > 6;
            Place(ball.rectTransform, new Vector2(0.5f, 1), new Vector2(0, narrow ? -56 : -42), Vector2.one * (narrow ? 44 : 56));
            var symbol = LabUI.Text(ball.transform, part.Symbol, 20, part.Color.grayscale > 0.6f ? LabUI.Ink : Color.white);
            symbol.alignment = TextAlignmentOptions.Center; symbol.fontStyle = FontStyles.Bold; LabUI.Fill(symbol.rectTransform);
            var label = tile.GetComponentInChildren<TMP_Text>();
            label.text = part.Name; label.color = LabUI.Ink; label.alignment = TextAlignmentOptions.Bottom; label.fontSizeMin = 14; label.fontSizeMax = 20;
            label.rectTransform.offsetMax = new Vector2(-8, -78);
            var badge = LabUI.Panel(tile.transform, "რაოდენობა", counts[i] > 0 ? LabUI.Navy : new Color32(150, 156, 140, 255));
            Place(badge.rectTransform, Vector2.one, new Vector2(-26, -20), new Vector2(46, 30));
            var count = LabUI.Text(badge.transform, "×" + counts[i], 16, Color.white); count.alignment = TextAlignmentOptions.Center; count.fontStyle = FontStyles.Bold; LabUI.Fill(count.rectTransform, 2);
            if (counts[i] == 0) return;
            var minus = LabUI.Button(tile.transform, "−", () => Add(i, -1), 44);
            minus.GetComponent<Image>().color = LabUI.Coral;
            Place((RectTransform)minus.transform, new Vector2(0, 1), narrow ? new Vector2(24, -24) : new Vector2(30, -26), narrow ? new Vector2(40, 40) : new Vector2(46, 44));
        }

        private static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = anchor; rect.anchoredPosition = position; rect.sizeDelta = size;
        }

        // ------------------------------------------------------------ the stage

        private void Draw()
        {
            if (anim != null) { StopCoroutine(anim); anim = null; }
            StopLoop();
            Ui.Clear(stage);
            if (kit.Lab) { DrawLab(); return; }
            if (kit == Kits.Atoms) DrawAtom();
            else if (kit == Kits.Molecules) DrawMolecule(false);
            else if (kit == Kits.Solutions) DrawSolution();
            else DrawPotion();
        }

        private Image Ball(Vector2 at, float size, Color color, string text = "")
        {
            var image = LabUI.Panel(stage, "ბურთი", color); image.sprite = Disc;
            Place(image.rectTransform, new Vector2(0.5f, 0.5f), at, Vector2.one * size);
            if (text != "")
            {
                var label = LabUI.Text(image.transform, text, size * 0.4f, color.grayscale > 0.6f ? LabUI.Ink : Color.white);
                label.alignment = TextAlignmentOptions.Center; label.fontStyle = FontStyles.Bold; LabUI.Fill(label.rectTransform);
            }
            return image;
        }

        private void DrawAtom()
        {
            int p = counts[0], n = counts[1], e = counts[2];
            // Shells first so they sit behind the nucleus.
            for (int shell = 0; shell < 2; shell++)
            {
                var ring = LabUI.Panel(stage, "გარსი", new Color(1, 1, 1, 0.18f)); ring.sprite = RingSprite;
                Place(ring.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * (shell == 0 ? 170 : 300));
            }
            var nucleus = new List<Color>();
            for (int i = 0; i < Mathf.Max(p, n); i++) { if (i < p) nucleus.Add(Kits.Atoms.Parts[0].Color); if (i < n) nucleus.Add(Kits.Atoms.Parts[1].Color); }
            for (int i = 0; i < nucleus.Count; i++)
            {
                float angle = i * 2.4f, radius = 7 * Mathf.Sqrt(i);
                Ball(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius, 22, nucleus[i]);
            }
            var electrons = new List<Image>();
            for (int i = 0; i < e; i++) electrons.Add(Ball(Vector2.zero, 18, Kits.Atoms.Parts[2].Color));
            anim = StartCoroutine(Orbit(electrons));
        }

        private IEnumerator Orbit(List<Image> electrons)
        {
            while (true)
            {
                float t = Time.unscaledTime;
                for (int i = 0; i < electrons.Count; i++)
                {
                    if (electrons[i] == null) yield break;
                    bool inner = i < 2;
                    int count = inner ? Mathf.Min(2, electrons.Count) : electrons.Count - 2;
                    int slot = inner ? i : i - 2;
                    float angle = slot * Mathf.PI * 2 / Mathf.Max(1, count) + t * (inner ? 1.6f : 0.9f);
                    electrons[i].rectTransform.anchoredPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (inner ? 85 : 150);
                }
                yield return null;
            }
        }

        private void DrawMolecule(bool bonded)
        {
            var atoms = new List<(KitPart part, int index)>();
            for (int i = 0; i < counts.Length; i++) for (int k = 0; k < counts[i]; k++) atoms.Add((kit.Parts[i], i));
            if (atoms.Count == 0) return;
            // The atom with the most bonds sits in the middle; the rest ring around it.
            var centre = atoms.OrderByDescending(a => a.index == 1 ? 4 : a.index == 2 ? 3 : a.index == 3 ? 2 : 1).First();
            atoms.Remove(centre);
            float radius = atoms.Count <= 4 ? 95 : 140;
            var spots = atoms.Select((a, i) => new Vector2(Mathf.Cos(i * Mathf.PI * 2 / atoms.Count + 0.4f), Mathf.Sin(i * Mathf.PI * 2 / atoms.Count + 0.4f)) * radius).ToList();
            if (bonded) foreach (var at in spots) Line(Vector2.zero, at);
            Ball(Vector2.zero, 64, centre.part.Color, centre.part.Symbol);
            for (int i = 0; i < atoms.Count; i++) Ball(spots[i], atoms.Count > 8 ? 38 : 48, atoms[i].part.Color, atoms[i].part.Symbol);
        }

        private void Line(Vector2 a, Vector2 b)
        {
            var bar = LabUI.Panel(stage, "ბმა", new Color(0.85f, 0.88f, 0.9f, 0.9f)); bar.sprite = null;
            var rect = bar.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = (a + b) / 2; rect.sizeDelta = new Vector2((b - a).magnitude, 8);
            rect.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg);
        }

        private Image potion;
        private void DrawPotion()
        {
            Ball(new Vector2(0, -40), 250, new Color(1, 1, 1, 0.12f));
            Color colour = counts[2] == 0 ? new Color(0.85f, 0.92f, 0.95f, 0.8f) : Kits.Potions.Judge(counts).Find.Color;
            potion = Ball(new Vector2(0, -40), 220, counts[2] == 0 ? colour : Color.Lerp(colour, Color.white, 0.35f));
            var neck = LabUI.Panel(stage, "ყელი", new Color(1, 1, 1, 0.15f));
            Place(neck.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 110), new Vector2(70, 90));
        }

        /// <summary>A beaker: liquid rises with each cup, takes the result's colour, keeps undissolved powder on the bottom; flames below, steam above.</summary>
        private void DrawSolution()
        {
            int w = counts[0], s = counts[1], g = counts[2], c = counts[3], h = counts[4];
            var result = kit.Judge(counts);
            for (int i = 0; i < h * 3; i++)
                Ball(new Vector2(-40 + i * 16, -178 + (i % 2) * 6), 20 + h * 5, i % 2 == 0 ? new Color32(240, 130, 50, 255) : new Color32(250, 210, 80, 255));
            var glass = LabUI.Panel(stage, "ჭიქა", new Color(1, 1, 1, 0.14f));
            Place(glass.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(230, 280));
            float height = w == 0 ? 0 : 50 + w * 52;
            Color liquid = result.Find != null ? result.Find.Color : w > 0 ? new Color32(120, 190, 240, 255) : Color.clear;
            if (height > 0)
            {
                var fill = LabUI.Panel(stage, "სითხე", new Color(liquid.r, liquid.g, liquid.b, 0.85f));
                Place(fill.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -160 + height / 2), new Vector2(214, height));
            }
            // Whatever the water can't hold (or all of it, with no water) lies on the bottom.
            int pile = w == 0 ? s + g + c : Mathf.Max(0, s - Kits.Capacity(1, w, h)) + Mathf.Max(0, g - Kits.Capacity(2, w, h)) + Mathf.Max(0, c - Kits.Capacity(3, w, h));
            Color powder = c > 0 ? new Color32(60, 130, 230, 255) : g > 0 && w == 0 && h == 2 ? new Color32(150, 80, 30, 255) : Color.white;
            for (int i = 0; i < Mathf.Min(pile, 12); i++)
            {
                var grain = LabUI.Panel(stage, "კრისტალი", powder);
                Place(grain.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-88 + (i * 37) % 176, -150 + (i / 5) * 14), new Vector2(18, 14));
            }
            if (h == 2 && w > 0)
                for (int i = 0; i < 3; i++) Ball(new Vector2(-50 + i * 50, 150 + (i % 2) * 18), 46, new Color(1, 1, 1, 0.35f));
        }

        private void Play(IEnumerator routine) { if (anim != null) StopCoroutine(anim); anim = StartCoroutine(routine); }

        /// <summary>A good result: the molecule snaps its bonds / the potion brightens, and the stage glows.</summary>
        private IEnumerator Glow(Color color)
        {
            if (kit == Kits.Molecules) { Ui.Clear(stage); DrawMolecule(true); }
            if (kit == Kits.Potions) { Ui.Clear(stage); DrawPotion(); }
            if (kit == Kits.Solutions) { Ui.Clear(stage); DrawSolution(); }
            var halo = LabUI.Panel(stage, "ნათება", color); halo.sprite = Disc; halo.transform.SetAsFirstSibling();
            Place(halo.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * 80);
            for (float t = 0; t < 1.2f; t += Time.unscaledDeltaTime)
            {
                if (halo == null) yield break;
                halo.rectTransform.sizeDelta = Vector2.one * Mathf.Lerp(80, 420, t / 1.2f);
                halo.color = new Color(color.r, color.g, color.b, 0.55f * (1 - t / 1.2f));
                if (potion != null) potion.color = Color.Lerp(color, Color.white, 0.35f * (1 - Mathf.Sin(t * 8) * 0.5f));
                yield return null;
            }
            if (halo != null) Destroy(halo.gameObject);
            anim = null;
        }

        /// <summary>A failure: the stage shakes, everything flies apart in a puff, then the parts come back.</summary>
        private IEnumerator Burst()
        {
            var bits = stage.Cast<Transform>().Select(t => (RectTransform)t).ToList();
            var from = bits.Select(b => b.anchoredPosition).ToList();
            var away = bits.Select(_ => Random.insideUnitCircle.normalized * Random.Range(180f, 320f)).ToList();
            var puff = LabUI.Panel(stage, "ბოლი", new Color(0.9f, 0.6f, 0.9f, 0.6f)); puff.sprite = Disc;
            Place(puff.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * 60);
            for (float t = 0; t < 0.9f; t += Time.unscaledDeltaTime)
            {
                float k = Mathf.Clamp01(t / 0.6f);
                for (int i = 0; i < bits.Count; i++) if (bits[i] != null) bits[i].anchoredPosition = from[i] + away[i] * Mathf.SmoothStep(0, 1, k);
                if (puff != null) { puff.rectTransform.sizeDelta = Vector2.one * Mathf.Lerp(60, 380, k); puff.color = new Color(0.9f, 0.6f, 0.9f, 0.6f * (1 - k)); }
                yield return null;
            }
            anim = null;
            Draw();
        }

        // ------------------------------------------------------------ book and "NEW!" card

        public void Book()
        {
            var book = LabUI.Window(kit.Title + " • წიგნი", 900, 620);
            Ui.Line(book.Body, "აღმოჩენილია " + kit.Finds.Count(f => LabProgress.Owns(kit.Key(f))) + " / " + kit.Finds.Length + ". სახელი უკვე იცი — აწყობა შენ მოიფიქრე!", 17, LabUI.Muted, 28);
            var scroll = LabUI.Scroll(book.Body);
            foreach (var f in kit.Finds)
            {
                bool owned = LabProgress.Owns(kit.Key(f));
                var card = LabUI.Card(scroll, f.ID, owned ? LabUI.Paper : new Color(0.86f, 0.86f, 0.82f), 10);
                var row = LabUI.Row(card.transform, "ჩანაწერი", 12); LabUI.Size(row.gameObject, 52);
                var swatch = LabUI.Panel(row.transform, "ფერი", owned ? f.Color : new Color(0.7f, 0.7f, 0.7f)); swatch.sprite = Disc; LabUI.Size(swatch.gameObject, 48, 48, 0);
                var text = LabUI.Text(row.transform, owned ? "<b>" + f.Title + "</b>  •  " + f.Formula + "\n<size=14>" + f.Why + "</size>" : "<b>" + f.Title + " — ???</b>\n<size=14>ჯერ არ აგიწყვია.</size>", 17, owned ? LabUI.Ink : LabUI.Muted);
                text.enableAutoSizing = true; text.fontSizeMin = 11; text.fontSizeMax = 17;
                LabUI.Size(text.gameObject, 52, -1, 1);
            }
        }

        private IEnumerator NewCard(KitFind find)
        {
            var card = LabUI.Panel(window.Card, "ახალი აღმოჩენა", LabUI.Gold);
            card.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            Place(card.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(460, 210));
            var group = card.gameObject.AddComponent<CanvasGroup>(); group.blocksRaycasts = false;
            var swatch = LabUI.Panel(card.transform, "ფერი", find.Color); swatch.sprite = Disc;
            Place(swatch.rectTransform, new Vector2(0, 0.5f), new Vector2(70, 0), new Vector2(92, 92));
            var text = LabUI.Text(card.transform, "<size=30><b>ახალი აღმოჩენა!</b></size>\n<b>" + find.Title + "</b>\n" + find.Formula + "\n<size=17>+" + Pay + " ქიმქოინი  •  +" + XP + " XP</size>", 21, LabUI.Ink);
            text.enableAutoSizing = true; text.fontSizeMin = 12; text.fontSizeMax = 21;
            LabUI.Fill(text.rectTransform, 14); text.rectTransform.offsetMin = new Vector2(132, 14);
            for (float t = 0; t < 2.6f && card != null; t += Time.unscaledDeltaTime)
            {
                float u = Mathf.Clamp01(t / 0.3f);
                card.transform.localScale = Vector3.one * (t < 0.3f ? Mathf.LerpUnclamped(0.6f, 1, 1 + 2.7f * Mathf.Pow(u - 1, 3) + 1.7f * Mathf.Pow(u - 1, 2)) : 1);
                group.alpha = t > 2.2f ? 1 - (t - 2.2f) / 0.4f : 1;
                yield return null;
            }
            if (card != null) Destroy(card.gameObject);
        }

        private static Sprite disc, ring;
        private static Sprite Disc => disc ??= PixelArt.Make(32, 32, (x, y) => Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f)) < 15.5f ? Color.white : Color.clear);
        private static Sprite RingSprite => ring ??= PixelArt.Make(64, 64, (x, y) =>
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f));
            return d < 31.5f && d > 30 ? Color.white : Color.clear;
        });
    }
}
