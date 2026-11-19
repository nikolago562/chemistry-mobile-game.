using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChemistryGame
{
    public sealed class StudyQuestion
    {
        public string ID, Prompt, Hint, Explanation;
        public string[] Choices, Feedback, Elements = Array.Empty<string>();
        public int Correct, Reward = 10;
    }

    public static class ChemistryChallenges
    {
        private static StudyQuestion Q(string id, string prompt, string hint, string explanation, string[] choices, string[] wrong)
        {
            return new StudyQuestion { ID = id, Prompt = prompt, Hint = hint, Explanation = explanation,
                Choices = choices, Feedback = new[] { explanation, wrong[0], wrong[1], wrong[2] } };
        }
        public static readonly StudyQuestion[] General = {
            Q("charge", "რომელ ნაწილაკს აქვს უარყოფითი მუხტი?", "მოძებნე ნაწილაკი, რომელიც ბირთვის გარეთაა.", "ელექტრონი უარყოფითადაა დამუხტული; პროტონი დადებითია, ნეიტრონი კი ნეიტრალური.",
                new[]{"ელექტრონი", "პროტონი", "ნეიტრონი", "ატომის ბირთვი"}, new[]{"პროტონს დადებითი მუხტი აქვს.", "ნეიტრონი ელექტრულად ნეიტრალურია.", "ბირთვის მუხტი დადებითია, რადგან მასში პროტონებია."}),
            Q("number", "რას გვიჩვენებს ატომური ნომერი?", "ელემენტის ვინაობას ბირთვი განსაზღვრავს.", "ატომური ნომერი პროტონების რაოდენობაა. მისი შეცვლა სხვა ელემენტს ქმნის.",
                new[]{"პროტონების რაოდენობას", "ნეიტრონების რაოდენობას", "პროტონებისა და ნეიტრონების ჯამს", "მოლეკულების რაოდენობას"}, new[]{"ნეიტრონების რაოდენობა ერთი ელემენტის იზოტოპებში განსხვავდება.", "ეს მასური რიცხვია და არა ატომური ნომერი.", "მოლეკულების რაოდენობა ატომის ელემენტს არ განსაზღვრავს."}),
            Q("isotope", "რით განსხვავდება ერთი ელემენტის იზოტოპები?", "პროტონების რაოდენობა იგივე რჩება.", "იზოტოპებს პროტონების ერთნაირი, ნეიტრონების კი განსხვავებული რაოდენობა აქვთ.",
                new[]{"ნეიტრონების რაოდენობით", "პროტონების რაოდენობით", "ყოველთვის ელექტრონების რაოდენობით", "ქიმიური სიმბოლოთი"}, new[]{"პროტონების შეცვლა სხვა ელემენტს ნიშნავს.", "ელექტრონების შეცვლით იონი მიიღება; ეს იზოტოპის განმარტება არაა.", "ერთი ელემენტის იზოტოპებს ერთი ქიმიური სიმბოლო აქვთ."}),
            Q("bond", "რა ხდება კოვალენტური ბმის წარმოქმნისას?", "წარმოიდგინე ელექტრონების საერთო წყვილი.", "კოვალენტური ბმა ატომებს შორის ელექტრონული წყვილების გაზიარებით წარმოიქმნება.",
                new[]{"ატომები ელექტრონებს იზიარებენ", "ბირთვები ერთდება", "პროტონები გადადის", "ნეიტრონები ქრება"}, new[]{"ბირთვების შეერთება ბირთვული პროცესია და არა ჩვეულებრივი ქიმიური ბმა.", "ქიმიურ ბმაში ბირთვის პროტონები სხვა ატომში არ გადადის.", "ქიმიური ბმის წარმოქმნისას ნეიტრონები არ ქრება."}),
            Q("acid", "რომელი pH შეესაბამება მჟავე წყალხსნარს 25°C-ზე?", "ნეიტრალური წყლის pH დაახლოებით 7-ია.", "მჟავე წყალხსნარში pH 7-ზე ნაკლებია; ტუტე ხსნარში — მეტი.",
                new[]{"pH = 3", "pH = 7", "pH = 10", "pH = 12"}, new[]{"25°C-ზე pH 7 ნეიტრალურ წყალხსნარს შეესაბამება.", "pH 10 ტუტე ხსნარს შეესაბამება.", "pH 12 ძლიერ ტუტე ხსნარს შეესაბამება."}),
            Q("dilution", "რა ხდება, თუ ხსნარს წყალს ვუმატებთ?", "გახსნილი ნივთიერების რაოდენობა არ იცვლება.", "წყალი ზრდის მოცულობას. იგივე რაოდენობის ნივთიერება მეტ მოცულობაში ნაწილდება და კონცენტრაცია მცირდება.",
                new[]{"კონცენტრაცია მცირდება", "კონცენტრაცია იზრდება", "ნივთიერების მოლები იზრდება", "ატომები ქრება"}, new[]{"გამხსნელის დამატებისას ნივთიერების იგივე რაოდენობა მეტ მოცულობაში ნაწილდება.", "სუფთა წყლის დამატებით გახსნილი ნივთიერების მოლები არ ემატება.", "განზავება ატომებს არ ანადგურებს."}),
            Q("catalyst", "რას აკეთებს კატალიზატორი?", "ის რეაქციის უფრო მარტივ გზას ქმნის.", "კატალიზატორი ამცირებს აქტივაციის ენერგიას და აჩქარებს რეაქციას; მთლიან პროცესში არ იხარჯება.",
                new[]{"ამცირებს აქტივაციის ენერგიას", "ქმნის ენერგიას არაფრისგან", "ყოველთვის ზრდის საბოლოო გამოსავალს წონასწორობაზე", "სრულად იხარჯება"}, new[]{"კატალიზატორი ენერგიის შენახვის კანონს არ არღვევს.", "კატალიზატორი წონასწორობის მდებარეობას არ ცვლის; მის მიღწევას აჩქარებს.", "საერთო რეაქციის ბოლოს კატალიზატორი აღდგება."}),
            Q("diffusion", "რომელი მიმართულებით მიემართება დიფუზიის წმინდა ნაკადი?", "შეადარე კონცენტრაცია მემბრანის ორივე მხარეს.", "შემთხვევითი მოძრაობა ორივე მიმართულებით ხდება, მაგრამ წმინდა ნაკადი მაღალი კონცენტრაციის არიდან დაბალი კონცენტრაციის არეშიაა.",
                new[]{"მაღალი კონცენტრაციის არიდან დაბალი კონცენტრაციის არეში", "ყოველთვის დაბლიდან მაღლისკენ", "მხოლოდ ქვემოთ", "მხოლოდ ATP-ის ხარჯვით"}, new[]{"გრადიენტის წინააღმდეგ ტრანსპორტს ხშირად ენერგია სჭირდება.", "დიფუზია მხოლოდ სიმძიმის მიმართულებით არ ხდება.", "პასიურ დიფუზიას ATP-ის ხარჯვა არ სჭირდება."}),
            Q("osmosis", "რა არის ოსმოსი?", "აქ მოძრავი ნივთიერება წყალია.", "ოსმოსი წყლის წმინდა გადაადგილებაა შერჩევითად გამტარ მემბრანაში წყლის ქიმიური პოტენციალის სხვაობის გამო.",
                new[]{"წყლის გადაადგილებას შერჩევით მემბრანაში", "ყველა იონის აქტიურ გადატანას", "წყლის ქიმიურ დაშლას", "ხსნარის დუღილს"}, new[]{"იონების აქტიური გადატანა სხვა პროცესია და შეიძლება ATP-ს საჭიროებდეს.", "ოსმოსის დროს წყლის მოლეკულა არ იშლება.", "დუღილი ფაზური გარდაქმნაა და მემბრანას არ მოითხოვს."}),
            Q("gas", "როგორ შეიცვლება წნევა, თუ უცვლელ ტემპერატურაზე აირის მოცულობა განახევრდება?", "იგივე ნაწილაკებს ნაკლები სივრცე აქვთ.", "იდეალური აირისთვის მუდმივ ტემპერატურასა და რაოდენობაზე P·V მუდმივია: მოცულობის განახევრება წნევას აორმაგებს.",
                new[]{"დაახლოებით ორმაგდება", "ნახევრდება", "უცვლელია", "ნულდება"}, new[]{"შეკუმშვისას კედელზე შეჯახებები უფრო ხშირია, ამიტომ წნევა იზრდება.", "მოცულობის ცვლილება ფიქსირებული T და n პირობებში წნევას ცვლის.", "დახურულ ჭურჭელში ნაწილაკები და მათი შეჯახებები არ ქრება."}),
            Q("chromatography", "რატომ იყოფა ნარევი ქრომატოგრაფიაში?", "პიგმენტებს ქაღალდი და გამხსნელი სხვადასხვაგვარად იზიდავს.", "კომპონენტები მოძრავ და უძრავ ფაზებს განსხვავებულად უკავშირდება, ამიტომ სხვადასხვა მანძილს გადის.",
                new[]{"კომპონენტებს ფაზებთან განსხვავებული მიდრეკილება აქვთ", "ყველა პიგმენტი ერთი სიჩქარით მოძრაობს", "ქაღალდი ახალ ელემენტებს ქმნის", "ყველა ნარევი აუცილებლად რეაგირებს"}, new[]{"თუ ყველა პიგმენტი ერთნაირი სიჩქარით იმოძრავებდა, ზოლები ერთმანეთისგან ვერ განცალკევდებოდა.", "ქაღალდი ელემენტებს არ ქმნის; ეს განცალკევების მეთოდია.", "განცალკევებისთვის ქიმიური რეაქცია აუცილებელი არაა."}),
            Q("conservation", "რა უნდა ემთხვეოდეს დაბალანსებული განტოლების ორივე მხარეს?", "დათვალე თითოეული ელემენტი ცალ-ცალკე.", "ჩვეულებრივ ქიმიურ რეაქციაში თითოეული ელემენტის ატომების რაოდენობა ინახება.",
                new[]{"თითოეული ელემენტის ატომების რაოდენობა", "ყოველთვის მოლეკულების რაოდენობა", "ყოველთვის ნივთიერებების რაოდენობა", "ყოველთვის აირის მოცულობა"}, new[]{"რამდენიმე მოლეკულა შეიძლება ერთ ან სხვა რაოდენობის მოლეკულად გარდაიქმნას.", "რეაგენტებისა და პროდუქტების სახეობების რაოდენობა შეიძლება განსხვავდებოდეს.", "აირის მოცულობა პირობებსა და მოლების რაოდენობაზეა დამოკიდებული."}),
            Q("ion", "ნეიტრალურმა ატომმა ერთი ელექტრონი დაკარგა. რა მუხტი ექნება მას?", "ელექტრონს უარყოფითი მუხტი აქვს.", "ერთი უარყოფითი ელექტრონის დაკარგვის შემდეგ ატომს ერთი ზედმეტი დადებითი მუხტი რჩება: +1.",
                new[]{"+1", "−1", "0", "+2"}, new[]{"უარყოფითი მუხტი ელექტრონის მიღებისას წარმოიქმნებოდა.", "დაკარგვის შემდეგ პროტონებისა და ელექტრონების რაოდენობა აღარ ემთხვევა.", "ორი დადებითი მუხტისთვის ორი ელექტრონი უნდა დაეკარგა."}),
            Q("mixture", "რომელია ნარევი?", "ნარევს ცვლადი შემადგენლობა შეიძლება ჰქონდეს.", "ჰაერი სხვადასხვა აირის ნარევია, მათ შორის აზოტისა და ჟანგბადის.",
                new[]{"ჰაერი", "სუფთა ჟანგბადი O2", "სუფთა წყალი H2O", "სუფთა ოქრო Au"}, new[]{"სუფთა O2 ერთი ნივთიერებაა.", "სუფთა H2O ერთი ქიმიური ნაერთია.", "სუფთა ოქრო ერთი ქიმიური ელემენტია."}),
            Q("heat", "რას ნიშნავს ეგზოთერმული რეაქცია?", "შეხედე სითბოს გადაცემას გარემოსთან.", "ეგზოთერმული რეაქცია გარემოში სითბოს გამოყოფს; სისტემის ენთალპიის ცვლილება უარყოფითია.",
                new[]{"რეაქცია გარემოში სითბოს გამოყოფს", "რეაქცია აუცილებლად შთანთქავს სითბოს", "რეაქცია არასდროს იწყება", "მასა ენერგიის გარეშე იქმნება"}, new[]{"სითბოს შთანთქმა ენდოთერმულ პროცესს ახასიათებს.", "ეგზოთერმულ რეაქციასაც შეიძლება საწყისი აქტივაციის ენერგია სჭირდებოდეს.", "ეს ენერგიისა და მასის შენახვას არ შეესაბამება."}),
            Q("mole", "რისი საზომი ერთეულია მოლი?", "ეს ნივთიერების რაოდენობის SI ერთეულია.", "მოლი ნივთიერების რაოდენობის ერთეულია: 1 მოლი ზუსტად 6.02214076 × 10^23 განსაზღვრულ ნაწილაკს შეიცავს.",
                new[]{"ნივთიერების რაოდენობას", "მხოლოდ მასას გრამებში", "ტემპერატურას", "ელექტრულ ძაბვას"}, new[]{"ერთი მოლის მასა ნივთიერების მოლურ მასაზეა დამოკიდებული.", "თერმოდინამიკური ტემპერატურის SI ერთეული კელვინია.", "ძაბვა ვოლტებში იზომება."})
        };

        public static StudyQuestion Equation(ChemistryQuestionSO source)
        {
            var match = Regex.Match(source.correctAnswer, @"^(\d*)(.*)$");
            int coefficient = match.Groups[1].Length == 0 ? 1 : int.Parse(match.Groups[1].Value);
            string formula = match.Groups[2].Value;
            var variants = new List<string> { source.correctAnswer };
            foreach (int n in new[] { 1, 2, 3, 4, 5, 6, coefficient + 1, coefficient + 2 })
            {
                string value = (n == 1 ? "" : n.ToString()) + formula;
                if (!variants.Contains(value)) variants.Add(value);
                if (variants.Count == 4) break;
            }
            return new StudyQuestion { ID = source.questionID, Prompt = source.equationPrompt, Hint = source.hintText,
                Explanation = source.hintText + "\n" + source.equationPrompt.Replace("?", source.correctAnswer),
                Choices = variants.ToArray(), Correct = 0, Reward = source.coinReward,
                Elements = source.involvedElements ?? Array.Empty<string>(),
                Feedback = variants.Select(v => v == source.correctAnswer ? source.hintText : ExplainMismatch(source.equationPrompt, v)).ToArray() };
        }
        public static Dictionary<string, int> Atoms(string side)
        {
            var result = new Dictionary<string, int>();
            foreach (string raw in side.Split('+'))
            {
                string term = raw.Replace(" ", "");
                if (term.Length == 0) continue;
                int index = 0; int coefficient = Number(term, ref index);
                var atoms = Group(term, ref index);
                foreach (var pair in atoms) result[pair.Key] = result.GetValueOrDefault(pair.Key) + pair.Value * coefficient;
            }
            return result;
        }
        private static int Number(string s, ref int i)
        { int n = 0; bool found = false; while(i < s.Length && char.IsDigit(s[i])) { found = true; n = n * 10 + s[i++] - '0'; } return found ? n : 1; }
        private static Dictionary<string, int> Group(string s, ref int i)
        {
            var result = new Dictionary<string, int>();
            while(i < s.Length && s[i] != ')')
            {
                if(s[i] == '(') { i++; var group = Group(s, ref i); if(i < s.Length) i++; int n = Number(s, ref i); foreach(var p in group) result[p.Key] = result.GetValueOrDefault(p.Key) + p.Value * n; }
                else if(char.IsUpper(s[i])) { string symbol = s[i++].ToString(); if(i < s.Length && char.IsLower(s[i])) symbol += s[i++]; int n = Number(s, ref i); result[symbol] = result.GetValueOrDefault(symbol) + n; }
                else throw new ArgumentException("Unsupported formula: " + s);
            }
            return result;
        }
        // ------------------------------------------------------------------ advanced floor tiers

        private static readonly string[] Formulas = { "H2O", "CO2", "NH3", "CH4", "O2", "NaCl", "H2SO4", "CaCO3", "C6H12O6", "Ca(OH)2", "Al2O3", "HCl", "NaOH", "MgCl2", "C2H6", "N2" };
        public static readonly int[] TierRewards = { 0, 8, 10, 14, 16 };
        private static T Pick<T>(IList<T> list) => list[UnityEngine.Random.Range(0, list.Count)];

        /// <summary>A four-choice question with the right answer first; duplicates of the answer are dropped.</summary>
        private static StudyQuestion Make(string id, string prompt, string hint, string right, string why, int reward, params (string choice, string why)[] wrong)
        {
            var distinct = wrong.Where(w => w.choice != right).GroupBy(w => w.choice).Select(g => g.First()).Take(3).ToList();
            if (distinct.Count < 3) return null;
            return new StudyQuestion { ID = id, Prompt = prompt, Hint = hint, Explanation = why, Reward = reward,
                Choices = new[] { right }.Concat(distinct.Select(w => w.choice)).ToArray(),
                Feedback = new[] { why }.Concat(distinct.Select(w => w.why)).ToArray() };
        }

        private static string Show(string equation) => equation.Replace("->", "→");
        private static string FullEquation(ChemistryQuestionSO q) => q.equationPrompt.Replace("?", q.correctAnswer);
        private static (int coefficient, string formula)[] Terms(string side) => side.Split('+').Select(t => t.Trim()).Where(t => t.Length > 0)
            .Select(t => { var m = Regex.Match(t, @"^(\d*)(.+)$"); return (m.Groups[1].Length == 0 ? 1 : int.Parse(m.Groups[1].Value), m.Groups[2].Value); }).ToArray();
        private static string Compose(Dictionary<string, int> atoms) => string.Concat(atoms.Select(p => p.Key + (p.Value > 1 ? p.Value.ToString() : "")));
        private static string Describe(Dictionary<string, int> atoms) => string.Join(", ", atoms.Select(p => p.Value + " " + p.Key));

        /// <summary>One question for a tier of the advanced floor (4 = mixed exam). Prerequisites rise with the tier.</summary>
        public static StudyQuestion Tier(int tier, LabGame game)
        {
            for (int attempt = 0; attempt < 20; attempt++)
            {
                int level = tier >= 4 ? UnityEngine.Random.Range(1, 4) : tier;
                var question = Generate(level, game);
                if (question == null) continue;
                question.Reward = TierRewards[Math.Max(1, Math.Min(4, tier))];
                if (tier >= 4) question.ID = "exam_" + question.ID;
                return question;
            }
            return General[0];
        }

        private static StudyQuestion Generate(int level, LabGame game)
        {
            int kind = UnityEngine.Random.Range(0, 4);
            var equations = game.questions.Where(q => q != null && !q.correctAnswer.Contains("(")).ToList();
            switch (level * 10 + kind)
            {
                case 10: // element identification
                {
                    var pool = game.elements.elements.Where(e => e.atomicNumber <= 20).ToList();
                    var e = Pick(pool);
                    var others = pool.Where(o => o != e).OrderBy(_ => UnityEngine.Random.value).Take(3);
                    return Make("t1_symbol_" + e.symbol, "რომელია ელემენტის — " + e.georgianName + " — ქიმიური სიმბოლო?", "სიმბოლოს პირველი ასო დიდია. ატომური ნომერი: " + e.atomicNumber + ".",
                        e.symbol, e.georgianName + " — " + e.symbol + ", ატომური ნომერი " + e.atomicNumber + ".", 0,
                        others.Select(o => (o.symbol, o.symbol + " სხვა ელემენტის სიმბოლოა: " + o.georgianName + ".")).ToArray());
                }
                case 11: // total atom count
                {
                    string f = Pick(Formulas); var atoms = Atoms(f); int total = atoms.Values.Sum();
                    string count = string.Join(" + ", atoms.Select(p => p.Key + " " + p.Value)) + " = " + total;
                    return Make("t1_count_" + f, "სულ რამდენი ატომია ფორმულაში " + f + "?", "დაითვალე თითოეული ელემენტის ატომები და შეკრიბე. ინდექსის გარეშე — 1.", total.ToString(), f + ": " + count + ".", 0,
                        ((total + 1).ToString(), "ერთით მეტია. " + count + "."), ((total - 1).ToString(), "ერთი ატომი გამოგრჩა. " + count + "."),
                        (atoms.Count.ToString(), "ეს ელემენტების რაოდენობაა და არა ატომების."), ((total + 2).ToString(), count + "."));
                }
                case 12: // formula reading
                {
                    string f = Pick(Formulas.Where(x => Atoms(x).Count > 1).ToList()); var atoms = Atoms(f);
                    string symbol = Pick(atoms.Keys.ToList()); int n = atoms[symbol];
                    return Make("t1_read_" + f + symbol, "რამდენი " + symbol + " ატომია ფორმულაში " + f + "?", "ინდექსი მის წინ მდგარ სიმბოლოს ეკუთვნის; ფრჩხილის შემდეგი ინდექსი მთელ ჯგუფს ამრავლებს.", n.ToString(),
                        f + "-ში " + symbol + "-ის ატომების რაოდენობაა " + n + ".", 0,
                        ((n + 1).ToString(), "ერთით მეტია."), ((n + 2).ToString(), "ორით მეტია."), (atoms.Values.Sum().ToString(), "ეს ყველა ატომის ჯამია."), (Math.Max(0, n - 1).ToString(), "ერთით ნაკლებია."));
                }
                case 13: // reactants and products
                {
                    var q = Pick(equations); var sides = FullEquation(q).Split(new[] { "->" }, StringSplitOptions.None);
                    var left = Terms(sides[0]).Select(t => t.formula).ToList(); var right = Terms(sides[1]).Select(t => t.formula).ToList();
                    string product = Pick(right);
                    var wrong = left.Select(r => (r, r + " ისრის მარცხნივაა — ის რეაგენტია.")).Concat(Formulas.Where(x => !left.Contains(x) && !right.Contains(x)).Take(3).Select(x => (x, x + " ამ რეაქციაში საერთოდ არ მონაწილეობს."))).ToArray();
                    return Make("t1_product_" + q.questionID, Show(FullEquation(q)) + "\nრომელია პროდუქტი?", "პროდუქტები ისრის მარჯვნივ იწერება.", product, product + " ისრის მარჯვნივაა, ესე იგი პროდუქტია.", 0, wrong);
                }
                case 20: // molecule construction
                {
                    string f = Pick(Formulas.Where(x => !x.Contains("(") && Atoms(x).Count > 1).ToList()); var atoms = Atoms(f);
                    var keysList = atoms.Keys.ToList();
                    var swapped = new Dictionary<string, int>(atoms); (swapped[keysList[0]], swapped[keysList[1]]) = (atoms[keysList[1]], atoms[keysList[0]]);
                    var more = new Dictionary<string, int>(atoms); more[keysList[0]] += 1;
                    var other = Pick(Formulas.Where(x => x != f && !x.Contains("(")).ToList());
                    return Make("t2_build_" + f, "რომელი ფორმულა შეესაბამება მოლეკულას: " + Describe(atoms) + "?", "ელემენტის სიმბოლოს შემდეგ ინდექსად მისი ატომების რაოდენობა დაწერე.", f, Describe(atoms) + " → " + f + ".", 0,
                        (Compose(swapped), "ინდექსები სხვა ელემენტებს მიაწერე."), (Compose(more), keysList[0] + "-ის ატომები ზედმეტია."), (other, other + " სულ სხვა ნივთიერებაა."), (f + "2", "ბოლოს ზედმეტი ინდექსია."));
                }
                case 21: // reaction classification
                {
                    var pool = equations.Where(q => q.reactionCategory == ReactionCategory.Decomposition
                        || q.reactionCategory == ReactionCategory.Synthesis && !q.equationPrompt.Contains("O2")
                        || q.reactionCategory == ReactionCategory.Combustion && q.equationPrompt.Contains("H")
                        || q.reactionCategory == ReactionCategory.AcidBase && q.equationPrompt.Split(new[] { "->" }, StringSplitOptions.None)[0].Contains("OH")).ToList();
                    var q2 = Pick(pool);
                    string[] names = { "შეერთება", "დაშლა", "წვა", "ნეიტრალიზაცია" };
                    string[] why = { "შეერთებისას რამდენიმე ნივთიერება ერთ პროდუქტს ქმნის.", "დაშლისას ერთი ნივთიერება რამდენიმედ იშლება.", "წვისას საწვავი ჟანგბადთან რეაგირებს და CO2 და H2O მიიღება.", "ნეიტრალიზაციისას მჟავა და ფუძე მარილსა და წყალს იძლევა." };
                    int right = q2.reactionCategory == ReactionCategory.Synthesis ? 0 : q2.reactionCategory == ReactionCategory.Decomposition ? 1 : q2.reactionCategory == ReactionCategory.Combustion ? 2 : 3;
                    return Make("t2_type_" + q2.questionID, Show(FullEquation(q2)) + "\nრა ტიპის რეაქციაა?", "დათვალე რეაგენტები და პროდუქტები. არის O2 და CO2 + H2O? მჟავა და ფუძე?", names[right], why[right], 0,
                        Enumerable.Range(0, 4).Where(i => i != right).Select(i => (names[i], "არა: " + why[i])).ToArray());
                }
                case 22: // beginner balancing
                    return Equation(Pick(game.questions.Where(q => q.difficultyTier == 1).ToList()));
                case 23: // concentration
                {
                    float n = Pick(new[] { 0.2f, 0.5f, 1f, 1.5f, 2f }), v = Pick(new[] { 0.5f, 1f, 2f, 4f });
                    string F(float x) => x.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " მოლ/ლ";
                    string N(float x) => x.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
                    float c = n / v;
                    return Make("t2_conc_" + N(n) + "_" + N(v), N(n) + " მოლი ნივთიერება გახსნილია " + N(v) + " ლ ხსნარში. რა არის კონცენტრაცია?", "C = n / V.", F(c), "C = n / V = " + N(n) + " / " + N(v) + " = " + F(c) + ".", 0,
                        (F(n * v), "ეს n × V-ია; საჭიროა გაყოფა."), (F(v / n), "ეს V / n-ია — პირიქით გაყავი."), (F(n), "ეს მხოლოდ ნივთიერების რაოდენობაა."), (F(c * 2), "ორჯერ მეტია."), (F(c / 2), "ორჯერ ნაკლებია."));
                }
                case 30: // harder balancing
                    return Equation(Pick(game.questions.Where(q => q.difficultyTier >= 2).ToList()));
                case 31: // stoichiometry: mole ratios from a balanced equation
                {
                    var q = Pick(equations.Where(x => x.difficultyTier == 1).ToList());
                    var sides = FullEquation(q).Split(new[] { "->" }, StringSplitOptions.None);
                    var a = Pick(Terms(sides[0])); var p = Pick(Terms(sides[1]));
                    int k = UnityEngine.Random.Range(2, 5), given = a.coefficient * k, answer = p.coefficient * k;
                    return Make("t3_stoich_" + q.questionID + k, Show(FullEquation(q)) + "\nრამდენი მოლი " + p.formula + " მიიღება " + given + " მოლი " + a.formula + "-დან?", "კოეფიციენტები მოლურ თანაფარდობას გვიჩვენებს: " + a.formula + " : " + p.formula + " = " + a.coefficient + " : " + p.coefficient + ".",
                        answer.ToString(), a.formula + " : " + p.formula + " = " + a.coefficient + " : " + p.coefficient + ", ამიტომ " + given + " × " + p.coefficient + " / " + a.coefficient + " = " + answer + ".", 0,
                        (given.ToString(), "თანაფარდობა 1 : 1 არ არის."), ((answer * 2).ToString(), "ორჯერ მეტია; კოეფიციენტები გადაამოწმე."), ((answer + 1).ToString(), "ერთით მეტია."), (k.ToString(), "ეს მხოლოდ მამრავლია."));
                }
                case 32: // acid-base calculation
                {
                    int x = UnityEngine.Random.Range(1, 5); bool acid = UnityEngine.Random.value < 0.5f;
                    string c = (1 / Math.Pow(10, x)).ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);
                    int ph = acid ? x : 14 - x;
                    return Make("t3_ph_" + (acid ? "a" : "b") + x, "რა არის " + c + " მოლ/ლ " + (acid ? "HCl" : "NaOH") + " ხსნარის pH? (ძლიერი " + (acid ? "მჟავა" : "ფუძე") + ", 25°C)",
                        acid ? "pH = −log[H⁺]." : "ჯერ იპოვე pOH = −log[OH⁻], შემდეგ pH = 14 − pOH.", ph.ToString(),
                        acid ? "[H⁺] = " + c + ", ამიტომ pH = " + x + "." : "pOH = " + x + ", ამიტომ pH = 14 − " + x + " = " + ph + ".", 0,
                        ((14 - ph).ToString(), acid ? "ეს ფუძის pH იქნებოდა." : "ეს pOH-ია და არა pH."), ("7", "7 მხოლოდ ნეიტრალური ხსნარია."), ((ph + 1).ToString(), "ერთით მეტია."), ((ph - 1).ToString(), "ერთით ნაკლებია."));
                }
            }
            return Pick(General);
        }

        public static string ExplainMismatch(string equation, string choice)
        {
            var sides = equation.Replace("?", choice).Split(new[] { "->" }, StringSplitOptions.None);
            var left = Atoms(sides[0]); var right = Atoms(sides[1]);
            foreach(string element in left.Keys.Union(right.Keys))
                if(left.GetValueOrDefault(element) != right.GetValueOrDefault(element))
                    return element + ": მარცხნივ " + left.GetValueOrDefault(element) + " ატომია, მარჯვნივ — " + right.GetValueOrDefault(element) + ". რეაქციის დროს ატომების რაოდენობა უნდა შენარჩუნდეს. შეცვალე კოეფიციენტი და არა ფორმულის ინდექსი.";
            return "ატომების რაოდენობა ემთხვევა.";
        }
    }
}

