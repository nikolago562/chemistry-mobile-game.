using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    /// <summary>
    /// The heart of the game, at the reaction table, laid out for thumbs: a grid of jars, a big
    /// beaker, big guess tiles and one big "mix" button. Mixing uses up ingredients, new results
    /// pop a "NEW!" card and fill the discovery book, and a mix with the sign a classmate ordered
    /// fills that order.
    /// </summary>
    public sealed class MixBenchUI : MonoBehaviour
    {
        public const int Pay = 5, PayGuess = 3, DiscoveryXP = 5;
        /// <summary>The opening mix. Until it is found the bench only takes these two jars.</summary>
        public static readonly string[] FirstPair = { "vinegar", "soda" };
        public static bool Teaching => !LabProgress.Owns(Mixing.Key(FirstPair[0], FirstPair[1]));

        private LabWindow window;
        private Transform shelf, guesses, ordersRow;
        private RectTransform stage;
        private TMP_Text headline, detail, pairText;
        private Button mixButton, book;
        private NextGlow next;
        private readonly List<string> picked = new List<string>();
        private readonly Dictionary<string, RectTransform> spots = new Dictionary<string, RectTransform>();
        private MixSign? guess;
        private Coroutine show;
        private string last = "";

        public bool IsOpen => window != null;
        /// <summary>Everything the last action said, headline and explanation together.</summary>
        public string Feedback => last;
        public IReadOnlyList<string> Picked => picked;
        public MixSign? Guessed => guess;
        /// <summary>Where a jar ("vinegar"…), the guess tiles ("guess") or the mix button ("mix") is, for the tutorial pointer.</summary>
        public RectTransform Spot(string key) => window != null && spots.TryGetValue(key, out var rect) && rect != null ? rect : null;

        public void Open()
        {
            if (window != null) { LabUI.BringToFront(window); return; }
            picked.Clear(); guess = null; last = "";
            window = LabUI.Window("შერევის მაგიდა", 1260, 712);
            window.Closed = () => { window = null; show = null; spots.Clear(); };
            next = NextGlow.On(window);

            // Top strip: today's orders and the book, whose fill is the bench's goal.
            var top = LabUI.Row(window.Body, "ზედა ზოლი", 10); LabUI.Size(top.gameObject, 58);
            var orderBox = LabUI.Row(top.transform, "შეკვეთები", 8); LabUI.Size(orderBox.gameObject, 62, -1, 1); ordersRow = orderBox.transform;
            book = LabUI.Button(top.transform, "", Book, 62); book.GetComponent<Image>().color = new Color32(120, 86, 160, 255);
            LabUI.Size(book.gameObject, 62, 240, 0);

            // Middle: jars | beaker | guesses and the mix button.
            var middle = LabUI.Row(window.Body, "მაგიდა", 14); LabUI.FlexibleHeight(middle.gameObject);
            middle.childForceExpandHeight = true;
            var jars = LabUI.Object(middle.transform, "თარო", typeof(GridLayoutGroup));
            var grid = jars.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(108, 100); grid.spacing = new Vector2(6, 6);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 4;
            LabUI.Size(jars, 418, 450, 0); shelf = jars.transform;

            var panel = LabUI.Panel(middle.transform, "ჭიქა", LabUI.Navy); LabUI.Size(panel.gameObject, 418, 290, 0); stage = panel.rectTransform;
            pairText = LabUI.Text(panel.transform, "", 24, LabUI.Gold); pairText.alignment = TextAlignmentOptions.Center; pairText.fontStyle = FontStyles.Bold;
            pairText.enableAutoSizing = true; pairText.fontSizeMin = 15; pairText.fontSizeMax = 24;
            Place(pairText.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(270, 44));

            var right = LabUI.Column(middle.transform, "ვარაუდი", 8); LabUI.Size(right.gameObject, -1, -1, 1);
            var ask = LabUI.Text(right.transform, "რა მოხდება?", 20, LabUI.Ink); ask.fontStyle = FontStyles.Bold; LabUI.Size(ask.gameObject, 28);
            var tiles = LabUI.Object(right.transform, "ვარაუდები", typeof(GridLayoutGroup));
            var tileGrid = tiles.GetComponent<GridLayoutGroup>();
            tileGrid.cellSize = new Vector2(206, 88); tileGrid.spacing = new Vector2(8, 8);
            tileGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; tileGrid.constraintCount = 2;
            LabUI.Size(tiles, 280); guesses = tiles.transform; spots["guess"] = (RectTransform)tiles.transform;
            mixButton = LabUI.Button(right.transform, "შერევა!", Mix, 96); mixButton.GetComponent<Image>().color = new Color32(224, 160, 60, 255);
            LabUI.Size(mixButton.gameObject, 96); spots["mix"] = (RectTransform)mixButton.transform;

            // Bottom: one big line for what happened, one small line for why.
            var result = LabUI.Card(window.Body, "შედეგი", LabUI.Soft, 8); LabUI.Size(result.gameObject, 84);
            result.GetComponent<VerticalLayoutGroup>().spacing = 2;
            headline = LabUI.Text(result.transform, "შეეხე ორ ქილას, გამოიცანი და შეურიე!", 22, LabUI.Green); headline.fontStyle = FontStyles.Bold;
            headline.enableAutoSizing = true; headline.fontSizeMin = 16; headline.fontSizeMax = 25; LabUI.Size(headline.gameObject, 30);
            detail = LabUI.Text(result.transform, "", 15, LabUI.Ink);
            detail.enableAutoSizing = true; detail.fontSizeMin = 12; detail.fontSizeMax = 17; LabUI.Size(detail.gameObject, 38);
            Refresh();
            Beaker(null, null);
        }

        public void Pick(string id)
        {
            if (window == null || Mixing.Get(id) == null) return;
            if (picked.Remove(id)) { Refresh(); return; }
            if (Teaching && !FirstPair.Contains(id)) { Say("ჯერ ძმარი და სოდა!", "პირველ ცდას მასწავლებელი გვთავაზობს — დანარჩენს მერე სცდი."); return; }
            if (Mixing.Stock(id) == 0 && !Buy(id)) return;
            if (picked.Count == 2) picked.RemoveAt(1);
            picked.Add(id);
            AudioManager.Instance?.Click();
            Refresh();
        }

        /// <summary>An empty jar buys one more from the storage shop on tap; greenhouse plants are picked there.</summary>
        private bool Buy(string id)
        {
            var material = LabProgress.Material(id);
            var data = GameSaveManager.Instance.Data;
            string name = Mixing.Get(id).Name;
            if (material == null || material.Price == 0) { Say(name + " გამოგელია.", "ის სათბურში იზრდება — მოკრიფე იქ."); return false; }
            if (data.chemCoins < material.Price) { Say("ქიმქოინები არ გყოფნის.", name + " " + material.Price + " ღირს. შეასრულე შეკვეთა!"); return false; }
            data.chemCoins -= material.Price;
            LabProgress.Give(id, 1);
            GameSaveManager.Instance.Commit();
            AudioManager.Instance?.Coin();
            Say("იყიდე: " + name, "−" + material.Price + " ქიმქოინი");
            return true;
        }

        public void Guess(MixSign sign) { if (window == null) return; guess = sign; AudioManager.Instance?.Click(); Refresh(); }

        public void Mix()
        {
            if (window == null) return;
            if (picked.Count < 2) { Say("შეეხე ორ ქილას.", "ორი ნივთიერება გვჭირდება."); return; }
            if (guess == null) { Say("ჯერ გამოიცანი, რა მოხდება!", "აირჩიე ერთი მარჯვნივ — ვარაუდი მეცნიერების პირველი ნაბიჯია."); return; }
            if (picked.Any(p => Mixing.Stock(p) == 0)) { Say("ინგრედიენტი გამოგელია.", "შეეხე ქილას, რომ იყიდო."); return; }
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

            // What happened, big; whether the guess was right and why, small.
            string head = Mixing.SignName(sign) + "!" + (right ? "  სწორად გამოიცანი!" : "");
            string why = (right ? "" : "შენ ფიქრობდი: „" + Mixing.SignName(guess) + "“, სინამდვილეში — „" + Mixing.SignName(sign) + "“. ")
                + (result != null ? result.Why : Mixing.NoReaction(a, b))
                + (result != null && !fresh ? " (ეს წყვილი უკვე გამოცდილი გაქვს.)" : "")
                + (order != null ? "  " + order.Name + ": შეკვეთა შესრულდა! +" + Orders.Pay + " ქიმქოინი" : "");
            Say(head, why, right ? LabUI.Green : LabUI.Ink);
            if (fresh || order != null) AmbientAudio.Instance?.Sparkle(); else if (right) AudioManager.Instance?.Correct();
            guess = null;
            Refresh();
            Beaker(a, b, result);
            if (fresh) StartCoroutine(NewCard(result, paid));
            if (order != null) LabGame.Instance?.HUD.Refresh();
        }

        private void Say(string head, string why, Color? color = null)
        {
            last = head + "\n" + why;
            if (headline == null) return;
            headline.text = head; headline.color = color ?? LabUI.Green;
            detail.text = why;
        }

        // ------------------------------------------------------------ layout

        private void Refresh()
        {
            Ui.Clear(shelf);
            foreach (var r in Mixing.Reagents) Jar(r);
            Ui.Clear(guesses);
            for (int i = 0; i < Mixing.SignNames.Length; i++) GuessTile((MixSign)i);
            Ui.Clear(ordersRow);
            foreach (var o in Orders.Today)
            {
                bool done = Orders.Done(o);
                var card = LabUI.Panel(ordersRow, o.Name, done ? new Color32(176, 222, 190, 255) : LabUI.Paper);
                LabUI.Size(card.gameObject, 62, -1, 1);
                var icon = LabUI.Object(card.transform, "ნიშანი", typeof(Image)).GetComponent<Image>();
                icon.sprite = done ? LabIcons.Get(LabIcon.Check) : SignSprite(o.Wants); icon.preserveAspect = true; icon.raycastTarget = false;
                Place(icon.rectTransform, new Vector2(0, 0.5f), new Vector2(28, 0), new Vector2(34, 34));
                var text = LabUI.Text(card.transform, "<b>" + o.Name + "</b>" + (done ? "" : " უკვეთავს:") + "\n<size=13>" + (done ? "შესრულდა!" : Mixing.SignName(o.Wants)) + "</size>", 16, LabUI.Ink);
                text.enableAutoSizing = true; text.fontSizeMin = 10; text.fontSizeMax = 16;
                LabUI.Fill(text.rectTransform, 6); text.rectTransform.offsetMin = new Vector2(52, 6);
            }
            int found = Mixing.Results.Count(r => LabProgress.Owns(r.ID));
            NextGlow.Progress(book, "წიგნი " + found + "/" + Mixing.Results.Length, found, Mixing.Results.Length);
            // The beaker's two slots: "? + ?" fills in as jars are picked.
            pairText.text = (picked.Count > 0 ? Mixing.Get(picked[0]).Name : "?") + "  +  " + (picked.Count > 1 ? Mixing.Get(picked[1]).Name : "?");
            // The big button and the gold ring both say what comes next: jars, then a guess, then mix.
            bool ready = picked.Count == 2 && guess != null;
            mixButton.interactable = ready;
            mixButton.GetComponentInChildren<TMP_Text>().text = picked.Count < 2 ? "აირჩიე 2 ქილა" : guess == null ? "რა მოხდება? გამოიცანი" : "შეურიე!";
            next.Target = picked.Count < 2 ? (RectTransform)shelf : guess == null ? (RectTransform)guesses : (RectTransform)mixButton.transform;
        }

        /// <summary>One jar on the shelf: its colour in a glass, the name and how many are left (or the price).</summary>
        private void Jar(Reagent r)
        {
            string id = r.ID;
            bool selected = picked.Contains(id), locked = Teaching && !FirstPair.Contains(id);
            var tile = LabUI.Button(shelf, "", () => Pick(id), 100);
            tile.GetComponent<Image>().color = selected ? LabUI.Mint : locked ? new Color32(205, 205, 196, 255) : LabUI.Paper;
            spots[id] = (RectTransform)tile.transform;
            var liquid = LabUI.Object(tile.transform, "სითხე", typeof(Image)).GetComponent<Image>();
            liquid.sprite = Glass(true); liquid.color = r.Color; liquid.raycastTarget = false;
            Place(liquid.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(38, 46));
            var glass = LabUI.Object(tile.transform, "მინა", typeof(Image)).GetComponent<Image>();
            glass.sprite = Glass(false); glass.raycastTarget = false;
            Place(glass.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(38, 46));
            var label = tile.GetComponentInChildren<TMP_Text>();
            label.text = r.Name; label.color = LabUI.Ink; label.fontStyle = FontStyles.Bold;
            label.fontSizeMin = 11; label.fontSizeMax = 15; label.alignment = TextAlignmentOptions.Bottom;
            label.rectTransform.offsetMin = new Vector2(4, 11); label.rectTransform.offsetMax = new Vector2(-4, -54);
            int stock = Mixing.Stock(id);
            var material = LabProgress.Material(id);
            string badgeText = Mixing.Free(id) ? "∞" : stock > 0 ? "×" + stock : material != null && material.Price > 0 ? material.Price.ToString() : "სათბ.";
            var badge = LabUI.Panel(tile.transform, "რაოდენობა", stock > 0 ? LabUI.Navy : LabUI.Coral);
            Place(badge.rectTransform, Vector2.one, new Vector2(-22, -16), new Vector2(42, 26));
            var count = LabUI.Text(badge.transform, badgeText, 12, Color.white); count.alignment = TextAlignmentOptions.Center; count.fontStyle = FontStyles.Bold;
            count.enableAutoSizing = true; count.fontSizeMin = 10; count.fontSizeMax = 15;
            LabUI.Fill(count.rectTransform, 2);
        }

        private void GuessTile(MixSign sign)
        {
            var tile = LabUI.Button(guesses, "", () => Guess(sign), 88);
            tile.GetComponent<Image>().color = guess == sign ? LabUI.Green : LabUI.Paper;
            var icon = LabUI.Object(tile.transform, "ნიშანი", typeof(Image)).GetComponent<Image>();
            icon.sprite = SignSprite(sign); icon.preserveAspect = true; icon.raycastTarget = false;
            Place(icon.rectTransform, new Vector2(0, 0.5f), new Vector2(36, 4), new Vector2(46, 46));
            var label = tile.GetComponentInChildren<TMP_Text>();
            label.text = Mixing.SignNames[(int)sign]; label.color = guess == sign ? Color.white : LabUI.Ink;
            label.alignment = TextAlignmentOptions.Left; label.fontSizeMin = 13; label.fontSizeMax = 19;
            label.rectTransform.offsetMin = new Vector2(68, 12);
        }

        private static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = anchor; rect.anchoredPosition = position; rect.sizeDelta = size;
        }

        // ------------------------------------------------------------ the book and the "NEW!" card

        /// <summary>The discovery book: found results in full, the rest as "ingredient + ???" hints.</summary>
        public void Book()
        {
            var book = LabUI.Window("აღმოჩენების წიგნი", 900, 620);
            Ui.Line(book.Body, "აღმოჩენილია " + Mixing.Results.Count(r => LabProgress.Owns(r.ID)) + " / " + Mixing.Results.Length + ". ერთი ინგრედიენტი უკვე იცი — მეორე შენ იპოვე!", 17, LabUI.Muted, 28);
            var scroll = LabUI.Scroll(book.Body);
            foreach (var r in Mixing.Results)
            {
                bool owned = LabProgress.Owns(r.ID);
                var card = LabUI.Card(scroll, r.ID, owned ? LabUI.Paper : new Color(0.86f, 0.86f, 0.82f), 10);
                var row = LabUI.Row(card.transform, "ჩანაწერი", 12); LabUI.Size(row.gameObject, 52);
                var icon = LabUI.Panel(row.transform, "ფერი", owned ? r.Color : new Color(0.7f, 0.7f, 0.7f)); LabUI.Size(icon.gameObject, 48, 48, 0);
                var text = LabUI.Text(row.transform, owned ? "<b>" + r.Title + "</b>  •  " + Mixing.SignName(r.Sign) + "\n<size=14>" + r.Why + "</size>"
                    : "<b>" + Mixing.Get(r.A).Name + " + ???</b>\n<size=14>ჯერ არ გიცდია. რას შეურევდი?</size>", 17, owned ? LabUI.Ink : LabUI.Muted);
                text.enableAutoSizing = true; text.fontSizeMin = 11; text.fontSizeMax = 17;
                LabUI.Size(text.gameObject, 52, -1, 1);
            }
        }

        /// <summary>A gold card pops over the bench for every first discovery, then fades away.</summary>
        private IEnumerator NewCard(MixResult result, int paid)
        {
            var card = LabUI.Panel(window.Card, "ახალი აღმოჩენა", LabUI.Gold);
            card.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            Place(card.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(460, 210));
            var group = card.gameObject.AddComponent<CanvasGroup>(); group.blocksRaycasts = false;
            var swatch = LabUI.Panel(card.transform, "ფერი", result.Color);
            Place(swatch.rectTransform, new Vector2(0, 0.5f), new Vector2(70, 0), new Vector2(92, 92));
            var text = LabUI.Text(card.transform, "<size=30><b>ახალი აღმოჩენა!</b></size>\n<b>" + result.Title + "</b>\n" + Mixing.SignName(result.Sign) + "\n<size=17>+" + paid + " ქიმქოინი  •  +" + DiscoveryXP + " XP</size>", 21, LabUI.Ink);
            text.enableAutoSizing = true; text.fontSizeMin = 12; text.fontSizeMax = 21;
            LabUI.Fill(text.rectTransform, 14); text.rectTransform.offsetMin = new Vector2(132, 14);
            for (float t = 0; t < 2.6f && card != null; t += Time.unscaledDeltaTime)
            {
                float pop = t < 0.3f ? Mathf.LerpUnclamped(0.6f, 1, 1 + 2.7f * Mathf.Pow(t / 0.3f - 1, 3) + 1.7f * Mathf.Pow(t / 0.3f - 1, 2)) : 1;
                card.transform.localScale = Vector3.one * pop;
                group.alpha = t > 2.2f ? 1 - (t - 2.2f) / 0.4f : 1;
                yield return null;
            }
            if (card != null) Destroy(card.gameObject);
        }

        // ------------------------------------------------------------ the beaker

        private void Beaker(Reagent a, Reagent b, MixResult result = null)
        {
            if (show != null) StopCoroutine(show);
            show = null;
            foreach (Transform child in stage) if (child != pairText.transform) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            Shape("შუქი", new Vector2(0, -40), new Vector2(210, 270), new Color(1, 1, 1, 0.12f));
            var start = a == null ? new Color(0.6f, 0.75f, 0.85f, 0.5f) : Color.Lerp(a.Color, b.Color, 0.5f);
            var liquid = Shape("სითხე", new Vector2(0, -100), new Vector2(194, 140), start);
            if (a == null) return;
            var label = LabUI.Text(stage, "", 20, Color.white);
            label.alignment = TextAlignmentOptions.Center; label.fontStyle = FontStyles.Bold;
            Place(label.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 110), new Vector2(280, 40));
            show = StartCoroutine(Animate(liquid, label, start, result));
        }

        private IEnumerator Animate(Image liquid, TMP_Text label, Color start, MixResult result)
        {
            var sign = result?.Sign ?? MixSign.Nothing;
            var end = result?.Color ?? start;
            var bits = new List<(Image image, Vector2 from, float delay)>();
            if (sign == MixSign.Gas || sign == MixSign.Solid || sign == MixSign.Dissolves)
                for (int i = 0; i < 18; i++)
                {
                    var from = sign == MixSign.Gas ? new Vector2(Random.Range(-84f, 84f), -165) : new Vector2(Random.Range(-84f, 84f), sign == MixSign.Solid ? -40 : -70);
                    var color = sign == MixSign.Gas ? new Color(1, 1, 1, 0.85f) : sign == MixSign.Solid ? Color.Lerp(end, Color.white, 0.3f) : Color.white;
                    bits.Add((Shape("ნაწილაკი", from, Vector2.one * (sign == MixSign.Gas ? Random.Range(9f, 18f) : 11f), color, true), from, Random.Range(0f, 1.2f)));
                }
            for (float t = 0; t < 2.4f; t += Time.unscaledDeltaTime)
            {
                if (liquid == null) yield break;
                liquid.color = Color.Lerp(start, end, Mathf.Clamp01(t / 1.4f));
                if (sign == MixSign.Heat) liquid.color = Color.Lerp(liquid.color, LabUI.Coral, 0.35f * Mathf.Abs(Mathf.Sin(t * 4)));
                // The beaker jolts once when something happens; nothing happens quietly.
                if (sign != MixSign.Nothing && t < 0.35f) liquid.rectTransform.anchoredPosition = new Vector2(Mathf.Sin(t * 70) * 5 * (1 - t / 0.35f), -100);
                foreach (var (image, from, delay) in bits)
                {
                    float s = Mathf.Clamp01((t - delay) / 1.1f);
                    if (sign == MixSign.Gas) image.rectTransform.anchoredPosition = from + new Vector2(Mathf.Sin(t * 6 + delay * 9) * 6, s * 150);
                    else if (sign == MixSign.Solid) image.rectTransform.anchoredPosition = from + new Vector2(0, -s * 120);
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
            if (round) image.sprite = Disc;
            Place(image.rectTransform, new Vector2(0.5f, 0.5f), point, size);
            image.raycastTarget = false;
            return image;
        }

        // ------------------------------------------------------------ pixel sprites

        private static Sprite disc, glass, fill;
        private static readonly Dictionary<MixSign, Sprite> signs = new Dictionary<MixSign, Sprite>();
        private static Sprite Disc => disc ??= PixelArt.Make(32, 32, (x, y) => Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f)) < 15.5f ? Color.white : Color.clear);

        /// <summary>A small jar: <paramref name="liquid"/> is the white fill to tint, otherwise the glass outline and shine.</summary>
        public static Sprite Glass(bool liquid)
        {
            if (liquid && fill != null) return fill;
            if (!liquid && glass != null) return glass;
            var ink = new Color32(46, 58, 64, 255);
            var sprite = PixelArt.Make(10, 12, (x, y) =>
            {
                bool neck = y >= 9 && x >= 3 && x <= 6, body = y <= 8 && x >= 1 && x <= 8;
                bool edge = neck ? x == 3 || x == 6 || y == 11 : body && (x == 1 || x == 8 || y == 0 || (y == 8 && (x < 3 || x > 6)));
                if (liquid) return body && !edge && y <= 6 ? Color.white : Color.clear;
                if (edge) return ink;
                if (body && x == 2 && y >= 2 && y <= 6) return new Color(1, 1, 1, 0.7f);
                return (neck || body) && y > 6 ? new Color(0.85f, 0.95f, 1f, 0.35f) : Color.clear;
            });
            return liquid ? fill = sprite : glass = sprite;
        }

        /// <summary>A 12px icon for each sign, so the guess tiles read without the words.</summary>
        public static Sprite SignSprite(MixSign sign)
        {
            if (signs.TryGetValue(sign, out var cached)) return cached;
            if (sign == MixSign.Heat) return signs[sign] = LabIcons.Get(LabIcon.Flame);
            Color ink = new Color32(46, 58, 64, 255), blue = new Color32(120, 190, 235, 255), white = Color.white;
            Color32[] rainbow = { new Color32(220, 70, 90, 255), new Color32(240, 190, 70, 255), new Color32(90, 190, 120, 255), new Color32(90, 140, 230, 255) };
            var sprite = PixelArt.Make(12, 12, (x, y) =>
            {
                switch (sign)
                {
                    case MixSign.Gas:
                        foreach (var (cx, cy, r) in new[] { (3.5f, 3.5f, 2.6f), (8f, 6f, 2.2f), (4.5f, 9f, 1.8f) })
                        {
                            float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                            if (d < r - 0.9f) return blue; if (d < r) return ink;
                        }
                        return Color.clear;
                    case MixSign.Colour: return x >= 1 && x <= 10 && y >= 1 && y <= 10 ? (Color)rainbow[(x - 1) / 3 % 4] : Color.clear;
                    case MixSign.Solid:
                        if (y <= 1 && x >= 1 && x <= 10) return ink;
                        return y >= 2 && y <= 4 && (x + y) % 2 == 0 && x >= 2 && x <= 9 ? (Color)new Color32(110, 170, 230, 255) : Color.clear;
                    case MixSign.Dissolves:
                        if (x >= 4 && x <= 8 && y >= 6 && y <= 10) return x == 4 || x == 8 || y == 6 || y == 10 ? ink : white;
                        return y >= 1 && y <= 3 && x % 3 == 1 ? (Color)blue : Color.clear;
                    default: // nothing: a calm dash
                        return y >= 5 && y <= 6 && x >= 2 && x <= 9 ? ink : Color.clear;
                }
            });
            return signs[sign] = sprite;
        }
    }
}
