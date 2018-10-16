using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class ProfilePicture : Command
    {
        public ProfilePicture() : base("profilePicture", "Gets the profile picture of a mentioned target or yourself if you dont mention anyone", false)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            try
            {
                SocketUser Target;
                if (commandmessage.MentionedUsers.Count > 0)
                    Target = commandmessage.MentionedUsers.ElementAt(0);
                else
                    Target = commandmessage.Author;

                ushort size = 128;
                try
                {
                    size = Convert.ToUInt16(commandmessage.Content.Split(' ')[1]);
                }
                catch { }

                await Global.SendText(Target.GetAvatarUrl(Discord.ImageFormat.Auto, size), commandmessage.Channel);

                Console.CursorLeft = 0;
                Console.WriteLine("Profile picture in " + commandmessage.Channel.Name + " for " + commandmessage.Author.Username);
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
