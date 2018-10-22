using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class Place : Command
    {
        string filePath = Global.CurrentExecutablePath + "\\place.png";
        const int placeSize = 500;
        const int pixelSize = 10;
        PlaceCommand[] subCommands;

        public Place() : base("place", "An idea that I shamelessly stole from r/place.", false)
        {
            subCommands = new PlaceCommand[] {
            new PlaceCommand("print", "Prints the canvas without this annoying help message.",
                (string[] split, ISocketMessageChannel Channel) => { return true; },
                async (SocketMessage commandmessage, string filePath, string[] split) => { await Global.SendFile(filePath, commandmessage.Channel); }),
            new PlaceCommand("draw", "Draws the specified color to the specified place\neg. " + prefix + command + " draw 10,45 Red",
                (string[] split, ISocketMessageChannel Channel) => {

                    int X, Y;
                    if (split.Length != 4)
                    {
                        Global.SendText("I need 4 arguments to draw!", Channel);
                        return false;
                    }

                    try
                    {
                        string[] temp = split[2].Split(',');
                        X = Convert.ToInt32(temp[0]);
                        Y = Convert.ToInt32(temp[1]);
                    }
                    catch
                    {
                        Global.SendText("I don't understand those coordinates, fam!", Channel);
                        return false;
                    }

                    if (X >= (placeSize / pixelSize) || Y >= (placeSize / pixelSize))
                    {
                        Global.SendText("The picture is only " + (placeSize / pixelSize) + "x" + (placeSize / pixelSize) + " big!\nTry smaller coordinates.", Channel);
                        return false;
                    }

                    System.Drawing.Color brushColor = System.Drawing.Color.FromName(split[3]);

                    if (brushColor.R == 0 && brushColor.G == 0 && brushColor.B == 0 && split[3].ToLower() != "black")
                    {
                        Global.SendText("I dont think I know that color :thinking:", Channel);
                        return false;
                    }

                    return true;

                },
                (SocketMessage commandmessage, string filePath, string[] split) => {

                    string[] temps = split[2].Split(',');
                    int X = Convert.ToInt32(temps[0]);
                    int Y = Convert.ToInt32(temps[1]);

                    Bitmap temp;
                    System.Drawing.Color brushColor = System.Drawing.Color.FromName(split[3]);
                    using (FileStream stream = new FileStream(filePath, FileMode.Open))
                        temp = (Bitmap)Bitmap.FromStream(stream);

                    using (Graphics graphics = Graphics.FromImage(temp))
                    {
                        graphics.FillRectangle(new SolidBrush(brushColor), new Rectangle(X * pixelSize, Y * pixelSize, pixelSize, pixelSize));
                    }

                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                        temp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                    Global.SendFile(filePath, "Succsessfully drawn!", commandmessage.Channel);

                }) };
        }

        class PlaceCommand
        {
            public delegate bool conditionCheck(string[] split, ISocketMessageChannel Channel);
            public delegate void Execution(SocketMessage commandmessage, string filePath, string[] split);
            public string command, desc;
            public conditionCheck check;
            public Execution execute;

            public PlaceCommand(string command, string desc, conditionCheck check, Execution execute)
            {
                this.command = command;
                this.desc = desc;
                this.check = check;
                this.execute = execute;
            }
        }

        public override async Task execute(SocketMessage commandmessage)
        {
            string[] split = commandmessage.Content.Split(' ');
            if (split.Length == 1)
            {
                EmbedBuilder Embed = new EmbedBuilder();
                Embed.WithColor(0, 128, 255);
                foreach (PlaceCommand Pcommand in subCommands)
                {
                    Embed.AddField(prefix + command + " " + Pcommand.command, Pcommand.desc);
                }
                Embed.WithDescription("Place Commands:");
                await Global.SendEmbed(Embed, commandmessage.Channel);

                await Global.SendFile(filePath, commandmessage.Channel);
            }
            else
            {
                foreach (PlaceCommand Pcommand in subCommands)
                {
                    if (split[1] == Pcommand.command && Pcommand.check(split, commandmessage.Channel))
                    {
                        Pcommand.execute(commandmessage, filePath, split);
                        break;
                    }
                }
            }
        }
    }
}
