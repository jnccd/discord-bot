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
            new ArgumentParseMethod(typeof(int), (string s) => Convert.ToInt32(s)),
            new ArgumentParseMethod(typeof(long), (string s) => Convert.ToInt64(s)),
            new ArgumentParseMethod(typeof(ulong), (string s) => Convert.ToUInt64(s)),
            new ArgumentParseMethod(typeof(float), (string s) => (float)s.ConvertToDouble()),
            new ArgumentParseMethod(typeof(double), (string s) => s.ConvertToDouble()),
            new ArgumentParseMethod(typeof(PointF), (string s) => new PointF((float)s.ConvertToDouble(), (float)s.ConvertToDouble())),
            new ArgumentParseMethod(typeof(Vector2), (string s) => new Vector2((float)s.ConvertToDouble(), (float)s.ConvertToDouble())),
        };
    }
}
