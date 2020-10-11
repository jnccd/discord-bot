using Discord;
using MEE7.Backend.HelperFunctions;
using System;
using System.Linq;
using System.Text;
using TweetSharp;

namespace MEE7.Commands.Edit
{
    public class TextCommands : EditCommandProvider
    {
        public string mockDesc = "Mock the text";
        public string Mock(string s, IMessage m)
        {
            return string.Join("", s.Select((x) => { return Program.RDM.Next(2) == 1 ? char.ToUpper(x) : char.ToLower(x); })) +
                    "\n https://images.complex.com/complex/images/c_limit,w_680/fl_lossy,pg_1,q_auto/bujewhyvyyg08gjksyqh/spongebob";
        }

        public string crabDesc = "Crab the text";
        public string Crab(string s, IMessage m)
        {
            return ":crab: " + s + " :crab:\n https://www.youtube.com/watch?v=LDU_Txk06tM&t=75s";
        }

        public string CAPSDesc = "Convert text to CAPS";
        public string CAPS(string s, IMessage m)
        {
            return string.Join("", s.Select((x) => { return char.ToUpper(x); }));
        }

        public string SUPERCAPSDesc = "Convert text to SUPER CAPS";
        public string SUPERCAPS(string s, IMessage m)
        {
            return string.Join("", s.Select((x) => { return char.ToUpper(x) + " "; }));
        }

        public string CopySpoilerifyDesc = "Convert text to a spoiler";
        public string CopySpoilerify(string s, IMessage m)
        {
            return "`" + string.Join("", s.Select((x) => { return "||" + x + "||"; })) + "`";
        }

        public string SpoilerifyDesc = "Convert text to a spoiler";
        public string Spoilerify(string s, IMessage m)
        {
            return string.Join("", s.Select((x) => { return "||" + x + "||"; }));
        }

        public string UnspoilerifyDesc = "Convert spoiler text to readable text";
        public string Unspoilerify(string s, IMessage m)
        {
            return s.Replace("|", "");
        }

        public string AestheticifyDesc = "Convert text to Ａｅｓｔｈｅｔｉｃ text";
        public string Aestheticify(string s, IMessage m)
        {
            return s.Select(x => x == ' ' || x == '\n' ? x : (char)(x - '!' + '！')).Foldl("", (x, y) => x + y);
        }

        //public string getHTMLFromWebsiteDesc = "Get the websites html";
        //public string GetHTMLFromWebsite(string url, IMessage m) => url.GetHTMLfromURL();

