using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    public sealed class ChemChallengeUI : MonoBehaviour
    {
        private LabGame game;
        private LabWindow window;
        private TMP_Text feedback, rewardLabel;
        private Button next, hintButton;
        private readonly List<Button> options = new List<Button>();
        private readonly List<int> quizDeck = new List<int>(), equationDeck = new List<int>();
        private ChemistryQuestionSO payout;
        private bool equations, answered, usedHint, hadError;
        private int sessionSolved;
        public StudyQuestion Current { get; private set; }
        public bool IsOpen => window != null;
        public void Initialize(LabGame owner)
        {
            game = owner;
            payout = ScriptableObject.CreateInstance<ChemistryQuestionSO>();
        }
        public void Open()
        {
            if (window != null) { LabUI.BringToFront(window); return; }
            window = LabUI.Window("მთავარი მაგიდა • აირჩიე დავალება", 1040, 650);
            window.Closed = () => { window = null; Current = null; };
            Home();
        }
        private void Clear()
        {
            foreach(Transform child in window.Body) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            options.Clear();
        }
        private TMP_Text Copy(Transform parent, string text, float height, float size = 19, Color? color = null)
        {
            var label = LabUI.Text(parent, text, size, color);
            LabUI.Size(label.gameObject, height);
            return label;
        }
        private void Home()
        {
            Clear(); Current = null;
            Copy(window.Body, "შეამოწმე ცოდნა, შეასრულე დავალებები და დააგროვე ქიმქოინები", 38, 21, LabUI.Green);
            var row = LabUI.Row(window.Body, "გამოწვევების არჩევანი", 18);
            LabUI.Size(row.gameObject, 302);
            ModeCard(row.transform, false, LabIcon.Atom, "ქიმიის ქვიზი", "16 კითხვა ატომებზე, ბმებზე, ხსნარებსა და აირებზე. აირჩიე პასუხი და გაეცანი განმარტებას.", LabUI.Mint);
            ModeCard(row.transform, true, LabIcon.Flask, "განტოლების დეტექტივი", "26 ქიმიური რეაქცია. შეავსე განტოლება და შეადარე ატომების რაოდენობა ორივე მხარეს.", LabUI.Gold);
            Copy(window.Body, "სწორ პასუხებზე ქიმქოინებს იღებ და კომბოს ზრდი. მინიშნების გამოყენების ან შეცდომის შემდეგ საბაზო ჯილდოს 60%-ს მიიღებ. ცდა უფასოა.", 64, 17, LabUI.Muted);
            var note = LabUI.Card(window.Body, "კვლევის ბონუსი", LabUI.Soft, 12);
            Copy(note.transform, "შენს ლაბორატორიაში: დიფუზია • აქტიური ტრანსპორტი • ფერების განცალკევება • აირის შეკუმშვა\nთითოეული მიზნისთვის — 20 ქიმქოინი, დღეში ერთხელ.", 62, 18, LabUI.Green);
        }
        private void ModeCard(Transform parent, bool equation, LabIcon icon, string title, string description, Color accent)
        {
            var card = LabUI.Card(parent, title, LabUI.Navy, 20); LabUI.Size(card.gameObject, 302, -1, 1);
            var top = LabUI.Row(card.transform, "ნიშანი", 16);
            var image = LabUI.Icon(top.transform, icon, Color.white); LabUI.Size(image.gameObject, 56, 56);
            var label = Copy(top.transform, title, 60, 25, accent); label.fontStyle = FontStyles.Bold; LabUI.Size(label.gameObject, 60, -1, 1);
            Copy(card.transform, description, 86, 19, LabUI.Paper);
            Copy(card.transform, equation ? "ატომების რაოდენობა • ოთხი პასუხი" : "აირჩიე პასუხი • გაიგე მეტი", 32, 16, accent);
            LabUI.Button(card.transform, "დაწყება →", () => StartRound(equation), 54).GetComponent<Image>().color = equation ? new Color32(160, 105, 35, 255) : LabUI.Green;
        }
        private static int Draw(List<int> deck, int count)
        {
            if(deck.Count == 0)
            {
                for(int i=0;i<count;i++) deck.Add(i);
                for(int i=deck.Count-1;i>0;i--) { int j=UnityEngine.Random.Range(0,i+1); (deck[i],deck[j])=(deck[j],deck[i]); }
            }
            int value=deck[deck.Count-1]; deck.RemoveAt(deck.Count-1); return value;
        }
        public void StartRound(bool equation)
        {
            if(window == null) Open();
            equations = equation; answered = usedHint = hadError = false;
            Current = equation ? ChemistryChallenges.Equation(game.questions[Draw(equationDeck, game.questions.Length)]) : ChemistryChallenges.General[Draw(quizDeck, ChemistryChallenges.General.Length)];
            Clear();
            var tags=LabUI.Row(window.Body,"სასწავლო ბარათი",12);
            LabUI.Pill(tags.transform,equation?"განტოლება":"ქიმიის ქვიზი",LabUI.Green);
            rewardLabel=LabUI.Pill(tags.transform,"ჯილდო: +"+Current.Reward+" ქიმქოინი",new Color32(170,112,34,255));
            LabUI.Pill(tags.transform,"სწორი პასუხები: "+sessionSolved,LabUI.Green);
            var panel=LabUI.Panel(window.Body,"შეკითხვა",LabUI.Navy);LabUI.Size(panel.gameObject,112);
            var prompt=LabUI.Text(panel.transform,Current.Prompt,equation?32:25,LabUI.Paper);
            LabUI.Fill(prompt.rectTransform,18);prompt.alignment=TextAlignmentOptions.Center;prompt.enableAutoSizing=true;prompt.fontSizeMin=19;prompt.fontSizeMax=equation?32:25;
            var order=new List<int>{0,1,2,3};for(int i=3;i>0;i--){int j=UnityEngine.Random.Range(0,i+1);(order[i],order[j])=(order[j],order[i]);}
            for(int r=0;r<2;r++)
            {
                var row=LabUI.Row(window.Body,"პასუხები",14);LabUI.Size(row.gameObject,70);
                for(int c=0;c<2;c++)
                {
                    int index=order[r*2+c];
                    var button=LabUI.Button(row.transform,((char)('A'+r*2+c))+"  "+Current.Choices[index],()=>Choose(index),70);
                    button.GetComponent<Image>().color=new Color32(55,91,77,255);
                    options.Add(button);
                }
            }
            var explanation=LabUI.Card(window.Body,"ახსნა და მინიშნება",LabUI.Soft,10);LabUI.Size(explanation.gameObject,112);
            var scroll=LabUI.Scroll(explanation.transform);
            feedback=LabUI.Text(scroll,"აირჩიე პასუხი. აქ გაიგებ, რატომ არის ის სწორი ან არასწორი.",18,LabUI.Ink);
            var footer=LabUI.Row(window.Body,"ქმედებები",12);
            hintButton=LabUI.Secondary(footer.transform,"მინიშნება",UseHint,44);
            LabUI.Secondary(footer.transform,"← მთავარ მენიუში",Home,44);
            next=LabUI.Button(footer.transform,"შემდეგი →",()=>StartRound(equations),44);next.interactable=false;
        }
        public void UseHint()
        {
            if(Current==null || answered) return;
            usedHint=true;feedback.text="მინიშნება: "+Current.Hint;
            rewardLabel.text="ჯილდო: +"+Mathf.Max(1,Mathf.RoundToInt(Current.Reward*0.6f))+" ქიმქოინი";
        }
        public void Choose(int index)
        {
            if(Current==null || answered || index<0 || index>=Current.Choices.Length) return;
            if(index!=Current.Correct)
            {
                hadError=true;GameSaveManager.Instance.RecordWrong();AudioManager.Instance?.Wrong();
                feedback.color=new Color32(151,62,45,255);
                feedback.text="არასწორია. "+Current.Feedback[index]+"\nაირჩიე სხვა პასუხი ან გამოიყენე მინიშნება.";
                rewardLabel.text="ჯილდო: +"+Mathf.Max(1,Mathf.RoundToInt(Current.Reward*0.6f))+" ქიმქოინი";
                return;
            }
            answered=true;
            payout.questionID="study_"+Current.ID;
            payout.coinReward=usedHint || hadError ? Mathf.Max(1,Mathf.RoundToInt(Current.Reward*0.6f)) : Current.Reward;
            payout.involvedElements=Current.Elements;
            int coins=GameSaveManager.Instance.RecordCorrect(payout,DateTime.UtcNow.Date);
            game.Rankings.SubmitScore();sessionSolved++;
            feedback.color=LabUI.Green;feedback.text="სწორია! +"+coins+" ქიმქოინი\n"+Current.Explanation;
            foreach(var button in options) button.interactable=false;
            hintButton.interactable=false;next.interactable=true;
            AudioManager.Instance?.Correct();AudioManager.Instance?.Coin();game.Visuals.React(true);
        }
        public void Close(){if(window!=null)window.Close();}
        private void OnDestroy(){if(payout!=null)Destroy(payout);}
    }
}
