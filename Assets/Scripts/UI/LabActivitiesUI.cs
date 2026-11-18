using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    public static class ChemistrySandbox
    {
        public static float Concentration(float moles, float litres) => moles / Mathf.Max(0.001f, litres);
        // Strong monoprotic acid/base, equal 0.1 mol/L stocks, Kw = 1e-14 at 25 C.
        public static double PH(double acidML, double baseML, double waterML)
        {
            double delta = 0.1 * (acidML - baseML) / Math.Max(0.1, acidML + baseML + waterML);
            double root = Math.Sqrt(delta * delta + 4e-14);
            double hydrogen = delta >= 0 ? (delta + root) / 2 : 2e-14 / (root - delta);
            return -Math.Log10(hydrogen);
        }
        public static readonly string[] Symbols = { "", "H", "He", "Li", "Be", "B", "C", "N", "O", "F", "Ne" };
        public static readonly string[] Names = { "", "წყალბადი", "ჰელიუმი", "ლითიუმი", "ბერილიუმი", "ბორი", "ნახშირბადი", "აზოტი", "ჟანგბადი", "ფთორი", "ნეონი" };
        public static bool MoleculeMatches(int goal, int h, int o, int c, int n)
        {
            int[,] targets = { { 2, 1, 0, 0 }, { 0, 2, 0, 0 }, { 0, 2, 1, 0 }, { 3, 0, 0, 1 } };
            return h == targets[goal, 0] && o == targets[goal, 1] && c == targets[goal, 2] && n == targets[goal, 3];
        }
    }

    public sealed class LabActivitiesUI : MonoBehaviour
    {
        private LabWindow window;
        private RectTransform stage;
        private Transform controls;
        private TMP_Text readout;
        private static Sprite circle;
        private int goal, h, o, c, n, protons = 1, neutrons, electrons = 1;
        private float moles = 0.2f, litres = 1, acid, basis, water = 100;
        public string ActiveID { get; private set; }
        public void Open(string id)
        {
            if (id == "membrane" || id == "chromatography" || id == "gas")
            {
                var research = GetComponent<LabResearchUI>();
                if (research == null) research = gameObject.AddComponent<LabResearchUI>();
                research.Open(id);
                return;
            }
            if (window != null) { LabUI.BringToFront(window); return; }
            ActiveID = id;
            var item = Array.Find(PersonalLab.Catalog, entry => entry.ID == id);
            if (item == null) return;
            window = LabUI.Window(item.Name, 1040, 640);
            window.Closed = () => { window = null; ActiveID = null; };
            var subtitle = LabUI.Text(window.Body, "დამოუკიდებელი კვლევა • შეცვალე პირობები და დააკვირდი შედეგს", 18, LabUI.Muted);
            LabUI.Size(subtitle.gameObject, 30);
            var split = LabUI.Row(window.Body, "კვლევა", 24); LabUI.FlexibleHeight(split.gameObject);
            var panel = LabUI.Panel(split.transform, "ცდის მოდელი", LabUI.Navy);
            LabUI.Size(panel.gameObject, 360, 450, 0); stage = panel.rectTransform;
            var column = LabUI.Column(split.transform, "მართვა", 10); LabUI.Size(column.gameObject, 360, -1, 1); controls = column.transform;
            readout = LabUI.Text(window.Body, "", 20, item.Color); LabUI.Size(readout.gameObject, 58);
            readout.enableAutoSizing = true; readout.fontSizeMin = 14; readout.fontSizeMax = 20;
            if (id == "molecule") Molecule(); else if (id == "solution") Solution(); else if (id == "ph") AcidBase(); else Atom();
        }
        private void ClearStage()
        {
            foreach (Transform child in stage) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
        }
        private static Sprite Circle()
        {
            if (circle == null) circle = PixelArt.Make(32, 32, (x, y) => Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f)) < 15.5f ? Color.white : Color.clear);
            return circle;
        }
        private Image Shape(string name, Vector2 point, Vector2 size, Color color, bool round = false)
        {
            var image = LabUI.Panel(stage, name, color); if (round) image.sprite = Circle();
            image.rectTransform.anchorMin = image.rectTransform.anchorMax = Vector2.one * 0.5f;
            image.rectTransform.anchoredPosition = point; image.rectTransform.sizeDelta = size;
            image.raycastTarget = false; return image;
        }
        private void Ball(string symbol, Vector2 point, float diameter, Color color)
        {
            var image = Shape(symbol, point, Vector2.one * diameter, color, true);
            var text = LabUI.Text(image.transform, symbol, diameter > 40 ? 24 : 14, Color.white);
            text.alignment = TextAlignmentOptions.Center; LabUI.Fill(text.rectTransform);
        }
        private void Label(string value, Vector2 point, float width = 400, Color? color = null)
        {
            var text = LabUI.Text(stage, value, 19, color ?? Color.white); text.alignment = TextAlignmentOptions.Center;
            text.rectTransform.anchorMin = text.rectTransform.anchorMax = Vector2.one * 0.5f;
            text.rectTransform.anchoredPosition = point; text.rectTransform.sizeDelta = new Vector2(width, 50);
        }
        private void Bond(Vector2 a, Vector2 b, bool doubleBond = false)
        {
            var delta = b - a;
            var normal = new Vector2(-delta.y, delta.x).normalized;
            for (int line = 0; line < (doubleBond ? 2 : 1); line++)
            {
                var image = Shape("ბმა", (a + b) / 2 + (doubleBond ? normal * (line == 0 ? -5 : 5) : Vector2.zero),
                    new Vector2(delta.magnitude, doubleBond ? 4 : 7), new Color32(192, 207, 228, 255));
                image.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            }
        }
        private void Text(string value, float height = 60) => LabUI.Size(LabUI.Text(controls, value, 18).gameObject, height);
        private void Counter(string label, Func<int> get, Action<int> set, int min, int max, Action changed)
        {
            var row = LabUI.Row(controls, label, 10);
            var caption = LabUI.Text(row.transform, label + ": " + get(), 20); LabUI.Size(caption.gameObject, 42, -1, 1);
            Action<int> adjust = amount => { set(Mathf.Clamp(get() + amount, min, max)); caption.text = label + ": " + get(); changed(); };
            LabUI.Size(LabUI.Secondary(row.transform, "−", () => adjust(-1)).gameObject, 42, 48, 0);
            LabUI.Size(LabUI.Secondary(row.transform, "+", () => adjust(1)).gameObject, 42, 48, 0);
        }
        private Slider Slider(string name, float value, float min, float max, string unit, Action<float> changed)
        {
            var text = LabUI.Text(controls, name + ": " + value.ToString("0.00") + unit, 18); LabUI.Size(text.gameObject, 25);
            var go = LabUI.Object(controls, name, typeof(Slider)); LabUI.Size(go, 30);
            var track = LabUI.Panel(go.transform, "ზოლი", LabUI.Soft); LabUI.Fill(track.rectTransform, 8);
            var fill = LabUI.Panel(track.transform, "შევსება", LabUI.Mint); LabUI.Fill(fill.rectTransform);
            var handle = LabUI.Panel(go.transform, "სახელური", LabUI.Green); handle.rectTransform.sizeDelta = new Vector2(22, 30);
            track.raycastTarget = handle.raycastTarget = true;
            var slider = go.GetComponent<Slider>(); slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform; slider.targetGraphic = handle;
            slider.minValue = min; slider.maxValue = max; slider.SetValueWithoutNotify(value);
            slider.onValueChanged.AddListener(v => { text.text = name + ": " + v.ToString("0.00") + unit; changed(v); });
            return slider;
        }
        private void Molecule()
        {
            h = o = c = n = 0; goal = 0;
            Text("აირჩიე თითოეული ელემენტის ატომების რაოდენობა და ააწყვე ნაჩვენები მოლეკულა.", 64);
            Counter("წყალბადი • H", () => h, v => h = v, 0, 6, DrawMolecule);
            Counter("ჟანგბადი • O", () => o, v => o = v, 0, 6, DrawMolecule);
            Counter("ნახშირბადი • C", () => c, v => c = v, 0, 6, DrawMolecule);
            Counter("აზოტი • N", () => n, v => n = v, 0, 6, DrawMolecule);
            LabUI.Button(controls, "შემდეგი მოლეკულა", () => { goal = (goal + 1) % 4; h = o = c = n = 0; RebuildMoleculeControls(); });
            DrawMolecule();
        }
        private void RebuildMoleculeControls()
        {
            foreach (Transform child in controls) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            // Keep the selected goal when recreating the counters.
            int currentGoal = goal; Molecule(); goal = currentGoal; DrawMolecule();
        }
        private void DrawMolecule()
        {
            ClearStage();
            string[] formulas = { "H2O • წყალი", "O2 • ჟანგბადი", "CO2 • ნახშირორჟანგი", "NH3 • ამიაკი" };
            Label("მიზანი: " + formulas[goal], new Vector2(0, 138));
            bool correct = ChemistrySandbox.MoleculeMatches(goal, h, o, c, n);
            if (correct)
            {
                if (goal == 0) { Bond(new Vector2(-80, -30), Vector2.zero); Bond(Vector2.zero, new Vector2(80, -30)); Ball("O", Vector2.zero, 70, LabUI.Coral); Ball("H", new Vector2(-80, -30), 50, LabUI.Mint); Ball("H", new Vector2(80, -30), 50, LabUI.Mint); }
                if (goal == 1) { Bond(new Vector2(-45, 0), new Vector2(45, 0), true); Ball("O", new Vector2(-45, 0), 70, LabUI.Coral); Ball("O", new Vector2(45, 0), 70, LabUI.Coral); }
                if (goal == 2) { Bond(new Vector2(-95, 0), Vector2.zero, true); Bond(Vector2.zero, new Vector2(95, 0), true); Ball("O", new Vector2(-95, 0), 65, LabUI.Coral); Ball("C", Vector2.zero, 65, LabUI.Muted); Ball("O", new Vector2(95, 0), 65, LabUI.Coral); }
                if (goal == 3) { foreach (var p in new[] { new Vector2(-80, -40), new Vector2(80, -40), new Vector2(0, 80) }) { Bond(Vector2.zero, p); Ball("H", p, 50, LabUI.Mint); } Ball("N", Vector2.zero, 70, LabUI.Green); }
                Label("მოლეკულა აწყობილია!", new Vector2(0, -130), 400, LabUI.Mint);
            }
            else
            {
                int total = h + o + c + n, index = 0;
                string[] symbols = { "H", "O", "C", "N" }; int[] counts = { h, o, c, n };
                Color[] colors = { LabUI.Mint, LabUI.Coral, LabUI.Muted, LabUI.Green };
                for (int element = 0; element < 4; element++) for (int atom = 0; atom < counts[element]; atom++)
                { Ball(symbols[element], new Vector2(-150 + index % 6 * 60, 70 - index / 6 * 60), 44, colors[element]); index++; }
                if (total == 0) Label("დაამატე პირველი ატომი", Vector2.zero);
            }
            readout.text = correct ? "სწორია! ფორმულის ქვედა ინდექსი მოლეკულაში ამ ელემენტის ატომების რაოდენობას გვიჩვენებს." :
                "შენი ატომები: H = " + h + "   O = " + o + "   C = " + c + "   N = " + n + "\nშეცვალე რაოდენობები სამიზნე ფორმულის შესაბამისად.";
        }
        private void Solution()
        {
            moles = 0.2f; litres = 1;
            Text("იდეალიზებული ხსნარი: ნივთიერება მთლიანად იხსნება. მიზანი — 0.50 მოლ/ლ კონცენტრაცია.", 72);
            var amount = Slider("ნივთიერება", moles, 0, 1, " მოლი", v => { moles = v; DrawSolution(); });
            var volume = Slider("ხსნარის მოცულობა", litres, 0.2f, 2, " ლ", v => { litres = v; DrawSolution(); });
            LabUI.Button(controls, "წყლის დამატება • +0.20 ლ", () => volume.value = Mathf.Min(2, litres + 0.2f));
            LabUI.Secondary(controls, "ნახევარი ხსნარის ჩამოსხმა", () =>
            {
                if (litres < 0.4f) return;
                float fraction = 0.5f; moles *= fraction; litres *= fraction;
                amount.SetValueWithoutNotify(moles); volume.SetValueWithoutNotify(litres);
                // Refresh readouts through the same slider event path.
                amount.onValueChanged.Invoke(moles); volume.onValueChanged.Invoke(litres);
            });
            DrawSolution();
        }
        private void DrawSolution()
        {
            ClearStage();
            float concentration = ChemistrySandbox.Concentration(moles, litres);
            Label("C = n / V", new Vector2(0, 140));
            Shape("ჭიქა", new Vector2(0, -15), new Vector2(250, 235), new Color32(183, 203, 219, 255));
            Shape("შიდა სივრცე", new Vector2(0, -10), new Vector2(234, 228), LabUI.Navy);
            float height = litres / 2 * 210;
            Shape("ხსნარი", new Vector2(0, -120 + height / 2), new Vector2(230, height), Color.Lerp(new Color(0.5f, 0.8f, 0.94f), LabUI.Green, Mathf.Clamp01(concentration / 2)));
            int particles = Mathf.RoundToInt(moles * 45);
            for (int i = 0; i < particles; i++)
                Shape("გახსნილი ნაწილაკი", new Vector2(-100 + (i * 47 % 200), -112 + (i * 31 % 197) / 197f * Mathf.Max(1, height - 12)), Vector2.one * 7, Color.white, true);
            readout.text = "კონცენტრაცია: " + concentration.ToString("0.00") + " მოლ/ლ   •   n = " + moles.ToString("0.00") + " მოლი   •   V = " + litres.ToString("0.00") + " ლ\n" +
                (Mathf.Abs(concentration - 0.5f) < 0.03f ? "მიზანი მიღწეულია! " : "") + "წყლის დამატება აზავებს ხსნარს; ჩამოსხმა კონცენტრაციას არ ცვლის.";
        }
        private void AcidBase()
        {
            acid = basis = 0; water = 100;
            Text("ძლიერი ერთპროტონიანი მჟავისა და ფუძის ვირტუალური მოდელი, 25°C. ორივე საწყისი ხსნარი: 0.10 მოლ/ლ.", 64);
            Slider("მჟავის ხსნარი", acid, 0, 100, " მლ", v => { acid = v; DrawPH(); });
            Slider("ფუძის ხსნარი", basis, 0, 100, " მლ", v => { basis = v; DrawPH(); });
            Slider("დამატებული წყალი", water, 100, 1000, " მლ", v => { water = v; DrawPH(); });
            Text("მიზანი: დაამატე მჟავაც და ფუძეც, შემდეგ გაანეიტრალე ხსნარი. წყლით განზავება pH-ს 7-ისკენ აახლოებს.", 65);
            DrawPH();
        }
        private void DrawPH()
        {
            ClearStage();
            double ph = ChemistrySandbox.PH(acid, basis, water);
            Color tint = ph < 6.9 ? LabUI.Coral : ph > 7.1 ? LabUI.Green : LabUI.Mint;
            Label("pH = " + ph.ToString("0.00"), new Vector2(0, 140), 400, tint);
            Shape("ჭიქა", new Vector2(0, 10), new Vector2(190, 170), new Color32(186, 204, 216, 255));
            Shape("ხსნარი", new Vector2(0, 0), new Vector2(174, 130), tint);
            for (int i = 0; i <= 14; i++)
                Shape("pH ფერი", new Vector2(-175 + i * 25, -115), new Vector2(24, 24), i < 7 ? Color.Lerp(LabUI.Coral, LabUI.Gold, i / 7f) : Color.Lerp(LabUI.Mint, LabUI.Green, (i - 7) / 7f));
            Shape("მაჩვენებელი", new Vector2(-175 + (float)ph * 25, -91), new Vector2(5, 16), Color.white);
            Label("0          მჟავა          7          ფუძე          14", new Vector2(0, -150));
            bool neutral = Math.Abs(ph - 7) < 0.05 && acid >= 10 && basis >= 10;
            readout.text = (neutral ? "ნეიტრალიზაცია მიღწეულია! " : ph < 6.95 ? "ხსნარი მჟავეა. " : ph > 7.05 ? "ხსნარი ტუტეა. " : "ხსნარი ნეიტრალურია. ") +
                "H⁺ + OH⁻ → H2O\nფერი ინდიკატორის სქემაა. მოდელი ითვლის ჭარბ იონებსა და წყლის თვითიონიზაციას.";
        }
        private void Atom()
        {
            protons = electrons = 1; neutrons = 0;
            Text("ელემენტს პროტონების რაოდენობა განსაზღვრავს. ააწყვე ჟანგბად-16-ის ნეიტრალური ატომი: 8 პროტონი, 8 ნეიტრონი და 8 ელექტრონი.", 76);
            Counter("პროტონები", () => protons, v => protons = v, 1, 10, DrawAtom);
            Counter("ნეიტრონები", () => neutrons, v => neutrons = v, 0, 12, DrawAtom);
            Counter("ელექტრონები", () => electrons, v => electrons = v, 0, 10, DrawAtom);
            Text("მასური რიცხვი = პროტონები + ნეიტრონები\nმუხტი = პროტონები − ელექტრონები\nბირთვის სტაბილურობა ამ მოდელში არ ითვლება.", 85);
            DrawAtom();
        }
        private void DrawAtom()
        {
            ClearStage();
            Label(ChemistrySandbox.Symbols[protons] + " • " + ChemistrySandbox.Names[protons], new Vector2(0, 145));
            for (int shell = 0; shell < 2; shell++) for (int i = 0; i < 48; i++)
            {
                float angle = i * Mathf.PI / 24;
                Shape("შრის ხაზი", new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (shell == 0 ? 80 : 125), Vector2.one * 2, LabUI.Muted, true);
            }
            for (int i = 0; i < protons + neutrons; i++)
            {
                float angle = i * 2.4f; float radius = Mathf.Sqrt(i) * 7;
                Shape("ბირთვის ნაწილაკი", new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius, Vector2.one * 15, i < protons ? LabUI.Coral : LabUI.Gold, true);
            }
            for (int i = 0; i < electrons; i++)
            {
                int shell = i < 2 ? 0 : 1; int index = shell == 0 ? i : i - 2;
                float angle = index * Mathf.PI * 2 / (shell == 0 ? 2 : 8);
                Ball("−", new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (shell == 0 ? 80 : 125), 22, LabUI.Mint);
            }
            int charge = protons - electrons;
            Label("მასური რიცხვი: " + (protons + neutrons) + "    მუხტი: " + (charge > 0 ? "+" : "") + charge, new Vector2(0, -150));
            readout.text = (protons == 8 && neutrons == 8 && electrons == 8 ? "მიზანი მიღწეულია! " : "") +
                (charge == 0 ? "ატომი ნეიტრალურია." : "ეს იონია: პროტონებისა და ელექტრონების რაოდენობა განსხვავდება.") +
                "\nწითელი — პროტონი • ოქროსფერი — ნეიტრონი • მწვანე — ელექტრონი. შრეები სქემატურია.";
        }
    }
}


