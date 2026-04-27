using System;

namespace MEE7.Backend.HelperFunctions
{
    public static class ConsoleWrapper
    {
        public static readonly object lockject = new object();

        public static void WriteLine(object text, ConsoleColor Color)
        {
            Helper.TimedLock(lockject, 1000, () =>
            {
                if (Program.RunningOnCI)
                    Console.WriteLine(text);
                else if (Program.RunningOnLinux)
                {
                    Console.ForegroundColor = Color;
                    Console.WriteLine(text);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    try
                    {
                        if (Console.CursorLeft == 1)
                            Console.CursorLeft = 0;
                    }
                    catch { }
                    Console.ForegroundColor = Color;
                    Console.WriteLine(text);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("$");
                }
            });
        }
        public static void WriteLine(object text)
        {
            Helper.TimedLock(lockject, 1000, () =>
            {
                if (Program.RunningOnCI || Program.RunningOnLinux)
                    Console.WriteLine(text);
                else
                {
                    try
                    {
                        if (Console.CursorLeft == 1)
                            Console.CursorLeft = 0;
                    }
                    catch { }
                    Console.WriteLine(text);
                    Console.Write("$");
                }
            });
        }
        public static void WriteLineAndDiscordLog(object text)
        {
            WriteLine(text);

            LogToDiscordIfEnabled(text);
        }
        public static void WriteLineAndDiscordLog(object text, ConsoleColor Color)
        {
            WriteLine(text, Color);

            LogToDiscordIfEnabled(text);
        }
        public static void Write(object text, ConsoleColor Color)
        {
            Helper.TimedLock(lockject, 1000, () =>
            {
                if (Program.RunningOnCI)
                    Console.Write(text);
                else if (Program.RunningOnLinux)
                {
                    Console.ForegroundColor = Color;
                    Console.Write(text);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    if (Console.CursorLeft == 1)
                        Console.CursorLeft = 0;
                    Console.ForegroundColor = Color;
                    Console.Write(text);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            });
        }
        public static void Write(object text)
        {
            Helper.TimedLock(lockject, 1000, () =>
            {
                if (Program.RunningOnCI || Program.RunningOnLinux)
                    Console.Write(text);
                else
                {
                    if (Console.CursorLeft == 1)
                        Console.CursorLeft = 0;
                    Console.Write(text);
                }
            });
        }

        public static void LogToDiscordIfEnabled(object msg)
        {
            if (Program.logToDiscord)
                try
                {
                    _ = DiscordNETWrapper.SendText(msg.ToString() ?? "Null Message", Program.logChannel);
                }
                catch { }
        }
    }
}
