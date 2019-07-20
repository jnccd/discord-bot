using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions.Extensions;

namespace MEE7.Commands
{
    public partial class Edit : Command
    {
        readonly EditCommand[] TextCommands = new EditCommand[]
        {
            new EditCommand("swedish", "Convert the text to swedish", typeof(string), typeof(string), new Argument[0], (SocketMessage m, object[] a, object o) => {
                return string.Join("", (o as string).Select(x => x + "f"));
            }),
            new EditCommand("mock", "Mock the text", typeof(string), typeof(string), new Argument[0], (SocketMessage m, object[] a, object o) => {
                return string.Join("", (o as string).Select((x) => { return (Program.RDM.Next(2) == 1 ? char.ToUpper(x) : char.ToLower(x)); })) + 
                    "\n https://images.complex.com/complex/images/c_limit,w_680/fl_lossy,pg_1,q_auto/bujewhyvyyg08gjksyqh/spongebob";
            }),
            new EditCommand("crab", "Crab the text", typeof(string), typeof(string), new Argument[0], (SocketMessage m, object[] a, object o) => {
                return ":crab: " + (o as string) + " :crab:\n https://www.youtube.com/watch?v=LDU_Txk06tM&t=75s";
            }),
            new EditCommand("CAPS", "Convert text to CAPS", typeof(string), typeof(string), new Argument[0], (SocketMessage m, object[] a, object o) => {
                return string.Join("", (o as string).Select((x) => { return char.ToUpper(x); }));
            }),
            new EditCommand("SUPERCAPS", "Convert text to SUPER CAPS", typeof(string), typeof(string), new Argument[0], (SocketMessage m, object[] a, object o) => {
                return string.Join("", (o as string).Select((x) => { return char.ToUpper(x) + " "; }));
            }),
            new EditCommand("CopySpoilerify", "Convert text to a spoiler", typeof(string), typeof(string), new Argument[0], (SocketMessage m, object[] a, object o) => {
                return "`" + string.Join("", (o as string).Select((x) => { return "||" + x + "||"; })) + "`";
            }),
            new EditCommand("Spoilerify", "Convert text to a spoiler", typeof(string), typeof(string), new Argument[0], (SocketMessage m, object[] a, object o) => {
                return string.Join("", (o as string).Select((x) => { return "||" + x + "||"; }));
            }),
            new EditCommand("Unspoilerify", "Convert spoiler text to readable text", typeof(string), typeof(string), new Argument[0], (SocketMessage m, object[] a, object o) => {
                return (o as string).Replace("|", "");
            }),
            new EditCommand("Aestheticify", "Convert text to Ａｅｓｔｈｅｔｉｃ text", typeof(string), typeof(string), new Argument[0], (SocketMessage m, object[] a, object o) => {
                return (o as string).Select(x => x == ' ' || x == '\n' ? x : (char)(x - '!' + '！')).Foldl("", (x, y) => x + y);
            }),
        };
    }
}
