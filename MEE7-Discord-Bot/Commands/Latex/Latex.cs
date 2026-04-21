using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using Discord;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using SkiaSharp;

namespace MEE7.Commands;

public class Latex : Command
{
    readonly string inputPath = $"Commands{Path.DirectorySeparatorChar}Latex{Path.DirectorySeparatorChar}input.tex";
    readonly string batchPath = $"Commands{Path.DirectorySeparatorChar}Latex{Path.DirectorySeparatorChar}latex.bat";
    readonly string folderPath = $"Commands{Path.DirectorySeparatorChar}Latex";

    public Latex() : base("latex", "Renders latex strings", false)
    {

    }

    public override void Execute(IMessage message)
    {
        lock (this)
        {
            string latex = message.Content.Split(new char[] { ' ', '\n' }).Skip(1).Aggregate((x, y) => x + " " + y);

            if (!latex.StartsWith("\\documentclass"))
                latex = "\\documentclass[preview,border=12pt]{standalone}\n\\usepackage{amsmath}\n\\usepackage{tikz}\n\\begin{document}\n" + latex + "\\end{document}";

            if (!File.Exists(inputPath))
                File.Create(inputPath).Dispose();
            File.WriteAllText(inputPath, latex);

            batchPath.RunAsConsoleCommand(3, () => { }, (s, e) => { }, (StreamWriter s) =>
            {
                Thread.Sleep(500);
                s.WriteLine("return");
            });

            string[] outputFilePaths = Directory.GetFiles(folderPath).Where(x => Path.GetFileNameWithoutExtension(x).Contains("output") && x.EndsWith(".png")).ToArray();

            if (outputFilePaths.Length == 0)
            {
                DiscordNETWrapper.SendText("That didn't work.", message.Channel).Wait();
                return;
            }

            foreach (string outputPath in outputFilePaths)
            {
                using var latexOutput = SKBitmap.Decode(outputPath);
                // Create a new bitmap with the same dimensions as the original
                var output = new SKBitmap(latexOutput.Width, latexOutput.Height, SKColorType.Rgba8888, SKAlphaType.Premul);

                // Fill the new bitmap with a white background
                using (var canvas = new SKCanvas(output))
                {
                    canvas.Clear(SKColors.White);
                    canvas.DrawBitmap(latexOutput, 0, 0);
                }

                DiscordNETWrapper.SendBitmap(output, message.Channel).Wait();
                output.Dispose();
            }

            outputFilePaths.Select(x => { File.Delete(x); return x; }).ToArray();
        }

        return;
    }
}
