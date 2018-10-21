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
        bool working = false;
        int placeSize = 500;
        int pixelSize = 10;
        Bitmap temp;
        System.Drawing.Color brushColor;

        public Place() : base("place", "An idea that I shamelessly stole from r/place.", false)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            while (working) { Thread.Sleep(500); }

            working = true;

            string[] split = commandmessage.Content.Split(' ');
            if (split.Length == 1)
            {
                EmbedBuilder Embed = new EmbedBuilder();
                Embed.WithColor(0, 128, 255);
                Embed.AddField(prefix + command + " print", "Prints the canvas without this annoying help message.");
                Embed.AddField(prefix + command + " draw + coordinates(0 - " + (placeSize / 10) + ", 0 - " + (placeSize / 10) + ") + color", 
                    "Draws the specified color the the specified place\neg. " + prefix + command + " draw 10,45 Red");
                Embed.WithDescription("Place Commands:");
                await Global.SendEmbed(Embed, commandmessage.Channel);

                await Global.SendFile(filePath, commandmessage.Channel);
            }
            else if (split[1] == "print")
            {
                await Global.SendFile(filePath, commandmessage.Channel);
            }
            else if (split[1] == "draw")
            {
                int X, Y;

                if (split.Length != 4)
                {
                    await Global.SendText("I need 4 arguments to draw!", commandmessage.Channel);
                    return;
                }

                try
                {
                    string[] temp = split[2].Split(',');
                    X = Convert.ToInt32(temp[0]);
                    Y = Convert.ToInt32(temp[1]);
                }
                catch
                {
                    await Global.SendText("I don't understand those coordinates, fam!", commandmessage.Channel);
                    return;
                }

                if (X >= (placeSize / pixelSize) || Y >= (placeSize / pixelSize))
                {
                    await Global.SendText("The picture is only " + (placeSize / pixelSize) + "x" + (placeSize / pixelSize) + " big!\nTry smaller coordinates.", commandmessage.Channel);
                    return;
                }

                brushColor = System.Drawing.Color.FromName(split[3]);

                if (brushColor.R == 0 && brushColor.G == 0 && brushColor.B == 0 && split[3].ToLower() != "black")
                {
                    await Global.SendText("I dont think I know that color :thinking:", commandmessage.Channel);
                    return;
                }
                
                using (FileStream stream = new FileStream(filePath, FileMode.Open))
                    temp = (Bitmap)Bitmap.FromStream(stream);

                using (Graphics graphics = Graphics.FromImage(temp))
                {
                    graphics.FillRectangle(new SolidBrush(brushColor), new Rectangle(X * pixelSize, Y * pixelSize, pixelSize, pixelSize));
                }

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    temp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                
                await Global.SendFile(filePath, "Succsessfully drawn!", commandmessage.Channel);
            }

            working = false;
        }
    }
}
