using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using static MEE7.Commands.Edit;

namespace MEE7.Commands
{
    class FunctionalEditCommands : EditCommandProvider
    {
        public string AddDesc = "Addition";
        public double Add(Null n, SocketMessage m, double a, double b) => a + b;

        public string MultDesc = "Multiplication";
        public double Mult(Null n, SocketMessage m, double a, double b) => a * b;

        public string PowDesc = "Powification";
        public double Pow(Null n, SocketMessage m, double a, double b) => Math.Pow(a, b);

        public string SqrtDesc = "Sqrtification";
        public double Sqrt(Null n, SocketMessage m, double a) => Math.Sqrt(a);

        public string LogDesc = "Logification";
        public double Log(Null n, SocketMessage m, double a, double b) => Math.Log(a, b);

        public string SinDesc = "Sinification";
        public double Sin(Null n, SocketMessage m, double a) => Math.Sin(a);

        public string CosDesc = "Cosification";
        public double Cos(Null n, SocketMessage m, double a) => Math.Cos(a);

        public string PiDesc = "Pi";
        public double Pi(Null n, SocketMessage m) => Math.PI;

        public string EDesc = "E";
        public double E(Null n, SocketMessage m) => Math.E;

        public string newVectorDesc = "Creates a new Vector object";
        public Vector2 NewVector(Null n, SocketMessage m, double a, double b) => new Vector2((float)a, (float)b);

        public string ForFuncDesc = "foori foori";
        public a[] ForFunc<a>(a o, SocketMessage m, string pipe, string varName, float startValue, float endValue, float stepWidth)
        {
            List<a> results = new List<a>();

            for (float f = startValue; f < endValue; f += stepWidth)
                results.Add((a)Pipe.Parse(m, pipe.Replace("%" + varName, f.ToString().Replace(",", "."))).Apply(m, o));

            return results.ToArray();
        }

        public string MapDesc = "'tis map from haskel, your favorite language";
        public a[] Map<a>(a[] os, SocketMessage m, string pipe)
        {
            var parsedPipe = Pipe.Parse(m, pipe);
            return os.Select(x => (a)parsedPipe.Apply(m, x)).ToArray();
        }

        public string MapGDesc = "Map for gifs, because gifs are special now";
        public Gif MapG<a>(Gif gif, SocketMessage m, string pipe, string varName = "i", float startValue = 0, float endValue = int.MinValue)
        {
            if (endValue == int.MinValue) endValue = gif.Item1.Length;
            for (int i = 0; i < gif.Item1.Length; i++)
                gif.Item1[i] = (Bitmap)Pipe.Parse(m, pipe.Replace("%" + varName, ((i / (float)gif.Item1.Length) * endValue).ToString().Replace(",", "."))).Apply(m, gif.Item1[i]);
            return gif;
        }
    }
}
