using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestDiscordBot;

namespace TestDiscordBot.Commands
{
    public class Brainfuck : Command
    {
        public Brainfuck() : base("bf", "Brainfuck Interpreter", false)
        {

        }

        public override async Task Execute(SocketMessage message)
        {
            await Global.SendText("kek", message.Channel);
        }
    }
}
