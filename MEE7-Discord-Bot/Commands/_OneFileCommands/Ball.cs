using Discord.WebSocket;
using MEE7.Backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEE7.Commands
{
    public class Ball : Command
    {
        string[] answers = new string[] { "NO!", "YES!", "No", "Yes", "Maybe", "Ask my wife", "Ask 8ball", "Uhm... I have no idea", "Possibly", "No u" };

        public Ball() : base("9ball", "Decides your fate", false)
        {

        }

        public override void Execute(SocketMessage commandmessage)
        {
            string m = commandmessage.Content.ToLower();
            if (m.Split(' ').Length >= 4 && m.Contains("?"))
            {
                if (!m.Contains("why ") && !m.Contains("what ") && !m.Contains("who ") && 
                    !m.Contains("warum ") && !m.Contains("was ") && !m.Contains("wieso ") && !m.Contains("weshalb "))
                {
                    long sum = 0;
                    for (int i = 0; i < m.Length; i++)
                        sum += m.ToCharArray()[i] << i;

                    int answerIndex = Math.Abs((int)(sum % answers.Length));
                    DiscordNETWrapper.SendText("9ball says: " + answers[answerIndex], commandmessage.Channel).Wait();
                }
                else
                    DiscordNETWrapper.SendText("I can only answer yes no questions!", commandmessage.Channel).Wait();
            }
            else
                DiscordNETWrapper.SendText("Thats not a question!", commandmessage.Channel).Wait();
        }
    }
}
