using Discord;
using MEE7.Backend.HelperFunctions;
using System;
using System.Drawing;
using System.Numerics;
using static MEE7.Commands.Edit.Edit;

namespace MEE7.Commands.Edit
{
    public class ArgumentParseMethod
    {
        public Type Type;
        public Func<IMessage, string, object> Function;

        public ArgumentParseMethod(Type Type, Func<IMessage, string, object> Function)
        {
            this.Function = Function;
            this.Type = Type;
        }

        public static readonly ArgumentParseMethod[] ArgumentParseMethods = new ArgumentParseMethod[]
        {
            new ArgumentParseMethod(typeof(byte), (IMessage m, string s) => Convert.ToByte(s)),
            new ArgumentParseMethod(typeof(short), (IMessage m, string s) => Convert.ToInt16(s)),
            new ArgumentParseMethod(typeof(int), (IMessage m, string s) => Convert.ToInt32(s)),
            new ArgumentParseMethod(typeof(long), (IMessage m, string s) => Convert.ToInt64(s)),
            new ArgumentParseMethod(typeof(ulong), (IMessage m, string s) => Convert.ToUInt64(s)),
            new ArgumentParseMethod(typeof(float), (IMessage m, string s) => (float)s.ConvertToDouble()),
            new ArgumentParseMethod(typeof(double), (IMessage m, string s) => s.ConvertToDouble()),
            new ArgumentParseMethod(typeof(string), (IMessage m, string s) => s),
            new ArgumentParseMethod(typeof(bool), (IMessage m, string s) => {
                if (s.ToLower() == "true" || s == "1")
                    return true;
                else if (s.ToLower() == "false" || s == "0")
                    return false;
                else
                    throw new Exception("no");
            }),
            new ArgumentParseMethod(typeof(PointF), (IMessage m, string s) => {
                string[] sp = s.Split(':');
                return new PointF((float)sp[0].ConvertToDouble(), (float)sp[1].ConvertToDouble());
            }),
            new ArgumentParseMethod(typeof(Vector2), (IMessage m, string s) => {
                string[] sp = s.Split(':');
                return new Vector2((float)sp[0].ConvertToDouble(), (float)sp[1].ConvertToDouble());
            }),
             new ArgumentParseMethod(typeof(IUser), (IMessage m, string s) => {
                return DiscordNETWrapper.ParseUser(s, m.Channel);
            }),
            new ArgumentParseMethod(typeof(Pipe), (IMessage m, string s) => Pipe.Parse(m, s)),
        };
    }
}
