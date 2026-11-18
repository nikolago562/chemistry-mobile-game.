using System.Linq;
using TMPro;
using UnityEngine;

namespace ChemistryGame
{
    public sealed class LabWorkshopUI : MonoBehaviour
    {
        private PersonalLab lab;
        private LabWindow window;
        private string selected = "molecule", tab = "shop";
        private bool decorShop;
        private TMP_Text status;
        public bool IsOpen => window != null;
        public void Initialize(PersonalLab owner) => lab = owner;
        public void Toggle() { if (window != null) window.Close(); else Open(); }
        public void Open()
        {
            if (!lab.Inside) return;
            if (window != null) { LabUI.BringToFront(window); return; }
            window = LabUI.Window("შენი ლაბორატორია • აღჭურვილობა და მოწყობა", 1100, 650);
            window.Closed = () => window = null;
            Render();
        }
        private void Render()
        {
            foreach (Transform child in window.Body) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            var save = GameSaveManager.Instance;
            var tabs = LabUI.Row(window.Body, "ჩანართები", 10);
            Tab(tabs.transform, "მაღაზია", "shop");
            Tab(tabs.transform, "ოთახის მოწყობა", "layout");
            Tab(tabs.transform, "გაფართოება", "upgrade");
            var coins = LabUI.Pill(tabs.transform, save.Data.chemCoins + " ქიმქოინი", LabUI.Mint, 180);
            status = LabUI.Text(window.Body, "", 16, LabUI.Muted); LabUI.Size(status.gameObject, 34);
            if (tab == "shop") Shop(); else if (tab == "layout") Layout(); else Upgrade();
        }
        private void Tab(Transform parent, string name, string id)
        {
            var button = tab == id ? LabUI.Button(parent, name, () => { tab = id; Render(); }) :
                LabUI.Secondary(parent, name, () => { tab = id; Render(); });
            LabUI.Size(button.gameObject, 46, -1, 1);
        }
        private void Shop()
        {
            status.text = decorShop ? "დეკორაციები ცვლის ოთახის განწყობას • სიმშვიდე: " + PersonalDecor.CalmName : "იყიდე ერთხელ • ჩაატარე ცდები უფასოდ • შეცვალე განლაგება ნებისმიერ დროს";
            var kinds = LabUI.Row(window.Body, "კატეგორია", 10); LabUI.Size(kinds.gameObject, 40);
            LabUI.Size((decorShop ? LabUI.Secondary(kinds.transform, "აღჭურვილობა", () => { decorShop = false; Render(); }, 38) : LabUI.Button(kinds.transform, "აღჭურვილობა", () => { }, 38)).gameObject, 38, -1, 1);
            LabUI.Size((decorShop ? LabUI.Button(kinds.transform, "დეკორაციები", () => { }, 38) : LabUI.Secondary(kinds.transform, "დეკორაციები", () => { decorShop = true; Render(); }, 38)).gameObject, 38, -1, 1);
            var content = LabUI.Scroll(window.Body);
            foreach (var item in PersonalLab.Catalog.Where(c => c.Decor == decorShop))
            {
                bool owned = GameSaveManager.Instance.Data.ownedLabItems.Contains(item.ID);
                bool locked = !owned && LabProgress.ReputationLevel < item.ReputationLevel;
                bool earned = !owned && !string.IsNullOrEmpty(item.Unlock);
                var card = LabUI.Card(content, item.Name, LabUI.Soft, 14);
                var row = LabUI.Row(card.transform, item.Name, 16);
                var icon = LabUI.Icon(row.transform, LabIcon.Flask, item.Color); LabUI.Size(icon.gameObject, 58, 58);
                var copy = LabUI.Column(row.transform, "აღწერა", 4); LabUI.Size(copy.gameObject, 72, -1, 1);
                var title = LabUI.Text(copy.transform, item.Name, 21, item.Color); title.fontStyle = FontStyles.Bold; LabUI.Size(title.gameObject, 28);
                var description = LabUI.Text(copy.transform, item.Description, 16);
                description.enableAutoSizing = true; description.fontSizeMin = 14; description.fontSizeMax = 16;
                LabUI.Size(description.gameObject, 40);
                string label = owned ? "მოწყობა" : earned ? "იხსნება: " + item.Unlock : locked ? "საჭიროა: " + LabProgress.ReputationTitles[item.ReputationLevel]
                    : item.Price == 0 ? "უფასოა • აიღე" : item.Price + " ქიმქოინი • ყიდვა";
                var buy = LabUI.Button(row.transform, label, () =>
                {
                    if (earned || locked) { status.text = earned ? "ეს ნივთი იყიდება არა, იმსახურება: " + item.Unlock + "." : "გაზარდე ლაბორატორიის რეპუტაცია: კვლევები, თხოვნები და ბაზრობები."; return; }
                    if (!owned && !lab.Buy(item.ID)) { status.text = "ქიმქოინები არ გყოფნის. შეასრულე დავალებები, დააგროვე ქიმქოინები და დაბრუნდი."; return; }
                    selected = item.ID; tab = "layout"; Render();
                });
                buy.interactable = !(earned || locked) || true;
                if (earned || locked) buy.GetComponent<UnityEngine.UI.Image>().color = LabUI.Muted;
                buy.GetComponentInChildren<TMP_Text>().fontSizeMin = 11;
                LabUI.Size(buy.gameObject, 48, 240, 0);
            }
        }
        private void Layout()
        {
            status.text = "აირჩიე ნივთი, შემდეგ უჯრა. დაკავებულ უჯრაზე დადებისას ნივთები ადგილს გაცვლიან. კარის წინ ორი უჯრა თავისუფალი რჩება.";
            var split = LabUI.Row(window.Body, "მოწყობა", 20); LabUI.FlexibleHeight(split.gameObject);
            var inventory = LabUI.Card(split.transform, "შენი ნივთები", LabUI.Soft, 16);
            LabUI.Size(inventory.gameObject, 390, 270, 0);
            LabUI.Size(LabUI.Text(inventory.transform, "შენი აღჭურვილობა", 20).gameObject, 30);
            var owned = PersonalLab.Catalog.Where(c => GameSaveManager.Instance.Data.ownedLabItems.Contains(c.ID)).ToArray();
            if (!owned.Any())
                LabUI.Size(LabUI.Text(inventory.transform, "ჯერ მაღაზიის ჩანართიდან იყიდე პირველი ხელსაწყო.", 18).gameObject, 100);
            var inventoryList = LabUI.Scroll(inventory.transform);
            foreach (var item in owned)
            {
                var choose = LabUI.Button(inventoryList, (selected == item.ID ? "● " : "") + item.Name, () => { selected = item.ID; Render(); }, 54);
                choose.GetComponent<UnityEngine.UI.Image>().color = selected == item.ID ? item.Color : LabUI.Navy;
            }
            if (owned.Any(c => c.ID == selected))
                LabUI.Secondary(inventory.transform, "აღჭურვილობის შენახვა", () => { lab.Store(selected); Render(); });
            var plan = LabUI.Column(split.transform, "ოთახის გეგმა", 8); LabUI.Size(plan.gameObject, 390, -1, 1);
            LabUI.Size(LabUI.Text(plan.transform, "ოთახის გეგმა • ზედხედიდან", 20).gameObject, 30);
            for (int row = lab.Rows - 1; row >= 0; row--)
            {
                var line = LabUI.Row(plan.transform, "რიგი", 7);
                for (int col = 0; col < lab.Columns; col++)
                {
                    int c = col, r = row;
                    bool entrance = lab.IsEntranceCell(c, r);
                    var placed = GameSaveManager.Instance.Data.labPlacements.FirstOrDefault(p => p.column == c && p.row == r);
                    var item = placed == null ? null : PersonalLab.Catalog.FirstOrDefault(i => i.ID == placed.itemID);
                    var button = LabUI.Button(line.transform, entrance ? "გასასვლელი" : item == null ? "+" : item.Name, () =>
                    {
                        if (!lab.Place(selected, c, r))
                        { status.text = "აირჩიე შეძენილი აღჭურვილობა და თავისუფალი უჯრა. თუ ამ ადგილას დგახარ, ჯერ გვერდზე გადადი."; status.color = LabUI.Coral; return; }
                        Render();
                    }, CellHeight);
                    button.interactable = !entrance;
                    button.GetComponent<UnityEngine.UI.Image>().color = item?.Color ?? new Color32(216, 226, 228, 255);
                    button.GetComponentInChildren<TMP_Text>().color = item == null ? LabUI.Muted : Color.white;
                    button.GetComponentInChildren<TMP_Text>().fontSizeMin = 10;
                    button.GetComponentInChildren<TMP_Text>().fontSizeMax = 15;
                    LabUI.Size(button.gameObject, CellHeight, -1, 1);
                }
            }
            LabUI.Size(LabUI.Text(plan.transform, "↑ კარი ზედა კედელშია • მის წინ ორი ადგილი თავისუფალი რჩება", 16, LabUI.Muted).gameObject, 44);
        }
        private int CellHeight => lab.Rows >= 5 ? 48 : lab.Rows == 4 ? 61 : 76;
        private void Upgrade()
        {
            int level = lab.Level;
            bool max = level >= PersonalLab.ExpansionPrices.Length;
            status.text = "შეძენილი აღჭურვილობა და განლაგება ყოველი გაფართოებისას შენარჩუნდება.";
            var card = LabUI.Card(window.Body, "მეტი ადგილი შენი აღმოჩენებისთვის", LabUI.Soft, 28);
            LabUI.FlexibleHeight(card.gameObject);
            LabUI.Size(LabUI.Text(card.transform, max ? "ყველაზე დიდი ლაბორატორია შენია!" : "გაფართოება " + (level + 1) + " / " + PersonalLab.ExpansionPrices.Length, 30, LabUI.Green).gameObject, 55);
            string sizes = "";
            for (int i = 0; i < PersonalLab.LevelColumns.Length; i++)
                sizes += (i == level ? "→ " : "") + PersonalLab.LevelColumns[i] + " × " + PersonalLab.LevelRows[i] + " (" + (PersonalLab.LevelColumns[i] * PersonalLab.LevelRows[i] - 2) + " ადგილი)" +
                    (i > 0 ? " • " + PersonalLab.ExpansionPrices[i - 1] + " ქიმქოინი" + (PersonalLab.ExpansionReputation[i - 1] > 0 ? " • " + LabProgress.ReputationTitles[PersonalLab.ExpansionReputation[i - 1]] : "") : " • საწყისი") + "\n";
            LabUI.Size(LabUI.Text(card.transform, sizes + "\nკარის წინ ორი უჯრა გასავლელად რჩება. გადაადგილება და შენახვა უფასოა.", 21).gameObject, 220);
            var button = LabUI.Button(card.transform, max ? "უკვე მაქსიმალურია" : PersonalLab.ExpansionPrices[level] + " ქიმქოინი • გაფართოება", () =>
            {
                if (!lab.Expand())
                {
                    status.text = LabProgress.ReputationLevel < PersonalLab.ExpansionReputation[lab.Level] ? "ამ გაფართოებისთვის საჭიროა რეპუტაცია: " + LabProgress.ReputationTitles[PersonalLab.ExpansionReputation[lab.Level]] + "."
                        : "ქიმქოინები არ გყოფნის. გაფართოება მოგვიანებითაც შეგიძლია.";
                    return;
                }
                Render();
            }, 54);
            button.interactable = !max;
        }
    }
}
