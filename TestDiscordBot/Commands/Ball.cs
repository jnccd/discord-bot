using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class Ball : Command
    {
        string[] answers = new string[] { "NO!", "YES!", "No", "Yes", "Maybe", "Ask my wife", "Ask 8ball",  };

        public Ball() : base("9ball", false)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            try
            {
                string m = commandmessage.Content;
                long sum = 0;
                for (int i = 0; i < m.Length; i++)
                    sum += m.ToCharArray()[i] << i;

                int answerIndex = (int)(sum % answers.Length);
                await Global.SendText(answers[answerIndex], commandmessage.Channel);

                Console.CursorLeft = 0;
                Console.WriteLine("Send 9ball in " + commandmessage.Channel.Name + " for " + commandmessage.Author.Username);
                Console.Write("$");
            }
            catch (Exception e)
            {
                await Global.SendText("Uwu We made a fucky wucky!! A wittle fucko boingo! The code monkeys at our headquarters are working VEWY HAWD to fix this!", commandmessage.Channel);

                Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("$");
            }
        }
    }
}
