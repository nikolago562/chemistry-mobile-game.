using System.Linq;
using TMPro;
using UnityEngine;

namespace ChemistryGame
{
    public sealed class FormulaNotebookUI : MonoBehaviour
    {
        private LabWindow window;
        private RectTransform content;
        private readonly System.Collections.Generic.List<UnityEngine.UI.Button> tabs = new System.Collections.Generic.List<UnityEngine.UI.Button>();
        private const string Intro =
            "როგორ ვისწავლოთ\nყოველი გაკვეთილი ასე მიდის: ახსნა → ჩვენება → ვარჯიში მინიშნებით → დამოუკიდებელი ვარჯიში → პატარა ტესტი → ჯილდო.\n\n" +
            "მიზნის ბარათი\nზედა მარცხენა ბარათზე ყოველთვის წერია შემდეგი ნაბიჯი.\n\n" +
            "რვეული [N]\nაქ ინახება ყოველი დასრულებული გაკვეთილის მოკლე ჩანაწერი. ცდისა და ტესტის დროსაც შეგიძლია გახსნა.\n\n" +
            "სასწავლო რუკა [M]\nაჩვენებს გავლილ და შემდეგ გაკვეთილებს, შენს შეცდომებს და გამეორების ღილაკებს.\n\n" +
            "კლასის მაგიდები\nმასწავლებლის მაგიდასთან ახალ გაკვეთილს იწყებ. სხვა მაგიდები შესაბამისი თეორიის შემდეგ იხსნება და იქ ნასწავლს იმეორებ.\n\n" +
            "უსაფრთხოება\nლაბორატორიაში ხალათი და სათვალე ყოველთვის გაცვია. შეცდომა საშიში არ არის: მინიშნებას მიიღებ და თავიდან სცდი.";
        private readonly string[] advanced =
        {
            "შეერთება\nორი ან მეტი ნივთიერება ერთ პროდუქტს წარმოქმნის.\n2H2 + O2 → 2H2O\n\n" +
            "დაშლა\nერთი ნივთიერება რამდენიმე პროდუქტად იშლება.\n2H2O2 → 2H2O + O2\n\n" +
            "ერთმაგი ჩანაცვლება\nელემენტი ნაერთში სხვა ელემენტს ცვლის.\nZn + 2HCl → ZnCl2 + H2\n\n" +
            "ორმაგი ჩანაცვლება\nორი ნაერთი ერთმანეთში იონებს ცვლის.\nAgNO3 + NaCl → AgCl↓ + NaNO3\n\n" +
            "წვა\nნახშირწყალბადის სრული წვისას მიიღება CO2 და H2O.\nCH4 + 2O2 → CO2 + 2H2O",

            "ვალენტობა და იონის მუხტი\nვალენტობა აღწერს ბმების წარმოქმნის უნარს, იონის მუხტი კი ელექტრონების სიჭარბეს ან დეფიციტს.\n\n" +
            "გავრცელებული კათიონები\nNa⁺, K⁺ — +1\nMg²⁺, Ca²⁺, Zn²⁺ — +2\nAl³⁺ — +3\nFe²⁺ / Fe³⁺, Cu⁺ / Cu²⁺ — ცვლადი მუხტები\nNH4⁺ — ამონიუმი\n\n" +
            "გავრცელებული ანიონები\nCl⁻, Br⁻, I⁻ — −1\nO²⁻, S²⁻ — −2\nOH⁻ — ჰიდროქსიდი\nNO3⁻ — ნიტრატი\nSO4²⁻ — სულფატი\nCO3²⁻ — კარბონატი\n\n" +
            "ნეიტრალურ ნაერთში მუხტების ჯამი ნულია. Ca²⁺-სა და Cl⁻-ისგან მიიღება CaCl2.",

            "ხსნადობის წესები წყალში\nნიტრატები და ტუტე ლითონების მარილების უმეტესობა ხსნადია. ამონიუმის მარილების უმეტესობაც ხსნადია.\n\n" +
            "ქლორიდები უმეტესად ხსნადია. გამონაკლისებია AgCl, PbCl2 და Hg2Cl2.\nსულფატები უმეტესად ხსნადია. BaSO4 და PbSO4 მცირედ ხსნადია.\n" +
            "კარბონატები და ჰიდროქსიდები ხშირად მცირედ ხსნადია, თუმცა ტუტე ლითონების ნაერთები გამონაკლისია. ტემპერატურაც მნიშვნელოვანია.\n\n" +
            "დაბალანსების ნაბიჯები\n1. ჩამოწერე თითოეული ელემენტის ატომების რაოდენობა ორივე მხარეს.\n2. შეცვალე ფორმულების წინ კოეფიციენტები. ინდექსები უცვლელად დატოვე.\n" +
            "3. რთული ნაერთებით დაიწყე. H და O ხშირად ბოლოს უფრო ადვილად ბალანსდება.\n4. შეამცირე კოეფიციენტები უმცირეს მთელ რიცხვებამდე.\n\n" +
            "მოლი და თანაფარდობა\n2H2 + O2 → 2H2O ნიშნავს 2 : 1 : 2 მოლურ თანაფარდობას.\nნივთიერების რაოდენობაა n = m / M, სადაც m მასაა, M კი მოლური მასა."
        };
        public void Toggle() { if (window != null) window.Close(); else Open(); }
        private string[] pages = new string[0];
        private static string LessonNotes()
        {
            var saved = Curriculum.Lessons.Where(l => GameSaveManager.Instance.Data.notebookPages.Contains(l.ID)).ToList();
            if (saved.Count == 0) return "ჯერ ჩანაწერები არ გაქვს\nპირველი გაკვეთილის შემდეგ აქ მისი მოკლე შინაარსი გამოჩნდება.";
            return string.Join("\n\n", saved.Select(l => Curriculum.Label(l) + "\n" + l.Note));
        }
        public void Open() => Open(0);
        public void Open(int page)
        {
            if (window != null) { LabUI.BringToFront(window); ShowPage(Mathf.Clamp(page, 0, pages.Length - 1)); return; }
            var names = new System.Collections.Generic.List<string> { "როგორ ვისწავლო", "გაკვეთილების ჩანაწერები" };
            var list = new System.Collections.Generic.List<string> { Intro, LessonNotes() };
            if (Curriculum.Complete) { names.AddRange(new[] { "რეაქციების ტიპები", "ვალენტობა და იონები", "ხსნადობა და ბალანსი" }); list.AddRange(advanced); }
            pages = list.ToArray();
            window = LabUI.Window("ქიმიკოსის რვეული", 1040, 640);
            window.Closed = () => window = null;
            var split = LabUI.Row(window.Body, "რვეულის გვერდები", 22);
            LabUI.FlexibleHeight(split.gameObject);
            // A leather cover on the left with coloured sticky tabs; the ruled page on the right.
            var sidebar = LabUI.Card(split.transform, "სარჩევი", UIArt.Walnut, 18);
            LabUI.Size(sidebar.gameObject, 460, 250, 0);
            var icon = LabUI.Icon(sidebar.transform, LabIcon.Book, LabUI.Gold);
            LabUI.Size(icon.gameObject, 54);
            var cover = LabUI.Text(sidebar.transform, "<b>შენი გზამკვლევი</b>", 20, LabUI.Paper);
            cover.alignment = TextAlignmentOptions.Center; LabUI.Size(cover.gameObject, 32);
            tabs.Clear();
            for (int i = 0; i < names.Count; i++)
            {
                int index = i;
                var tab = LabUI.Button(sidebar.transform, names[i], () => ShowPage(index), 46);
                tab.GetComponentInChildren<TMP_Text>().color = new Color32(70, 48, 34, 255);
                tabs.Add(tab);
            }
            var note = LabUI.Text(sidebar.transform, "რვეული ცდის დროსაც შეგიძლია გამოიყენო.", 15, new Color32(232, 214, 186, 255));
            note.alignment = TextAlignmentOptions.Center;
            LabUI.Size(note.gameObject, 40);
            var right = UIArt.Paper(split.transform, "რვეულის გვერდი");
            LabUI.Size(right.gameObject, 460, -1, 1);
            var pad = right.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            pad.padding = new RectOffset(62, 14, 12, 12);
            pad.childControlWidth = pad.childControlHeight = true; pad.childForceExpandWidth = pad.childForceExpandHeight = true;
            content = LabUI.Scroll(right.transform);
            ShowPage(Mathf.Clamp(page, 0, pages.Length - 1));
        }
        private static readonly Color[] TabColours = { new Color32(239, 194, 102, 255), new Color32(130, 200, 160, 255), new Color32(236, 150, 130, 255), new Color32(180, 160, 220, 255), new Color32(130, 190, 220, 255) };
        /// <summary>An equation line (has an arrow and no Georgian words), shown on a highlighter stripe.</summary>
        private static bool Formula(string line) => line.Contains("→") && !line.Any(c => c >= 'ა' && c <= 'ჿ');
        private void ShowPage(int index)
        {
            foreach (Transform child in content) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            for (int i = 0; i < tabs.Count; i++)
            {
                var colour = TabColours[i % TabColours.Length];
                tabs[i].GetComponent<UnityEngine.UI.Image>().color = i == index ? colour : Color.Lerp(colour, new Color32(110, 70, 44, 255), 0.45f);
                tabs[i].transform.localScale = i == index ? new Vector3(1.05f, 1, 1) : Vector3.one;
            }
            // Handwritten-style entries: a green heading, ink text, formulas on a highlighter stripe.
            foreach (string paragraph in pages[index].Split(new[] { "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                var entry = LabUI.Column(content, "ჩანაწერი", 4);
                int newline = paragraph.IndexOf('\n');
                string heading = newline < 0 ? "" : paragraph.Substring(0, newline);
                string body = newline < 0 ? paragraph : paragraph.Substring(newline + 1);
                if (heading.Length > 0)
                {
                    var title = LabUI.Text(entry.transform, heading, 24, LabUI.Green);
                    title.fontStyle = FontStyles.Bold | FontStyles.Underline;
                }
                foreach (string line in body.Split('\n'))
                {
                    if (!Formula(line)) { LabUI.Text(entry.transform, line, 20, new Color32(52, 50, 70, 255)); continue; }
                    var stripe = LabUI.Panel(entry.transform, "ფორმულა", new Color32(255, 234, 140, 200));
                    var formula = LabUI.Text(stripe.transform, ChemFx.Pretty(line), 21, new Color32(52, 50, 70, 255));
                    formula.fontStyle = FontStyles.Bold; LabUI.Fill(formula.rectTransform, 6);
                    formula.enableAutoSizing = true; formula.fontSizeMin = 12; formula.fontSizeMax = 21;
                    LabUI.Size(stripe.gameObject, 38);
                }
                LabUI.Size(LabUI.Object(entry.transform, "ცარიელი"), 10);
            }
            content.GetComponentInParent<UnityEngine.UI.ScrollRect>().verticalNormalizedPosition = 1;
        }
    }
}
