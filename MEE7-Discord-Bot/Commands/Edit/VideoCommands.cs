using Discord;
using MEE7.Backend.HelperFunctions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;

namespace MEE7.Commands.Edit
{
    class VideoCommands : EditCommandProvider
    {
        public object firstOrderModelLock = new object();
        public string BakaMitaiDesc = "Picture goes Baka Mitai - using code from https://github.com/AliaksandrSiarohin/first-order-model - needs vram";
        public void BakaMitai(Bitmap bmp, IMessage m)
        {
            if (!Directory.Exists("Commands\\Edit\\first-order-model"))
                throw new Exception("AI model is not set up on this bot instance");

            lock (firstOrderModelLock)
            {
                string finalFile = "Commands\\Edit\\first-order-model\\content\\final.mp4";
                string picFile = "Commands\\Edit\\first-order-model\\content\\gdrive\\My Drive\\first-order-motion-model\\02.png";

                Bitmap resized = new Bitmap(bmp, new Size(256, 256));

                if (File.Exists(picFile))
                    File.Delete(picFile);

                resized.Save(picFile);

                if (File.Exists(finalFile))
                    File.Delete(finalFile);

                Process runner = Process.Start("python.exe",
                    "Commands\\Edit\\first-order-model\\executer.py");

                bmp.Dispose();
                resized.Dispose();

                runner.WaitForExit();

                if (!File.Exists(finalFile))
                    throw new Exception("Couldn't get enough VRAM to run the model D:");
                else
                    DiscordNETWrapper.SendFile(finalFile, m.Channel).Wait();
            }
        }
    }
}
