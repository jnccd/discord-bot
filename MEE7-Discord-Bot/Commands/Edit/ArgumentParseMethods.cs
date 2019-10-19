using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace MEE7.Commands
{
    public partial class Edit : Command
    {
        static readonly ArgumentParseMethod[] ArgumentParseMethods = new ArgumentParseMethod[]
        {
            new ArgumentParseMethod(typeof(string), (string s) => s),
            new ArgumentParseMethod(typeof(bool), (string s) => { if (s == "true") return true; else return false; }),
            new ArgumentParseMethod(typeof(int), (string s) => (int)s.ConvertToDouble()),
            new ArgumentParseMethod(typeof(long), (string s) => (long)s.ConvertToDouble()),
            new ArgumentParseMethod(typeof(ulong), (string s) => Convert.ToUInt64(s)),
            new ArgumentParseMethod(typeof(float), (string s) => (float)s.ConvertToDouble()),
            new ArgumentParseMethod(typeof(double), (string s) => s.ConvertToDouble()),
            new ArgumentParseMethod(typeof(PointF), (string s) => {
                string[] sp = s.Split(':');
                return new PointF((float)sp[0].ConvertToDouble(), (float)sp[1].ConvertToDouble());
            }),
            new ArgumentParseMethod(typeof(Vector2), (string s) => {
                string[] sp = s.Split(':');
                return new Vector2((float)sp[0].ConvertToDouble(), (float)sp[1].ConvertToDouble());
            }),
        };
    }
}
