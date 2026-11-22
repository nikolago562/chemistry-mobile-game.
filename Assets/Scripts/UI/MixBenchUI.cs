using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    /// <summary>
    /// The heart of the game, at the reaction table: pick two ingredients, predict the sign, mix.
    /// Mixing uses up ingredients, new results fill the discovery book, and any mix that shows
    /// the sign a classmate ordered fills that order.
    /// </summary>
    public sealed class MixBenchUI : MonoBehaviour
    {
        public const int Pay = 5, PayGuess = 3, DiscoveryXP = 5;

        private LabWindow window;
        private Transform shelf, guesses;
        private RectTransform stage;
        private TMP_Text readout, count, orders;
        private readonly List<string> picked = new List<string>();
        private MixSign? guess;
        private Coroutine show;
        private static Sprite circle;

        public bool IsOpen => window != null;
        public string Feedback => readout != null ? readout.text : "";

        public void Open()
        {
            if (window != null) { LabUI.BringToFront(window); return; }
            picked.Clear(); guess = null;
            window = LabUI.Window("შერევის მაგიდა", 1100, 696);
            window.Closed = () => { window = null; show = null; };
            count = Ui.Line(window.Body, "", 16, LabUI.Muted, 28);
            var split = LabUI.Row(window.Body, "მაგიდა", 18); LabUI.FlexibleHeight(split.gameObject);
            var left = LabUI.Row(split.transform, "თარო", 8); LabUI.Size(left.gameObject, -1, 440, 0); shelf = left.transform;
            var panel = LabUI.Panel(split.transform, "ჭიქა", LabUI.Navy); LabUI.Size(panel.gameObject, 400, 260, 0); stage = panel.rectTransform;
            var right = LabUI.Column(split.transform, "ვარაუდი", 6); LabUI.Size(right.gameObject, -1, -1, 1); guesses = right.transform;
            readout = Ui.Line(window.Body, "ჯერ აირჩიე ორი ნივთიერება თაროდან.", 17, LabUI.Ink, 92);
            orders = Ui.Line(window.Body, "", 15, LabUI.Green, 24);
            Refresh();
            Beaker(null, null);
        }

        public void Pick(string id)
        {
            if (window == null || Mixing.Get(id) == null) return;
            if (picked.Remove(id)) { Refresh(); return; }
            if (Mixing.Stock(id) == 0 && !Buy(id)) return;
            if (picked.Count == 2) picked.RemoveAt(1);
            picked.Add(id);
            Refresh();
        }

        /// <summary>An empty jar buys one more from the storage shop on tap; greenhouse plants are picked there.</summary>
        private bool Buy(string id)
        {
            var material = LabProgress.Material(id);
            var data = GameSaveManager.Instance.Data;
            if (material == null || material.Price == 0) { readout.text = Mixing.Get(id).Name + " გამოგელია. ის სათბურში იზრდება — მოკრიფე იქ."; return false; }
            if (data.chemCoins < material.Price) { readout.text = "ქიმქოინები არ გყოფნის: " + Mixing.Get(id).Name + " " + material.Price + " ღირს. შეასრულე შეკვეთა!"; return false; }
            data.chemCoins -= material.Price;
            LabProgress.Give(id, 1);
            GameSaveManager.Instance.Commit();
            AudioManager.Instance?.Coin();
            readout.text = "იყიდე: " + Mixing.Get(id).Name + " (−" + material.Price + " ქიმქოინი).";
            return true;
        }

        public void Guess(MixSign sign) { if (window == null) return; guess = sign; Refresh(); }

        public void Mix()
        {
            if (window == null) return;
            if (picked.Count < 2) { readout.text = "აირჩიე ორი ნივთიერება."; return; }
            if (guess == null) { readout.text = "ჯერ გამოიცანი, რა მოხდება — მერე შეურიე. ვარაუდი მეცნიერების პირველი ნაბიჯია."; return; }
            if (picked.Any(p => Mixing.Stock(p) == 0)) { readout.text = "ერთ-ერთი ინგრედიენტი გამოგელია. შეეხე მას, რომ იყიდო."; return; }
            foreach (string p in picked) if (!Mixing.Free(p)) LabProgress.Take(p, 1);
            var a = Mixing.Get(picked[0]); var b = Mixing.Get(picked[1]);
            var result = Mixing.Find(a.ID, b.ID);
            var sign = result?.Sign ?? MixSign.Nothing;
            bool right = guess == sign, fresh = result != null && LabProgress.Collect(result.ID);
            LabProgress.CountStat("mix");
            int paid = 0;
            if (fresh)
            {
                paid = Pay + (right ? PayGuess : 0);
                GameSaveManager.Instance.Data.chemCoins += paid;
                LabProgress.AddReputation(1);
                GameSaveManager.Instance.AddXP(DiscoveryXP);
            }
            var order = Orders.Deliver(sign);
            GameSaveManager.Instance.Commit();
            Missions.Refresh(); // a mix can finish a story step (the first mission is a mix)

            // The teacher answers the prediction first, then explains what actually happened.
            string head = right
                ? (sign == MixSign.Nothing ? "სწორად გამოიცანი: არაფერი მოხდა. " : "ზუსტად ასე! წინასწარ გამოიცანი — ეს უკვე ნამდვილი მეცნიერებაა. ")
                : "შენ ფიქრობდი: „" + Mixing.SignName(guess) + "“, სინამდვილეში — „" + Mixing.SignName(sign) + "“. ვარაუდი, ცდა, დასკვნა — ასე სწავლობენ. ";
            string body = (result != null && result.Equation != "" ? result.Equation + "\n" : "") + (result != null ? result.Why : Mixing.NoReaction(a, b));
            string tail = fresh ? "\nახალი აღმოჩენა! +" + paid + " მონეტა, +" + DiscoveryXP + " XP" : result != null ? "\nეს წყვილი უკვე გამოცდილი გაქვს — სცადე ახალი!" : "";
            if (order != null) tail += (tail == "" ? "\n" : "  •  ") + order.Name + ": შეკვეთა შესრულდა! +" + Orders.Pay + " ქიმქოინი";
            readout.text = head + body + tail;
            readout.color = right ? LabUI.Green : LabUI.Ink;
            if (fresh || order != null) AmbientAudio.Instance?.Sparkle(); else if (right) AudioManager.Instance?.Correct();
            if (order != null) LabGame.Instance?.HUD.Refresh();
            guess = null;
            Refresh();
            Beaker(a, b, result);
        }

        private void Refresh()
        {
            Ui.Clear(shelf);
            for (int c = 0; c < 2; c++)
            {
                var column = LabUI.Column(shelf, "სვეტი", 6); LabUI.Size(column.gameObject, -1, 216, 1);
                foreach (var r in Mixing.Reagents.Skip(c * 8).Take(8))
                {
                    var id = r.ID;
                    int stock = Mixing.Stock(id);
                    var material = LabProgress.Material(id);
                    string label = r.Name + "  <size=13>" + (Mixing.Free(id) ? "უფასო" : stock > 0 ? "×" + stock : material != null && material.Price > 0 ? "იყიდე " + material.Price : "სათბურში") + "</size>";
                    var button = picked.Contains(id) ? LabUI.Button(column.transform, label, () => Pick(id), 36) : LabUI.Secondary(column.transform, label, () => Pick(id), 36);
                    LabUI.Size(button.gameObject, 36);
                }
            }
            Ui.Clear(guesses);
            Ui.Line(guesses, "რას ფიქრობ, რა მოხდება?", 17, LabUI.Ink, 26, true);
            for (int i = 0; i < Mixing.SignNames.Length; i++)
            {
                var sign = (MixSign)i;
                var button = guess == sign ? LabUI.Button(guesses, Mixing.SignNames[i], () => Guess(sign), 36) : LabUI.Secondary(guesses, Mixing.SignNames[i], () => Guess(sign), 36);
                LabUI.Size(button.gameObject, 36);
            }
            string pair = picked.Count == 0 ? "" : string.Join(" + ", picked.Select(p => Mixing.Get(p).Name));
            Ui.Line(guesses, pair, 15, LabUI.Muted, 40);
            var buttons = LabUI.Row(guesses, "ღილაკები", 8); LabUI.Size(buttons.gameObject, 46);
            LabUI.Size(LabUI.Button(buttons.transform, "შერევა", Mix, 46).gameObject, 46, -1, 1);
            LabUI.Size(LabUI.Secondary(buttons.transform, "წიგნი", Book, 46).gameObject, 46, 110, 0);
            orders.text = "დღის შეკვეთები:  " + string.Join("   •   ", Orders.Today.Select(o => o.Name + " — " + Mixing.SignName(o.Wants) + (Orders.Done(o) ? " (შესრულდა)" : "")));
            count.text = "აღმოჩენები: " + Mixing.Results.Count(r => LabProgress.Owns(r.ID)) + " / " + Mixing.Results.Length + "  •  ქიმქოინები: " + GameSaveManager.Instance.Data.chemCoins + "  •  აირჩიე ორი ნივთიერება, გამოიცანი რა მოხდება და შეურიე.";
        }

        /// <summary>The discovery book: found results in full, the rest as "ingredient + ???" hints.</summary>
        public void Book()
        {
            var book = LabUI.Window("აღმოჩენების წიგნი", 900, 620);
            Ui.Line(book.Body, "აღმოჩენილია " + Mixing.Results.Count(r => LabProgress.Owns(r.ID)) + " / " + Mixing.Results.Length + ". მინიშნება: ერთი ინგრედიენტი უკვე იცი — მეორე შენ იპოვე!", 17, LabUI.Muted, 28);
            var scroll = LabUI.Scroll(book.Body);
            foreach (var r in Mixing.Results)
            {
                bool owned = LabProgress.Owns(r.ID);
                var card = LabUI.Card(scroll, r.ID, owned ? LabUI.Soft : new Color(0.9f, 0.9f, 0.88f), 10);
                var row = LabUI.Row(card.transform, "ჩანაწერი", 12); LabUI.Size(row.gameObject, 52);
                LabUI.Size(LabUI.Panel(row.transform, "ფერი", owned ? r.Color : new Color(0.75f, 0.75f, 0.75f)).gameObject, 44, 44, 0);
                var text = LabUI.Text(row.transform, owned ? "<b>" + r.Title + "</b>  •  " + Mixing.SignName(r.Sign) + "\n<size=14>" + r.Why + "</size>"
                    : "<b>" + Mixing.Get(r.A).Name + " + ???</b>\n<size=14>ჯერ არ გიცდია. რას შეურევდი?</size>", 17, owned ? LabUI.Ink : LabUI.Muted);
                text.enableAutoSizing = true; text.fontSizeMin = 11; text.fontSizeMax = 17;
                LabUI.Size(text.gameObject, 52, -1, 1);
            }
        }

        // ------------------------------------------------------------ the beaker

        private void Beaker(Reagent a, Reagent b, MixResult result = null)
        {
            if (show != null) StopCoroutine(show);
            show = null;
            Ui.Clear(stage);
            Shape("შუქი", new Vector2(0, -30), new Vector2(200, 270), new Color(1, 1, 1, 0.12f));
            var start = a == null ? new Color(0.6f, 0.75f, 0.85f, 0.5f) : Color.Lerp(a.Color, b.Color, 0.5f);
            var liquid = Shape("სითხე", new Vector2(0, -90), new Vector2(184, 140), start);
            if (a == null) return;
            var label = LabUI.Text(stage, "", 18, Color.white);
            label.alignment = TextAlignmentOptions.Center;
            var rect = label.rectTransform; rect.anchorMin = rect.anchorMax = Vector2.one * 0.5f; rect.sizeDelta = new Vector2(280, 60); rect.anchoredPosition = new Vector2(0, 150);
            show = StartCoroutine(Animate(liquid, label, start, result));
        }

        private IEnumerator Animate(Image liquid, TMP_Text label, Color start, MixResult result)
        {
            var sign = result?.Sign ?? MixSign.Nothing;
            var end = result?.Color ?? start;
            var bits = new List<(Image image, Vector2 from, float delay)>();
            if (sign == MixSign.Gas || sign == MixSign.Solid || sign == MixSign.Dissolves)
                for (int i = 0; i < 14; i++)
                {
                    var from = sign == MixSign.Gas ? new Vector2(Random.Range(-80f, 80f), -150) : new Vector2(Random.Range(-80f, 80f), sign == MixSign.Solid ? -30 : -60);
                    var color = sign == MixSign.Gas ? new Color(1, 1, 1, 0.8f) : sign == MixSign.Solid ? Color.Lerp(end, Color.white, 0.3f) : Color.white;
                    bits.Add((Shape("ნაწილაკი", from, Vector2.one * (sign == MixSign.Gas ? Random.Range(8f, 16f) : 10f), color, true), from, Random.Range(0f, 1.2f)));
                }
            for (float t = 0; t < 2.4f; t += Time.unscaledDeltaTime)
            {
                if (liquid == null) yield break;
                float k = Mathf.Clamp01(t / 1.4f);
                liquid.color = Color.Lerp(start, end, k);
                if (sign == MixSign.Heat) liquid.color = Color.Lerp(liquid.color, LabUI.Coral, 0.35f * Mathf.Abs(Mathf.Sin(t * 4)));
                foreach (var (image, from, delay) in bits)
                {
                    float s = Mathf.Clamp01((t - delay) / 1.1f);
                    if (sign == MixSign.Gas) image.rectTransform.anchoredPosition = from + new Vector2(Mathf.Sin(t * 6 + delay * 9) * 6, s * 130);
                    else if (sign == MixSign.Solid) image.rectTransform.anchoredPosition = from + new Vector2(0, -s * 110);
                    else image.rectTransform.localScale = Vector3.one * (1 - s);
                }
                yield return null;
            }
            if (label != null) label.text = Mixing.SignName(sign);
            show = null;
        }

        private Image Shape(string name, Vector2 point, Vector2 size, Color color, bool round = false)
        {
            var image = LabUI.Panel(stage, name, color);
            if (round)
            {
                if (circle == null) circle = PixelArt.Make(32, 32, (x, y) => Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f)) < 15.5f ? Color.white : Color.clear);
                image.sprite = circle;
            }
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = Vector2.one * 0.5f;
            rect.anchoredPosition = point; rect.sizeDelta = size;
            image.raycastTarget = false;
            return image;
        }
    }
}
