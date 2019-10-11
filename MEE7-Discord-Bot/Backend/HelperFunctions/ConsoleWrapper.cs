using System;
using System.Collections.Generic;
using System.Text;

namespace MEE7.Backend.HelperFunctions
{
    public static class ConsoleWrapper
    {
        private static readonly object lockject = new object();

        public static void ConsoleWriteLine(object text, ConsoleColor Color)
        {
            lock (lockject)
            {
                if (Program.RunningOnCI)
                    Console.WriteLine(text);
                else
                {
                    if (Console.CursorLeft == 1)
                        Console.CursorLeft = 0;
                    Console.ForegroundColor = Color;
                    Console.WriteLine(text);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("$");
                }
            }
        }
        public static void ConsoleWriteLine(object text)
        {
            lock (lockject)
            {
                if (Program.RunningOnCI)
                    Console.WriteLine(text);
                else
                {
                    if (Console.CursorLeft == 1)
                        Console.CursorLeft = 0;
                    Console.WriteLine(text);
                    Console.Write("$");
                }
            }
        }
        public static void ConsoleWrite(object text, ConsoleColor Color)
        {
            lock (lockject)
            {
                if (Program.RunningOnCI)
                    Console.Write(text);
                else
                {

                }
                if (Console.CursorLeft == 1)
                    Console.CursorLeft = 0;
                Console.ForegroundColor = Color;
                Console.Write(text);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        public static void ConsoleWrite(object text)
        {
            lock (lockject)
            {
                if (Program.RunningOnCI)
                    Console.Write(text);
                else
                {
                    if (Console.CursorLeft == 1)
                        Console.CursorLeft = 0;
                    Console.Write(text);
                }
            }
        }
    }
}
