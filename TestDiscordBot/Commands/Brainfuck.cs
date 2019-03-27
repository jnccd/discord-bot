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

        public class Cell
        {
            public Cell LeftCell;
            public Cell RightCell;
            public int Number;
        }

        public override async Task Execute(SocketMessage message)
        {
            int pc = 0;
            int steps = 0;
            Cell cell = new Cell();
            string output = "";
            
            while (pc < message.Content.Length)
            {
                switch (message.Content[pc])
                {
                    case '>':
                        if (cell.RightCell == null)
                            cell.RightCell = new Cell() { LeftCell = cell };
                        cell = cell.RightCell;
                        break;

                    case '<':
                        if (cell.LeftCell == null)
                            cell.LeftCell = new Cell() { RightCell = cell };
                        cell = cell.LeftCell;
                        break;

                    case '+':
                        cell.Number++;
                        break;

                    case '-':
                        cell.Number--;
                        break;

                    case '/':
                        cell.Number *= 2;
                        break;

                    case '.':
                        output += (char)cell.Number;
                        break;

                    case '[':
                        if (cell.Number == 0)
                        {
                            int height = 0;
                            while (height >= 0)
                            {
                                pc++;
                                if (pc >= message.Content.Length)
                                {
                                    await Program.SendText("Check the [] Brackets!", message.Channel);
                                    return;
                                }
                                if (message.Content[pc] == '[')
                                    height++;
                                if (message.Content[pc] == ']')
                                    height--;
                            }
                        }
                        break;

                    case ']':
                        if (cell.Number != 0)
                        {
                            int height = 0;
                            while (height >= 0)
                            {
                                pc--;
                                if (pc < 0)
                                {
                                    await Program.SendText("Check the [] Brackets!", message.Channel);
                                    return;
                                }
                                if (message.Content[pc] == ']')
                                    height++;
                                if (message.Content[pc] == '[')
                                    height--;
                            }
                        }
                        break;
                }

                if (steps > 5000)
                {
                    await Program.SendText("The execution eceeded the instruction limit!\nThe output so far was:\n" + output, message.Channel);
                    return;
                }

                pc++;
                steps++;
            }

            await Program.SendText($"```ruby\n {output}```", message.Channel);
        }
    }
}
