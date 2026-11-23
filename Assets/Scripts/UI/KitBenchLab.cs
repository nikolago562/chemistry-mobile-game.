using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    /// <summary>Animated stages for the personal lab's tables (see <see cref="LabKits"/>).</summary>
    public sealed partial class KitBenchUI
    {
        private static readonly Color GlassTint = new Color(1, 1, 1, 0.14f), Steel = new Color32(150, 156, 170, 255), Dark = new Color32(90, 96, 110, 255),
            Fire = new Color32(240, 130, 50, 255), FireTip = new Color32(250, 210, 80, 255), Icy = new Color(0.75f, 0.9f, 1f, 0.85f);
        private float mercury = -1;

        private void DrawLab()
        {
            switch (kit.ID)
            {
                case "balance": DrawBalance(); break;
                case "thermo": DrawThermo(); break;
                case "analysis": DrawFlame(); break;
                case "valve": DrawBalloon(); break;
                case "particles": DrawBattery(); break;
                case "compare": DrawFizz(); break;
                case "membrane": DrawMembrane(); break;
                case "chromatography": DrawPaper(); break;
                case "gas": DrawGas(); break;
            }
        }

        /// <summary>The recipe card: what is on the table in words, plus the number that matters for this table.</summary>
        private string LabRecipe()
        {
            string Small(string text) => "<size=22>" + text + "</size>";
            string items = string.Join(NL, kit.Parts.Select((p, i) => counts[i] == 0 ? null : p.Name + (p.Max > 1 ? " ×" + counts[i] : "")).Where(t => t != null));
            switch (kit.ID)
            {
                case "balance":
                    string side = string.Join(" + ", kit.Parts.Select((p, i) => counts[i] == 0 ? null : (counts[i] > 1 ? counts[i].ToString() : "") + p.Symbol).Where(t => t != null));
                    return side == "" ? "—" : side + " →";
                case "thermo": return LabKits.Temperature(counts) + " °C" + (items == "" ? "" : NL + Small(string.Join(" • ", kit.Parts.Take(5).Select((p, i) => counts[i] == 0 ? null : p.Name).Where(t => t != null))));
                case "gas": return "P ≈ " + LabKits.Pressure(counts).ToString("0.00") + " ატმ" + NL + Small(counts[0] + " ნაწილაკი • " + LabKits.Kelvin(counts).ToString("0") + " K • V " + LabKits.Volume(counts).ToString("0.0"));
                case "valve": return counts.Sum() == 0 ? "—" : "ბუშტი ×" + counts.Sum() + NL + Small("საშუალო მასა ≈ " + LabKits.Mass(counts).ToString("0") + " გ/მოლი (ჰაერი 29)");
                case "particles": return counts[0] + counts[1] == 0 ? "—" : (counts[0] + counts[1]) + " უჯრედი" + NL + Small(items.Replace(NL, " • "));
                default: return items == "" ? "—" : Small(items);
            }
        }
        private const string NL = "\n";

        // ------------------------------------------------------------ helpers

        private void StopLoop() { if (loop != null) StopCoroutine(loop); loop = null; }
        /// <summary>Runs <paramref name="step"/> every frame with the seconds since the stage was drawn, until it returns false.</summary>
        private void Loop(Func<float, bool> step) { StopLoop(); loop = StartCoroutine(Looping(step)); }
        private IEnumerator Looping(Func<float, bool> step)
        {
            float start = Time.unscaledTime;
            while (window != null && stage != null && step(Time.unscaledTime - start)) yield return null;
            loop = null;
        }
        private Image Block(Vector2 at, Vector2 size, Color color, bool round = false)
        {
            var image = LabUI.Panel(stage, "ნაწილი", color);
            if (round) image.sprite = Disc;
            Place(image.rectTransform, new Vector2(0.5f, 0.5f), at, size);
            return image;
        }
        private TMP_Text Words(string text, Vector2 at, float size, Color color, float width = 320)
        {
            var label = LabUI.Text(stage, text, size, color);
            label.alignment = TextAlignmentOptions.Center; label.fontStyle = FontStyles.Bold;
            Place(label.rectTransform, new Vector2(0.5f, 0.5f), at, new Vector2(width, size * 1.7f));
            return label;
        }
        private int Has(string id) => counts[Array.FindIndex(kit.Parts, p => p.ID == id)];
        private string Found => shown?.Find?.ID;
        private static void Move(Graphic g, Vector2 at) { if (g != null) g.rectTransform.anchoredPosition = at; }

        // ------------------------------------------------------------ reaction scale

        private void DrawBalance()
        {
            bool done = shown?.Find != null;
            var bob = new List<(RectTransform rect, Vector2 home, float delay)>();
            var left = new List<int>();
            for (int i = 0; i < counts.Length; i++) for (int k = 0; k < counts[i]; k++) left.Add(i);
            Grid(left.Select(i => (kit.Parts[i].Symbol, kit.Parts[i].Color)).ToList(), done ? -95 : 0, done ? 3 : 4, 0, bob);
            string equation = string.Join(" + ", kit.Parts.Select((p, i) => counts[i] == 0 ? null : (counts[i] > 1 ? counts[i].ToString() : "") + p.Symbol).Where(t => t != null));
            if (done)
            {
                Words("→", new Vector2(0, 40), 44, LabUI.Gold, 60);
                int times = Mathf.Max(1, LabKits.Times(counts, shown.Find.Counts));
                var products = new List<(string, Color)>();
                foreach (var term in shown.Find.Formula.Split('→')[1].Split('+').Select(t => t.Trim()))
                {
                    int digits = term.TakeWhile(char.IsDigit).Count();
                    int coefficient = digits > 0 ? int.Parse(term.Substring(0, digits)) : 1;
                    for (int c = 0; c < coefficient * times; c++) products.Add((term.Substring(digits), shown.Find.Color));
                }
                Grid(products, 100, 2, 0.4f, bob);
            }
            Words(done ? shown.Find.Formula : equation == "" ? "შეარჩიე მოლეკულები" : equation + " → ?", new Vector2(0, 178), 21, LabUI.Gold);
            Loop(t =>
            {
                foreach (var (rect, home, delay) in bob)
                {
                    if (rect == null) return false;
                    rect.anchoredPosition = home + Vector2.up * Mathf.Sin(t * 3 + home.x * 0.05f + home.y * 0.03f) * 4;
                    if (delay > 0) rect.localScale = Vector3.one * Mathf.Clamp01((t - delay) / 0.25f);
                }
                return true;
            });
        }

        /// <summary>Molecule balls in rows; a positive delay makes them pop in one after another.</summary>
        private void Grid(List<(string symbol, Color color)> items, float x, int columns, float delay, List<(RectTransform, Vector2, float)> into)
        {
            for (int j = 0; j < items.Count; j++)
            {
                int row = j / columns, col = j % columns, inRow = Mathf.Min(columns, items.Count - row * columns);
                var at = new Vector2(x + (col - (inRow - 1) / 2f) * 48, 120 - row * 48);
                var ball = Ball(at, 44, items[j].color, items[j].symbol);
                if (delay > 0) ball.transform.localScale = Vector3.zero;
                into.Add((ball.rectTransform, at, delay > 0 ? delay + j * 0.12f : 0));
            }
        }

        // ------------------------------------------------------------ thermostat

        private void DrawThermo()
        {
            int temperature = LabKits.Temperature(counts), cold = Has("cold"), heat = Has("flame");
            string found = Found;
            if (cold > 0) Block(new Vector2(40, -120), new Vector2(240, 80 + 20 * cold), new Color(0.6f, 0.85f, 1f, 0.35f));
            Block(new Vector2(-125, 10), new Vector2(30, 300), new Color(1, 1, 1, 0.85f));
            Ball(new Vector2(-125, -150), 46, new Color32(224, 80, 70, 255));
            var column = Block(new Vector2(-125, -140), new Vector2(14, 10), new Color32(224, 80, 70, 255));
            column.sprite = null;
            var reading = Words("", new Vector2(-125, 184), 22, Color.white, 130);
            float target = Mathf.Clamp01((temperature + 40) / 180f) * 280;
            if (mercury < 0) mercury = 60 / 180f * 280;

            var flames = new List<Image>();
            for (int i = 0; i < heat; i++) flames.Add(Ball(new Vector2(40 - (heat - 1) * 14 + i * 28, -172), 26, i % 2 == 0 ? Fire : FireTip));
            Block(new Vector2(40, -40), new Vector2(190, 230), GlassTint);

            var melting = new List<Image>();
            Image flame = null;
            if (Has("water") > 0)
            {
                Block(new Vector2(40, -100), new Vector2(174, 100), found == "freeze" ? Icy : new Color(0.47f, 0.75f, 0.94f, 0.8f));
                if (found == "freeze") for (int i = 0; i < 4; i++) Block(new Vector2(-10 + i * 34, -100 + (i % 2) * 20), new Vector2(4, 30), Color.white);
            }
            else if (Has("ice") > 0)
            {
                bool melt = found == "melt" || found == "roadsalt";
                if (melt) Block(new Vector2(40, -125), new Vector2(174, 50), new Color(0.47f, 0.75f, 0.94f, 0.8f));
                for (int i = 0; i < 3; i++) melting.Add(Block(new Vector2(-10 + i * 50, -120 + (i == 1 ? 30 : 0)), new Vector2(44, 44), Icy));
                if (!melt) melting.Clear();
            }
            else if (Has("wax") > 0)
            {
                if (found == "wax") Block(new Vector2(40, -140), new Vector2(160, 24), new Color32(245, 230, 190, 255));
                else { Block(new Vector2(40, -90), new Vector2(44, 110), new Color32(245, 230, 190, 255)); flame = Ball(new Vector2(40, -22), 18, FireTip); }
            }
            else if (Has("choco") > 0)
                Block(found == "choco" ? new Vector2(40, -140) : new Vector2(40, -125), found == "choco" ? new Vector2(170, 24) : new Vector2(110, 44), new Color32(120, 70, 40, 255));
            if (Has("salt") > 0 && Has("water") == 0) for (int i = 0; i < 6; i++) Block(new Vector2(-20 + i * 24, -150), new Vector2(10, 8), Color.white);

            var steam = new List<Image>();
            if (found == "boil" || found == "distill") for (int i = 0; i < 5; i++) steam.Add(Ball(new Vector2(40, 90), 34, new Color(1, 1, 1, 0.4f)));
            Image drop = null;
            if (found == "distill")
            {
                Line(new Vector2(60, 110), new Vector2(140, 20));
                Block(new Vector2(140, -150), new Vector2(54, 54), GlassTint);
                Block(new Vector2(140, -165), new Vector2(46, 20), new Color(0.6f, 0.85f, 1f, 0.8f));
                drop = Ball(new Vector2(140, 0), 12, new Color(0.6f, 0.85f, 1f, 1));
            }

            Loop(t =>
            {
                if (column == null) return false;
                mercury = Mathf.MoveTowards(mercury, target, 160 * Time.unscaledDeltaTime);
                column.rectTransform.sizeDelta = new Vector2(14, mercury);
                column.rectTransform.anchoredPosition = new Vector2(-125, -140 + mercury / 2);
                reading.text = Mathf.RoundToInt(mercury / 280f * 180 - 40) + " °C";
                for (int i = 0; i < flames.Count; i++) if (flames[i] != null) flames[i].rectTransform.sizeDelta = new Vector2(24, 28 + Mathf.Sin(t * 14 + i * 2) * 8);
                if (flame != null) flame.rectTransform.sizeDelta = new Vector2(14, 20 + Mathf.Sin(t * 12) * 4);
                for (int i = 0; i < steam.Count; i++)
                {
                    float p = (t * 0.5f + i / (float)steam.Count) % 1;
                    Move(steam[i], new Vector2(40 + Mathf.Sin(i * 2 + t) * 22, 90 + p * 110));
                    steam[i].color = new Color(1, 1, 1, 0.5f * (1 - p));
                    steam[i].rectTransform.sizeDelta = Vector2.one * (28 + p * 30);
                }
                if (drop != null) Move(drop, new Vector2(140, -20 - (t * 1.2f % 1) * 120));
                foreach (var cube in melting) if (cube != null) cube.transform.localScale = Vector3.one * Mathf.Max(0.25f, 1 - t * 0.2f);
                return true;
            });
        }

        // ------------------------------------------------------------ flame test

        private void DrawFlame()
        {
            Block(new Vector2(0, -172), new Vector2(110, 26), Steel);
            Block(new Vector2(0, -125), new Vector2(34, 70), Dark);
            var salts = Enumerable.Range(0, 6).Where(i => counts[i] > 0).ToList();
            Color colour = new Color32(90, 140, 240, 255);
            if (shown?.Find != null) colour = shown.Find.Color;
            else if (shown != null && salts.Count > 1) colour = salts.Contains(0) ? kit.Parts[0].Color : Color.Lerp(kit.Parts[salts[0]].Color, kit.Parts[salts[1]].Color, 0.5f);
            Image outer = null, inner = null, core = null;
            if (counts[6] == 0) Words("სანთურა ჩამქრალია", new Vector2(0, 20), 20, new Color(1, 1, 1, 0.6f));
            else
            {
                outer = Block(new Vector2(0, -20), new Vector2(90, 160), new Color(colour.r, colour.g, colour.b, 0.6f), true);
                inner = Block(new Vector2(0, -45), new Vector2(50, 100), Color.Lerp(colour, Color.white, 0.45f), true);
                core = Block(new Vector2(0, -70), new Vector2(24, 46), new Color(1, 1, 1, 0.8f), true);
            }
            if (salts.Count > 0)
            {
                Line(new Vector2(150, -60), new Vector2(40, -30));
                for (int i = 0; i < salts.Count; i++) Ball(new Vector2(36 + i * 10, -30 + i * 6), 18, kit.Parts[salts[i]].Color);
            }
            bool sparks = shown?.Find != null;
            float next = 0;
            Loop(t =>
            {
                if (outer == null) return counts[6] == 0;
                outer.rectTransform.sizeDelta = new Vector2(90 + Mathf.Sin(t * 13) * 8, 160 + Mathf.Sin(t * 9) * 16);
                outer.rectTransform.anchoredPosition = new Vector2(Mathf.Sin(t * 5) * 3, -20 + Mathf.Sin(t * 7) * 4);
                inner.rectTransform.sizeDelta = new Vector2(50 + Mathf.Sin(t * 11) * 5, 100 + Mathf.Sin(t * 8) * 10);
                core.rectTransform.sizeDelta = new Vector2(24, 46 + Mathf.Sin(t * 15) * 5);
                if (sparks && t > next) { next = t + 0.18f; UIFx.Burst(stage, new Vector2(UnityEngine.Random.Range(-25, 25), 60), colour, 2); }
                return true;
            });
        }

        // ------------------------------------------------------------ balloon valve

        private void DrawBalloon()
        {
            int total = counts.Sum();
            Block(new Vector2(0, -184), new Vector2(84, 30), Steel);
            Block(new Vector2(0, -160), new Vector2(16, 22), Dark);
            if (total == 0) { Words("ბუშტი ცარიელია", new Vector2(0, 20), 20, new Color(1, 1, 1, 0.6f)); return; }
            var mix = new Color(0, 0, 0, 0);
            for (int i = 0; i < counts.Length; i++) mix += kit.Parts[i].Color * counts[i] / total;
            mix.a = 1;
            float size = 60 + 22 * Mathf.Min(total, 6), baseY = -150 + size / 2 + 18;
            var balloon = Block(new Vector2(0, baseY), new Vector2(size * 0.85f, size), mix, true);
            var shine = Ball(new Vector2(-size * 0.18f, baseY + size * 0.2f), size * 0.18f, new Color(1, 1, 1, 0.6f));
            var knot = Ball(new Vector2(0, baseY - size / 2), 12, mix);
            bool released = shown != null;
            if (!released) Line(new Vector2(0, baseY - size / 2), new Vector2(0, -150));
            float lift = released ? LabKits.Lift(counts) : 0, top = 205 - size / 2, floor = -178 + size / 2;
            Loop(t =>
            {
                if (balloon == null) return false;
                float y = released
                    ? lift > 0 ? Mathf.Min(top, baseY + t * 110) + (baseY + t * 110 > top ? Mathf.Sin(t * 2) * 4 : 0)
                    : lift == 0 ? baseY + 60 * Mathf.Clamp01(t) + Mathf.Sin(t * 1.5f) * 6
                    : Mathf.Max(floor, baseY + 40 - t * t * 160 * -lift)
                    : baseY + Mathf.Sin(t * 2) * 3;
                float x = released ? Mathf.Sin(t * 1.3f) * 8 : 0;
                Move(balloon, new Vector2(x, y));
                Move(shine, new Vector2(x - size * 0.18f, y + size * 0.2f));
                Move(knot, new Vector2(x, y - size / 2));
                return true;
            });
        }

        // ------------------------------------------------------------ fruit battery

        private static Vector2 Circuit(float s)
        {
            // Up the left side, across the top past the lamp, down the right side (lengths 180, 300, 180).
            float d = Mathf.Repeat(s, 1) * 660;
            if (d < 180) return new Vector2(-150, -60 + d);
            if (d < 480) return new Vector2(-150 + (d - 180), 120);
            return new Vector2(150, 120 - (d - 480));
        }

        private void DrawBattery()
        {
            int lemons = Has("lemon"), cells = lemons + Has("potato");
            bool zinc = Has("zinc") > 0, copper = Has("copper") > 0, wire = Has("wire") > 0, led = Has("led") > 0, lit = Found == "led";
            float gap = cells > 0 ? Mathf.Min(74, 290f / cells) : 0;
            if (wire) { Line(new Vector2(-150, -60), new Vector2(-150, 120)); Line(new Vector2(-150, 120), new Vector2(150, 120)); Line(new Vector2(150, 120), new Vector2(150, -60)); }
            for (int i = 0; i < cells; i++)
            {
                var at = new Vector2((i - (cells - 1) / 2f) * gap, -90);
                Block(at, new Vector2(gap * 0.9f, 52), i < lemons ? kit.Parts[0].Color : kit.Parts[1].Color, true);
                if (zinc) Block(at + new Vector2(-gap * 0.2f, 30), new Vector2(8, 34), kit.Parts[2].Color);
                if (copper) Block(at + new Vector2(gap * 0.2f, 30), new Vector2(8, 34), kit.Parts[3].Color);
            }
            Image halo = null, lamp = null;
            if (led)
            {
                if (lit) halo = Block(new Vector2(0, 120), Vector2.one * 90, new Color(1, 0.95f, 0.5f, 0.4f), true);
                lamp = Ball(new Vector2(0, 120), 44, lit ? new Color32(255, 240, 120, 255) : new Color32(120, 120, 110, 255));
            }
            if (cells > 0 && zinc && copper) Words((cells * 0.85f).ToString("0.0") + " ვოლტი", new Vector2(0, -160), 22, LabUI.Gold);
            var electrons = new List<Image>();
            if (shown?.Find != null) for (int i = 0; i < 10; i++) electrons.Add(Ball(Circuit(i / 10f), 13, new Color32(90, 170, 240, 255)));
            Loop(t =>
            {
                for (int i = 0; i < electrons.Count; i++) Move(electrons[i], Circuit(t * 0.12f * Mathf.Min(cells, 3) + i / 10f));
                if (halo != null) halo.rectTransform.sizeDelta = Vector2.one * (90 + Mathf.Sin(t * 6) * 14);
                return true;
            });
        }

        // ------------------------------------------------------------ reaction speed

        private void DrawFizz()
        {
            bool acid = Has("vinegar") > 0, peroxide = Has("peroxide") > 0;
            if (Has("warm") > 0) Block(new Vector2(0, -70), new Vector2(240, 250), new Color(1, 0.6f, 0.3f, 0.22f));
            Block(new Vector2(0, -60), new Vector2(200, 220), GlassTint);
            Block(new Vector2(0, 80), new Vector2(70, 60), GlassTint);
            if (acid || peroxide) Block(new Vector2(0, -110), new Vector2(184, 110), acid ? new Color(0.92f, 0.9f, 0.77f, 0.8f) : new Color(0.78f, 0.9f, 0.96f, 0.8f));
            if (Has("lump") > 0) Block(new Vector2(-20, -148), new Vector2(56, 40), Color.white);
            if (Has("powder") > 0) for (int i = 0; i < 9; i++) Block(new Vector2(-70 + i * 17, -158 + (i % 2) * 4), new Vector2(12, 9), Color.white);
            if (Has("yeast") > 0) for (int i = 0; i < 6; i++) Ball(new Vector2(-50 + i * 20, -70 + (i % 2) * 8), 10, kit.Parts[5].Color);
            string found = Found;
            int rate = found == "slow" ? 2 : found == "warmlump" ? 5 : found == "fast" ? 9 : found == "hot" ? 16 : found == "catalyst" ? 8 : 0;
            var bubbles = new List<Image>();
            for (int i = 0; i < rate; i++) bubbles.Add(Ball(new Vector2(0, -160), 10 + i % 3 * 3, new Color(1, 1, 1, 0.8f)));
            var foam = new List<Image>();
            if (found == "catalyst") for (int i = 0; i < 12; i++) foam.Add(Block(new Vector2(0, 60), Vector2.one * (34 + i * 3), new Color(1, 1, 1, 0.95f), true));
            Loop(t =>
            {
                for (int i = 0; i < bubbles.Count; i++)
                {
                    float p = (t * (0.5f + rate * 0.06f) + i / (float)bubbles.Count) % 1;
                    Move(bubbles[i], new Vector2(-75 + (i * 37) % 150 + Mathf.Sin(t * 5 + i) * 4, -160 + p * 110));
                }
                float grow = Mathf.Clamp01(t / 2.5f);
                for (int i = 0; i < foam.Count; i++) Move(foam[i], new Vector2(Mathf.Sin(i * 1.7f) * grow * (14 + i * 5), 60 + grow * (30 + i * 11)));
                return true;
            });
        }

        // ------------------------------------------------------------ membrane tank

        private void DrawMembrane()
        {
            bool water = Has("water") > 0;
            string found = Found;
            Block(new Vector2(0, -30), new Vector2(300, 290), GlassTint);
            if (water) Block(new Vector2(0, -55), new Vector2(286, 230), new Color(0.47f, 0.75f, 0.94f, 0.35f));
            for (int i = 0; i < 11; i++) Block(new Vector2(0, -165 + i * 22), new Vector2(6, 12), new Color(1, 1, 1, 0.75f));
            var drift = new List<(Image dot, Vector2 from, Vector2 to)>();
            var random = new System.Random(7);
            float R(float a, float b) => a + (float)random.NextDouble() * (b - a);
            if (water) for (int i = 0; i < Has("salt") * 6; i++) { var at = new Vector2(R(10, 135), R(-160, 50)); drift.Add((Ball(at, 7, Color.white), at, at)); }
            if (Has("dye") > 0)
                for (int i = 0; i < 26; i++)
                {
                    var at = new Vector2(R(-130, -20), R(-155, 45));
                    var end = found == "diffusion" ? new Vector2(R(-135, 135), R(-160, 50)) : at;
                    drift.Add((Ball(at, 10, kit.Parts[2].Color), at, end));
                }
            Image raisin = null, potato = null;
            if (Has("raisin") > 0) raisin = Ball(new Vector2(-70, -70), 40, kit.Parts[3].Color);
            if (Has("potato") > 0) potato = Block(new Vector2(70, -80), new Vector2(70, 90), kit.Parts[4].Color);
            if (found == "swell") Words("წყალი → შიგნით", new Vector2(-70, 30), 18, Color.white, 200);
            if (found == "firm") Words("წყალი → შიგნით", new Vector2(70, 40), 18, Color.white, 200);
            if (found == "limp") Words("წყალი ← გარეთ", new Vector2(70, 40), 18, Color.white, 200);
            Loop(t =>
            {
                float k = Mathf.Clamp01(t / 4f);
                for (int i = 0; i < drift.Count; i++)
                    Move(drift[i].dot, Vector2.Lerp(drift[i].from, drift[i].to, Mathf.SmoothStep(0, 1, k)) + new Vector2(Mathf.Sin(t * 3 + i), Mathf.Cos(t * 2.7f + i * 1.3f)) * 4);
                float g = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / 2.5f));
                if (raisin != null && found == "swell") { raisin.rectTransform.sizeDelta = Vector2.one * Mathf.Lerp(40, 74, g); raisin.color = Color.Lerp(kit.Parts[3].Color, new Color32(170, 100, 140, 255), g); }
                if (potato != null && found == "firm") potato.rectTransform.sizeDelta = Vector2.Lerp(new Vector2(70, 90), new Vector2(82, 104), g);
                if (potato != null && found == "limp") { potato.rectTransform.sizeDelta = Vector2.Lerp(new Vector2(70, 90), new Vector2(56, 70), g); potato.rectTransform.localRotation = Quaternion.Euler(0, 0, -14 * g); }
                return true;
            });
        }

        // ------------------------------------------------------------ paper chromatography

        private void DrawPaper()
        {
            int marker = Array.FindIndex(counts.Take(5).ToArray(), c => c > 0);
            bool water = Has("water") > 0, alcohol = Has("alcohol") > 0;
            Block(new Vector2(0, -30), new Vector2(200, 330), GlassTint);
            if (water || alcohol) Block(new Vector2(0, -172), new Vector2(184, 40), water ? new Color(0.47f, 0.75f, 0.94f, 0.5f) : new Color(0.85f, 0.92f, 0.97f, 0.5f));
            Block(new Vector2(0, -20), new Vector2(110, 290), new Color(0.98f, 0.98f, 0.95f));
            var wet = Block(new Vector2(0, -165), new Vector2(110, 0), new Color(0.55f, 0.75f, 1f, 0.35f));
            wet.sprite = null;
            Block(new Vector2(0, -120), new Vector2(110, 3), new Color32(120, 120, 120, 255));
            Image dot = marker >= 0 ? Ball(new Vector2(0, -120), 18, kit.Parts[marker].Color) : null;
            var bands = new List<(Image blob, float height)>();
            if (Found != null && LabKits.Bands.TryGetValue(Found, out var list))
                foreach (var (color, height) in list) bands.Add((Block(new Vector2(0, -120), new Vector2(60, 14), new Color(color.r, color.g, color.b, 0.85f), true), height));
            bool runs = shown != null && (water || alcohol);
            Loop(t =>
            {
                if (wet == null) return false;
                if (!runs) return true;
                float front = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / 2.8f)), height = front * 270;
                wet.rectTransform.sizeDelta = new Vector2(110, height);
                Move(wet, new Vector2(0, -165 + height / 2));
                foreach (var (blob, h) in bands)
                {
                    Move(blob, new Vector2(0, -120 + h * front * 230));
                    blob.rectTransform.sizeDelta = new Vector2(60 + 14 * front, 14 + 10 * front);
                }
                if (dot != null && bands.Count > 0) dot.color = new Color(dot.color.r, dot.color.g, dot.color.b, 1 - front);
                return true;
            });
        }

        // ------------------------------------------------------------ gas pressure

        private void DrawGas()
        {
            float volume = LabKits.Volume(counts), kelvin = LabKits.Kelvin(counts), pressure = LabKits.Pressure(counts);
            float width = 270 * volume, left = -150, right = left + width;
            var flames = new List<Image>();
            for (int i = 0; i < Has("heat"); i++) flames.Add(Ball(new Vector2(left + 40 + i * 50, -178), 26, i % 2 == 0 ? Fire : FireTip));
            for (int i = 0; i < Has("cold"); i++) Block(new Vector2(left + 50 + i * 70, -176), new Vector2(44, 30), Icy);
            Block(new Vector2(left + width / 2, -10), new Vector2(width, 300), new Color(0.3f, 0.45f, 0.4f, 0.9f));
            Block(new Vector2(right + 8, -10), new Vector2(14, 312), Steel);
            Block(new Vector2(right + 70, -10), new Vector2(110, 10), Steel);
            Words("P = " + pressure.ToString("0.00") + " ატმ  •  " + kelvin.ToString("0") + " K", new Vector2(0, 184), 21, pressure > 3 ? LabUI.Coral : LabUI.Gold, 330);
            int n = Has("n");
            var random = new System.Random(n * 31 + 5);
            var dots = new List<Image>(); var position = new Vector2[n]; var velocity = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                position[i] = new Vector2(left + 12 + (float)random.NextDouble() * (width - 24), -150 + (float)random.NextDouble() * 280);
                float angle = (float)random.NextDouble() * Mathf.PI * 2;
                velocity[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                dots.Add(Ball(position[i], 15, kit.Parts[0].Color));
            }
            float speed = 110 * Mathf.Sqrt(kelvin / 300f);
            Loop(t =>
            {
                float dt = Time.unscaledDeltaTime;
                for (int i = 0; i < n; i++)
                {
                    position[i] += velocity[i] * speed * dt;
                    if (position[i].x < left + 9 || position[i].x > right - 9) { velocity[i].x = -velocity[i].x; position[i].x = Mathf.Clamp(position[i].x, left + 9, right - 9); }
                    if (position[i].y < -152 || position[i].y > 132) { velocity[i].y = -velocity[i].y; position[i].y = Mathf.Clamp(position[i].y, -152, 132); }
                    Move(dots[i], position[i]);
                }
                for (int i = 0; i < flames.Count; i++) if (flames[i] != null) flames[i].rectTransform.sizeDelta = new Vector2(24, 28 + Mathf.Sin(t * 14 + i * 2) * 8);
                return true;
            });
        }
    }
}
