using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Starter
{
    class Program
    {
        static void Main(string[] args)
        {
            while (!Directory.GetCurrentDirectory().EndsWith("MEE7 Discord-Bot"))
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Directory.GetCurrentDirectory()));
            Directory.SetCurrentDirectory(Directory.GetCurrentDirectory() + "\\MEE7 Discord-Bot");

#if DEBUG
            string runConfig = "Debug";
#else
            string runConfig = "Release";
#endif

            Process.Start(new ProcessStartInfo() { FileName = "dotnet", Arguments = "run -c " + runConfig, UseShellExecute = false, RedirectStandardInput = false });
        }
    }
}
