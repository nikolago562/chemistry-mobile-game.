using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    /// <summary>
    /// Free mixing bench at the reaction table: pick any two substances, predict the sign, mix.
    /// Every pair answers from <see cref="Mixing"/>; new listed results fill the reaction collection.
    /// </summary>
    public sealed class MixBenchUI : MonoBehaviour
    {
        /// <summary>The bench opens once the signs of a reaction have been taught.</summary>
        public const string Lesson = "c6_signs";
        public const int Pay = 5, PayGuess = 3;

        private LabWindow window;
        private Transform shelf, guesses;
        private RectTransform stage;
        private TMP_Text readout, count;
        private readonly List<string> picked = new List<string>();
        private MixSign? guess;
        private Coroutine show;
        private static Sprite circle;

        public bool IsOpen => window != null;
        public string Feedback => readout != null ? readout.text : "";

        public void Open()
        {
            if (!Curriculum.Done(Lesson)) { LabGame.Instance?.HUD.Notify(Curriculum.LockedText + " საჭიროა გაკვეთილი: „" + Curriculum.Get(Lesson).Title + "“."); return; }
            if (window != null) { LabUI.BringToFront(window); return; }
            picked.Clear(); guess = null;
            window = LabUI.Window("შერევის მაგიდა", 1100, 660);
            window.Closed = () => { window = null; show = null; };
            Ui.Line(window.Body, "აირჩიე ორი ნივთიერება, გამოიცანი რა მოხდება და შეურიე. ზოგი წყვილი არ რეაგირებს — ესეც აღმოჩენაა!", 17, LabUI.Muted, 30);
            var split = LabUI.Row(window.Body, "მაგიდა", 18); LabUI.FlexibleHeight(split.gameObject);
            var left = LabUI.Row(split.transform, "თარო", 8); LabUI.Size(left.gameObject, -1, 440, 0); shelf = left.transform;
            var panel = LabUI.Panel(split.transform, "ჭიქა", LabUI.Navy); LabUI.Size(panel.gameObject, 400, 260, 0); stage = panel.rectTransform;
            var right = LabUI.Column(split.transform, "ვარაუდი", 6); LabUI.Size(right.gameObject, -1, -1, 1); guesses = right.transform;
            readout = Ui.Line(window.Body, "ჯერ აირჩიე ორი ნივთიერება თაროდან.", 18, LabUI.Ink, 70);
            count = Ui.Line(window.Body, "", 15, LabUI.Muted, 22);
            Refresh();
            Beaker(null, null);
        }

        public void Pick(string id)
        {
            if (window == null || Mixing.Get(id) == null) return;
            if (picked.Remove(id)) { Refresh(); return; }
            if (picked.Count == 2) picked.RemoveAt(1);
            picked.Add(id);
            Refresh();
        }

        public void Guess(MixSign sign) { if (window == null) return; guess = sign; Refresh(); }

        public void Mix()
        {
            if (window == null) return;
            if (picked.Count < 2) { readout.text = "აირჩიე ორი ნივთიერება."; return; }
            if (guess == null) { readout.text = "ჯერ გამოიცანი, რა მოხდება — მერე შეურიე. ვარაუდი მეცნიერების პირველი ნაბიჯია."; return; }
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
            }
            GameSaveManager.Instance.Commit();

            // The teacher answers the prediction first, then explains what actually happened.
            string head = right
                ? (sign == MixSign.Nothing ? "სწორად გამოიცანი: არაფერი მოხდა. " : "ზუსტად ასე! წინასწარ გამოიცანი — ეს უკვე ნამდვილი მეცნიერებაა. ")
                : "შენ ფიქრობდი: „" + Mixing.SignName(guess) + "“, სინამდვილეში — „" + Mixing.SignName(sign) + "“. ვარაუდი, ცდა, დასკვნა — ასე სწავლობენ. ";
            string body = (result != null && result.Equation != "" ? result.Equation + "\n" : "") + (result != null ? result.Why : Mixing.NoReaction(a, b));
            string tail = fresh ? "\nახალი აღმოჩენა! +" + paid + " მონეტა" : result != null ? "\nეს წყვილი უკვე გამოცდილი გაქვს — სცადე ახალი!" : "";
            readout.text = head + body + tail;
            readout.color = right ? LabUI.Green : LabUI.Ink;
            if (fresh) AmbientAudio.Instance?.Sparkle(); else if (right) AudioManager.Instance?.Correct();
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
                    string label = r.Name + "  <size=13>" + r.Formula + "</size>";
                    var button = picked.Contains(id) ? LabUI.Button(column.transform, label, () => Pick(id), 40) : LabUI.Secondary(column.transform, label, () => Pick(id), 40);
                    LabUI.Size(button.gameObject, 40);
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
            LabUI.Size(LabUI.Button(guesses, "შერევა", Mix, 46).gameObject, 46);
            count.text = "აღმოჩენილი შედეგები: " + Mixing.Results.Count(r => LabProgress.Owns(r.ID)) + " / " + Mixing.Results.Length + "  •  ყველა აღმოჩენა კოლექციის „რეაქციების“ ჩანართშია [C]";
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
