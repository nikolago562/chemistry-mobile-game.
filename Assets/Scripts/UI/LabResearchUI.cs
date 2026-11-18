using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChemistryGame
{
    public sealed class LabResearchUI : MonoBehaviour
    {
        private sealed class Particle { public RectTransform Rect; public Vector2 Position, Velocity; }
        private LabWindow window;
        private RectTransform stage;
        private Transform controls;
        private TMP_Text readout, stats;
        private string activity;
        private readonly List<Particle> particles = new List<Particle>();
        private readonly HashSet<string> achieved = new HashSet<string>();
        private RectTransform gate, piston, solventFront;
        private readonly List<RectTransform> bands = new List<RectTransform>();
        private bool channel, paused, running, solventAlcohol;
        private int atp, pumped, prediction=-1, ink;
        private float motion=1, volume=2, temperature=300, runTime, stableTime, nextReadout;
        private static readonly float[][] WaterRf = { new[]{0.28f,0.55f,0.83f},new[]{0.2f,0.65f,0.8f},new[]{0.38f,0.6f,0.75f} };
        private static readonly float[][] AlcoholRf = { new[]{0.82f,0.35f,0.6f},new[]{0.78f,0.3f,0.6f},new[]{0.76f,0.4f,0.56f} };
        private static readonly string[] Pigments={"წითელი","ლურჯი","ყვითელი"};
        private static readonly Color[] PigmentColors={new Color32(238,121,105,255),new Color32(106,177,229,255),new Color32(247,206,92,255)};
        public bool IsOpen => window!=null;
        public string ActiveID => activity;
        public int RightCount => particles.Count(p=>p.Position.x>0);
        public int Pumped => pumped;
        public float RelativePressure => temperature/300f*2f/volume;

        public void Open(string id)
        {
            if(window!=null){LabUI.BringToFront(window);return;}
            var item=Array.Find(PersonalLab.Catalog,c=>c.ID==id);
            if(item==null)return;
            activity=id;achieved.Clear();particles.Clear();bands.Clear();channel=paused=running=false;
            window=LabUI.Window(item.Name+" • კვლევის მაგიდა",1120,660);
            window.Closed=()=>{window=null;particles.Clear();bands.Clear();activity=null;};
            window.Body.GetComponent<VerticalLayoutGroup>().spacing=10;
            Text(window.Body,"მიაღწიე მიზანს და მიიღე 20 ქიმქოინი • თითოეული ჯილდო დღეში ერთხელ • ცდა ყოველთვის უფასოა",36,17,LabUI.Green);
            var row=LabUI.Row(window.Body,"აპარატი და მართვა",18);LabUI.Size(row.gameObject,360);
            var board=LabUI.Panel(row.transform,"ცოცხალი მოდელი",LabUI.Navy);LabUI.Size(board.gameObject,360,550,0);stage=board.rectTransform;
            board.gameObject.AddComponent<Canvas>();
            stats=Label("",new Vector2(0,153),510,19,LabUI.Paper);
            var card=LabUI.Card(row.transform,"ცდის მართვა",LabUI.Soft,12);LabUI.Size(card.gameObject,360,-1,1);
            controls=LabUI.Scroll(card.transform);
            readout=Text(window.Body,"",88,19,LabUI.Ink);
            readout.enableAutoSizing=true;readout.fontSizeMin=16;readout.fontSizeMax=19;
            Text(window.Body,"გამარტივებული სასწავლო მოდელი • ზომები და დრო პირობითია • შედეგები რეალურ გაზომვებს არ ასახავს",34,14,LabUI.Muted);
            if(id=="membrane")Membrane();else if(id=="chromatography")Chromatography();else Gas();
        }
        private TMP_Text Text(Transform parent,string value,float height,float size=18,Color? color=null)
        {var text=LabUI.Text(parent,value,size,color);LabUI.Size(text.gameObject,height);return text;}
        private RectTransform Shape(string name,Vector2 point,Vector2 size,Color color)
        {
            var image=LabUI.Panel(stage,name,color);var rect=image.rectTransform;
            rect.anchorMin=rect.anchorMax=Vector2.one*0.5f;rect.anchoredPosition=point;rect.sizeDelta=size;
            return rect;
        }
        private TMP_Text Label(string value,Vector2 point,float width,float size,Color color)
        {
            var text=LabUI.Text(stage,value,size,color);text.alignment=TextAlignmentOptions.Center;
            text.rectTransform.anchorMin=text.rectTransform.anchorMax=Vector2.one*0.5f;
            text.rectTransform.anchoredPosition=point;text.rectTransform.sizeDelta=new Vector2(width,42);return text;
        }
        private UnityEngine.UI.Slider Slider(string title,float initial,float min,float max,Action<float> changed)
        {
            var label=Text(controls,title+": "+initial.ToString("0.0"),28,17);
            var go=LabUI.Object(controls,title,typeof(UnityEngine.UI.Slider));LabUI.Size(go,32);
            var track=LabUI.Panel(go.transform,"ზოლი",LabUI.Paper);LabUI.Fill(track.rectTransform,8);track.raycastTarget=true;
            var fill=LabUI.Panel(track.transform,"შევსება",LabUI.Mint);LabUI.Fill(fill.rectTransform);
            var handle=LabUI.Panel(go.transform,"სახელური",LabUI.Green);handle.raycastTarget=true;handle.rectTransform.sizeDelta=new Vector2(22,30);
            var slider=go.GetComponent<UnityEngine.UI.Slider>();slider.fillRect=fill.rectTransform;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;
            slider.minValue=min;slider.maxValue=max;slider.SetValueWithoutNotify(initial);
            slider.onValueChanged.AddListener(v=>{label.text=title+": "+v.ToString("0.0");changed(v);});
            return slider;
        }
        private void Award(string id,string why)
        {
            if(!achieved.Add(id))return;
            bool paid=GameSaveManager.Instance.ClaimResearch(id);
            readout.text=(paid?"მიზანი შესრულებულია! +20 ქიმქოინი. ":"ამ მიზნის ჯილდო დღეს უკვე მიღებული გაქვს. ")+why;
            if(paid){AudioManager.Instance?.Correct();AudioManager.Instance?.Coin();}
        }
        private void MakeParticles(int count,bool membrane)
        {
            for(int i=0;i<count;i++)
            {
                var point=new Vector2(membrane?(i<24?UnityEngine.Random.Range(-235,-25):UnityEngine.Random.Range(25,235)):UnityEngine.Random.Range(-225,45),UnityEngine.Random.Range(-115,115));
                var rect=Shape("ნაწილაკი",point,Vector2.one*12,membrane?new Color32(103,199,236,255):LabUI.Gold);
                particles.Add(new Particle{Rect=rect,Position=point,Velocity=UnityEngine.Random.insideUnitCircle.normalized*90});
            }
        }
        private void Membrane()
        {
            atp=6;pumped=0;motion=1;
            Shape("მემბრანა",Vector2.zero,new Vector2(14,270),new Color32(218,168,93,255));
            Shape("ლიპიდური ფენა",new Vector2(-7,0),new Vector2(3,270),LabUI.Gold);
            Shape("ლიპიდური ფენა",new Vector2(7,0),new Vector2(3,270),LabUI.Gold);
            gate=Shape("შერჩევითი არხი",Vector2.zero,new Vector2(24,136),LabUI.Coral);
            MakeParticles(30,true);
            Text(controls,"1. გახსენი არხი და დაელოდე, სანამ მარჯვნივ სულ მცირე 11 ნაწილაკი აღმოჩნდება.\n2. ტუმბოთი 6 ნაწილაკი გრადიენტის საწინააღმდეგოდ გადაიტანე.",122);
            var toggle=LabUI.Button(controls,"არხის გახსნა",()=>{},44);
            toggle.onClick.AddListener(()=>{channel=!channel;gate.GetComponent<Image>().color=channel?LabUI.Mint:LabUI.Coral;toggle.GetComponentInChildren<TMP_Text>().text=channel?"არხის დახურვა":"არხის გახსნა";});
            LabUI.Button(controls,"ტუმბოს ამოქმედება ← • 1 ATP",()=>Pump(),44);
            Slider("მოძრაობის სიჩქარე",1,0.5f,3,v=>motion=v);
            LabUI.Secondary(controls,"პაუზა / გაგრძელება",()=>paused=!paused,40);
            LabUI.Secondary(controls,"საწყისი პირობების აღდგენა",ResetMembrane,40);
            readout.text="დახურულ არხში ნაწილაკები ვერ გადადის. არხის გახსნისას იწყება პასიური დიფუზია. ტუმბოს კი ენერგია სჭირდება. მუხტი და ძაბვა ამ მოდელში გათვალისწინებული არ არის.";
        }
        public bool Pump()
        {
            if(activity!="membrane" || atp<=0)return false;
            if(RightCount>15){readout.text="ტუმბოს მიზნის შესასრულებლად მარცხნივ კონცენტრაცია უფრო მაღალი უნდა იყოს. აღადგინე საწყისი პირობები.";return false;}
            var particle=particles.Find(p=>p.Position.x>0);
            if(particle==null){readout.text="მარჯვენა მხარეს ნაწილაკი აღარ დარჩა. გახსენი არხი და დაელოდე ნაწილაკების გადასვლას.";return false;}
            particle.Position=new Vector2(-35,0);particle.Rect.anchoredPosition=particle.Position;particle.Velocity=Vector2.left*90;atp--;pumped++;
            readout.text="ტუმბომ ნაწილაკი კონცენტრაციის გრადიენტის საწინააღმდეგოდ გადაიტანა. დაიხარჯა 1 ATP.";
            if(pumped>=6)Award("membrane_pump","პასიური დიფუზიისგან განსხვავებით, ტუმბოს მუშაობისთვის ენერგიაა საჭირო.");
            return true;
        }
        private void Chromatography()
        {
            prediction=-1;ink=0;solventAlcohol=false;runTime=0;
            Shape("ქაღალდი",new Vector2(0,0),new Vector2(270,262),LabUI.Paper);
            Shape("საწყისი ხაზი",new Vector2(0,-110),new Vector2(240,2),LabUI.Ink);
            solventFront=Shape("გამხსნელის ფრონტი",new Vector2(0,-110),new Vector2(240,3),LabUI.Mint);
            for(int i=0;i<3;i++)bands.Add(Shape(Pigments[i],new Vector2((i-1)*58,-110),new Vector2(44,12),PigmentColors[i]));
            Text(controls,"აირჩიე გამხსნელი და მელნის ნარევი. შენი აზრით, რომელი პიგმენტი გაივლის ყველაზე დიდ მანძილს?",94);
            var solvent=LabUI.Button(controls,"გამხსნელი: წყალი",()=>{},42);
            solvent.onClick.AddListener(()=>{if(running)return;solventAlcohol=!solventAlcohol;runTime=0;solvent.GetComponentInChildren<TMP_Text>().text=solventAlcohol?"გამხსნელი: სპირტი":"გამხსნელი: წყალი";});
            var inkButton=LabUI.Secondary(controls,"მელანი: ნარევი 1",()=>{},40);
            inkButton.onClick.AddListener(()=>{if(running)return;ink=(ink+1)%3;runTime=0;inkButton.GetComponentInChildren<TMP_Text>().text="მელანი: ნარევი "+(ink+1);});
            var row=LabUI.Row(controls,"პროგნოზი",6);LabUI.Size(row.gameObject,48);
            for(int i=0;i<3;i++){int p=i;var b=LabUI.Button(row.transform,Pigments[i],()=>Predict(p),48);b.GetComponent<Image>().color=Color.Lerp(PigmentColors[i],LabUI.Navy,0.35f);}
            LabUI.Button(controls,"ცდის დაწყება / გამეორება",StartSeparation,44);
            readout.text="ამ მოდელში პიგმენტების გადაადგილება გამხსნელზეა დამოკიდებული. აირჩიე პიგმენტი, ჩაატარე ცდა და შეამოწმე შენი ვარაუდი.";
        }
        public void Predict(int index){if(activity=="chromatography" && !running && index>=0 && index<3){prediction=index;readout.text="შენი ვარაუდი: "+Pigments[index]+". ახლა დაიწყე ცდა.";}}
        public void StartSeparation(){if(activity!="chromatography")return;if(prediction<0){readout.text="ცდის დაწყებამდე აირჩიე პიგმენტი, რომელიც, შენი აზრით, ყველაზე შორს გადაადგილდება.";return;}runTime=0;running=true;}
        public void ResetMembrane()
        {
            if(activity!="membrane")return;
            atp=6;pumped=0;paused=false;
            for(int i=0;i<particles.Count;i++)
            {
                var p=particles[i];p.Position=new Vector2(i<24?UnityEngine.Random.Range(-235,-25):UnityEngine.Random.Range(25,235),UnityEngine.Random.Range(-115,115));
                p.Velocity=UnityEngine.Random.insideUnitCircle.normalized*90;p.Rect.anchoredPosition=p.Position;
            }
            readout.text="საწყისი პირობები აღდგა: მარცხნივ 24 ნაწილაკია, მარჯვნივ — 6. გაქვს 6 ATP. დღიური ჯილდო ხელმეორედ არ გაიცემა.";
        }
        private void Gas()
        {
            volume=2;temperature=300;stableTime=0;motion=1;
            Shape("ჭურჭელი",Vector2.zero,new Vector2(490,266),new Color32(63,86,75,255));
            piston=Shape("მოძრავი დგუში",new Vector2(72,0),new Vector2(18,270),LabUI.Paper);
            MakeParticles(24,false);
            Text(controls,"შეინარჩუნე ტემპერატურა 300 K-ზე და მოცულობა 2-დან 1-მდე შეამცირე. შეინარჩუნე ეს პირობები 2 წამით: წნევა გაორმაგდება.",98);
            Slider("მოცულობა V",2,1,3,v=>{volume=v;stableTime=0;});
            var thermal=Slider("ტემპერატურა K",300,200,600,v=>{temperature=v;stableTime=0;});
            LabUI.Secondary(controls,"ტემპერატურის აღდგენა: 300 K",()=>thermal.value=300,40);
            LabUI.Secondary(controls,"პაუზა / გაგრძელება",()=>paused=!paused,40);
            readout.text="დახურული იდეალური აირი: ნაწილაკების რაოდენობა ფიქსირებულია. P/P₀ = (T / 300) × (2 / V). შეადარე შეკუმშვა და გაცხელება.";
        }
        private void Update()
        {
            if(window==null)return;
            float dt=Mathf.Min(Time.unscaledDeltaTime,0.05f);
            if(activity=="chromatography")
            {
                var rf=solventAlcohol?AlcoholRf[ink]:WaterRf[ink];
                if(running)
                {
                    runTime=Mathf.Min(8,runTime+Time.unscaledDeltaTime);
                    if(runTime>=8)
                    {
                        running=false;
                        int fastest=Array.IndexOf(rf,rf.Max());
                        string why="ყველაზე შორს წავიდა "+Pigments[fastest]+". Rf = პიგმენტის მანძილი / გამხსნელის მანძილი = "+rf[fastest].ToString("0.00")+". გამხსნელთან უფრო ძლიერი მიდრეკილება გადაადგილებას ზრდის.";
                        if(prediction==fastest)Award("chromatography",why);
                        else readout.text="პროგნოზი არ დაემთხვა. "+why+" შეცვალე პროგნოზი და ხელახლა სცადე.";
                    }
                }
                float front=218*runTime/8;
                solventFront.anchoredPosition=new Vector2(0,-110+front);
                for(int i=0;i<3;i++)bands[i].anchoredPosition=new Vector2((i-1)*58,-110+front*rf[i]);
                if(Time.unscaledTime>=nextReadout){nextReadout=Time.unscaledTime+0.15f;stats.text="ფრონტი "+Mathf.RoundToInt(front)+" • Rf: "+rf[0].ToString("0.00")+" / "+rf[1].ToString("0.00")+" / "+rf[2].ToString("0.00");}
                return;
            }
            if(!paused)
            {
                float rightWall=activity=="gas"?-240+volume*150:245;
                if(activity=="gas")piston.anchoredPosition=new Vector2(rightWall+12,0);
                foreach(var p in particles)
                {
                    float previousX=p.Position.x;
                    p.Velocity=Quaternion.Euler(0,0,UnityEngine.Random.Range(-30f,30f)*dt)*p.Velocity;
                    p.Position+=p.Velocity*dt*(activity=="gas"?Mathf.Sqrt(temperature/300f):motion);
                    if(p.Position.x < -240){p.Position.x=-240;p.Velocity.x=Mathf.Abs(p.Velocity.x);}
                    if(p.Position.x > rightWall){p.Position.x=rightWall;p.Velocity.x=-Mathf.Abs(p.Velocity.x);}
                    if(Mathf.Abs(p.Position.y)>125){p.Position.y=Mathf.Clamp(p.Position.y,-125,125);p.Velocity.y=-p.Velocity.y;}
                    if(activity=="membrane" && Mathf.Sign(previousX)!=Mathf.Sign(p.Position.x) && (!channel || Mathf.Abs(p.Position.y)>66))
                    {p.Position.x=previousX<0?-10:10;p.Velocity.x=previousX<0?-Mathf.Abs(p.Velocity.x):Mathf.Abs(p.Velocity.x);}
                    p.Rect.anchoredPosition=p.Position;
                }
                if(activity=="membrane" && channel && RightCount>=11)Award("membrane_diffusion","გახსნილი არხით ნაწილაკები ორივე მიმართულებით მოძრაობს. საწყისად წმინდა ნაკადი მაღალი კონცენტრაციის არიდან დაბალი კონცენტრაციის არეშიაა.");
                if(activity=="gas")
                {
                    stableTime=Mathf.Abs(temperature-300)<1 && volume<1.02f?stableTime+Time.unscaledDeltaTime:0;
                    if(stableTime>=2)Award("gas_compression","უცვლელ ტემპერატურაზე მოცულობის განახევრებამ წნევა გააორმაგა: P·V მუდმივია.");
                }
            }
            if(Time.unscaledTime>=nextReadout)
            {
                nextReadout=Time.unscaledTime+0.15f;
                stats.text=activity=="membrane"?"მარცხნივ "+(30-RightCount)+"  |  მარჯვნივ "+RightCount+"  •  ATP "+atp:"T = "+temperature.ToString("0")+" K  •  V = "+volume.ToString("0.00")+"  •  P/P₀ = "+RelativePressure.ToString("0.00");
            }
        }
        public void Close(){if(window!=null)window.Close();}
    }
}

