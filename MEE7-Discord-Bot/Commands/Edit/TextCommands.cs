using Discord;
using Discord.WebSocket;
using System;
using System.Linq;

namespace MEE7.Commands
{
    public partial class Edit : Command
    {
        readonly EditCommand[] TextCommands = new EditCommand[]
        {
            new EditCommand("swedish", "Convert the text to swedish", (SocketMessage m, string a, object o) => {
                return string.Join("", (o as string).Select(x => x + "f"));
            }, typeof(string), typeof(string)),
            new EditCommand("mock", "Mock the text", (SocketMessage m, string a, object o) => {
                return string.Join("", (o as string).Select((x) => { return (Program.RDM.Next(2) == 1 ? char.ToUpper(x) : char.ToLower(x)); })) + 
                    "https://images.complex.com/complex/images/c_limit,w_680/fl_lossy,pg_1,q_auto/bujewhyvyyg08gjksyqh/spongebob";
            }, typeof(string), typeof(string)),
            new EditCommand("crab", "Crab the text", (SocketMessage m, string a, object o) => {
                return ":crab: " + (o as string) + " :crab:\n https://www.youtube.com/watch?v=LDU_Txk06tM&t=75s";
            }, typeof(string), typeof(string)),
            new EditCommand("CAPS", "Convert text to CAPS", (SocketMessage m, string a, object o) => {
                return string.Join("", (o as string).Select((x) => { return char.ToUpper(x); }));
            }, typeof(string), typeof(string)),
            new EditCommand("SUPERCAPS", "Convert text to SUPER CAPS", (SocketMessage m, string a, object o) => {
                return string.Join("", (o as string).Select((x) => { return char.ToUpper(x) + " "; }));
            }, typeof(string), typeof(string)),
            new EditCommand("CopySpoilerify", "Convert text to a spoiler", (SocketMessage m, string a, object o) => {
                return "`" + string.Join("", (o as string).Select((x) => { return "||" + x + "||"; })) + "`";
            }, typeof(string), typeof(string)),
            new EditCommand("Spoilerify", "Convert text to a spoiler", (SocketMessage m, string a, object o) => {
                return string.Join("", (o as string).Select((x) => { return "||" + x + "||"; }));
            }, typeof(string), typeof(string)),
            new EditCommand("Unspoilerify", "Convert spoiler text to readable text", (SocketMessage m, string a, object o) => {
                return (o as string).Replace("|", "");
            }, typeof(string), typeof(string)),
            new EditCommand("Aestheticify", "Convert text to Ａｅｓｔｈｅｔｉｃ text", (SocketMessage m, string a, object o) => {
                return (o as string).Select(x => x == ' ' || x == '\n' ? x : (char)(x - '!' + '！')).Foldl("", (x, y) => x + y);
            }, typeof(string), typeof(string)),
        };
    }
}
