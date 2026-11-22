using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChemistryGame
{
    /// <summary>One line of a lesson and what the screen shows while the teacher says it.</summary>
    public sealed class GuideStep
    {
        public string Text;
        /// <summary>World point the camera pans to (only inside the current room).</summary>
        public Func<LabGame, Vector3?> Pan;
        /// <summary>World point shown in a small picture-in-picture snapshot (any room).</summary>
        public Func<LabGame, Vector3?> Snap;
        public string Caption = "";
        public string Hud = "";
        public float Zoom = 4.6f;
    }

    public sealed class GuideLesson
    {
        public string ID, Title;
        public Func<bool> Ready;
        public GuideStep[] Steps;
    }

    /// <summary>
    /// The teacher-led tutorial. It starts with the basics on the first day and unlocks new
    /// lessons as the player grows: personal lab, the rest of the school, and long-term goals.
    /// While she talks, the camera pans to what she describes, or a snapshot of a far room
    /// appears above the dialogue box.
    /// </summary>
    public sealed class TeacherGuide : MonoBehaviour
    {
        private static GameSaveData Data => GameSaveManager.Instance.Data;
        private LabGame game;
        private readonly Queue<string> queue = new Queue<string>();
        private readonly HashSet<string> autoPlayed = new HashSet<string>();

        public void Initialize(LabGame lab) => game = lab;

        private static Vector3 Class(LabGame g, float x, float y) => g.classroom.TransformPoint(new Vector3(x, y));
        private static Vector3 Area(LabGame g, string id, float y = 2) => SchoolAreas.Def(id).Origin + new Vector2(0, y);
        private static GuideStep S(string text, Func<LabGame, Vector3?> pan = null, string hud = "", float zoom = 4.6f) =>
            new GuideStep { Text = text, Pan = pan, Hud = hud, Zoom = zoom };
        private static GuideStep Shot(string text, string caption, Func<LabGame, Vector3?> snap) =>
            new GuideStep { Text = text, Snap = snap, Caption = caption };

        public static readonly GuideLesson[] Lessons =
        {
            new GuideLesson
            {
                ID = "welcome", Title = "პირველი ნაბიჯები", Ready = () => true,
                Steps = new[]
                {
                    S("გამარჯობა, {name}! ქვედა მარცხენა ჯოისტიკით (ან WASD-ით) იარე. რასაც მბზინავი ჩარჩო აქვს, იმას „გამოყენების“ ღილაკით (ან [E]-ით) გამოიყენებ.", g => g.player.transform.position),
                    S("ჯერ უსაფრთხოება: იმ მბზინავ კარადაში დამცავი ხალათი გელოდება.", g => Class(g, g.lockerPosition.x, g.lockerPosition.y + 1), "", 5),
                }
            },
            new GuideLesson
            {
                ID = "coat", Title = "პირველი ცდა", Ready = () => Data.starterCoatCollected,
                Steps = new[]
                {
                    S("ხალათი გიხდება! ახლა რამე შევურიოთ: რეაქციების მაგიდაზე ძმარი და სოდა გელოდება. ჯერ გამოიცანი, რა მოხდება!", g => g.StationPoint(StationType.ReactionTable), "", 5),
                }
            },
            new GuideLesson
            {
                ID = "tour", Title = "კლასის დათვალიერება", Ready = () => Data.tutorialCompleted,
                Steps = new[]
                {
                    S("ეს მაგიდები ცდების სადგურებია. თითოეული აინთება, როცა მის გაკვეთილს გაივლი.", g => Class(g, 44.75f, -3), "", 5.4f),
                    S("ოქროსფრად მბზინავი მაგიდა ოსტატებისთვისაა: რთული გამოწვევები. ის ბოლოს გაიხსნება.", g => Class(g, 37.5f, -20), "", 4.4f),
                    S("თაროზე ხალათებს და ქუდებს იყიდი, კედელზე კი პერიოდული ცხრილი კიდია.", g => Class(g, 60, 2), "", 6),
                    S("ეს კარი შენს პირად ლაბორატორიაში გადის. წადი და დაათვალიერე!", g => Class(g, 46.5f, 12.5f), "", 4.6f),
                }
            },
            new GuideLesson
            {
                ID = "lab", Title = "პირადი ლაბორატორია", Ready = () => Data.tutorialCompleted,
                Steps = new[]
                {
                    S("ამ კარიდან შენს პირად ლაბორატორიაში შედიხარ. იქ საკუთარი აღჭურვილობა და ოთახი გექნება.", g => Class(g, 46.5f, 12.5f), "", 4.6f),
                    Shot("კედელთან გეგმის დაფაა: იქ ხელსაწყოს იყიდი და ოთახში დადგამ.", "პირადი ლაბორატორია", g => g.Personal.RoomOrigin + new Vector2(0, 3)),
                    Shot("დადგმული ხელსაწყოს გამოყენება ცდას იწყებს. თუ ქაფი ამოვა, არაუშავს — დაასუფთავე და კიდევ სცადე.", "ხელსაწყოები", g => g.Personal.RoomOrigin + new Vector2(-2, 1)),
                }
            },
            new GuideLesson
            {
                ID = "school", Title = "სკოლა და კლასელები", Ready = () => Data.guideLessons.Contains("lab"),
                Steps = new[]
                {
                    S("კლასის ქვედა ნაწილში ფეხსაწმენდია. იქიდან დერეფანში გახვალ.", g => Class(g, SchoolAreas.ClassroomDoor.x, SchoolAreas.ClassroomDoor.y + 1.5f), "", 4.6f),
                    Shot("დერეფნიდან ოთხი ოთახია: სათავსო, კითხვის კუთხე, სათბური და გამოფენის დარბაზი.", "დერეფანი", g => Area(g, "hallway")),
                    Shot("სათავსოში ყოველდღე უფასო მასალებს აიღებ. იქ ლაბორანტი ქეთევანიც დაგეხმარება.", "სათავსო", g => Area(g, "storage")),
                    Shot("სათბურში ჯერ მცენარეები მორწყე, მერე მოსავალი აიღე. წვიმიან დღეს კასრში წვიმის წყალიც გროვდება.", "სათბური", g => Area(g, "greenhouse", 1)),
                    Shot("კითხვის კუთხეში ყოველდღე ერთ ახალ ფაქტს ისწავლი.", "კითხვის კუთხე", g => Area(g, "reading")),
                    S("კლასელებს ესაუბრე. თითოეულს თავისი ამბავი და თხოვნები აქვს. რაც უფრო ხშირად ესაუბრები, მით უფრო დაუმეგობრდები.", g =>
                        g.Classmates.FirstOrDefault(n => n.gameObject.activeInHierarchy && n.transform.GetChild(0).gameObject.activeSelf && Vector2.Distance(n.transform.position, g.classroom.position) < 80)?.transform.position
                        ?? g.Dialogue.Teacher.transform.position, "", 5),
                    S("კლასელის თავზე ძახილის ნიშანი ახალ ამბავს ნიშნავს, კითხვის ნიშანი კი იმას, რომ მისი თხოვნა შეასრულე."),
                }
            },
            new GuideLesson
            {
                ID = "growth", Title = "გრძელი გზა", Ready = () => Data.guideLessons.Contains("school") && (LabProgress.ReputationLevel >= 1 || Data.projects.Any(p => p.timesCompleted > 0)),
                Steps = new[]
                {
                    S("შენი ლაბორატორიის რეპუტაცია იზრდება. ზედა ზოლში შენი წოდება ჩანს.", null, "status"),
                    S("რეპუტაცია იზრდება პროექტებით, კლასელების თხოვნებითა და ბაზრობებით. ის ახალ ოთახებს, პროექტებს და ნივთებს ხსნის."),
                    Shot("გამოფენის დარბაზში ყოველ კვირას სამეცნიერო ბაზრობა იმართება. დასრულებული პროექტი წარადგინე და კვირის ბოლოს შედეგებს გაიგებ.", "გამოფენის დარბაზი", g => Area(g, "exhibition")),
                    S("დღე სამ ნაწილად იყოფა: დილა, შუადღე და საღამო. კლასელები დღის სხვადასხვა დროს სხვადასხვა ადგილას არიან.", null, "status"),
                    S("კვირის განმავლობაში სხვადასხვა მოვლენაა: შეჯიბრი, ინსპექცია, მოგზაური მკვლევარი. ყველაფერი განცხადებების დაფაზე წერია.", g => Class(g, 42.9f, 12), "", 4.6f),
                    S("და ბოლოს: ყველაფრის ერთ დღეში გაკეთება საჭირო არ არის. ცოტ-ცოტა დაბრუნდი, ლაბორატორია შენ გელოდება.", g => g.Dialogue.Teacher.transform.position),
                }
            },
        };

        public static bool Done(string id) => Data.guideLessons.Contains(id);
        public static GuideLesson Get(string id) => Lessons.FirstOrDefault(l => l.ID == id);

        /// <summary>The next lesson the teacher can give, if any.</summary>
        public static GuideLesson Next => Lessons.FirstOrDefault(l => !Done(l.ID) && l.Ready() &&
            (l.ID != "welcome" && l.ID != "coat" || !Data.tutorialCompleted));

        public void Queue(string id)
        {
            if (Done(id) || autoPlayed.Contains(id)) return;
            autoPlayed.Add(id);
            queue.Enqueue(id);
            TryPlay();
        }

        private void Update() { if (queue.Count > 0) TryPlay(); }

        private void TryPlay()
        {
            if (LabUI.AnyOpen || game.Dialogue == null || game.Dialogue.IsOpen || !Data.profileCompleted) return;
            var lesson = Get(queue.Dequeue());
            if (lesson != null) Play(game, lesson, null);
        }

        /// <summary>Plays a lesson. If the dialogue is already open the lesson continues inside it.</summary>
        public static void Play(LabGame game, GuideLesson lesson, Action then)
        {
            var dialogue = game.Dialogue;
            string name = string.IsNullOrWhiteSpace(Data.playerName) ? "ჩემო მოსწავლე" : Data.playerName;
            var lines = lesson.Steps.Select(s => s.Text.Replace("{name}", name)).ToArray();
            void Run() => dialogue.Tour(() =>
            {
                if (!Data.guideLessons.Contains(lesson.ID)) { Data.guideLessons.Add(lesson.ID); GameSaveManager.Instance.Commit(); }
                dialogue.PointAt(null);
                dialogue.Snap(null, "");
                game.HUD.Highlight("");
                if (then != null) then();
                // After the classroom tour the teacher begins the chemistry curriculum.
                else if (lesson.ID == "tour" && Curriculum.Next != null && game.Lessons != null) game.Lessons.Begin(Curriculum.Next);
                else dialogue.End();
            }, lines, page =>
            {
                var step = lesson.Steps[page];
                dialogue.PointAt(step.Pan?.Invoke(game), step.Zoom);
                dialogue.Snap(step.Snap?.Invoke(game), step.Caption);
                game.HUD.Highlight(step.Hud);
            });
            if (dialogue.IsOpen) Run();
            else dialogue.Converse(dialogue.Teacher, "მასწავლებელი • " + lesson.Title, Run);
        }
    }
}
