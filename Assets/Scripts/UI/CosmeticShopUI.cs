using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    public sealed class CosmeticShopUI : MonoBehaviour
    {
        private LabGame game;
        private LabWindow window;
        private RectTransform content;
        private TMP_Text balance;
        private CosmeticCategory category;
        private readonly System.Collections.Generic.List<Button> tabs = new System.Collections.Generic.List<Button>();
        public void Initialize(LabGame lab) => game = lab;
        public void Toggle() { if (window != null) window.Close(); else Open(); }
        public void Open()
        {
            if (window != null) { LabUI.BringToFront(window); return; }
            window = LabUI.Window("შექმენი შენი ლაბორატორიული სტილი", 1020, 640);
            window.Closed = () => window = null;
            balance = LabUI.Text(window.Body, "", 20, LabUI.Green);
            LabUI.Size(balance.gameObject, 30);
            var tabRow = LabUI.Row(window.Body, "კატეგორიები");
            string[] names = { "სათვალე", "ხალათები", "ქუდები", "მაგიდის დეკორი" };
            tabs.Clear();
            for (int i = 0; i < names.Length; i++)
            {
                var value = (CosmeticCategory)i;
                tabs.Add(LabUI.Secondary(tabRow.transform, names[i], () => { category = value; Refresh(); }));
            }
            content = LabUI.Scroll(window.Body);
            Refresh();
        }
        private void Refresh()
        {
            foreach (Transform child in content) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            var save = GameSaveManager.Instance;
            balance.text = "შენი ბალანსი — " + save.Data.chemCoins + " ქიმქოინი";
            for (int i = 0; i < tabs.Count; i++)
            {
                tabs[i].GetComponent<Image>().color = i == (int)category ? LabUI.Green : LabUI.Soft;
                tabs[i].GetComponentInChildren<TMP_Text>().color = i == (int)category ? Color.white : LabUI.Green;
            }
            var items = game.cosmetics.Where(c => c.category == category).ToArray();
            for (int start = 0; start < items.Length; start += 2)
            {
                var row = LabUI.Row(content, "ნივთების თარო", 18);
                for (int index = start; index < Mathf.Min(start + 2, items.Length); index++)
                    ItemCard(row.transform, items[index]);
                if (start + 1 == items.Length)
                {
                    var spacer = LabUI.Object(row.transform, "ცარიელი ადგილი");
                    LabUI.Size(spacer, 255, -1, 1);
                }
            }
        }
        private void ItemCard(Transform parent, CosmeticItemSO item)
        {
            var save = GameSaveManager.Instance;
            var card = LabUI.Card(parent, item.displayName, LabUI.Soft, 18);
            LabUI.Size(card.gameObject, 255, -1, 1);
            var top = LabUI.Row(card.transform, "ნივთი");
            var preview = LabUI.Panel(top.transform, "ნივთის გამოსახულება", Color.white);
            LabUI.Size(preview.gameObject, 80, 92);
            LabIcon kind = item.category == CosmeticCategory.PersonalDeskDecor ? LabIcon.Flask : LabIcon.Coat;
            var icon = LabUI.Icon(preview.transform, kind, item.category == CosmeticCategory.LabCoat && item.itemID == "coat_white" ? LabUI.Green : item.color);
            // Item previews retain their purchasable color; navigation icons use the shared palette.
            icon.color = item.category == CosmeticCategory.LabCoat && item.itemID == "coat_white" ? Color.white : item.color;
            if (item.category == CosmeticCategory.HairOrHat) icon.sprite = PixelArt.Hat(item.weeklyExclusive);
            if (item.category == CosmeticCategory.SafetyGoggles) icon.sprite = PixelArt.Goggles();
            if (item.itemID == "decor_globe") icon.sprite = PixelArt.Globe();
            if (item.itemID == "decor_poster") icon.sprite = PixelArt.Poster();
            LabUI.Fill(icon.rectTransform, 13);
            var name = LabUI.Text(top.transform, item.displayName, 21);
            name.fontStyle = FontStyles.Bold;
            LabUI.Size(name.gameObject, 80, -1, 1);
            var description = LabUI.Text(card.transform, item.description, 16, LabUI.Muted);
            LabUI.Size(description.gameObject, 55);
            bool owned = save.Data.unlockedCosmeticIDs.Contains(item.itemID);
            string equipped = category == CosmeticCategory.LabCoat ? save.Data.equippedLabCoatID
                : category == CosmeticCategory.PersonalDeskDecor ? save.Data.equippedDeskDecorID : save.Data.equippedHatOrGogglesID;
            string label = equipped == item.itemID ? "მორგებულია" : owned ? "მორგება" : item.weeklyExclusive ? "კვირის პრიზი" : item.price + " ქიმქოინი";
            var buy = LabUI.Button(card.transform, label, () =>
            {
                if (!save.PurchaseAndEquip(item)) return;
                AudioManager.Instance?.Coin();
                game.HUD.Notify(item.displayName + " მორგებულია.");
                Refresh();
            });
            buy.GetComponent<Image>().color = owned ? LabUI.Mint : LabUI.Green;
            buy.interactable = equipped != item.itemID && (owned || !item.weeklyExclusive && save.Data.chemCoins >= item.price);
        }
    }
}
