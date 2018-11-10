using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{

    public class GetScreenshot : Command
    {
        public GetScreenshot() : base("getScreenshot", "", true, true)
        {

        }

        public override async Task execute(SocketMessage message)
        {
            if (message.Author.Id == Global.Master.Id)
            {
                Rectangle AllScreenBounds = new Rectangle(-1360, 0, 1360 + 1600, 900);

                Bitmap bmp = new Bitmap(AllScreenBounds.Width, AllScreenBounds.Height, PixelFormat.Format32bppArgb);
                Graphics graphics = Graphics.FromImage(bmp);
                graphics.CopyFromScreen(AllScreenBounds.X, AllScreenBounds.Y, 0, 0, new Size(AllScreenBounds.Width, AllScreenBounds.Height), CopyPixelOperation.SourceCopy);

                bmp.Save(Global.CurrentExecutablePath + "\\screenshot.png");

                await Global.SendFile(Global.CurrentExecutablePath + "\\screenshot.png", message.Channel);
            }
        }
    }
}
