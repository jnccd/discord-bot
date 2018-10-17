using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Helper_Classes
{
    public static class RuntimeCompile
    {
        public static void writeLine(string lineToWrite)
        {
            string code = @"
    using System;
    using TestDiscordBot;
    using TestDiscordBot.Base;
    using TestDiscordBot.Commands;
    using TestDiscordBot.Helper_Classes;

    namespace First
    {
        public class Program
        {
            public static void Main()
            {
                Console.WriteLine(" + lineToWrite + @");
            }
        }
    }
";

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerResults results = provider.CompileAssemblyFromSource(new CompilerParameters(), code);

            if (results.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();

                foreach (CompilerError error in results.Errors)
                {
                    sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                }

                throw new InvalidOperationException(sb.ToString());
            }

            Assembly assembly = results.CompiledAssembly;
            Type program = assembly.GetType("First.Program");
            MethodInfo main = program.GetMethod("Main");
            main.Invoke(null, null);
        }
    }
}
