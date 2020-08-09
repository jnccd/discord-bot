using Discord;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using static MEE7.Commands.Edit.Edit;

namespace MEE7.Commands.Edit
{
    class FunctionalEditCommands : EditCommandProvider
    {
        public string AddDesc = "Addition";
        public double Add(EditNull n, IMessage m, double a, double b) => a + b;

        public string SubDesc = "Subtraction";
        public double Sub(EditNull n, IMessage m, double a, double b) => a - b;

        public string MultDesc = "Multiplication";
        public double Mult(EditNull n, IMessage m, double a, double b) => a * b;

        public string DivDesc = "Dividing";
        public double Div(EditNull n, IMessage m, double a, double b) => a / b;

        public string PowDesc = "Powification";
        public double Pow(EditNull n, IMessage m, double a, double b) => Math.Pow(a, b);

        public string SqrtDesc = "Sqrtification";
        public double Sqrt(EditNull n, IMessage m, double a) => Math.Sqrt(a);

        public string LogDesc = "Logification";
        public double Log(EditNull n, IMessage m, double a, double b) => Math.Log(a, b);

        public string SinDesc = "Sinification";
        public double Sin(EditNull n, IMessage m, double a) => Math.Sin(a);

        public string CosDesc = "Cosification";
        public double Cos(EditNull n, IMessage m, double a) => Math.Cos(a);

        public string PiDesc = "Pi";
        public double Pi(EditNull n, IMessage m) => Math.PI;

        public string EDesc = "E";
        public double E(EditNull n, IMessage m) => Math.E;


        public string GtDesc = "Greater than";
        public bool Gt(EditNull n, IMessage m, double a, double b) => a > b;

        public string LtDesc = "Lower than";
        public bool Lt(EditNull n, IMessage m, double a, double b) => a < b;

        public string GeqDesc = "Greater than or equal";
        public bool Geq(EditNull n, IMessage m, double a, double b) => a >= b;

        public string LeqDesc = "Lower than or equal";
        public bool Leq(EditNull n, IMessage m, double a, double b) => a <= b;

        public string EqDesc = "Equal";
        public bool Eq(EditNull n, IMessage m, double a, double b) => a == b;

        public string AndDesc = "And";
        public bool And(EditNull n, IMessage m, bool a, bool b) => a & b;

        public string OrDesc = "And";
        public bool Or(EditNull n, IMessage m, bool a, bool b) => a | b;


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

        public string MapGDesc = "Map for gifs, because gifs are special now";
        public Gif MapG(Gif gif, IMessage m, string pipe, string varName = "i", float startValue = 0, float endValue = int.MinValue)
        {
            if (endValue == int.MinValue) endValue = gif.Item1.Length;
            for (int i = 0; i < gif.Item1.Length; i++)
                gif.Item1[i] = (Bitmap)Pipe.Parse(m, pipe.Replace("%" + varName, ((i / (float)gif.Item1.Length) * endValue).ToString().Replace(",", "."))).Apply(m, gif.Item1[i]);
            return gif;
        }

        public string ErrorDesc = "Literally just crashes";
        public void Error(EditNull n, IMessage m)
        {
            throw new Exception("notlikethis");
        }
    }
}
