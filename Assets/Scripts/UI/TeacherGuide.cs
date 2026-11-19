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
                    S("გამარჯობა, {name}! მე შენი ქიმიის მასწავლებელი ვარ. მოკლედ აგიხსნი, როგორ მუშაობს ჩვენი ლაბორატორია.", g => g.Dialogue.Teacher.transform.position),
                    S("სასიარულოდ გამოიყენე WASD ან ისრები. ნივთთან ან ადამიანთან რომ მიხვალ, მის თავზე [E] მინიშნება გამოჩნდება.", g => g.player.transform.position),
                    S("[E] ღილაკით იწყებ მოქმედებას: საუბარს, ცდას ან ყიდვას. ფანჯარას Esc ღილაკით დახურავ."),
                    S("ლაბორატორიაში ყველაფერი უსაფრთხოებით იწყება. ამ მბზინავ კარადაში დამცავი ხალათი გელოდება.", g => Class(g, g.lockerPosition.x, g.lockerPosition.y + 1), "", 5),
                    S("ზედა მარცხენა ბარათზე შენი მიმდინარე მიზანი ჩანს, ისარი კი გზას გაჩვენებს. ახლა კარადასთან მიდი და ხალათი აიღე.", null, "tracker"),
                }
            },
            new GuideLesson
            {
                ID = "coat", Title = "რვეული და მიზნები", Ready = () => Data.starterCoatCollected,
                Steps = new[]
                {
                    S("ხალათი კარგად გიხდება. ქიმიაზე სანამ დავიწყებთ, გაჩვენებ, სად ჩაიწერო ნასწავლი.", g => g.Dialogue.Teacher.transform.position),
                    S("ჩემს მაგიდასთან მიდი და [E] დააჭირე. იქ შენი რვეული გელოდება.", g => Class(g, g.teacherDeskPosition.x, g.teacherDeskPosition.y + 1), "", 5),
                    S("რვეულს N ღილაკით ყოველთვის გახსნი, სასწავლო რუკას კი — M ღილაკით. ზედა მარცხენა ბარათი კი ყოველთვის გეტყვის, რა არის შემდეგი ნაბიჯი.", null, "tracker"),
                }
            },
            new GuideLesson
            {
                ID = "tour", Title = "კლასის დათვალიერება", Ready = () => Data.tutorialCompleted,
                Steps = new[]
                {
                    S("ყოჩაღ, {name}! რვეული გაიცანი და 10 ქიმქოინი მიიღე. ახლა კლასს დაგათვალიერებინებ.", g => g.Dialogue.Teacher.transform.position),
                    S("ზედა მარჯვენა კუთხეში ჩანს ქიმქოინები, დღიური სერია და ზედიზედ სწორი პასუხები. ყოველდღე თამაშისას სერია იზრდება.", null, "stats"),
                    S("ეს მაგიდები სწავლის გზის ნაწილია: ატომების, ელემენტების, მოლეკულების, რეაქციების, ხსნარებისა და მჟავა-ფუძის მაგიდები. თითოეული გაიხსნება, როცა შესაბამის გაკვეთილს გაივლი.", g => Class(g, 44.75f, -3), "", 5.4f),
                    S("ქვემოთ რთული გამოწვევების მაგიდაა: შემთხვევითი კითხვები, განტოლებები და შეჯიბრი. ის ბოლოს გაიხსნება, როცა საფუძვლებს ისწავლი.", g => Class(g, 37.5f, -20), "", 4.4f),
                    S("კარადასა და თაროსთან ხალათებს, სათვალეებს და ქუდებს იყიდი. მაღაზია B ღილაკითაც იხსნება.", g => Class(g, 62, -8), "", 4.4f),
                    S("აქ კვირის რეიტინგია. ისიც რთულ გამოწვევებთან ერთად გაიხსნება.", g => Class(g, 55, 10.5f), "", 4.4f),
                    S("განცხადებების დაფაზე დღის მოვლენები და პატარა საქმეები წერია. ის J ღილაკითაც იხსნება.", g => Class(g, 42.9f, 12), "", 4.4f),
                    S("ამ კარადაში შენი კოლექცია ინახება: მინერალები, ნიმუშები, ფაქტები და ჯილდოები. C ღილაკითაც გახსნი.", g => Class(g, 28.4f, 10.5f), "", 4.4f),
                    S("ქვედა პანელიდან ყველა მენიუ იხსნება. ღილაკების ასოები ქვემოთ წერია.", null, "dock"),
                    S("ახლა კი მთავარი: ქიმიის პირველ გაკვეთილს დავიწყებთ. ნულიდან ვისწავლით, ნაბიჯ-ნაბიჯ.", g => g.Dialogue.Teacher.transform.position),
                }
            },
            new GuideLesson
            {
                ID = "lab", Title = "პირადი ლაბორატორია", Ready = () => Data.tutorialCompleted,
                Steps = new[]
                {
                    S("ამ კარიდან შენს პირად ლაბორატორიაში შედიხარ. იქ საკუთარი აღჭურვილობა და ოთახი გექნება.", g => Class(g, 46.5f, 12.5f), "", 4.6f),
                    Shot("ლაბორატორიაში L ღილაკი მაღაზიასა და ოთახის გეგმას ხსნის. იყიდე ხელსაწყო და თავისუფალ უჯრაზე დადგი.", "პირადი ლაბორატორია", g => g.Personal.RoomOrigin + new Vector2(0, 3)),
                    Shot("დადგმულ ხელსაწყოსთან [E] ცდას იწყებს. ცდები უფასოა და ოსტატობას ზრდის.", "ხელსაწყოები", g => g.Personal.RoomOrigin + new Vector2(-2, 1)),
                    S("R ღილაკი კვლევების დაფას ხსნის. იქ გრძელი პროექტებია: მასალები, ცდა, დაკვირვება და ჩაბარება. ზოგი პროექტი რამდენიმე დღე გრძელდება.", null, "dock"),
                    S("ცდაში შეცდომა საშიში არ არის. თუ ქაფი ამოვა ან რამე დაიღვრება, [E]-ით დაასუფთავე და თავიდან სცადე."),
                    S("დეკორაციებიც შეგიძლია: მცენარეები, აკვარიუმი, რადიო. ისინი ოთახს უფრო მყუდროს ხდის.", g => g.Dialogue.Teacher.transform.position),
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
