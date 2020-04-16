using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System.Drawing;
using System.Drawing.Imaging;

namespace MEE7.Commands
{

    public class GetScreenshot : Command
    {
        public GetScreenshot() : base("getScreenshot", "", true, true)
        {

        }

        public override void Execute(SocketMessage message)
        {
            if (message.Author.Id == Program.Master.Id)
            {
                Rectangle AllScreenBounds = new Rectangle(-1360, 0, 1360 + 1600, 900);

                using (Bitmap bmp = new Bitmap(AllScreenBounds.Width, AllScreenBounds.Height, PixelFormat.Format32bppArgb))
                {
                    using (Graphics graphics = Graphics.FromImage(bmp))
                        graphics.CopyFromScreen(AllScreenBounds.X, AllScreenBounds.Y, 0, 0, new Size(AllScreenBounds.Width, AllScreenBounds.Height), CopyPixelOperation.SourceCopy);

                    DiscordNETWrapper.SendBitmap(bmp, message.Channel).Wait();
                }
            }
        }
    }
}
