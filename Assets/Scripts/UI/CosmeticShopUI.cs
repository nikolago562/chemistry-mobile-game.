using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    /// <summary>
    /// The academy supply catalogue: coloured section tabs, a grid of illustrated item cards with
    /// price tags, owned / worn status, and weekly prizes shown as silhouettes until won.
    /// </summary>
    public sealed class CosmeticShopUI : MonoBehaviour
    {
        private LabGame game;
        private LabWindow window;
        private RectTransform content;
        private TMP_Text balance;
        private CosmeticCategory category;
        private readonly System.Collections.Generic.List<Button> tabs = new System.Collections.Generic.List<Button>();
        private static readonly string[] Names = { "სათვალე", "ხალათები", "ქუდები", "მაგიდის დეკორი" };
        private static readonly Color[] TabColours = { new Color32(96, 168, 176, 255), new Color32(92, 150, 112, 255), new Color32(150, 120, 196, 255), new Color32(214, 164, 72, 255) };
        private static readonly Color Ink = new Color32(70, 48, 34, 255);

        public void Initialize(LabGame lab) => game = lab;
        public void Toggle() { if (window != null) window.Close(); else Open(); }
        public void Open()
        {
            if (window != null) { LabUI.BringToFront(window); return; }
            window = LabUI.Window("აკადემიის მარაგების კატალოგი", 1060, 650);
            window.Closed = () => window = null;
            var top = LabUI.Row(window.Body, "კატალოგის თავი", 12); LabUI.Size(top.gameObject, 36);
            balance = UIArt.Chip(top.transform, LabIcon.Coin, "", LabUI.Gold);
            var blurb = LabUI.Text(top.transform, "ყველაფერი, რაც ახალგაზრდა ქიმიკოსს სჭირდება • შეიძინე ერთხელ, მოირგე ნებისმიერ დროს", 15, LabUI.Muted);
            blurb.alignment = TextAlignmentOptions.Right; blurb.enableAutoSizing = true; blurb.fontSizeMin = 11; blurb.fontSizeMax = 15;
            LabUI.Size(blurb.gameObject, 34, -1, 1);
            var tabRow = LabUI.Row(window.Body, "კატეგორიები", 8); LabUI.Size(tabRow.gameObject, 44);
            tabs.Clear();
            for (int i = 0; i < Names.Length; i++)
            {
                var value = (CosmeticCategory)i;
                var tab = LabUI.Button(tabRow.transform, Names[i], () => { category = value; Refresh(); }, 44);
                tabs.Add(tab);
            }
            content = LabUI.Scroll(window.Body);
            Refresh();
        }

        private void Refresh()
        {
            foreach (Transform child in content) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            var save = GameSaveManager.Instance;
            balance.text = save.Data.chemCoins + " ქიმქოინი";
            for (int i = 0; i < tabs.Count; i++)
            {
                bool on = i == (int)category;
                tabs[i].GetComponent<Image>().color = on ? TabColours[i] : Color.Lerp(TabColours[i], LabUI.Paper, 0.7f);
                tabs[i].GetComponentInChildren<TMP_Text>().color = on ? Color.white : Ink;
                tabs[i].transform.localScale = on ? new Vector3(1, 1.06f, 1) : Vector3.one;
            }
            var grid = LabUI.Object(content, "კატალოგი", typeof(GridLayoutGroup)).GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(316, 250); grid.spacing = new Vector2(14, 14);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 3;
            var fit = grid.gameObject.AddComponent<ContentSizeFitter>(); fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            foreach (var item in game.cosmetics.Where(c => c.category == category)) ItemCard(grid.transform, item);
        }

        private static Sprite Illustration(CosmeticItemSO item) =>
            item.category == CosmeticCategory.HairOrHat ? PixelArt.Hat(item.weeklyExclusive)
            : item.category == CosmeticCategory.SafetyGoggles ? PixelArt.Goggles()
            : item.itemID == "decor_globe" ? PixelArt.Globe() : item.itemID == "decor_poster" ? PixelArt.Poster()
            : item.category == CosmeticCategory.PersonalDeskDecor ? PixelArt.Flask() : LabIcons.Get(LabIcon.Coat);

        private void ItemCard(Transform parent, CosmeticItemSO item)
        {
            var save = GameSaveManager.Instance;
            bool owned = save.Data.unlockedCosmeticIDs.Contains(item.itemID);
            string equipped = category == CosmeticCategory.LabCoat ? save.Data.equippedLabCoatID
                : category == CosmeticCategory.PersonalDeskDecor ? save.Data.equippedDeskDecorID : save.Data.equippedHatOrGogglesID;
            bool wearing = equipped == item.itemID, prize = item.weeklyExclusive && !owned, affordable = save.Data.chemCoins >= item.price;

            var card = LabUI.Panel(parent, item.displayName, LabUI.Paper);
            LabUI.Shadow(card, 4);
            // The illustration plate, tinted by section, with the item drawn large (a silhouette if it's a prize not yet won).
            var plate = LabUI.Panel(card.transform, "ილუსტრაცია", Color.Lerp(TabColours[(int)category], Color.white, 0.72f));
            var pr = plate.rectTransform; pr.anchorMin = new Vector2(0, 1); pr.anchorMax = Vector2.one; pr.pivot = new Vector2(0.5f, 1);
            pr.offsetMin = new Vector2(12, -118); pr.offsetMax = new Vector2(-12, -12);
            var halo = UIArt.Picture(plate.transform, "შუქი", DecorArt.Soft(), new Vector2(150, 150), new Color(1, 1, 1, 0.7f));
            bool whiteCoat = item.category == CosmeticCategory.LabCoat && item.itemID == "coat_white";
            var art = UIArt.Picture(plate.transform, "ნივთი", Illustration(item), new Vector2(150, 84),
                prize ? new Color(0.2f, 0.24f, 0.22f, 0.65f) : whiteCoat ? Color.white : item.color);
            if (wearing) Badge(plate.transform, "მორგებულია", LabUI.Green);
            else if (owned) Badge(plate.transform, "შენია", new Color32(120, 170, 140, 255));
            else if (prize) Badge(plate.transform, "კვირის პრიზი", LabUI.Coral);

            var name = LabUI.Text(card.transform, "<b>" + item.displayName + "</b>", 19, Ink);
            name.enableAutoSizing = true; name.fontSizeMin = 12; name.fontSizeMax = 19;
            Place(name.rectTransform, new Vector2(16, 98), new Vector2(-16, -124));
            var description = LabUI.Text(card.transform, item.description, 14, LabUI.Muted);
            description.enableAutoSizing = true; description.fontSizeMin = 10; description.fontSizeMax = 14;
            Place(description.rectTransform, new Vector2(16, 60), new Vector2(-16, -152));

            string label = wearing ? "მორგებულია" : owned ? "მორგება" : prize ? "მოიგე რეიტინგში" : item.price + " ქიმქოინი";
            var buy = LabUI.Button(card.transform, label, () =>
            {
                if (!save.PurchaseAndEquip(item)) return;
                AudioManager.Instance?.Coin();
                game.HUD.Notify(item.displayName + " მორგებულია.");
                Juice.Confetti(card.transform, 2, 90);
                Refresh();
            }, 46);
            var bl = (RectTransform)buy.transform; bl.anchorMin = new Vector2(0, 0); bl.anchorMax = new Vector2(1, 0); bl.pivot = new Vector2(0.5f, 0);
            bl.offsetMin = new Vector2(14, 10); bl.offsetMax = new Vector2(-14, 56);
            buy.GetComponent<LayoutElement>().ignoreLayout = true;
            buy.GetComponent<Image>().color = wearing ? LabUI.Green : owned ? LabUI.Mint : !prize && affordable ? LabUI.Gold : new Color32(214, 206, 188, 255);
            var buyText = buy.GetComponentInChildren<TMP_Text>(); buyText.color = owned || wearing ? Color.white : Ink;
            if (!owned && !prize)
            {   // a coin on the price tag
                var coin = LabUI.Icon(buy.transform, LabIcon.Coin, Color.white);
                var cr = coin.rectTransform; cr.anchorMin = cr.anchorMax = new Vector2(0, 0.58f); cr.sizeDelta = Vector2.one * 24; cr.anchoredPosition = new Vector2(22, 0);
            }
            buy.interactable = !wearing && (owned || !item.weeklyExclusive && affordable);
        }

        private static void Badge(Transform parent, string text, Color color)
        {
            var tag = LabUI.Panel(parent, text, color);
            var r = tag.rectTransform; r.anchorMin = r.anchorMax = r.pivot = new Vector2(1, 1); r.anchoredPosition = new Vector2(-6, -6); r.sizeDelta = new Vector2(124, 28);
            var label = LabUI.Text(tag.transform, text, 13, Color.white); label.alignment = TextAlignmentOptions.Center; label.fontStyle = FontStyles.Bold;
            label.enableAutoSizing = true; label.fontSizeMin = 9; label.fontSizeMax = 13; LabUI.Fill(label.rectTransform, 3);
        }

        private static void Place(RectTransform rect, Vector2 min, Vector2 maxFromTop)
        {
            rect.anchorMin = new Vector2(0, 0); rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(min.x, min.y); rect.offsetMax = new Vector2(maxFromTop.x, maxFromTop.y);
        }
    }
}
