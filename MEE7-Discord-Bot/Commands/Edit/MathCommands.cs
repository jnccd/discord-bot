using Discord;
using System;
using static MEE7.Commands.Edit.Edit;

namespace MEE7.Commands.Edit
{
    class MathCommands : EditCommandProvider
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
    }
}
