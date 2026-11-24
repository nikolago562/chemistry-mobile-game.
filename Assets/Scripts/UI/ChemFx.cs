using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    /// <summary>
    /// Chemistry you can watch: ball-and-stick molecules (CPK-style colours) built from a formula,
    /// a reaction strip where the reactants' atoms travel across the arrow and regroup into the
    /// products (atoms are conserved), an element reveal for newly forged atoms (periodic tile and
    /// a simple shell model) and a journal stamp when a research project is handed in.
    /// The molecule pictures are simplified school models, not true bond geometry.
    /// </summary>
    public static class ChemFx
    {
        // ------------------------------------------------------------ formulas

        private static readonly Dictionary<string, Color32> colours = new Dictionary<string, Color32>
        {
            ["H"] = new Color32(240, 240, 246, 255), ["O"] = new Color32(232, 84, 80, 255), ["C"] = new Color32(76, 78, 90, 255),
            ["N"] = new Color32(92, 124, 226, 255), ["Cl"] = new Color32(112, 204, 108, 255), ["Na"] = new Color32(170, 120, 226, 255),
            ["Ca"] = new Color32(150, 196, 150, 255), ["Cu"] = new Color32(222, 132, 80, 255), ["Fe"] = new Color32(204, 112, 70, 255),
            ["Zn"] = new Color32(150, 162, 186, 255), ["S"] = new Color32(242, 212, 80, 255), ["K"] = new Color32(176, 96, 210, 255),
            ["Mg"] = new Color32(130, 206, 120, 255), ["Al"] = new Color32(190, 176, 180, 255), ["He"] = new Color32(220, 250, 250, 255),
            ["Li"] = new Color32(200, 130, 255, 255), ["Be"] = new Color32(196, 255, 120, 255), ["B"] = new Color32(255, 182, 182, 255),
            ["F"] = new Color32(150, 230, 100, 255), ["Ne"] = new Color32(176, 226, 245, 255), ["Ag"] = new Color32(200, 200, 210, 255),
        };
        private static readonly HashSet<string> metals = new HashSet<string> { "Na", "K", "Ca", "Mg", "Al", "Fe", "Cu", "Zn", "Ag", "Li", "Be" };
        public static Color ColourOf(string element) => colours.TryGetValue(element, out var c) ? (Color)c : new Color(0.8f, 0.8f, 0.8f);

        /// <summary>The atoms in a formula ("Cu(OH)2" → Cu O H O H), or null if it isn't a plain formula.</summary>
        public static List<string> Atoms(string formula)
        {
            int i = 0;
            var atoms = Group(formula, ref i);
            return atoms != null && i == formula.Length && atoms.Count > 0 ? atoms : null;
        }
        private static List<string> Group(string s, ref int i)
        {
            var list = new List<string>();
            while (i < s.Length && s[i] != ')')
            {
                List<string> part;
                if (s[i] == '(')
                {
                    i++; part = Group(s, ref i);
                    if (part == null || i >= s.Length || s[i] != ')') return null;
                    i++;
                }
                else if (char.IsUpper(s[i]) && s[i] < 128)
                {
                    string symbol = s[i++].ToString();
                    if (i < s.Length && char.IsLower(s[i]) && s[i] < 128) symbol += s[i++];
                    if (!colours.ContainsKey(symbol)) return null;
                    part = new List<string> { symbol };
                }
                else return null;
                int n = 0;
                while (i < s.Length && char.IsDigit(s[i])) n = n * 10 + (s[i++] - '0');
                for (int k = 0; k < Mathf.Max(1, n); k++) list.AddRange(part);
            }
            return list;
        }

        /// <summary>Formula text with subscript counts (coefficients stay full size).</summary>
        public static string Pretty(string text)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i], prev = i > 0 ? text[i - 1] : ' ';
                bool sub = char.IsDigit(c) && (prev < 128 && char.IsLetter(prev) || prev == ')' || char.IsDigit(prev) && sb.ToString().EndsWith("</sub>"));
                if (sub) { if (sb.ToString().EndsWith("</sub>")) sb.Length -= 6; else sb.Append("<sub>"); sb.Append(c).Append("</sub>"); }
                else sb.Append(c);
            }
            return sb.ToString();
        }

        private sealed class Token { public string Label; public List<string> Atoms; public int Count = 1; public bool Gas; }

        private static List<Token> Tokens(string side)
        {
            var list = new List<Token>();
            foreach (string raw in side.Split('+'))
            {
                string t = raw.Trim();
                if (t.Length == 0 || t.Trim('.', ' ') == "") continue;
                var token = new Token { Gas = t.Contains("↑") };
                t = t.Replace("↑", "").Replace("(aq)", "").Replace("(s)", "").Replace("(g)", "").Trim();
                int n = 0, k = 0;
                while (k < t.Length && char.IsDigit(t[k])) n = n * 10 + (t[k++] - '0');
                string formula = t.Substring(k).Trim();
                token.Count = Mathf.Clamp(n == 0 ? 1 : n, 1, 2);
                token.Atoms = Atoms(formula);
                token.Label = token.Atoms != null ? Pretty(formula) : formula;
                list.Add(token);
            }
            return list;
        }

        // ------------------------------------------------------------ drawing

        private static Sprite ball, rays, ring;
        /// <summary>A shaded ball (white; tinted per element) with a dark rim and a shine.</summary>
        private static Sprite Ball => ball ??= DecorArt.Paint("chem:ball", 16, 16, p =>
        {
            for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(7.5f, 7.5f));
                if (d > 7.6f) continue;
                float shade = d > 6.5f ? 0.4f : Vector2.Distance(new Vector2(x, y), new Vector2(5.5f, 10)) < 2.2f ? 1.25f : 1 - Mathf.Clamp01((x - y + 4) / 22f) * 0.25f;
                p.Dot(x, y, shade > 1 ? Color.white : new Color(shade, shade, shade, 1));
            }
        }, 16, new Vector2(0.5f, 0.5f));
        private static Sprite Rays => rays ??= DecorArt.Paint("chem:rays", 64, 64, p =>
        {
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
            {
                float dx = x - 31.5f, dy = y - 31.5f, d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d > 31.5f) continue;
                float a = Mathf.Atan2(dy, dx) * 6 / Mathf.PI;
                if (Mathf.Repeat(a, 1) < 0.5f) p.Dot(x, y, new Color(1, 1, 1, (1 - d / 32f) * 0.8f));
            }
        }, 16, new Vector2(0.5f, 0.5f));
        private static Sprite Ring => ring ??= DecorArt.Paint("chem:ring", 64, 64, p =>
        {
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f));
                if (d < 31.6f && d > 30.2f) p.Dot(x, y, Color.white);
            }
        }, 16, new Vector2(0.5f, 0.5f));

        private static Image Img(Transform parent, string name, Sprite sprite, Color color, Vector2 at, Vector2 size)
        {
            var image = LabUI.Object(parent, name, typeof(Image)).GetComponent<Image>();
            image.sprite = sprite; image.color = color; image.raycastTarget = false;
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = at; rect.sizeDelta = size;
            return image;
        }
        private static TMP_Text Label(Transform parent, string text, float size, Color color, Vector2 at, Vector2 box)
        {
            var label = LabUI.Text(parent, text, size, color);
            label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
            var rect = label.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = at; rect.sizeDelta = box;
            return label;
        }
        private static float BallSize(string element) => element == "H" ? 22 : metals.Contains(element) ? 38 : 31;

        /// <summary>Simple 2D positions for a molecule: the first heavy atom in the middle, the rest around it.</summary>
        private static List<Vector2> Shape(List<string> atoms, float scale)
        {
            var shown = atoms.Take(9).ToList();
            var points = new List<Vector2>(new Vector2[shown.Count]);
            int centre = Mathf.Max(0, shown.FindIndex(a => a != "H"));
            var around = Enumerable.Range(0, shown.Count).Where(i => i != centre).ToList();
            for (int k = 0; k < around.Count; k++)
            {
                float angle = around.Count == 1 ? 0 : around.Count == 2 ? (k == 0 ? 215 : 325) : 90 + k * 360f / Mathf.Min(around.Count, 6) + (k >= 6 ? 30 : 0);
                float r = (shown[around[k]] == "H" ? 27 : 34) + (k >= 6 ? 24 : 0);
                if (metals.Contains(shown[centre]) || metals.Contains(shown[around[k]])) r += 4;
                points[around[k]] = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * r * scale;
            }
            var mean = points.Aggregate(Vector2.zero, (a, b) => a + b) / points.Count;
            for (int i = 0; i < points.Count; i++) points[i] -= mean;
            return points;
        }

        /// <summary>Draws a still ball-and-stick model of <paramref name="formula"/>; false if it can't be parsed.</summary>
        public static bool Molecule(Transform parent, string formula, Vector2 at, float scale = 1)
        {
            var atoms = Atoms(formula);
            if (atoms == null) return false;
            var shown = atoms.Take(9).ToList();
            var points = Shape(atoms, scale);
            int centre = Mathf.Max(0, shown.FindIndex(a => a != "H"));
            for (int i = 0; i < shown.Count; i++) if (i != centre) Stick(parent, at + points[centre], at + points[i]);
            for (int i = 0; i < shown.Count; i++) Img(parent, shown[i], Ball, ColourOf(shown[i]), at + points[i], Vector2.one * BallSize(shown[i]) * scale);
            return true;
        }

        private static Image Stick(Transform parent, Vector2 a, Vector2 b)
        {
            var stick = Img(parent, "ბმა", DecorArt.Square(), new Color(0.86f, 0.87f, 0.9f), (a + b) / 2, new Vector2((b - a).magnitude, 5));
            stick.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg);
            return stick;
        }

        // ------------------------------------------------------------ the reaction strip

        private sealed class Piece { public List<string> Elements = new List<string>(); public List<Image> Balls = new List<Image>(); public List<Vector2> Points = new List<Vector2>(); public List<Image> Sticks = new List<Image>(); public List<Vector2> StickBase = new List<Vector2>(); public Vector2 Centre; public TMP_Text Label; public Image Chip; public bool Gas; }

        /// <summary>
        /// Plays the reaction over <paramref name="card"/>: the reactant molecules appear left of the arrow,
        /// shake, then every atom flies across and regroups into the products on the right. Non-blocking.
        /// </summary>
        public static IEnumerator Reaction(RectTransform card, string formula)
        {
            if (card == null || string.IsNullOrEmpty(formula) || !formula.Contains("→")) yield break;
            var sides = formula.Split('→');
            var left = Tokens(sides[0]); var right = Tokens(sides[1]);
            if (left.Count == 0 || right.Count == 0) yield break;

            var panel = LabUI.Panel(card, "რეაქცია", new Color(0.12f, 0.19f, 0.16f, 0.95f));
            panel.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var root = panel.rectTransform;
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = new Vector2(0, 40); root.sizeDelta = new Vector2(760, 260);
            var group = panel.gameObject.AddComponent<CanvasGroup>(); group.blocksRaycasts = false; group.interactable = false;
            LabUI.Shadow(panel);
            Label(root, "რეაქცია", 17, LabUI.Gold, new Vector2(0, 108), new Vector2(300, 28));
            var arrow = Label(root, "→", 46, LabUI.Gold, new Vector2(0, 20), new Vector2(80, 60));
            var equation = Label(root, Pretty(formula), 19, LabUI.Paper, new Vector2(0, -100), new Vector2(720, 34));
            equation.enableAutoSizing = true; equation.fontSizeMin = 12; equation.fontSizeMax = 19;

            var reactants = Place(root, left, -1, true);
            var products = Place(root, right, 1, false);

            // Each product atom takes a matching reactant atom if one is free; the rest appear or fade.
            var free = reactants.SelectMany(m => m.Balls.Select((b, i) => (b, e: m.Elements[i]))).ToList();
            var moves = new List<(Image ball, Vector2 from, Vector2 to)>();
            var fresh = new List<Image>();
            foreach (var m in products)
                for (int i = 0; i < m.Elements.Count; i++)
                {
                    int k = free.FindIndex(f => f.e == m.Elements[i]);
                    Vector2 to = m.Centre + m.Points[i];
                    if (k >= 0) { moves.Add((free[k].b, free[k].b.rectTransform.anchoredPosition, to)); m.Balls.Add(free[k].b); free.RemoveAt(k); }
                    else { var b = Img(root, m.Elements[i], Ball, ColourOf(m.Elements[i]), to, Vector2.one * BallSize(m.Elements[i])); b.transform.localScale = Vector3.zero; fresh.Add(b); m.Balls.Add(b); }
                }
            var leftovers = free.Select(f => f.b).ToList();

            float t = 0;
            bool landed = false;
            while (t < 3.1f)
            {
                if (root == null) yield break;
                t += Time.unscaledDeltaTime;
                // 1. reactants pop in, one after another
                for (int i = 0; i < reactants.Count; i++)
                {
                    float u = Mathf.Clamp01((t - i * 0.08f) / 0.3f);
                    float s = u < 1 ? Mathf.LerpUnclamped(0, 1, 1 + 2.7f * Mathf.Pow(u - 1, 3) + 1.7f * Mathf.Pow(u - 1, 2)) : 1;
                    Show(reactants[i], s, t < 1.0f ? 1 : 0);
                }
                // 2. they jiggle as the reaction warms up
                if (t > 0.45f && t < 1.0f)
                {
                    float amp = (t - 0.45f) * 6;
                    foreach (var m in reactants) for (int i = 0; i < m.Balls.Count; i++)
                        m.Balls[i].rectTransform.anchoredPosition = m.Centre + m.Points[i] + new Vector2(Mathf.Sin(t * 60 + i), Mathf.Cos(t * 53 + i)) * amp;
                }
                // 3. atoms fly across the arrow and regroup
                if (t >= 1.0f)
                {
                    float u = Mathf.SmoothStep(0, 1, Mathf.Clamp01((t - 1.0f) / 0.7f));
                    foreach (var (b, from, to) in moves)
                        if (b != null) b.rectTransform.anchoredPosition = Vector2.Lerp(from, to, u) + new Vector2(0, Mathf.Sin(u * Mathf.PI) * 46);
                    foreach (var b in leftovers) if (b != null) b.transform.localScale = Vector3.one * (1 - u);
                    foreach (var b in fresh) if (b != null) b.transform.localScale = Vector3.one * Mathf.Clamp01((u - 0.5f) * 2);
                    foreach (var m in reactants) { if (m.Label != null) m.Label.alpha = 1 - u; if (m.Chip != null) m.Chip.color = new Color(1, 1, 1, 1 - u); }
                    arrow.transform.localScale = Vector3.one * (1 + Mathf.Sin(u * Mathf.PI) * 0.35f);
                }
                // 4. the products bond, are named, and gases float away
                if (t >= 1.7f)
                {
                    if (!landed)
                    {
                        landed = true;
                        foreach (var m in products)
                        {
                            if (m.Balls.Count > 1) { int c = Mathf.Max(0, m.Elements.FindIndex(e => e != "H")); for (int i = 0; i < m.Balls.Count; i++) if (i != c) { var s = Stick(root, m.Centre + m.Points[c], m.Centre + m.Points[i]); s.transform.SetSiblingIndex(1); m.Sticks.Add(s); m.StickBase.Add(s.rectTransform.anchoredPosition); } }
                            UIFx.Burst(root, m.Centre, LabUI.Gold, 6);
                        }
                        AmbientAudio.Instance?.Drop(1.3f);
                    }
                    float v = Mathf.Clamp01((t - 1.7f) / 0.3f);
                    foreach (var m in products)
                    {
                        if (m.Label != null) m.Label.alpha = v;
                        if (m.Chip != null) m.Chip.color = new Color(1, 1, 1, v);
                        foreach (var s in m.Sticks) s.color = new Color(0.86f, 0.87f, 0.9f, v);
                        if (!m.Gas) continue;
                        float rise = Mathf.Clamp01((t - 2.0f) / 1.1f) * 30;
                        for (int i = 0; i < m.Balls.Count; i++) if (m.Balls[i] != null) m.Balls[i].rectTransform.anchoredPosition = m.Centre + m.Points[i] + new Vector2(Mathf.Sin(t * 4) * 3, rise);
                        for (int i = 0; i < m.Sticks.Count; i++) m.Sticks[i].rectTransform.anchoredPosition = m.StickBase[i] + new Vector2(Mathf.Sin(t * 4) * 3, rise);
                        if (m.Label != null) m.Label.rectTransform.anchoredPosition = m.Centre + new Vector2(0, -52 + rise);
                    }
                }
                group.alpha = t > 2.75f ? 1 - (t - 2.75f) / 0.35f : 1;
                yield return null;
            }
            if (root != null) Object.Destroy(root.gameObject);
        }

        /// <summary>Lays out one side of the equation: up to four molecules spread across its half of the strip.</summary>
        private static List<Piece> Place(RectTransform root, List<Token> tokens, int side, bool visible)
        {
            var pieces = new List<Piece>();
            var copies = tokens.SelectMany(tk => Enumerable.Repeat(tk, tk.Count)).Take(4).ToList();
            for (int i = 0; i < copies.Count; i++)
            {
                var tk = copies[i];
                var centre = new Vector2(side * (70 + (i + 0.5f) * 290f / copies.Count), 22);
                var piece = new Piece { Centre = centre, Gas = tk.Gas };
                if (tk.Atoms != null)
                {
                    piece.Elements = tk.Atoms.Take(9).ToList();
                    piece.Points = Shape(tk.Atoms, copies.Count > 2 ? 0.8f : 1);
                    if (visible)
                    {
                        int c = Mathf.Max(0, piece.Elements.FindIndex(e => e != "H"));
                        for (int k = 0; k < piece.Elements.Count; k++) if (k != c) piece.Sticks.Add(Stick(root, centre + piece.Points[c], centre + piece.Points[k]));
                        for (int k = 0; k < piece.Elements.Count; k++)
                            piece.Balls.Add(Img(root, piece.Elements[k], Ball, ColourOf(piece.Elements[k]), centre + piece.Points[k], Vector2.one * BallSize(piece.Elements[k]) * (copies.Count > 2 ? 0.85f : 1)));
                    }
                }
                else
                {   // a word, not a formula ("მარილი"): a little paper chip
                    piece.Chip = LabUI.Panel(root, "სახელი", LabUI.Paper); piece.Chip.raycastTarget = false;
                    var rect = piece.Chip.rectTransform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = centre; rect.sizeDelta = new Vector2(Mathf.Min(150, 26 + tk.Label.Length * 11), 40);
                    var word = Label(rect, tk.Label, 15, LabUI.Ink, Vector2.zero, rect.sizeDelta - new Vector2(8, 4));
                    word.enableAutoSizing = true; word.fontSizeMin = 10; word.fontSizeMax = 15;
                    if (!visible) piece.Chip.color = new Color(1, 1, 1, 0);
                }
                if (tk.Atoms != null) piece.Label = Label(root, tk.Label + (tk.Gas ? " ↑" : ""), 18, LabUI.Paper, centre + new Vector2(0, -52), new Vector2(150, 26));
                if (piece.Label != null && !visible) piece.Label.alpha = 0;
                pieces.Add(piece);
            }
            return pieces;
        }

        private static void Show(Piece m, float scale, float sticks)
        {
            foreach (var b in m.Balls) if (b != null) b.transform.localScale = Vector3.one * scale;
            foreach (var s in m.Sticks) if (s != null) s.color = new Color(0.86f, 0.87f, 0.9f, sticks * Mathf.Clamp01(scale));
            if (m.Chip != null && sticks > 0) m.Chip.transform.localScale = Vector3.one * scale;
        }

        // ------------------------------------------------------------ element reveal

        /// <summary>A newly forged element: a periodic-table tile flips in over sun rays, beside its shell model.</summary>
        public static IEnumerator Element(RectTransform card, int z, string symbol, string name, Color colour, string reward)
        {
            if (card == null) yield break;
            var panel = LabUI.Panel(card, "ახალი ელემენტი", new Color(0.12f, 0.19f, 0.16f, 0.96f));
            panel.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var root = panel.rectTransform;
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = new Vector2(0, 30); root.sizeDelta = new Vector2(640, 330);
            var group = panel.gameObject.AddComponent<CanvasGroup>(); group.blocksRaycasts = false; group.interactable = false;
            LabUI.Shadow(panel);

            var glow = Img(root, "სხივები", Rays, new Color(LabUI.Gold.r, LabUI.Gold.g, LabUI.Gold.b, 0.55f), new Vector2(-150, -6), new Vector2(330, 330));
            var tile = LabUI.Panel(root, "ფილა", LabUI.Paper);
            var tileRect = tile.rectTransform; tileRect.anchorMin = tileRect.anchorMax = tileRect.pivot = new Vector2(0.5f, 0.5f);
            tileRect.anchoredPosition = new Vector2(-150, -6); tileRect.sizeDelta = new Vector2(176, 206);
            var band = LabUI.Panel(tile.transform, "ფერი", colour); var bandRect = band.rectTransform;
            bandRect.anchorMin = new Vector2(0, 1); bandRect.anchorMax = Vector2.one; bandRect.pivot = new Vector2(0.5f, 1);
            bandRect.anchoredPosition = new Vector2(0, -8); bandRect.sizeDelta = new Vector2(-16, 14);
            var number = Label(tile.transform, z.ToString(), 26, LabUI.Ink, new Vector2(-52, 64), new Vector2(60, 34)); number.alignment = TextAlignmentOptions.Left; number.fontStyle = FontStyles.Bold;
            var big = Label(tile.transform, symbol, 78, LabUI.Green, new Vector2(0, 6), new Vector2(170, 96)); big.fontStyle = FontStyles.Bold;
            var title = Label(tile.transform, name, 20, LabUI.Ink, new Vector2(0, -68), new Vector2(166, 30)); title.enableAutoSizing = true; title.fontSizeMin = 12; title.fontSizeMax = 20;

            Label(root, "ახალი ელემენტი!", 30, LabUI.Gold, new Vector2(140, 128), new Vector2(330, 40)).fontStyle = FontStyles.Bold;
            // Shell model: protons in the nucleus, electrons 2 / 8 / rest on rings.
            var model = new Vector2(140, 0);
            int[] shells = z <= 2 ? new[] { z } : z <= 10 ? new[] { 2, z - 2 } : new[] { 2, 8, z - 10 };
            var electrons = new List<(Image e, int shell, int index, int count)>();
            for (int s = 0; s < shells.Length; s++)
            {
                float r = 34 + s * 26;
                Img(root, "გარსი", Ring, new Color(0.7f, 0.86f, 0.9f, 0.55f), model, Vector2.one * r * 2);
                for (int k = 0; k < shells[s]; k++) electrons.Add((Img(root, "ელექტრონი", Ball, new Color(0.45f, 0.75f, 1f), model, Vector2.one * 13), s, k, shells[s]));
            }
            Img(root, "ბირთვი", Ball, new Color(0.95f, 0.45f, 0.4f), model, Vector2.one * (22 + Mathf.Min(z, 12)));
            var facts = Label(root, "პროტონი: " + z + "  •  ელექტრონი: " + z + "\n<size=15>" + reward + " • პერიოდულ ცხრილში დაემატა</size>", 17, LabUI.Paper, new Vector2(140, -128), new Vector2(340, 50));
            facts.enableAutoSizing = true; facts.fontSizeMin = 11; facts.fontSizeMax = 17;
            Juice.Confetti(root, 3, 200);
            AmbientAudio.Instance?.Sparkle();

            for (float t = 0; t < 3.4f; t += Time.unscaledDeltaTime)
            {
                if (root == null) yield break;
                float flip = Mathf.Clamp01(t / 0.45f);
                // the tile turns over like a card, then breathes gently
                tile.transform.localScale = flip < 1 ? new Vector3(Mathf.Sin(flip * Mathf.PI * 0.5f), 1, 1) : Vector3.one * (1 + Mathf.Sin(t * 3) * 0.015f);
                glow.transform.localRotation = Quaternion.Euler(0, 0, t * 18);
                glow.transform.localScale = Vector3.one * Mathf.Clamp01(t / 0.6f);
                foreach (var (e, shell, index, count) in electrons)
                {
                    float r = 34 + shell * 26, a = t * (1.6f - shell * 0.4f) + index * Mathf.PI * 2 / count;
                    e.rectTransform.anchoredPosition = model + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r * Mathf.Clamp01(t / 0.5f);
                }
                group.alpha = t > 3.0f ? 1 - (t - 3.0f) / 0.4f : Mathf.Clamp01(t / 0.15f);
                yield return null;
            }
            if (root != null) Object.Destroy(root.gameObject);
        }

        // ------------------------------------------------------------ research completion

        /// <summary>A journal page slides up with the project's flask, title and rewards, and a "done" stamp thumps onto it.</summary>
        public static void ResearchDone(ResearchProject project, string reward)
        {
            if (LabUI.Root == null || project == null || LabGame.Instance == null) return;
            LabGame.Instance.StartCoroutine(Journal(project, reward));
        }

        private static IEnumerator Journal(ResearchProject project, string reward)
        {
            var veil = LabUI.Object(LabUI.Root, "კვლევა დასრულდა", typeof(Image), typeof(CanvasGroup));
            var group = veil.GetComponent<CanvasGroup>(); group.blocksRaycasts = false; group.interactable = false;
            var shade = veil.GetComponent<Image>(); shade.color = new Color(0.05f, 0.08f, 0.06f, 0.35f); shade.raycastTarget = false;
            var full = shade.rectTransform; full.anchorMin = Vector2.zero; full.anchorMax = Vector2.one; full.offsetMin = full.offsetMax = Vector2.zero;
            veil.transform.SetAsLastSibling();

            var page = LabUI.Panel(veil.transform, "გვერდი", LabUI.Paper);
            var rect = page.rectTransform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f); rect.sizeDelta = new Vector2(600, 380);
            LabUI.Shadow(page, 12);
            var ribbon = LabUI.Panel(page.transform, "ლენტი", project.Color); var ribbonRect = ribbon.rectTransform;
            ribbonRect.anchorMin = new Vector2(0, 1); ribbonRect.anchorMax = Vector2.one; ribbonRect.pivot = new Vector2(0.5f, 1); ribbonRect.sizeDelta = new Vector2(-20, 56); ribbonRect.anchoredPosition = new Vector2(0, -10);
            Label(ribbon.transform, "კვლევის ჟურნალი", 22, LabUI.Ink, Vector2.zero, new Vector2(540, 40)).fontStyle = FontStyles.Bold;
            for (int i = 0; i < 6; i++) Img(page.transform, "ხაზი", DecorArt.Square(), new Color(0.6f, 0.72f, 0.8f, 0.35f), new Vector2(60, 70 - i * 34), new Vector2(420, 2));
            var flask = Img(page.transform, "კოლბა", Glassware.Get(GlassShape.Erlenmeyer, project.Color, 0.55f), Color.white, new Vector2(-196, -10), new Vector2(120, 150));
            flask.preserveAspect = true;
            var title = Label(page.transform, project.Title, 26, LabUI.Green, new Vector2(70, 60), new Vector2(400, 70)); title.fontStyle = FontStyles.Bold;
            title.enableAutoSizing = true; title.fontSizeMin = 16; title.fontSizeMax = 26;
            var rewards = Label(page.transform, reward.Replace(" • ", "\n"), 18, LabUI.Ink, new Vector2(70, -54), new Vector2(400, 130));
            rewards.enableAutoSizing = true; rewards.fontSizeMin = 12; rewards.fontSizeMax = 18;
            var stamp = LabUI.Panel(page.transform, "შტამპი", new Color(0, 0, 0, 0));
            var stampRect = stamp.rectTransform; stampRect.anchorMin = stampRect.anchorMax = stampRect.pivot = new Vector2(0.5f, 0.5f);
            stampRect.anchoredPosition = new Vector2(180, -130); stampRect.sizeDelta = new Vector2(210, 64); stampRect.localRotation = Quaternion.Euler(0, 0, 12);
            var outline = stamp.gameObject.AddComponent<Outline>(); outline.effectColor = LabUI.Coral; outline.effectDistance = new Vector2(4, 4);
            stamp.color = new Color(LabUI.Coral.r, LabUI.Coral.g, LabUI.Coral.b, 0.15f);
            Label(stamp.transform, "დასრულდა!", 26, LabUI.Coral, Vector2.zero, new Vector2(200, 56)).fontStyle = FontStyles.Bold;
            stamp.gameObject.SetActive(false);

            bool stamped = false;
            for (float t = 0; t < 3.8f; t += Time.unscaledDeltaTime)
            {
                if (veil == null) yield break;
                float rise = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / 0.4f));
                rect.anchoredPosition = new Vector2(0, Mathf.Lerp(-420, 0, rise));
                flask.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 3) * 4);
                if (t > 0.75f)
                {
                    if (!stamped)
                    {
                        stamped = true; stamp.gameObject.SetActive(true);
                        Juice.Confetti(page.transform, 5, 260); AmbientAudio.Instance?.Sparkle();
                    }
                    float s = Mathf.Clamp01((t - 0.75f) / 0.18f);
                    stamp.transform.localScale = Vector3.one * Mathf.Lerp(2.2f, 1, s);
                    rect.anchoredPosition += t < 1.05f && s >= 1 ? new Vector2(Random.Range(-4f, 4f), Random.Range(-4f, 4f)) : Vector2.zero;
                }
                group.alpha = t > 3.4f ? 1 - (t - 3.4f) / 0.4f : 1;
                yield return null;
            }
            if (veil != null) Object.Destroy(veil);
        }
    }
}
