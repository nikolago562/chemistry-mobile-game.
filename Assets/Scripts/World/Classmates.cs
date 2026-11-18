using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace ChemistryGame
{
    public sealed class StoryBeat
    {
        public int Friendship; public string[] Lines; public string Gift;
        public StoryBeat(int friendship, string gift, params string[] lines) { Friendship = friendship; Gift = gift; Lines = lines; }
    }

    public sealed class RequestDef
    {
        public string ID, Ask, Thanks, Kind, Key, Gift;
        public int Count, Coins, Friendship, Reputation;
    }

    public sealed class ClassmateDef
    {
        public string ID, Name, Role;
        public Color Skin, Hair, Shirt;
        public float Voice;
        /// <summary>Area and local position for morning, afternoon and evening ("" = not at school).</summary>
        public (string area, Vector2 point)[] Schedule;
        public string[] Activities;
        public string[] SmallTalk;
        public StoryBeat[] Story;
        public RequestDef[] Requests;
        public bool Visitor;
    }

    public static class Classmates
    {
        private static RequestDef R(string id, string kind, string key, int count, int coins, string ask, string thanks, string gift = "", int friendship = 10, int reputation = 4) =>
            new RequestDef { ID = id, Kind = kind, Key = key, Count = count, Coins = coins, Ask = ask, Thanks = thanks, Gift = gift, Friendship = friendship, Reputation = reputation };

        public static readonly ClassmateDef[] All =
        {
            new ClassmateDef
            {
                ID = "nika", Name = "ნიკა", Role = "გამომგონებელი • ბატარეას აწყობს", Voice = 0.82f,
                Skin = new Color32(232, 180, 130, 255), Hair = new Color32(60, 40, 30, 255), Shirt = new Color32(230, 160, 60, 255),
                Schedule = new[] { ("classroom", new Vector2(44.5f, -2.5f)), ("storage", new Vector2(2.5f, 0)), ("hallway", new Vector2(-8, 1)) },
                Activities = new[] { "სადენებს ახვევს", "ფირფიტებს ითვლის", "სქემას ხატავს" },
                SmallTalk = new[] { "ვფიქრობ, საათი ლიმონებით ავამუშაო. სასაცილოა, მაგრამ მუშაობს!", "იცი, რომ ელექტრონები თუთიიდან სპილენძისკენ მიდიან? ეს მთელი საიდუმლოა.", "დღეს კიდევ ერთი სადენი დავწვი... არა, ხუმრობ, უბრალოდ იზოლაცია გავფცქვენი." },
                Story = new[]
                {
                    new StoryBeat(0, "", "გამარჯობა! მე ნიკა ვარ. ვოცნებობ ბატარეაზე, რომელიც მთელ კლასს გაანათებს.", "ჯერ ერთი პატარა ნათურაც კი ვერ ავანთე, მაგრამ არ ვნებდები!"),
                    new StoryBeat(5, "", "ჩემი ბატარეა გუშინ არ ამუშავდა. ორივე ფირფიტა ერთი და იგივე ლითონისგან იყო!", "ქეთევანმა მითხრა: საჭიროა ორი სხვადასხვა ლითონი. სულელი ვარ, ხომ?"),
                    new StoryBeat(15, "", "ახალი იდეა: ლიმონი! მისი წვენი ელექტროლიტია, იონები მასში თავისუფლად მოძრაობს.", "თუ ლიმონებს მოიტან, ერთად ვცადოთ."),
                    new StoryBeat(30, "mineral:pyrite", "აინთო! პაწაწინა LED-მა დაიწყო ციმციმი! ოთხი ლიმონი მიმდევრობით შევაერთე.", "ეს პირიტი ჩემი იღბლიანი ქვაა. შენთვის მინდა, დიდი მადლობა დახმარებისთვის."),
                    new StoryBeat(55, "", "ბაზრობაზე ლიმონის საათს წარვადგენ. ლუკა იცინის, მაგრამ ვნახოთ, ვინ გაიცინებს ბოლოს!", "შენ ჩემი პირველი ნამდვილი ლაბორატორიის მეგობარი ხარ, იცი?"),
                },
                Requests = new[]
                {
                    R("nika_copper", "deliver", "copper", 1, 20, "სპილენძის ფირფიტა მჭირდება. სათავსოში იქნება. მომიტან?", "იდეალურია! ახლა ჩემს ბატარეას ორი სხვადასხვა ლითონი ექნება."),
                    R("nika_lemons", "deliver", "lemon", 2, 18, "ორი ლიმონი მჭირდება სათბურიდან. ჩემი ძველი უკვე გამოშრა.", "ვაიმე, რა სურნელია! ეს ელექტროლიტი ნამდვილად იმუშავებს."),
                    R("nika_particles", "stat", "activity:particles", 1, 25, "ერთხელ ნაწილაკების დამჭერი სცადე და მითხარი, როგორ მოძრაობენ ელექტრონები.", "ანუ თუთიიდან გარე სადენით მიდიან! ახლა ყველაფერი გასაგებია.", "fact:battery"),
                    R("nika_solve", "solved", "solved", 3, 15, "ამოხსენი სამი რეაქცია. ერთად ვივარჯიშოთ ბაზრობამდე!", "სუპერ! მოდი, ხვალაც ასე ვიმეცადინოთ."),
                }
            },
            new ClassmateDef
            {
                ID = "mariam", Name = "მარიამი", Role = "მინერალების კოლექციონერი", Voice = 1.12f,
                Skin = new Color32(250, 205, 160, 255), Hair = new Color32(140, 80, 50, 255), Shirt = new Color32(120, 170, 200, 255),
                Schedule = new[] { ("reading", new Vector2(-4, 1)), ("exhibition", new Vector2(-2, -1.5f)), ("", Vector2.zero) },
                Activities = new[] { "ლუპით ათვალიერებს ქვას", "კატალოგს ავსებს", "კრისტალს თვლის" },
                SmallTalk = new[] { "იცი, რომ ფლუორიტი ზოგჯერ ულტრაიისფერ შუქზე ანათებს? ჯადოსნურია.", "ბაბუაჩემი კავკასიონზე მინერალებს აგროვებდა. მისი ყუთი ახლა ჩემთანაა.", "კრისტალის ფორმა ატომების განლაგებაზე მეტყველებს." },
                Story = new[]
                {
                    new StoryBeat(0, "", "გამარჯობა. მარიამი ვარ. მინერალებს ვაგროვებ... თუ გაინტერესებს, რა თქმა უნდა.", "ბევრს მოსაწყენი ჰგონია, მაგრამ თითოეულ ქვას თავისი ისტორია აქვს."),
                    new StoryBeat(5, "mineral:quartz", "ეს კვარცია. სილიციუმი და ჟანგბადი, SiO2. ყველაზე ჩვეულებრივი და ყველაზე ლამაზი.", "შენთვის მომაქვს. კოლექციის დასაწყისად."),
                    new StoryBeat(15, "", "ბაბუას ყუთში ერთი ნიმუშის წარწერა წაშლილია. ვერ ვიგებ, რა არის.", "ძმარზე შიშინებს... ეგებ კალციტია? ერთად შევამოწმოთ უცნობი ნიმუშის კვლევით."),
                    new StoryBeat(30, "mineral:amethyst", "კალციტი ყოფილა! ძმარში ნახშირორჟანგი გამოიყო. ბაბუა იამაყებდა.", "ეს ამეთვისტო ჩემი საყვარელია. მინდა, შენთან იყოს."),
                    new StoryBeat(55, "mineral:malachite", "გადავწყვიტე, ბაზრობაზე ბაბუას კოლექცია გამოვფინო. ცოტა მეშინია...", "მადლობა, რომ ყოველთვის მისმენ. ეს მალაქიტი ჩვენი მეგობრობის ნიშანია."),
                },
                Requests = new[]
                {
                    R("mariam_salt", "deliver", "salt", 1, 15, "მარილი მომიტანე, ჰალიტის კრისტალების გაზრდა მინდა.", "ვნახოთ, კუბური კრისტალები გამოვა თუ არა!", "mineral:halite"),
                    R("mariam_analysis", "stat", "activity:analysis", 1, 22, "ერთხელ ანალიზის სადგურზე ნიმუში ამოიცანი. მინდა, ვნახო, როგორ კეთდება.", "ხსნადობის ტესტი გენიალურია! ახლა ჩემს ნიმუშებსაც შევამოწმებ.", "mineral:gypsum"),
                    R("mariam_crystal", "collect", "sample:crystal", 1, 30, "შაბის კრისტალი თუ გაზარდე, მაჩვენე. ჩემს კოლექციაში ასეთი არ მაქვს.", "რა სწორი წახნაგებია! ნელი გაციება ნამდვილად მუშაობს."),
                    R("mariam_read", "stat", "read", 1, 12, "კითხვის კუთხეში ერთი წიგნი წაიკითხე და მერე მომიყევი.", "საინტერესოა! ცოდნა საუკეთესო კოლექციაა."),
                }
            },
            new ClassmateDef
            {
                ID = "giorgi", Name = "გიორგი", Role = "ცდების ეშინია • ფრთხილი", Voice = 0.95f,
                Skin = new Color32(200, 150, 110, 255), Hair = new Color32(40, 30, 30, 255), Shirt = new Color32(140, 190, 120, 255),
                Schedule = new[] { ("classroom", new Vector2(29.5f, -3)), ("reading", new Vector2(4.5f, 1.4f)), ("", Vector2.zero) },
                Activities = new[] { "უსაფრთხოების წესებს კითხულობს", "სათვალეს იწმენდს", "ღრმად სუნთქავს" },
                SmallTalk = new[] { "ხალათი და სათვალე... ხალათი და სათვალე... არ დამავიწყდეს.", "ერთხელ ქაფი გადმოვიდა და მთელი კლასი იცინოდა. მას მერე ცოტა მეშინია.", "მასწავლებელი ამბობს, შეცდომა სწავლის ნაწილიაო. ვცდილობ, დავიჯერო." },
                Story = new[]
                {
                    new StoryBeat(0, "", "ო... გამარჯობა. მე გიორგი ვარ. ბოდიში, უბრალოდ... ცდებზე ძალიან ვნერვიულობ.", "ყოველთვის მგონია, რომ რაღაც აფეთქდება."),
                    new StoryBeat(5, "", "შენ ცდებს ისე მშვიდად ატარებ. როგორ ახერხებ?", "ეგებ ერთხელ ერთად ვცადოთ რამე პატარა და უსაფრთხო?"),
                    new StoryBeat(15, "", "გუშინ ქაფი გამომივიდა, მაგრამ... სასაცილო იყო! სულაც არ იყო საშიში.", "ქეთევანმა მაჩვენა, როგორ დავასუფთავო. თურმე ავარიაც კი სწავლაა."),
                    new StoryBeat(30, "", "პირველად თავად ჩავატარე ნეიტრალიზაცია. pH შვიდი გამოვიდა! ზუსტად შვიდი!", "მადლობა, რომ არასდროს დამცინე."),
                    new StoryBeat(55, "", "ბაზრობაზე უსაფრთხოების სტენდს ვამზადებ. მინდა, სხვებსაც აღარ ეშინოდეთ.", "შენ დამეხმარე, რომ მამაცი გავმხდარიყავი. ამას არ დავივიწყებ."),
                },
                Requests = new[]
                {
                    R("giorgi_any", "stat", "activity:any", 1, 15, "ერთი ნებისმიერი ცდა ჩაატარე და მითხარი, რომ ყველაფერი რიგზეა...", "ხედავ? არაფერი აფეთქებულა. ეგებ შემდეგ ჯერზე მეც ვცადო."),
                    R("giorgi_clean", "stat", "clean", 1, 15, "თუ რამე დაიღვრება, დაასუფთავე და მაჩვენე, როგორ ქნა.", "ანუ ჯერ ვწმენდთ, მერე ვიღიმით. ეს შემიძლია!"),
                    R("giorgi_solve", "solved", "solved", 2, 12, "ორი რეაქცია ამოხსენი. ქაღალდზე რეაქცია არ მაშინებს.", "მადლობა! ქაღალდიდან ჭიქამდე ერთი ნაბიჯია, არა?"),
                    R("giorgi_safety", "collect", "fact:rust", 1, 15, "კითხვის კუთხეში ჟანგზე წიგნი ეგულება. წაიკითხე და მომიყევი?", "ჟანგს წყალიც და ჟანგბადიც სჭირდება! ესე იგი, ჩემს ველოსიპედს ვერ დაემუქრება."),
                }
            },
            new ClassmateDef
            {
                ID = "ana", Name = "ანა", Role = "ფერებითა და პიგმენტებით გატაცებული", Voice = 1.2f,
                Skin = new Color32(245, 200, 170, 255), Hair = new Color32(180, 70, 90, 255), Shirt = new Color32(200, 120, 200, 255),
                Schedule = new[] { ("greenhouse", new Vector2(-4, -2)), ("classroom", new Vector2(59.5f, -2)), ("hallway", new Vector2(6, 1)) },
                Activities = new[] { "ფურცლებს აგროვებს", "ფერებს ურევს", "ესკიზს ხატავს" },
                SmallTalk = new[] { "იისფერი ჩემი საყვარელი ფერია. ის კომბოსტოს წვენშიც იმალება!", "ძველად ლურჯი საღებავი ოქროზე ძვირი ღირდა. წარმოგიდგენია?", "ფერი სინათლისა და ქიმიის შეხვედრაა." },
                Story = new[]
                {
                    new StoryBeat(0, "", "ჰეი! ანა ვარ. ფერების სიყვარული მომიყვანა ქიმიაში.", "მინდა გავიგო, როგორ ქმნის ბუნება ამ ყველაფერს."),
                    new StoryBeat(5, "", "იცი, რომ ყვავილის ფერი ხან ნიადაგის pH-ზეა დამოკიდებული? ჰორტენზია ცისფერია ან ვარდისფერი!", "ეს ხომ პირდაპირ ჯადოქრობაა."),
                    new StoryBeat(15, "fact:colour", "ფოიერვერკის ფერებს ლითონები აძლევს. სტრონციუმი წითელს, სპილენძი მწვანეს.", "ერთ დღეს ჩემი ფერადი ალი მინდა შევქმნა. რა თქმა უნდა, მასწავლებელთან ერთად!"),
                    new StoryBeat(30, "", "ფერცვალებადი ხსნარი ჩავატარე და ვიტირე. ლამაზია!", "წითელი, იისფერი, ლურჯი, მწვანე... ერთ ჭიქაში მთელი ცისარტყელა."),
                    new StoryBeat(55, "", "ბაზრობისთვის ბუნებრივი საღებავების პალიტრა მინდა გავაკეთო. დამეხმარები?", "შენ შენი ლაბორატორიის ყველაზე ფერადი ადამიანი ხარ. ეს კომპლიმენტია!"),
                },
                Requests = new[]
                {
                    R("ana_flowers", "deliver", "flowers", 2, 16, "სათბურიდან ყვავილის ფურცლები მომიტანე. ბუნებრივი პიგმენტი მინდა.", "ვარდისფერი ანთოციანები! ზუსტად ის, რაც მჭირდებოდა."),
                    R("ana_cabbage", "deliver", "cabbage", 1, 16, "წითელი კომბოსტო მჭირდება. მერე აგიხსნი, რატომ!", "ახლა ინდიკატორს მოვამზადებ. ფერები მოვა!"),
                    R("ana_valve", "stat", "activity:valve", 1, 22, "დოზირების სარქველით ფერი მიზნამდე მიიყვანე. ჩემი ხელი ყოველთვის კანკალებს.", "რა ნაზად გამოგივიდა! ეს მწვანე ზუსტად ჩემი ჩანახატის ფერია."),
                    R("ana_photo", "collect", "photo:colour", 1, 30, "ფერცვალებადი ხსნარის ფოტო მინდა ჩემს ალბომში.", "ეს ფოტო ჩემს კედელზე ჩამოეკიდება!"),
                }
            },
            new ClassmateDef
            {
                ID = "luka", Name = "ლუკა", Role = "შეჯიბრების მოყვარული • ბაზრობისთვის ემზადება", Voice = 0.9f,
                Skin = new Color32(240, 190, 150, 255), Hair = new Color32(220, 180, 90, 255), Shirt = new Color32(80, 110, 190, 255),
                Schedule = new[] { ("exhibition", new Vector2(4, -1)), ("classroom", new Vector2(45, -12)), ("exhibition", new Vector2(-4, -2)) },
                Activities = new[] { "თავის ქულებს ითვლის", "პრეზენტაციას იმეორებს", "წამზომით ვარჯიშობს" },
                SmallTalk = new[] { "ამ კვირაში ბაზრობაზე პირველი ვიქნები. უბრალოდ გაფრთხილებ.", "ჩემი რეკორდი თორმეტი რეაქციაა ზედიზედ. ცადე, გადააჭარბე.", "მეორე ადგილი პირველი დამარცხებულია. ასე ამბობს ჩემი მწვრთნელი." },
                Story = new[]
                {
                    new StoryBeat(0, "", "შენ ახალი ხარ? ლუკა. სამეცნიერო ბაზრობის ჩემპიონი. ორჯერ.", "იმედია, საინტერესო მეტოქე იქნები."),
                    new StoryBeat(5, "", "ჰმ, ვნახე შენი პროექტი. ...ცუდი არ იყო.", "მაგრამ ჩემი უკეთესი იქნება. ეს ჯერ დასაწყისია."),
                    new StoryBeat(15, "", "გუშინ ჩემი ცდა ჩავარდა. ყველას წინაშე.", "ვერავის ვეუბნები, მაგრამ... ზოგჯერ ძალიან ვღელავ, რომ პირველი არ ვიქნები."),
                    new StoryBeat(30, "", "იცი რა? შენთან შეჯიბრი სახალისოა. მეტს ვსწავლობ, ვიდრე მარტო.", "მოდი, ერთმანეთის პროექტები შევამოწმოთ ბაზრობამდე."),
                    new StoryBeat(55, "award:competition", "კლასის შეჯიბრში ორივემ ერთნაირი ქულა ავიღეთ. ორი ჩემპიონი. მომწონს.", "ეს თასი გაიყოფა. თუმცა მომავალ კვირას მაინც მოგიგებ!"),
                },
                Requests = new[]
                {
                    R("luka_solve", "solved", "solved", 5, 25, "შეჯიბრი: ამოხსენი ხუთი რეაქცია, სანამ მე ჩემს ხუთს ვამოხსნი.", "ჰმ. სწრაფი ხარ. შემდეგ ჯერზე ათი იყოს!"),
                    R("luka_balance", "stat", "activity:balance", 1, 22, "განტოლებების დაბალანსების დაფაზე ვარჯიში ჩაატარე. ნახე, რამდენად სწრაფი ხარ.", "ცუდი არ არის. ჩემი რეკორდი მაინც ჩემია."),
                    R("luka_project", "stat", "projects", 1, 35, "ერთი კვლევითი პროექტი დაასრულე. მინდა, ვნახო, რას აკეთებ.", "კარგი პროექტია. ბაზრობაზე ნამდვილი ბრძოლა იქნება."),
                    R("luka_compare", "stat", "activity:compare", 1, 22, "რეაქციების შედარებაში გამოიცანი, რომელი უფრო სწრაფია. მე სამივე გამოვიცანი.", "ტემპერატურა ყოველთვის აჩქარებს! ეს ჩემი საიდუმლო იყო."),
                }
            },
            new ClassmateDef
            {
                ID = "ketevan", Name = "ქეთევანი", Role = "ლაბორანტი • სათავსოს მცველი", Voice = 1.05f,
                Skin = new Color32(230, 185, 150, 255), Hair = new Color32(90, 60, 110, 255), Shirt = new Color32(235, 235, 235, 255),
                Schedule = new[] { ("storage", new Vector2(-3, 1)), ("hallway", new Vector2(11, 1)), ("storage", new Vector2(-3, 1)) },
                Activities = new[] { "ბოთლებზე წარწერებს აკრავს", "აღწერას ადგენს", "ჭურჭელს რეცხავს" },
                SmallTalk = new[] { "ყოველ ბოთლს წარწერა უნდა ჰქონდეს. ეს ჩემი ოქროს წესია.", "დილით ყველა ნიჟარა შევამოწმე. სუფთა ლაბორატორია ბედნიერი ლაბორატორიაა.", "თუ რამე გჭირდება, აქ ვარ. უბრალოდ თაროს წესრიგი არ დამირღვიო." },
                Story = new[]
                {
                    new StoryBeat(0, "", "გამარჯობა, მე ქეთევანი ვარ, ლაბორანტი. სათავსო ჩემზეა.", "ყოველდღე აქ მასალები გელოდება. უბრალოდ თაროსთან მიდი."),
                    new StoryBeat(5, "sample", "მასწავლებელმა მთხოვა, უცნობი ნიმუში მოგცე. ამოიცნობ?", "ტესტები გამოიყენე და ზედმეტი არ გაასინჯო. ეს ფხვნილი ჭამისთვის არ არის!"),
                    new StoryBeat(15, "", "ოცი წელია ამ სკოლაში ვმუშაობ. ერთხელ ერთმა მოსწავლემ ჭერამდე ქაფი ააყენა.", "ახლა ის ქიმიის პროფესორია. ჰოდა, ავარიებს ნუ შეუშინდები."),
                    new StoryBeat(30, "", "შენი ლაბორატორია ყველაზე მოწესრიგებულია. ინსპექციაზე ამას მასწავლებელსაც ვეტყვი.", "აი, ცოტა დამატებითი მასალა. ჩვენ შორის დარჩეს."),
                    new StoryBeat(55, "mineral:fluorite", "ეს ფლუორიტი ჩემი პირველი სკოლის კოლექციიდანაა. ულტრაიისფერ შუქზე ანათებს.", "შენ ნამდვილი მეცნიერი ხდები. დარწმუნებული ვარ."),
                },
                Requests = new[]
                {
                    R("ketevan_clean", "stat", "clean", 2, 18, "ორი ავარია დაასუფთავე. ძველი ჩვევაა: დაღვარე, მოწმინდე, ისწავლე.", "მშვენიერია. ამ სიზუსტით შენი ლაბორატორია ბრწყინავს."),
                    R("ketevan_water", "stat", "water", 1, 15, "სათბურის მცენარეები მორწყე, თორემ ანას გული ეტკინება.", "მადლობა! ყვავილები უკვე უკეთესად გამოიყურება."),
                    R("ketevan_tap", "deliver", "vinegar", 1, 12, "ცოტა ძმარი დამიბრუნე, ქვაბის ნადები უნდა მოვაშორო.", "კალციუმის კარბონატი ძმარს ვერ გაუძლებს! მადლობა.", "fact:catalyst"),
                    R("ketevan_any", "stat", "activity:any", 2, 18, "ორი ცდა ჩაატარე შენს ლაბორატორიაში. ხელსაწყოები უნდა გამოიყენო, რომ არ მოიწყინონ.", "ასეა! აღჭურვილობაც ცოცხლდება, როცა მას იყენებენ."),
                }
            },
            new ClassmateDef
            {
                ID = "levani", Name = "პროფესორი ლევანი", Role = "მოგზაური მკვლევარი • ხუთშაბათს და პარასკევს", Voice = 0.7f, Visitor = true,
                Skin = new Color32(220, 170, 130, 255), Hair = new Color32(200, 200, 205, 255), Shirt = new Color32(110, 90, 60, 255),
                Schedule = new[] { ("exhibition", new Vector2(8, 1)), ("exhibition", new Vector2(8, 1)), ("exhibition", new Vector2(8, 1)) },
                Activities = new[] { "რუკას ათვალიერებს", "ნიმუშებს ალაგებს", "ჩანაწერებს აკეთებს" },
                SmallTalk = new[] { "ახლახან ბორჯომიდან დავბრუნდი. იქაური მინერალური წყალი ნამდვილი ქიმიური ხსნარია!", "ყოველი მოგზაურობა ახალი ნიმუშია.", "ცნობისმოყვარეობა მეცნიერის ყველაზე ძვირფასი ხელსაწყოა." },
                Story = new[]
                {
                    new StoryBeat(0, "", "ოჰო, ახალგაზრდა კოლეგა! პროფესორი ლევანი, მოგზაური მკვლევარი.", "ყოველ ხუთშაბათს და პარასკევს აქ ვარ და იშვიათ ნიმუშებს ვყიდი."),
                    new StoryBeat(10, "fact:helium", "ერთხელ სპექტრის ხაზების გამო უცნობ ელემენტს მივაგენი... ჰელიუმი აღმოჩნდა, მაგრამ 150 წლის დაგვიანებით!", "ხუმრობაა. მაგრამ ეს ფაქტი შენთვის."),
                    new StoryBeat(30, "", "შენზე ბევრს ლაპარაკობენ ამ სკოლაში. მომწონს შენი ნაშრომები.", "ერთ დღეს ჩემს ექსპედიციაში წაგიყვან."),
                },
                Requests = new RequestDef[0]
            },
        };

        public static ClassmateDef Get(string id) => All.FirstOrDefault(c => c.ID == id);
        private static GameSaveData Data => GameSaveManager.Instance.Data;
        public static NpcProgress State(string id)
        {
            var state = Data.npcs.FirstOrDefault(n => n.id == id);
            if (state == null) { state = new NpcProgress { id = id }; Data.npcs.Add(state); }
            return state;
        }
        public static readonly string[] FriendshipNames = { "უცნობი", "ნაცნობი", "მეგობარი", "ახლო მეგობარი", "საუკეთესო მეგობარი" };
        public static readonly int[] FriendshipSteps = { 0, 10, 30, 60, 90 };
        public static int Level(NpcProgress state) { int level = 0; for (int i = 0; i < FriendshipSteps.Length; i++) if (state.friendship >= FriendshipSteps[i]) level = i; return level; }

        public static bool StoryReady(ClassmateDef def, NpcProgress state) =>
            state.story < def.Story.Length && state.lastStoryDay != LabProgress.Today && state.friendship >= def.Story[state.story].Friendship;

        public static int Progress(RequestDef request, NpcProgress state)
        {
            switch (request.Kind)
            {
                case "deliver": return LabProgress.Has(request.Key);
                case "solved": return Data.totalQuizzesSolved - state.requestStart;
                case "stat": return LabProgress.Stat(request.Key) - state.requestStart;
                case "collect": return LabProgress.Owns(request.Key) ? 1 : 0;
                default: return 0;
            }
        }
        public static RequestDef Request(ClassmateDef def, NpcProgress state) => def.Requests.FirstOrDefault(r => r.ID == state.requestID);
        public static bool RequestDone(ClassmateDef def, NpcProgress state)
        {
            var request = Request(def, state);
            return request != null && Progress(request, state) >= request.Count;
        }

        // ---------------- conversation ----------------

        public static void Talk(LabGame game, SchoolNPC npc)
        {
            var def = npc.Def;
            var state = State(def.ID);
            var dialogue = game.Dialogue;
            string today = LabProgress.Today;
            string name = string.IsNullOrWhiteSpace(Data.playerName) ? "მეგობარო" : Data.playerName;
            bool firstToday = state.lastTalkDay != today;
            if (firstToday) { state.lastTalkDay = today; AddFriendship(state, 2); GameSaveManager.Instance.Commit(); }
            string greeting = SchoolCalendar.Part == DayPart.Morning ? "დილა მშვიდობისა" : SchoolCalendar.Part == DayPart.Afternoon ? "გამარჯობა" : "საღამო მშვიდობისა";
            dialogue.Converse(npc, def.Name + " • " + FriendshipNames[Level(state)], () =>
                dialogue.Line(() => Menu(game, npc), Level(state) >= 2 ? greeting + ", " + name + "! რა კარგია, რომ მოხვედი." : greeting + "! " + Activity(def) + "."));
        }

        private static string Activity(ClassmateDef def) => "ახლა " + def.Activities[(SchoolCalendar.Now.Hour + DateTime.Now.Minute / 20) % def.Activities.Length];

        private static void Menu(LabGame game, SchoolNPC npc)
        {
            var def = npc.Def;
            var state = State(def.ID);
            var dialogue = game.Dialogue;
            var options = new List<(string, Action)>();
            options.Add((StoryReady(def, state) ? "როგორ ხარ? (ახალი ამბავი)" : "როგორ ხარ?", () => Chat(game, npc)));
            if (def.Requests.Length > 0)
                options.Add((RequestDone(def, state) ? "თხოვნის შესრულება!" : state.requestID != "" ? "შენი თხოვნა" : "რამე ხომ არ გჭირდება?", () => Requests(game, npc)));
            if (def.ID == "ketevan") options.Add(("ნიმუში მჭირდება", () => Sample(game, npc)));
            if (def.ID == "levani") options.Add(("რას ყიდით?", () => Shop(game, npc)));
            options.Add(("ნახვამდის", () => dialogue.Line(dialogue.End, "კარგი, მერე ვილაპარაკოთ!")));
            dialogue.Offer("რაზე ვილაპარაკოთ?", options.ToArray());
        }

        private static void Chat(LabGame game, SchoolNPC npc)
        {
            var def = npc.Def; var state = State(def.ID); var dialogue = game.Dialogue;
            if (StoryReady(def, state))
            {
                var beat = def.Story[state.story];
                state.story++; state.lastStoryDay = LabProgress.Today;
                AddFriendship(state, 4);
                string gift = "";
                if (!string.IsNullOrEmpty(beat.Gift))
                {
                    if (beat.Gift == "sample") { LabProgress.Give("sample", 1); gift = "მიიღე: უცნობი ნიმუში"; }
                    else if (LabProgress.Collect(beat.Gift)) gift = "საჩუქარი: " + LabProgress.Catalog.FirstOrDefault(c => c.ID == beat.Gift)?.Name;
                }
                GameSaveManager.Instance.Commit();
                var lines = beat.Lines.ToList();
                if (state.story < def.Story.Length) lines.Add("(" + def.Name + "ს ამბავი გაგრძელდება სხვა დღეს.)");
                dialogue.Line(() => Menu(game, npc), lines.ToArray());
                if (gift.Length > 0) game.HUD.Notify(gift);
                return;
            }
            string line = def.SmallTalk[UnityEngine.Random.Range(0, def.SmallTalk.Length)];
            string later = state.story < def.Story.Length ? "" : "";
            if (state.story < def.Story.Length && state.lastStoryDay == LabProgress.Today) later = "ხვალ მოდი, კიდევ რაღაცას მოგიყვები.";
            else if (state.story < def.Story.Length) later = "როცა უკეთ გავიცნობთ ერთმანეთს, მეტს მოგიყვები.";
            dialogue.Line(() => Menu(game, npc), later.Length > 0 ? new[] { line, later } : new[] { line });
        }

        private static void Requests(LabGame game, SchoolNPC npc)
        {
            var def = npc.Def; var state = State(def.ID); var dialogue = game.Dialogue;
            var request = Request(def, state);
            if (request != null)
            {
                if (RequestDone(def, state))
                {
                    if (request.Kind == "deliver") LabProgress.Take(request.Key, request.Count);
                    Data.chemCoins += request.Coins;
                    AddFriendship(state, request.Friendship);
                    LabProgress.AddReputation(request.Reputation);
                    state.requestsDone++; state.requestID = ""; state.lastRequestDay = LabProgress.Today;
                    LabProgress.CountStat("requests");
                    if (LabProgress.Stat("requests") >= 5) LabProgress.Collect("badge:helper");
                    if (!string.IsNullOrEmpty(request.Gift)) LabProgress.Collect(request.Gift);
                    GameSaveManager.Instance.Commit();
                    AudioManager.Instance?.Coin();
                    dialogue.Line(() => Menu(game, npc), request.Thanks, "+" + request.Coins + " ქიმქოინი • მეგობრობა გაიზარდა.");
                    return;
                }
                dialogue.Line(() => Menu(game, npc), request.Ask, "ჯერჯერობით: " + Mathf.Min(Progress(request, state), request.Count) + " / " + request.Count + ".");
                return;
            }
            if (state.lastRequestDay == LabProgress.Today)
            { dialogue.Line(() => Menu(game, npc), "დღეს უკვე ბევრი დამეხმარე. ხვალ ახალი თხოვნა მექნება!"); return; }
            var next = def.Requests[state.requestsDone % def.Requests.Length];
            dialogue.Line(null, next.Ask);
            dialogue.Offer("დამეხმარები? (+" + next.Coins + " ქიმქოინი)",
                ("რა თქმა უნდა!", () =>
                {
                    state.requestID = next.ID;
                    state.requestStart = next.Kind == "solved" ? Data.totalQuizzesSolved : next.Kind == "stat" ? LabProgress.Stat(next.Key) : 0;
                    GameSaveManager.Instance.Commit();
                    game.HUD.Notify(def.Name + "ს თხოვნა: " + next.Ask);
                    dialogue.Line(() => Menu(game, npc), "მადლობა! როცა მზად იქნები, მომაკითხე.");
                }),
                ("მოგვიანებით", () => Menu(game, npc)));
        }

        private static void Sample(LabGame game, SchoolNPC npc)
        {
            var dialogue = game.Dialogue;
            string key = "ketevan_sample:" + LabProgress.Today;
            if (Data.claimedEvents.Contains(key)) { dialogue.Line(() => Menu(game, npc), "დღეს უკვე მოგეცი ნიმუში. ხვალ ახალს მოგიმზადებ."); return; }
            Data.claimedEvents.Add(key);
            LabProgress.Give("sample", 1);
            GameSaveManager.Instance.Commit();
            dialogue.Line(() => Menu(game, npc), "აი, ახალი უცნობი ნიმუში. წარწერა არ აქვს, ამიტომ ფრთხილად გამოიკვლიე!");
        }

        public static readonly (string id, string name, int price)[] Wares =
        {
            ("mineral:fluorite", "ფლუორიტი", 30), ("mineral:malachite", "მალაქიტი", 35), ("mineral:pyrite", "პირიტი", 30),
            ("mineral:amethyst", "ამეთვისტო", 40), ("decor:mineral_case", "მინერალების ყუთი (დეკორაცია)", 80), ("cosmetic:hat_researcher", "მკვლევრის ქუდი", 60),
        };

        private static void Shop(LabGame game, SchoolNPC npc)
        {
            var dialogue = game.Dialogue;
            var options = Wares.Where(w => !Owned(w.id)).Take(5).Select(w => (w.name + " • " + w.price, (Action)(() => Buy(game, npc, w.id, w.name, w.price)))).ToList();
            options.Add(("არაფერი, მადლობა", () => Menu(game, npc)));
            dialogue.Offer(options.Count == 1 ? "ყველაფერი უკვე შენია! შემდეგ კვირას ახალს მოვიტან." : "ჩემი ექსპედიციების ნადავლი. რა მოგწონს?", options.ToArray());
        }
        private static bool Owned(string id) =>
            id.StartsWith("decor:") ? Data.ownedLabItems.Contains(id.Substring(6)) : id.StartsWith("cosmetic:") ? Data.unlockedCosmeticIDs.Contains(id.Substring(9)) : LabProgress.Owns(id);
        private static void Buy(LabGame game, SchoolNPC npc, string id, string name, int price)
        {
            var dialogue = game.Dialogue;
            if (Data.chemCoins < price) { dialogue.Line(() => Shop(game, npc), "ჰმ, ქიმქოინები არ გყოფნის. ნუ ღელავ, მოგიცდი."); return; }
            Data.chemCoins -= price;
            if (id.StartsWith("decor:")) Data.ownedLabItems.Add(id.Substring(6));
            else if (id.StartsWith("cosmetic:")) Data.unlockedCosmeticIDs.Add(id.Substring(9));
            else LabProgress.Collect(id);
            GameSaveManager.Instance.Commit();
            AudioManager.Instance?.Coin();
            dialogue.Line(() => Shop(game, npc), "შესანიშნავი არჩევანია! " + name + " შენია.");
        }

        private static void AddFriendship(NpcProgress state, int amount)
        {
            state.friendship = Mathf.Min(100, state.friendship + amount);
            if (state.friendship >= 90) LabProgress.Collect("badge:friend");
        }
    }

    /// <summary>A classmate walking the school on a daily schedule.</summary>
    public sealed class SchoolNPC : MonoBehaviour, ISpeaker
    {
        public ClassmateDef Def { get; private set; }
        public bool Talking { get; set; }
        public float Voice => Def.Voice;
        private LabGame game;
        private Animator animator;
        private SpriteRenderer sprite;
        private TMP_Text marker;
        private GameObject nameTag;
        private Vector2 anchor, target;
        private string area = "";
        private float wait, hop, stride;
        private static readonly int MoveX = Animator.StringToHash("MoveX"), MoveY = Animator.StringToHash("MoveY"), IsMoving = Animator.StringToHash("IsMoving");

        public static SchoolNPC Spawn(LabGame lab, ClassmateDef def)
        {
            var root = new GameObject(def.Name, typeof(Rigidbody2D));
            var body = root.GetComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic; body.simulated = true;
            var visual = new GameObject("Sprite", typeof(SpriteRenderer));
            visual.transform.SetParent(root.transform, false);
            var playerSprite = lab.player.transform.Find("Sprite").GetComponent<SpriteRenderer>();
            var renderer = visual.GetComponent<SpriteRenderer>();
            renderer.sprite = playerSprite.sprite;
            var material = new Material(playerSprite.sharedMaterial);
            material.SetColor("_SkinColor", def.Skin); material.SetColor("_HairColor", def.Hair); material.SetColor("_ShirtColor", def.Shirt);
            material.SetFloat("_HasCoat", def.ID == "ketevan" || def.ID == "levani" ? 1 : 0);
            material.SetColor("_CoatColor", def.ID == "levani" ? new Color32(170, 140, 100, 255) : Color.white);
            renderer.sharedMaterial = material;
            var group = root.AddComponent<UnityEngine.Rendering.SortingGroup>(); group.sortingOrder = 7;
            var animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = lab.player.GetComponent<Animator>().runtimeAnimatorController;
            var shadow = LaboratoryArt.Draw(root.transform, "ჩრდილი", new Vector2(0, 0.05f), PixelArt.Make(18, 6, (x, y) =>
                Mathf.Pow((x - 8.5f) / 8f, 2) + Mathf.Pow((y - 2.5f) / 2.5f, 2) < 1 ? new Color(0.07f, 0.06f, 0.13f, 0.2f) : Color.clear), -1);

            var label = new GameObject("სახელი", typeof(RectTransform), typeof(Canvas));
            label.transform.SetParent(root.transform, false);
            label.transform.localPosition = new Vector3(0, 2.45f);
            label.transform.localScale = Vector3.one / 70f;
            label.GetComponent<RectTransform>().sizeDelta = new Vector2(420, 70);
            label.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            label.GetComponent<Canvas>().sortingOrder = 11;
            var text = LabUI.Text(label.transform, def.Name, 19, Color.white);
            text.alignment = TextAlignmentOptions.Center; text.textWrappingMode = TextWrappingModes.NoWrap;
            LabUI.WorldLabel(text); LabUI.Fill(text.rectTransform);

            var markerCanvas = new GameObject("ნიშანი", typeof(RectTransform), typeof(Canvas));
            markerCanvas.transform.SetParent(root.transform, false);
            markerCanvas.transform.localPosition = new Vector3(0, 3.0f);
            markerCanvas.transform.localScale = Vector3.one / 60f;
            markerCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 50);
            markerCanvas.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            markerCanvas.GetComponent<Canvas>().sortingOrder = 11;
            var marker = LabUI.Text(markerCanvas.transform, "", 34, LabUI.Gold);
            marker.alignment = TextAlignmentOptions.Center; LabUI.WorldLabel(marker, LabUI.Gold); LabUI.Fill(marker.rectTransform);

            var talk = new GameObject("საუბარი", typeof(BoxCollider2D), typeof(InteractableStation));
            talk.transform.SetParent(root.transform, false);
            var zone = talk.GetComponent<BoxCollider2D>(); zone.size = new Vector2(2.4f, 2.4f); zone.offset = new Vector2(0, 0.4f);
            var station = talk.GetComponent<InteractableStation>();
            station.stationType = StationType.Classmate;
            station.stationPrompt = "[E] " + def.Name + "სთან საუბარი";
            station.promptHeight = 3.6f;
            station.Initialize(lab);

            var npc = root.AddComponent<SchoolNPC>();
            npc.game = lab; npc.Def = def; npc.animator = animator; npc.sprite = renderer; npc.marker = marker; npc.nameTag = label;
            npc.Reposition(true);
            return npc;
        }

        public void Pulse() => hop = 1;

        private (string area, Vector2 point) Slot()
        {
            if (Def.Visitor && !SchoolCalendar.ResearcherVisiting) return ("", Vector2.zero);
            return Def.Schedule[(int)SchoolCalendar.Part];
        }

        private void Reposition(bool force)
        {
            var slot = Slot();
            if (!force && slot.area == area && (anchor - slot.point).sqrMagnitude < 0.01f) return;
            bool sameArea = slot.area == area && !force;
            area = slot.area;
            bool present = area != "" && (area == "classroom" || SchoolAreas.Def(area) != null);
            foreach (Transform child in transform) child.gameObject.SetActive(present);
            if (!present) return;
            anchor = slot.point;
            target = anchor;
            if (!sameArea) transform.position = game.Areas.World(area, anchor);
        }

        private void Update()
        {
            if (game == null || game.Areas == null) return;
            Reposition(false);
            if (area == "") return;
            var state = Classmates.State(Def.ID);
            string symbol = Classmates.RequestDone(Def, state) ? "?" : Classmates.StoryReady(Def, state) ? "!" : "";
            if (marker.text != symbol) marker.text = symbol;
            marker.alpha = Talking ? 0 : 1;
            if (nameTag.activeSelf == Talking) nameTag.SetActive(!Talking);
            marker.rectTransform.localPosition = new Vector3(0, Mathf.Abs(Mathf.Sin(Time.time * 2.6f)) * 9f, 0);

            hop = Mathf.MoveTowards(hop, 0, Time.unscaledDeltaTime * 9);
            sprite.transform.localPosition = new Vector3(0, Mathf.Sin(hop * Mathf.PI) * 0.08f, 0);

            Vector2 world = transform.position;
            Vector2 goal = game.Areas.World(area, target);
            if (Talking)
            {
                var toPlayer = (Vector2)game.player.transform.position - world;
                animator.SetFloat(MoveX, toPlayer.normalized.x); animator.SetFloat(MoveY, toPlayer.normalized.y);
                animator.SetBool(IsMoving, false);
                return;
            }
            var delta = goal - world;
            if (delta.magnitude > 0.05f)
            {
                var step = delta.normalized * Mathf.Min(delta.magnitude, 1.5f * Time.deltaTime);
                transform.position = world + step;
                animator.SetFloat(MoveX, delta.normalized.x); animator.SetFloat(MoveY, delta.normalized.y);
                animator.SetBool(IsMoving, true);
                stride += step.magnitude;
                if (stride > 1.1f)
                {
                    stride = 0;
                    if (Vector2.Distance(world, game.player.transform.position) < 6) AudioManager.Instance?.Step();
                }
                return;
            }
            animator.SetBool(IsMoving, false);
            wait -= Time.deltaTime;
            if (wait > 0) return;
            wait = UnityEngine.Random.Range(3f, 7f);
            var candidate = anchor + UnityEngine.Random.insideUnitCircle * 1.6f;
            var candidateWorld = game.Areas.World(area, candidate);
            bool blocked = Physics2D.OverlapBoxAll(candidateWorld + new Vector2(0, 0.1f), new Vector2(0.8f, 0.5f), 0).Any(c => !c.isTrigger);
            if (!blocked) target = candidate;
        }

        public void Talk() => Classmates.Talk(game, this);
    }
}
