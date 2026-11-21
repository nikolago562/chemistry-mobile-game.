using System;
using System.Linq;
using UnityEngine;

namespace ChemistryGame
{
    /// <summary>What the player sees when two substances meet.</summary>
    public enum MixSign { Gas, Colour, Heat, Solid, Dissolves, Nothing }

    public sealed class Reagent
    {
        public readonly string ID, Name, Formula, Tag;
        public readonly Color Color;
        public Reagent(string id, string name, string formula, string tag, Color32 color) { ID = id; Name = name; Formula = formula; Tag = tag; Color = color; }
    }

    public sealed class MixResult
    {
        public readonly string ID, A, B, Equation, Why;
        public readonly MixSign Sign;
        public readonly Color Color;
        public MixResult(string a, string b, MixSign sign, Color32 color, string equation, string why)
        {
            A = a; B = b; Sign = sign; Color = color; Equation = equation; Why = why;
            ID = Mixing.Key(a, b);
        }
        public string Title => Mixing.Get(A).Name + " + " + Mixing.Get(B).Name;
        public string Note => Mixing.SignName(Sign) + " • " + (Equation != "" ? Equation : Why);
    }

    /// <summary>
    /// The free-mixing bench's rule table: every listed pair has a real, kid-safe school result,
    /// and every other pair answers "no reaction" with a reason drawn from what the substances are.
    /// </summary>
    public static class Mixing
    {
        public static readonly string[] SignNames = { "აირის ბუშტუკები", "ფერი შეიცვლება", "გათბება", "ნალექი გაჩნდება", "უბრალოდ გაიხსნება", "არაფერი მოხდება" };
        public static string SignName(MixSign? sign) => sign.HasValue ? SignNames[(int)sign.Value] : "";

        public static readonly Reagent[] Reagents =
        {
            new Reagent("water", "წყალი", "H2O", "water", new Color32(170, 214, 235, 255)),
            new Reagent("vinegar", "ძმარი", "CH3COOH", "acid", new Color32(236, 228, 196, 255)),
            new Reagent("lemon", "ლიმონის წვენი", "C6H8O7", "acid", new Color32(246, 230, 120, 255)),
            new Reagent("hcl", "მარილმჟავა", "HCl", "acid", new Color32(220, 236, 226, 255)),
            new Reagent("soda", "საცხობი სოდა", "NaHCO3", "carbonate", new Color32(245, 245, 240, 255)),
            new Reagent("chalk", "ცარცი", "CaCO3", "carbonate", new Color32(236, 232, 222, 255)),
            new Reagent("naoh", "ნატრიუმის ტუტე", "NaOH", "base", new Color32(214, 226, 240, 255)),
            new Reagent("salt", "სუფრის მარილი", "NaCl", "neutral", new Color32(250, 250, 250, 255)),
            new Reagent("sugar", "შაქარი", "C12H22O11", "neutral", new Color32(252, 246, 232, 255)),
            new Reagent("zinc", "თუთიის ნამცეცები", "Zn", "metal", new Color32(170, 178, 186, 255)),
            new Reagent("nail", "რკინის ლურსმანი", "Fe", "metal", new Color32(120, 124, 132, 255)),
            new Reagent("copper", "სპილენძის მავთული", "Cu", "metal", new Color32(200, 120, 80, 255)),
            new Reagent("cuso4", "სპილენძის სულფატი", "CuSO4", "coloured", new Color32(60, 140, 220, 255)),
            new Reagent("peroxide", "წყალბადის ზეჟანგი", "H2O2", "peroxide", new Color32(230, 240, 246, 255)),
            new Reagent("yeast", "საფუარი", "", "catalyst", new Color32(214, 186, 140, 255)),
            new Reagent("cabbage", "კომბოსტოს ინდიკატორი", "ანთოციანი", "indicator", new Color32(120, 70, 160, 255)),
        };