        public string SearchTweetDesc = "Search for a tweet with that text";
        public string SearchTweet(string s, IMessage m)
        {
            string re = "";
            foreach (string block in s.Split("\n\n"))
            {
                if (block.StartsWith("Confidence: "))
                    continue;
                var modBlock = block.
                    Split(' ').Where(x => !x.Contains('@')).Take(10).Combine(" ").
                    Replace("|", "I").
                    Replace("\"", "");
                if (string.IsNullOrWhiteSpace(modBlock))
                    continue;
                var res = Program.twitterService.Search(new SearchOptions() { Q = modBlock })?.Statuses;
                if (res != null && res.FirstOrDefault() != null)
                {
                    re = res.First().ToTwitterUrl().AbsoluteUri;
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(re))
                throw new Exception("Didn't find anything");

            return re;
        }

        public string japanifyDesc = "Convert the text into katakana symbols, doesnt actually translate";
        public string Japanify(string s, IMessage m)
        {
            StringBuilder input = new StringBuilder(s);

            for (int i = 0; i < input.Length; i++)
                input[i] = char.ToLower(input[i]);

            var oneSymbolKatakana = Katakana1S.Select(x => x.Item2);

            if (oneSymbolKatakana.FirstOrDefault(x => x[0] == input[input.Length - 1]) == null)
                input.Append("u");

            input = input.Replace("l", "r");
            input = input.Replace("v", "f");
            input = input.Replace("qu", "キュ");

            for (int i = 1; i < input.Length - 1; i++)
                if (input[i] == input[i - 1] &&
                    input[i] != 'a' &&
                    input[i] != 'i' &&
                    input[i] != 'u' &&
                    input[i] != 'e' &&
                    input[i] != 'o')
                    input[i - 1] = 'ッ';

            foreach (Tuple<string, string> t in Katakana3S)
                input = input.Replace(t.Item2, t.Item1);
            foreach (Tuple<string, string> t in Katakana2S)
                input = input.Replace(t.Item2, t.Item1);
            foreach (Tuple<string, string> t in Katakana1S)
                input = input.Replace(t.Item2, t.Item1);

            for (int i = 0; i < input.Length; i++)
                if (input[i] <= 'z' && input[i] >= 'A' && oneSymbolKatakana.FirstOrDefault(x => x[0] == input[i]) == null)
                    input.Insert(i + 1, 'o');

            foreach (Tuple<string, string> t in Katakana3S)
                input = input.Replace(t.Item2, t.Item1);
            foreach (Tuple<string, string> t in Katakana2S)
                input = input.Replace(t.Item2, t.Item1);
            foreach (Tuple<string, string> t in Katakana1S)
                input = input.Replace(t.Item2, t.Item1);

            input = input.Replace("x", "イクス");
            input = input.Replace("f", "フ");

            input = input.Replace(" ", "");

            return input.ToString();
        }

        public string unjapanifyDesc = "Convert katakana into readable stuff";
        public string Unjapanify(string s, IMessage m)
        {
            StringBuilder input = new StringBuilder(s);

            foreach (Tuple<string, string> t in Katakana)
                input = input.Replace(t.Item1, t.Item2);

            return input.ToString();
        }


        static readonly Tuple<string, string>[] Katakana = new Tuple<string, string>[] {
            new Tuple<string, string>("ア", "a"),
            new Tuple<string, string>("イ", "i"),
            new Tuple<string, string>("ウ", "u"),
            new Tuple<string, string>("エ", "e"),
            new Tuple<string, string>("オ", "o"),
            new Tuple<string, string>("カ", "ka"),
            new Tuple<string, string>("キ", "ki"),
            new Tuple<string, string>("ク", "ku"),
            new Tuple<string, string>("ケ", "ke"),
            new Tuple<string, string>("コ", "ko"),
            new Tuple<string, string>("サ", "sa"),
            new Tuple<string, string>("シ", "shi"),
            new Tuple<string, string>("ス", "su"),
            new Tuple<string, string>("セ", "se"),
            new Tuple<string, string>("ソ", "so"),
            new Tuple<string, string>("タ", "ta"),
            new Tuple<string, string>("チ", "chi"),
            new Tuple<string, string>("ツ", "tsu"),
            new Tuple<string, string>("テ", "te"),
            new Tuple<string, string>("ト", "to"),
            new Tuple<string, string>("ナ", "na"),
            new Tuple<string, string>("ニ", "ni"),
            new Tuple<string, string>("ヌ", "nu"),
            new Tuple<string, string>("ネ", "ne"),
            new Tuple<string, string>("ノ", "no"),
            new Tuple<string, string>("ハ", "ha"),
            new Tuple<string, string>("ヒ", "hi"),
            new Tuple<string, string>("フ", "fu"),
            new Tuple<string, string>("ヘ", "he"),
            new Tuple<string, string>("ホ", "ho"),
            new Tuple<string, string>("マ", "ma"),
            new Tuple<string, string>("ミ", "mi"),
            new Tuple<string, string>("ム", "mu"),
            new Tuple<string, string>("メ", "me"),
            new Tuple<string, string>("モ", "mo"),
            new Tuple<string, string>("ヤ", "ya"),
            new Tuple<string, string>("ユ", "yu"),
            new Tuple<string, string>("ヨ", "yo"),
            new Tuple<string, string>("ラ", "ra"),
            new Tuple<string, string>("リ", "ri"),
            new Tuple<string, string>("ル", "ru"),
            new Tuple<string, string>("レ", "re"),
            new Tuple<string, string>("ロ", "ro"),
            new Tuple<string, string>("ワ", "wa"),
            new Tuple<string, string>("ヰ", "wi"),
            new Tuple<string, string>("ヱ", "we"),
            new Tuple<string, string>("ヲ", "wo"),
            new Tuple<string, string>("ン", "n"),
            new Tuple<string, string>("ガ", "ga"),
            new Tuple<string, string>("ギ", "gi"),
            new Tuple<string, string>("グ", "gu"),
            new Tuple<string, string>("ゲ", "ge"),
            new Tuple<string, string>("ゴ", "go"),
            new Tuple<string, string>("ザ", "za"),
            new Tuple<string, string>("ジ", "ji"),
            new Tuple<string, string>("ズ", "zu"),
            new Tuple<string, string>("ゼ", "ze"),
            new Tuple<string, string>("ゾ", "zo"),
            new Tuple<string, string>("ダ", "da"),
            new Tuple<string, string>("ヂ", "ji/dji/jyi"),
            new Tuple<string, string>("ヅ", "zu/dzu"),
            new Tuple<string, string>("デ", "de"),
            new Tuple<string, string>("ド", "do"),
            new Tuple<string, string>("バ", "ba"),
            new Tuple<string, string>("ビ", "bi"),
            new Tuple<string, string>("ブ", "bu"),
            new Tuple<string, string>("ベ", "be"),
            new Tuple<string, string>("ボ", "bo"),
            new Tuple<string, string>("パ", "pa"),
            new Tuple<string, string>("ピ", "pi"),
            new Tuple<string, string>("プ", "pu"),
            new Tuple<string, string>("ペ", "pe"),
            new Tuple<string, string>("ポ", "po"),
            new Tuple<string, string>("キャ", "kya"),
            new Tuple<string, string>("キュ", "kyu"),
            new Tuple<string, string>("キョ", "kyo"),
            new Tuple<string, string>("シャ", "sha"),
            new Tuple<string, string>("シュ", "shu"),
            new Tuple<string, string>("ショ", "sho"),
            new Tuple<string, string>("チャ", "cha"),
            new Tuple<string, string>("チュ", "chu"),
            new Tuple<string, string>("チョ", "cho"),
            new Tuple<string, string>("ニャ", "nya"),
            new Tuple<string, string>("ニュ", "nyu"),
            new Tuple<string, string>("ニョ", "nyo"),
            new Tuple<string, string>("ヒャ", "hya"),
            new Tuple<string, string>("ヒュ", "hyu"),
            new Tuple<string, string>("ヒョ", "hyo"),
            new Tuple<string, string>("ミャ", "mya"),
            new Tuple<string, string>("ミュ", "myu"),
            new Tuple<string, string>("ミョ", "myo"),
            new Tuple<string, string>("リャ", "rya"),
            new Tuple<string, string>("リュ", "ryu"),
            new Tuple<string, string>("リョ", "ryo"),
            new Tuple<string, string>("ギャ", "gya"),
            new Tuple<string, string>("ギュ", "gyu"),
            new Tuple<string, string>("ギョ", "gyo"),
            new Tuple<string, string>("ジャ", "ja"),
            new Tuple<string, string>("ジュ", "ju"),
            new Tuple<string, string>("ジョ", "jo"),
            new Tuple<string, string>("ヂャ", "ja/dja/jya"),
            new Tuple<string, string>("ヂュ", "ju/dju/jyu"),
            new Tuple<string, string>("ヂョ", "jo/djo/jyo"),
            new Tuple<string, string>("ビャ", "bya"),
            new Tuple<string, string>("ビュ", "byu"),
            new Tuple<string, string>("ビョ", "byo"),
            new Tuple<string, string>("ピャ", "pya"),
            new Tuple<string, string>("ピュ", "pyu"),
            new Tuple<string, string>("ピョ", "pyo"),
        };
        static readonly Tuple<string, string>[] Katakana1S = new Tuple<string, string>[] {
            new Tuple<string, string>("ア", "a"),
            new Tuple<string, string>("イ", "i"),
            new Tuple<string, string>("ウ", "u"),
            new Tuple<string, string>("エ", "e"),
            new Tuple<string, string>("オ", "o"),
            new Tuple<string, string>("ン", "n"),
        };
        static readonly Tuple<string, string>[] Katakana2S = new Tuple<string, string>[] {
            new Tuple<string, string>("カ", "ka"),
            new Tuple<string, string>("キ", "ki"),
            new Tuple<string, string>("ク", "ku"),
            new Tuple<string, string>("ケ", "ke"),
            new Tuple<string, string>("コ", "ko"),
            new Tuple<string, string>("サ", "sa"),
            new Tuple<string, string>("ス", "su"),
            new Tuple<string, string>("セ", "se"),
            new Tuple<string, string>("ソ", "so"),
            new Tuple<string, string>("タ", "ta"),
            new Tuple<string, string>("テ", "te"),
            new Tuple<string, string>("ト", "to"),
            new Tuple<string, string>("ナ", "na"),
            new Tuple<string, string>("ニ", "ni"),
            new Tuple<string, string>("ヌ", "nu"),
            new Tuple<string, string>("ネ", "ne"),
            new Tuple<string, string>("ノ", "no"),
            new Tuple<string, string>("ハ", "ha"),
            new Tuple<string, string>("ヒ", "hi"),
            new Tuple<string, string>("フ", "fu"),
            new Tuple<string, string>("ヘ", "he"),
            new Tuple<string, string>("ホ", "ho"),
            new Tuple<string, string>("マ", "ma"),
            new Tuple<string, string>("ミ", "mi"),
            new Tuple<string, string>("ム", "mu"),
            new Tuple<string, string>("メ", "me"),
            new Tuple<string, string>("モ", "mo"),
            new Tuple<string, string>("ヤ", "ya"),
            new Tuple<string, string>("ユ", "yu"),
            new Tuple<string, string>("ヨ", "yo"),
            new Tuple<string, string>("ラ", "ra"),
            new Tuple<string, string>("リ", "ri"),
            new Tuple<string, string>("ル", "ru"),
            new Tuple<string, string>("レ", "re"),
            new Tuple<string, string>("ロ", "ro"),
            new Tuple<string, string>("ワ", "wa"),
            new Tuple<string, string>("ヰ", "wi"),
            new Tuple<string, string>("ヱ", "we"),
            new Tuple<string, string>("ヲ", "wo"),
            new Tuple<string, string>("ガ", "ga"),
            new Tuple<string, string>("ギ", "gi"),
            new Tuple<string, string>("グ", "gu"),
            new Tuple<string, string>("ゲ", "ge"),
            new Tuple<string, string>("ゴ", "go"),
            new Tuple<string, string>("ザ", "za"),
            new Tuple<string, string>("ジ", "ji"),
            new Tuple<string, string>("ズ", "zu"),
            new Tuple<string, string>("ゼ", "ze"),
            new Tuple<string, string>("ゾ", "zo"),
            new Tuple<string, string>("ダ", "da"),
            new Tuple<string, string>("ヂ", "ji"),
            new Tuple<string, string>("ヅ", "zu"),
            new Tuple<string, string>("デ", "de"),
            new Tuple<string, string>("ド", "do"),
            new Tuple<string, string>("バ", "ba"),
            new Tuple<string, string>("ビ", "bi"),
            new Tuple<string, string>("ブ", "bu"),
            new Tuple<string, string>("ベ", "be"),
            new Tuple<string, string>("ボ", "bo"),
            new Tuple<string, string>("パ", "pa"),
            new Tuple<string, string>("ピ", "pi"),
            new Tuple<string, string>("プ", "pu"),
            new Tuple<string, string>("ペ", "pe"),
            new Tuple<string, string>("ポ", "po"),
            new Tuple<string, string>("ジャ", "ja"),
            new Tuple<string, string>("ジュ", "ju"),
            new Tuple<string, string>("ジョ", "jo"),
            new Tuple<string, string>("ヂャ", "ja"),
            new Tuple<string, string>("ヂュ", "ju"),
            new Tuple<string, string>("ヂョ", "jo"),
        };
        static readonly Tuple<string, string>[] Katakana3S = new Tuple<string, string>[] {
            new Tuple<string, string>("シ", "shi"),
            new Tuple<string, string>("チ", "chi"),
            new Tuple<string, string>("ツ", "tsu"),
            new Tuple<string, string>("ヂ", "dji"),
            new Tuple<string, string>("ヂ", "jyi"),
            new Tuple<string, string>("ヅ", "dzu"),
            new Tuple<string, string>("キャ", "kya"),
            new Tuple<string, string>("キュ", "kyu"),
            new Tuple<string, string>("キョ", "kyo"),
            new Tuple<string, string>("シャ", "sha"),
            new Tuple<string, string>("シュ", "shu"),
            new Tuple<string, string>("ショ", "sho"),
            new Tuple<string, string>("チャ", "cha"),
            new Tuple<string, string>("チュ", "chu"),
            new Tuple<string, string>("チョ", "cho"),
            new Tuple<string, string>("ニャ", "nya"),
            new Tuple<string, string>("ニュ", "nyu"),
            new Tuple<string, string>("ニョ", "nyo"),
            new Tuple<string, string>("ヒャ", "hya"),
            new Tuple<string, string>("ヒュ", "hyu"),
            new Tuple<string, string>("ヒョ", "hyo"),
            new Tuple<string, string>("ミャ", "mya"),
            new Tuple<string, string>("ミュ", "myu"),
            new Tuple<string, string>("ミョ", "myo"),
            new Tuple<string, string>("リャ", "rya"),
            new Tuple<string, string>("リュ", "ryu"),
            new Tuple<string, string>("リョ", "ryo"),
            new Tuple<string, string>("ギャ", "gya"),
            new Tuple<string, string>("ギュ", "gyu"),
            new Tuple<string, string>("ギョ", "gyo"),
            new Tuple<string, string>("ヂャ", "dja"),
            new Tuple<string, string>("ヂュ", "dju"),
            new Tuple<string, string>("ヂョ", "djo"),
            new Tuple<string, string>("ヂャ", "jya"),
            new Tuple<string, string>("ヂュ", "jyu"),
            new Tuple<string, string>("ヂョ", "jyo"),
            new Tuple<string, string>("ビャ", "bya"),
            new Tuple<string, string>("ビュ", "byu"),
            new Tuple<string, string>("ビョ", "byo"),
            new Tuple<string, string>("ピャ", "pya"),
            new Tuple<string, string>("ピュ", "pyu"),
            new Tuple<string, string>("ピョ", "pyo"),
        };
    }
}
