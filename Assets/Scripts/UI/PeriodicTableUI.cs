using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    public sealed class PeriodicTableUI : MonoBehaviour
    {
        private LabGame game;
        private LabWindow window;
        private TMP_Text detail, elementName, symbol;
        private Image symbolCard;
        private Button discover;
        private ElementCollectionSO.Element selected;
        public void Initialize(LabGame lab) => game = lab;
        public void Toggle() { if (window != null) window.Close(); else Open(); }
        public void Open()
        {
            if (window != null) { LabUI.BringToFront(window); return; }
            window = LabUI.Window("აღმოაჩინე ელემენტების სამყარო", 1180, 650);
            window.Closed = () => window = null;
            int count = GameSaveManager.Instance.Data.unlockedElements.Count;
            var status = LabUI.Row(window.Body, "აღმოჩენის პროგრესი");
            LabUI.Size(LabUI.Text(status.transform, "შენი კოლექცია", 20).gameObject, 28, -1, 1);
            LabUI.Pill(status.transform, count + " / 118 აღმოჩენილი", LabUI.Mint, 240);
            LabUI.Progress(window.Body, count / 118f, LabUI.Mint, 6);
            var split = LabUI.Row(window.Body, "ცხრილი და აღმოჩენა", 18);
            LabUI.FlexibleHeight(split.gameObject);
            var gridRoot = LabUI.Object(split.transform, "პერიოდული ცხრილი", typeof(GridLayoutGroup));
            LabUI.Size(gridRoot, 400, 825, 0);
            var grid = gridRoot.GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 18;
            grid.cellSize = new Vector2(43, 41);
            grid.spacing = new Vector2(3, 3);
            grid.childAlignment = TextAnchor.MiddleCenter;
            for (int row = 0; row < 9; row++)
            for (int column = 0; column < 18; column++)
            {
                var element = game.elements.elements.FirstOrDefault(e => e.row == row && e.column == column);
                if (element == null) { LabUI.Object(gridRoot.transform, "ცარიელი უჯრა"); continue; }
                bool unlocked = GameSaveManager.Instance.Data.unlockedElements.Contains(element.symbol);
                var button = LabUI.Button(gridRoot.transform,
                    "<size=10>" + element.atomicNumber + "</size>\n" + (unlocked ? element.symbol : "?"), () => Select(element), 41);
                button.GetComponent<Image>().color = unlocked ? element.color : new Color32(61, 67, 103, 255);
                var label = button.GetComponentInChildren<TMP_Text>();
                label.fontSize = label.fontSizeMax = 16;
                label.fontSizeMin = 12;
                label.lineSpacing = -8;
                LabUI.Fill(label.rectTransform, 3);
            }
            var sidebar = LabUI.Card(split.transform, "ელემენტის ბარათი", LabUI.Soft, 16);
            LabUI.Size(sidebar.gameObject, 400, -1, 1);
            symbolCard = LabUI.Panel(sidebar.transform, "ქიმიური სიმბოლო", LabUI.Green);
            LabUI.Size(symbolCard.gameObject, 108);
            symbol = LabUI.Text(symbolCard.transform, "", 50, Color.white);
            symbol.fontStyle = FontStyles.Bold;
            symbol.alignment = TextAlignmentOptions.Center;
            LabUI.Fill(symbol.rectTransform, 10);
            elementName = LabUI.Text(sidebar.transform, "", 21);
            elementName.alignment = TextAlignmentOptions.Center;
            LabUI.Size(elementName.gameObject, 52);
            detail = LabUI.Text(LabUI.Scroll(sidebar.transform), "", 17, LabUI.Muted);
            discover = LabUI.Button(sidebar.transform, "აღმოჩენა", Discover);
            Select(game.elements.elements[0]);
            var note = LabUI.Text(window.Body, Curriculum.Done("c3_groups") ? "აირჩიე უჯრა — გახსენი ახალი ელემენტი ან გაიგე მისი ჯგუფი, პერიოდი და ამბავი."
                : "ცხრილი თანდათან იხსნება: ელემენტების თავში ისწავლი სიმბოლოებს, ლითონებს, ჯგუფებსა და პერიოდებს.", 16, LabUI.Muted);
            LabUI.Size(note.gameObject, 24);
        }
        private void Select(ElementCollectionSO.Element element)
        {
            selected = element;
            bool unlocked = GameSaveManager.Instance.Data.unlockedElements.Contains(element.symbol);
            symbolCard.color = unlocked ? element.color : LabUI.Navy;
            symbol.text = unlocked ? element.symbol : "?";
            elementName.text = unlocked ? element.georgianName : "უცნობი ელემენტი";
            detail.text = "ატომური ნომერი " + element.atomicNumber + Placement(element) + "\n\n" + (unlocked ? element.fact
                : Curriculum.Done("c3_symbols") ? "შეასრულე აღმოჩენის დავალება. ეს უჯრა შენს კოლექციაში ფერადად აინთება."
                : "ახალი ელემენტების აღმოჩენა გაიხსნება გაკვეთილის „" + Curriculum.Get("c3_symbols").Title + "“ შემდეგ.");
            discover.gameObject.SetActive(!unlocked && Curriculum.Done("c3_symbols"));
        }
        private static readonly string[] Nonmetals = { "H", "He", "C", "N", "O", "F", "Ne", "P", "S", "Cl", "Ar", "Se", "Br", "Kr", "I", "Xe", "Rn", "Ts", "Og" };
        private static readonly string[] Metalloids = { "B", "Si", "Ge", "As", "Sb", "Te", "At" };
        /// <summary>Group, period and metal type appear only after the lessons that teach them.</summary>
        private static string Placement(ElementCollectionSO.Element e)
        {
            string text = "";
            if (Curriculum.Done("c3_groups"))
                text += e.row >= 7 ? "\n" + (e.row == 7 ? "ლანთანოიდი" : "აქტინოიდი") + " • პერიოდი " + (e.row == 7 ? 6 : 7)
                    : "\nჯგუფი " + (e.column + 1) + " • პერიოდი " + (e.row + 1);
            if (Curriculum.Done("c3_metals"))
                text += "\n" + (Array.IndexOf(Nonmetals, e.symbol) >= 0 ? "არალითონი" : Array.IndexOf(Metalloids, e.symbol) >= 0 ? "ნახევრადლითონი" : "ლითონი");
            return text;
        }
        private void Discover()
        {
            if (selected == null) return;
            if (!Curriculum.Done("c3_symbols")) { game.HUD.Notify(Curriculum.LockedText); return; }
            if (game.Quiz.IsOpen)
            {
                game.HUD.Notify("ჯერ დაასრულე ან დახურე მიმდინარე ცდა.");
                return;
            }
            var question = selected.discoveryQuestion;
            window.Close();
            game.Quiz.Open(selected: question);
        }
    }
}