        public static readonly MixResult[] Results =
        {
            // Acids meet carbonates: carbon dioxide fizzes out.
            new MixResult("vinegar", "soda", MixSign.Gas, new Color32(236, 232, 214, 255), "CH3COOH + NaHCO3 → CO2↑ + H2O + მარილი", "მჟავა კარბონატს შლის და ნახშირორჟანგი გამოიყოფა — აქედანაა ბუშტუკები."),
            new MixResult("lemon", "soda", MixSign.Gas, new Color32(246, 236, 160, 255), "ლიმონმჟავა + NaHCO3 → CO2↑ + H2O + მარილი", "ლიმონის წვენიც მჟავაა, ამიტომ სოდასთან იგივე ნახშირორჟანგი გამოიყოფა."),
            new MixResult("hcl", "soda", MixSign.Gas, new Color32(226, 238, 230, 255), "HCl + NaHCO3 → NaCl + H2O + CO2↑", "ძლიერი მჟავა სოდას სწრაფად შლის: ბევრი ბუშტუკი ერთბაშად."),
            new MixResult("hcl", "chalk", MixSign.Gas, new Color32(232, 236, 226, 255), "CaCO3 + 2HCl → CaCl2 + H2O + CO2↑", "ასე შიშინებს კალციტი და მარმარილო მჟავაში: კარბონატიდან ნახშირორჟანგი გამოდის."),
            new MixResult("vinegar", "chalk", MixSign.Gas, new Color32(236, 232, 214, 255), "CaCO3 + მჟავა → CO2↑ + ...", "ძმარი სუსტი მჟავაა, ამიტომ ცარცი ნელა, მაგრამ მაინც შიშინებს."),
            new MixResult("lemon", "chalk", MixSign.Gas, new Color32(246, 236, 170, 255), "CaCO3 + ლიმონმჟავა → CO2↑ + ...", "ლიმონის წვენი ცარცს ნელ-ნელა ხსნის — ამიტომ აზიანებს მჟავე სასმელი კბილის მინანქარს."),
            // Neutralisation: the warmth is the sign.
            new MixResult("hcl", "naoh", MixSign.Heat, new Color32(214, 232, 236, 255), "HCl + NaOH → NaCl + H2O", "ნეიტრალიზაცია: მჟავა და ფუძე ერთმანეთს აბათილებს, სითბო გამოიყოფა და მარილიანი წყალი რჩება."),
            new MixResult("lemon", "naoh", MixSign.Heat, new Color32(236, 232, 200, 255), "ლიმონმჟავა + NaOH → მარილი + H2O", "ნეიტრალიზაცია: ლიმონის მჟავე გემო ქრება და ხსნარი ოდნავ თბება."),
            new MixResult("vinegar", "naoh", MixSign.Heat, new Color32(226, 230, 226, 255), "CH3COOH + NaOH → CH3COONa + H2O", "ესეც ნეიტრალიზაციაა, ოღონდ სუსტი მჟავით — ხსნარი მხოლოდ ოდნავ თბება."),
            // Metals and acids: hydrogen bubbles.
            new MixResult("hcl", "zinc", MixSign.Gas, new Color32(214, 222, 226, 255), "Zn + 2HCl → ZnCl2 + H2↑", "თუთია წყალბადზე აქტიურია, ამიტომ მჟავიდან წყალბადს გამოაძევებს."),
            new MixResult("hcl", "nail", MixSign.Gas, new Color32(190, 220, 190, 255), "Fe + 2HCl → FeCl2 + H2↑", "რკინაც წყალბადზე აქტიურია: ნელა იხსნება, ხსნარი კი მომწვანო ხდება."),
            new MixResult("vinegar", "zinc", MixSign.Gas, new Color32(226, 226, 214, 255), "Zn + მჟავა → H2↑ + მარილი", "სუსტ მჟავაში თუთია ნელა, წვრილი ბუშტუკებით რეაგირებს."),
            new MixResult("hcl", "copper", MixSign.Nothing, new Color32(220, 236, 226, 255), "", "სპილენძი წყალბადზე ნაკლებად აქტიურია, ამიტომ განზავებული მარილმჟავა მას ვერ ხსნის. „არაფერიც“ შედეგია!"),
            // A more active metal pushes copper out of its salt.
            new MixResult("cuso4", "nail", MixSign.Colour, new Color32(150, 196, 170, 255), "Fe + CuSO4 → FeSO4 + Cu", "ლურსმანზე წითელი სპილენძი ილექება და ლურჯი ფერი ქრება: რკინა სპილენძზე აქტიურია."),
            new MixResult("cuso4", "zinc", MixSign.Colour, new Color32(200, 210, 214, 255), "Zn + CuSO4 → ZnSO4 + Cu", "თუთია სპილენძს ხსნარიდან გამოაძევებს: ლურჯი ფერი ფერმკრთალდება, ფსკერზე კი მოწითალო სპილენძი ჩნდება."),
            new MixResult("cuso4", "copper", MixSign.Nothing, new Color32(60, 140, 220, 255), "", "სპილენძი საკუთარ მარილთან ვერაფერს „გაცვლის“ — ერთნაირი ლითონია. ხსნარი ისევ ლურჯია."),
            // Two liquids make a solid.
            new MixResult("cuso4", "naoh", MixSign.Solid, new Color32(110, 170, 230, 255), "CuSO4 + 2NaOH → Cu(OH)2↓ + Na2SO4", "ორი სითხიდან მყარი ნივთიერება წარმოიქმნა — ცისფერი ნალექი. ესეც რეაქციის ნიშანია."),
            new MixResult("cuso4", "soda", MixSign.Solid, new Color32(110, 190, 170, 255), "CuSO4 + NaHCO3 → სპილენძის კარბონატი↓ + CO2↑ + ...", "მომწვანო-ცისფერი ნალექი და ცოტა ბუშტუკი: ერთ რეაქციას ორი ნიშანიც შეიძლება ჰქონდეს."),
            new MixResult("cabbage", "cuso4", MixSign.Nothing, new Color32(80, 100, 200, 255), "", "ლურჯი და იისფერი უბრალოდ შეერია. ფერის შეცვლა ყოველთვის რეაქციას არ ნიშნავს — ახალი ნივთიერება აქ არ წარმოქმნილა."),
            // A catalyst speeds things up without being used up.
            new MixResult("peroxide", "yeast", MixSign.Gas, new Color32(240, 232, 214, 255), "2H2O2 → 2H2O + O2↑", "საფუარი კატალიზატორია: ზეჟანგის დაშლას აჩქარებს, თვითონ კი არ იხარჯება. ქაფში ჟანგბადია!"),
            new MixResult("peroxide", "water", MixSign.Nothing, new Color32(210, 230, 240, 255), "", "წყალი ზეჟანგს მხოლოდ აზავებს. კატალიზატორის გარეშე ის ძალიან ნელა იშლება — თვალით ვერაფერს დაინახავ."),
            // The cabbage indicator tells acids from bases by colour.
            new MixResult("cabbage", "vinegar", MixSign.Colour, new Color32(214, 70, 110, 255), "", "ინდიკატორი გაწითლდა: ძმარი მჟავაა."),
            new MixResult("cabbage", "lemon", MixSign.Colour, new Color32(226, 80, 120, 255), "", "ვარდისფერ-წითელი: ლიმონის წვენი მჟავაა."),
            new MixResult("cabbage", "hcl", MixSign.Colour, new Color32(210, 40, 70, 255), "", "მკვეთრი წითელი: ძლიერი მჟავა, pH დაბალია."),
            new MixResult("cabbage", "soda", MixSign.Colour, new Color32(70, 150, 170, 255), "", "ცისფერ-მწვანე: სოდა სუსტი ფუძეა."),
            new MixResult("cabbage", "naoh", MixSign.Colour, new Color32(170, 200, 60, 255), "", "ყვითელ-მწვანე: ძლიერი ფუძე, pH მაღალია."),
            new MixResult("cabbage", "water", MixSign.Nothing, new Color32(120, 70, 160, 255), "", "ფერი იისფერი რჩება: წყალი ნეიტრალურია. ასე ვამოწმებთ, რომ ინდიკატორი თავისთავად არ იცვლება."),
            // Dissolving is a physical change, not a reaction.
            new MixResult("water", "salt", MixSign.Dissolves, new Color32(180, 218, 236, 255), "", "მარილი იხსნება — ეს ფიზიკური ცვლილებაა: წყალს თუ აორთქლებ, მარილი ისევ დარჩება."),
            new MixResult("water", "sugar", MixSign.Dissolves, new Color32(186, 218, 232, 255), "", "შაქარი იხსნება და ქრება თვალიდან, მაგრამ არსად წასულა — წყალი ტკბილია."),
            new MixResult("water", "cuso4", MixSign.Dissolves, new Color32(70, 150, 225, 255), "", "ლურჯი ხსნარი! ფერი ჩნდება, მაგრამ ეს გახსნაა, არა რეაქცია: ახალი ნივთიერება არ წარმოიქმნა."),
            new MixResult("water", "soda", MixSign.Dissolves, new Color32(190, 222, 238, 255), "", "სოდა წყალში იხსნება ბუშტუკების გარეშე. ბუშტუკებს მჟავა სჭირდება."),
            new MixResult("water", "chalk", MixSign.Nothing, new Color32(200, 216, 222, 255), "", "ცარცი წყალში თითქმის არ იხსნება — ამიტომაც არ ქრება ქვა წვიმაში."),
        };

