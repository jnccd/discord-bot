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

        public Place() : base("place", "Basically just r/place", false)
        {
            subCommands = new PlaceCommand[] {
            new PlaceCommand("print", "Prints the canvas without this annoying help message.",
                (string[] split, ISocketMessageChannel Channel) => { return true; },
                async (SocketMessage commandmessage, string filePath, string[] split) => { await Global.SendFile(filePath, commandmessage.Channel); }),
            new PlaceCommand("drawPixel", "Draws the specified color to the specified place(0 - " + (placeSize / pixelSize - 1) + ", 0 - " + (placeSize / pixelSize - 1) + 
            ")\neg. " + prefix + command + " drawPixel 10,45 Red",
                (string[] split, ISocketMessageChannel Channel) => {

                    int X, Y;
                    if (split.Length != 4)
                    {
                        Global.SendText("I need 3 arguments to draw!", Channel);
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

                    Global.SendFile(filePath, commandmessage.Channel, "Succsessfully drawn!");

                }),
            new PlaceCommand("drawCircle", "Draws a circle in some color, in the given size and in the given coordinates(0 - " + (placeSize - 1) + ", 0 - " + (placeSize - 1) + 
            ")\neg. " + prefix + command + " drawCircle 100,450 Red 25",
                (string[] split, ISocketMessageChannel Channel) => {

                    int X, Y, S;
                    if (split.Length != 5)
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

                    //if (X >= placeSize || Y >= placeSize)
                    //{
                    //    Global.SendText("The picture is only " + placeSize + "x" + placeSize + " big!\nTry smaller coordinates.", Channel);
                    //    return false;
                    //}

                    System.Drawing.Color brushColor = System.Drawing.Color.FromName(split[3]);

                    if (brushColor.R == 0 && brushColor.G == 0 && brushColor.B == 0 && split[3].ToLower() != "black")
                    {
                        Global.SendText("I dont think I know that color :thinking:", Channel);
                        return false;
                    }

                    try
                    {
                        S = Convert.ToInt32(split[4]);
                    }
                    catch
                    {
                        Global.SendText("I don't understand that size, fam!", Channel);
                        return false;
                    }

                    if (S > 100)
                    {
                        Global.SendText("Thats a little big, don't ya think?", Channel);
                        return false;
                    }

                    return true;

                },
                (SocketMessage commandmessage, string filePath, string[] split) => {

                    string[] temps = split[2].Split(',');
                    int X = Convert.ToInt32(temps[0]);
                    int Y = Convert.ToInt32(temps[1]);

                    int S = Convert.ToInt32(split[4]);

                    Bitmap temp;
                    System.Drawing.Color brushColor = System.Drawing.Color.FromName(split[3]);
                    using (FileStream stream = new FileStream(filePath, FileMode.Open))
                        temp = (Bitmap)Bitmap.FromStream(stream);

                    using (Graphics graphics = Graphics.FromImage(temp))
                    {
                        graphics.FillPie(new SolidBrush(brushColor), new Rectangle(X - S, Y - S, S * 2, S * 2), 0, 360);
                    }

                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                        temp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                    Global.SendFile(filePath, commandmessage.Channel, "Succsessfully drawn!");

                }),
            new PlaceCommand("drawRekt", "Draws a rectangle in some color and in the given coordinates(0 - " + (placeSize - 1) + ", 0 - " + (placeSize - 1) +
            ") and size\neg. " + prefix + command + " drawRekt 100,250 Red 200,100",
                (string[] split, ISocketMessageChannel Channel) => {

                    int X, Y, W, H;
                    if (split.Length < 5)
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

                    try
                    {
                        string[] temp = split[4].Split(',');
                        W = Convert.ToInt32(temp[0]);
                        H = Convert.ToInt32(temp[1]);
                    }
                    catch
                    {
                        Global.SendText("I don't understand that size, fam!", Channel);
                        return false;
                    }

                    if (W + H > 500)
                    {
                        Global.SendText("Thats a little big, don't ya think?", Channel);
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
                    temps = split[4].Split(',');
                    int W = Convert.ToInt32(temps[0]);
                    int H = Convert.ToInt32(temps[1]);

                    Bitmap temp;
                    System.Drawing.Color brushColor = System.Drawing.Color.FromName(split[3]);
                    using (FileStream stream = new FileStream(filePath, FileMode.Open))
                        temp = (Bitmap)Bitmap.FromStream(stream);

                    using (Graphics graphics = Graphics.FromImage(temp))
                    {
                        graphics.FillRectangle(new SolidBrush(brushColor), new Rectangle(X, Y, W, H));
                    }

                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                        temp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                    Global.SendFile(filePath, commandmessage.Channel, "Succsessfully drawn!");

                }),
            new PlaceCommand("drawString", "Draws a string in some color and in the given coordinates(0 - " + (placeSize - 1) + ", 0 - " + (placeSize - 1) +
            ")\neg. " + prefix + command + " drawString 100,250 Red OwO what dis",
                (string[] split, ISocketMessageChannel Channel) => {

                    int X, Y, S;
                    if (split.Length < 5)
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

                    //if (X >= placeSize || Y >= placeSize)
                    //{
                    //    Global.SendText("The picture is only " + placeSize + "x" + placeSize + " big!\nTry smaller coordinates.", Channel);
                    //    return false;
                    //}

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
                        graphics.DrawString(string.Join(" ", split.Skip(4).ToArray()), new Font("Comic Sans", 16), new SolidBrush(brushColor), new PointF(X, Y) );
                    }

                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                        temp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                    Global.SendFile(filePath, commandmessage.Channel, "Succsessfully drawn!");

                }) };
        }

        class PlaceCommand
        {
            public delegate bool ConditionCheck(string[] split, ISocketMessageChannel Channel);
            public delegate void Execution(SocketMessage commandmessage, string filePath, string[] split);
            public string command, desc;
            public ConditionCheck check;
            public Execution execute;

            public PlaceCommand(string command, string desc, ConditionCheck check, Execution execute)
            {
                this.command = command;
                this.desc = desc;
                this.check = check;
                this.execute = execute;
            }
        }

        public override async Task execute(SocketMessage message)
        {
            string[] split = message.Content.Split(new char[] { ' ', '\n' });
            if (split.Length == 1)
            {
                EmbedBuilder Embed = new EmbedBuilder();
                Embed.WithColor(0, 128, 255);
                foreach (PlaceCommand Pcommand in subCommands)
                {
                    Embed.AddField(prefix + command + " " + Pcommand.command, Pcommand.desc);
                }
                Embed.WithDescription("Place Commands:");
                await Global.SendEmbed(Embed, message.Channel);

                await Global.SendFile(filePath, message.Channel);
            }
            else
            {
                foreach (PlaceCommand Pcommand in subCommands)
                {
                    if (split[1] == Pcommand.command && Pcommand.check(split, message.Channel))
                    {
                        Pcommand.execute(message, filePath, split);
                        break;
                    }
                }
            }
        }
    }
}
