using System;
using System.Collections.Generic;
using System.Text;

namespace MEE7.Backend.HelperFunctions
{
    public static class ConsoleWrapper
    {
        public static void ConsoleWriteLine(object text, ConsoleColor Color)
        {
            lock (Console.Title)
            {
                if (Console.CursorLeft == 1)
                    Console.CursorLeft = 0;
                Console.ForegroundColor = Color;
                Console.WriteLine(text);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("$");
            }
        }
        public static void ConsoleWriteLine(object text)
        {
            lock (Console.Title)
            {
                if (Console.CursorLeft == 1)
                    Console.CursorLeft = 0;
                Console.WriteLine(text);
                Console.Write("$");
            }
        }
        public static void ConsoleWrite(object text, ConsoleColor Color)
        {
            lock (Console.Title)
            {
                if (Console.CursorLeft == 1)
                    Console.CursorLeft = 0;
                Console.ForegroundColor = Color;
                Console.Write(text);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        public static void ConsoleWrite(object text)
        {
            lock (Console.Title)
            {
                if (Console.CursorLeft == 1)
                    Console.CursorLeft = 0;
                Console.Write(text);
            }
        }
    }
}