        public static Reagent Get(string id) => Reagents.FirstOrDefault(r => r.ID == id);
        public static string Key(string a, string b) => string.CompareOrdinal(a, b) <= 0 ? "mix:" + a + "+" + b : "mix:" + b + "+" + a;
        public static MixResult Find(string a, string b) { string key = Key(a, b); return Results.FirstOrDefault(r => r.ID == key); }

        /// <summary>Why an unlisted pair shows no change, from what kind of substances they are.</summary>
        public static string NoReaction(Reagent a, Reagent b)
        {
            if (a.ID == b.ID) return a.Name + " საკუთარ თავთან არ რეაგირებს — უბრალოდ მეტი გაქვს იგივე ნივთიერება.";
            if (a.Tag == b.Tag && (a.Tag == "acid" || a.Tag == "base")) return "ორივე " + (a.Tag == "acid" ? "მჟავაა" : "ფუძეა") + " — ერთმანეთს ვერ აბათილებენ, უბრალოდ ერევიან.";
            var metal = a.Tag == "metal" ? a : b.Tag == "metal" ? b : null;
            if (metal != null && (a.Tag == "acid" || b.Tag == "acid"))
                return metal.ID == "copper" ? "სპილენძი წყალბადზე ნაკლებად აქტიურია, ამიტომ მჟავა მას ვერ ხსნის."
                    : "სუსტ მჟავაში " + metal.Name + " ისე ნელა რეაგირებს, რომ ახლა ვერაფერს დაინახავ. რამდენიმე საათში წვრილი ბუშტუკები გამოჩნდება.";
            var indicator = a.Tag == "indicator" ? b : b.Tag == "indicator" ? a : null;
            if (indicator != null) return "ინდიკატორი იისფერი რჩება: " + indicator.Name + " არც მჟავაა, არც ფუძე.";
            var wet = a.Tag == "water" ? b : b.Tag == "water" ? a : null;
            if (wet != null)
                return wet.ID == "nail" ? "ლურსმანი ახლა არ იცვლება. ჟანგს წყალიც, ჟანგბადიც და რამდენიმე დღე სჭირდება."
                    : wet.Tag == "metal" ? wet.Name + " წყალში არ იცვლება — ლითონი არ იხსნება."
                    : "წყალთან შერევისას " + wet.Name + " უბრალოდ განზავდება — ახალი ნივთიერება არ წარმოიქმნება.";
            if (a.Tag == "metal" && b.Tag == "metal") return "ორი მყარი ლითონი ერთმანეთს უბრალოდ ეხება. ლითონს რეაქციისთვის მჟავა ან ნაკლებად აქტიური ლითონის მარილი სჭირდება.";
            if (a.Tag == "neutral" || b.Tag == "neutral") return (a.Tag == "neutral" ? a : b).Name + " აქ უბრალოდ ერევა. არც აირი, არც ფერი, არც სითბო, არც ნალექი — რეაქცია არ არის.";
            return "ქიმიური ცვლილების ნიშანი არ ჩანს: არც აირი, არც ფერი, არც სითბო, არც ნალექი. ეს წყვილი არ რეაგირებს.";
        }
    }
}
