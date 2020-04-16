using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System;
using System.Drawing;
using System.Numerics;

namespace MEE7.Commands
{
    public partial class Edit : Command
    {
        static readonly ArgumentParseMethod[] ArgumentParseMethods = new ArgumentParseMethod[]
        {
            new ArgumentParseMethod(typeof(string), (SocketMessage m, string s) => s),
            new ArgumentParseMethod(typeof(bool), (SocketMessage m, string s) => { if (s.ToLower() == "true" || s == "1") return true; else return false; }),
            new ArgumentParseMethod(typeof(int), (SocketMessage m, string s) => (int)s.ConvertToDouble()),
            new ArgumentParseMethod(typeof(long), (SocketMessage m, string s) => (long)s.ConvertToDouble()),
            new ArgumentParseMethod(typeof(ulong), (SocketMessage m, string s) => Convert.ToUInt64(s)),
            new ArgumentParseMethod(typeof(float), (SocketMessage m, string s) => (float)s.ConvertToDouble()),
            new ArgumentParseMethod(typeof(double), (SocketMessage m, string s) => s.ConvertToDouble()),
            new ArgumentParseMethod(typeof(PointF), (SocketMessage m, string s) => {
                string[] sp = s.Split(':');
                return new PointF((float)sp[0].ConvertToDouble(), (float)sp[1].ConvertToDouble());
            }),
            new ArgumentParseMethod(typeof(Vector2), (SocketMessage m, string s) => {
                string[] sp = s.Split(':');
                return new Vector2((float)sp[0].ConvertToDouble(), (float)sp[1].ConvertToDouble());
            }),
            new ArgumentParseMethod(typeof(Pipe), (SocketMessage m, string s) => Pipe.Parse(m, s)),
        };
    }
}
