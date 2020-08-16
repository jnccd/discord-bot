using Discord;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using static MEE7.Commands.Edit.Edit;

namespace MEE7.Commands.Edit
{
    class FunctionalCommands : EditCommandProvider
    {
        public string vectorDesc = "Creates a new Vector object";
        public Vector2 Vector(EditNull n, IMessage m, double a, double b) => new Vector2((float)a, (float)b);

        public string ForFuncDesc = "foori foori";
        public a[] ForFunc<a>(a o, IMessage m, string pipe, string varName, float startValue, float endValue, float stepWidth)
        {
            List<a> results = new List<a>();

            for (float f = startValue; f < endValue; f += stepWidth)
                results.Add((a)Pipe.Parse(m, pipe.Replace("%" + varName, f.ToString().Replace(",", "."))).Apply(m, o));

            return results.ToArray();
        }

        public string MapDesc = "'tis map from haskel";
        public b[] Map<a, b>(a[] os, IMessage m, string pipe)
        {
            if (pipe.Contains("=>"))
            {
                string[] split = pipe.Split("=>").Select(x => x.Trim(' ')).ToArray();
                var parsedPipe = Pipe.Parse(m, split[1]);
                return os.Select(x => (b)parsedPipe.
                    Apply(m, x, new Dictionary<string, object>() { { split[0], x } })).ToArray();
            }
            else
            {
                var parsedPipe = Pipe.Parse(m, pipe);
                return os.Select(x => (b)parsedPipe.Apply(m, x)).ToArray();
            }
        }

        public string ErrorDesc = "Literally just crashes lul";
        public void Error(EditNull n, IMessage m)
        {
            throw new Exception("notlikethis");
        }
    }
}
