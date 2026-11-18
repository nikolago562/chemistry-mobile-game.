using TMPro;
using UnityEngine;

namespace ChemistryGame
{
    public sealed class FormulaNotebookUI : MonoBehaviour
    {
        private LabWindow window;
        private RectTransform content;
        private readonly System.Collections.Generic.List<UnityEngine.UI.Button> tabs = new System.Collections.Generic.List<UnityEngine.UI.Button>();
        private readonly string[] pages =
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
        public void Open()
        {
            if (window != null) { LabUI.BringToFront(window); return; }
            window = LabUI.Window("ქიმიკოსის რვეული", 1040, 640);
            window.Closed = () => window = null;
            var split = LabUI.Row(window.Body, "რვეულის გვერდები", 22);
            LabUI.FlexibleHeight(split.gameObject);
            var sidebar = LabUI.Card(split.transform, "სარჩევი", LabUI.Navy, 18);
            LabUI.Size(sidebar.gameObject, 460, 240, 0);
            var icon = LabUI.Icon(sidebar.transform, LabIcon.Book, LabUI.Gold);
            LabUI.Size(icon.gameObject, 74);
            LabUI.Size(LabUI.Text(sidebar.transform, "შენი გზამკვლევი", 20, Color.white).gameObject, 34);
            string[] names = { "რეაქციების ტიპები", "ვალენტობა და იონები", "ხსნადობა და ბალანსი" };
            tabs.Clear();
            for (int i = 0; i < names.Length; i++)
            {
                int index = i;
                tabs.Add(LabUI.Button(sidebar.transform, names[i], () => ShowPage(index), 60));
            }
            var note = LabUI.Text(sidebar.transform, "რვეული ცდის დროსაც შეგიძლია გამოიყენო.", 16, new Color32(188, 193, 224, 255));
            LabUI.Size(note.gameObject, 66);
            var right = LabUI.Column(split.transform, "ჩანაწერები");
            LabUI.Size(right.gameObject, 460, -1, 1);
            content = LabUI.Scroll(right.transform);
            ShowPage(0);
        }
        private void ShowPage(int index)
        {
            foreach (Transform child in content) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            for (int i = 0; i < tabs.Count; i++)
                tabs[i].GetComponent<UnityEngine.UI.Image>().color = i == index ? LabUI.Green : new Color32(57, 64, 106, 255);
            foreach (string paragraph in pages[index].Split(new[] { "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                var card = LabUI.Card(content, "ჩანაწერი", LabUI.Soft, 20);
                int newline = paragraph.IndexOf('\n');
                string heading = newline < 0 ? "" : paragraph.Substring(0, newline);
                string body = newline < 0 ? paragraph : paragraph.Substring(newline + 1);
                if (heading.Length > 0)
                {
                    var title = LabUI.Text(card.transform, heading, 24, LabUI.Green);
                    title.fontStyle = FontStyles.Bold;
                }
                LabUI.Text(card.transform, body, 21);
            }
            content.GetComponentInParent<UnityEngine.UI.ScrollRect>().verticalNormalizedPosition = 1;
        }
    }
}
