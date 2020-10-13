using Discord;
using MEE7.Backend.HelperFunctions;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using static MEE7.Commands.Edit.Edit;
using Image = System.Drawing.Image;

namespace MEE7.Commands.Edit
{
    class VideoCommands : EditCommandProvider
    {
        public string GetVideoDesc = "Gets video from link";
        public Video GetVideo(string videoLink, IMessage m)
        {
            if (m.Author.Id != Program.Master.Id)
                throw new Exception("u r not allowed");

            if (!videoLink.Contains("mp4"))
                throw new Exception("Ew, give me a mp4 pls");

            string path = $"Commands{s}Edit{s}Workspace{s}video.mp4";

            lock (workspaceLock)
            {
                WebClient client = new WebClient();
                client.DownloadFile(videoLink, path);
                return new Video(path);
            }
        }

        public string GetVideoFromYTDesc = "Gets video from yt link";
        public Video GetVideoFromYT(string videoLink, IMessage m, string videoLink2)
        {
            if (m.Author.Id != Program.Master.Id)
                throw new Exception("u r not allowed");

            if (string.IsNullOrWhiteSpace(videoLink))
                videoLink = videoLink2;

            videoLink = videoLink.Trim('<', '>');
            if (!videoLink.StartsWith("https://www.youtube.com/watch?v="))
                throw new Exception("that no yt link D:");

            string videoName = $"youtube-dl --skip-download --get-title --no-warnings {videoLink}".GetShellOut();
            string path = $"Commands{s}Edit{s}Workspace{s}{new string(videoName.Where(x => char.IsLetterOrDigit(x) || x == ' ').ToArray())}.mp4";

            lock (workspaceLock)
            {
                FileStream f = new FileStream(path, FileMode.Create);
                Process p = MultiMediaHelper.GetStreamFromYouTubeVideo(videoLink);
                p.StandardOutput.BaseStream.CopyTo(f);
                return new Video(path);
            }
        }

        public string CutDesc = "Cut a video";
        public Video Cut(Video video, IMessage m, TimeSpan startTime, TimeSpan endTime)
        {
            lock (workspaceLock)
            {
                TimeSpan dureation = endTime - startTime;
                string targetPath = video.filePath.Split('.').SkipLast(1).Combine(".") + "-cut." + video.filePath.Split('.').Last();

                string args = $"-ss {startTime} -i {video.filePath} -to {dureation} -y {targetPath}";
                Process runner = Process.Start("ffmpeg", args);
                runner.WaitForExit();

                return new Video(targetPath);
            }
        }

        public string ConvertToGifDesc = "Convert video to gif";
        public Gif ConvertToGif(Video videoLink, IMessage m)
        {
            lock (workspaceLock)
            {
                string outPath = $"Commands{s}Edit{s}Workspace{s}output.gif";
                string args = $"-t 5 -y -i {videoLink.filePath} -vf \"fps = 10, scale = 420:-1:" +
                        $"flags = lanczos,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse\" -loop 0 {outPath}";
                Process runner = Process.Start("ffmpeg", args);
                runner.WaitForExit();

                Image i = Image.FromFile(outPath);
                return MultiMediaHelper.ImageToGif(i);
            }
        }

        public object firstOrderModelLock = new object();
        public string BakaMitaiDesc = "Picture goes Baka Mitai - using code from https://github.com/AliaksandrSiarohin/first-order-model - needs vram";
        public Video BakaMitai(Bitmap bmp, IMessage m)
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
                    "Commands\\Edit\\Resources\\first-order-model-executer.py");

                bmp.Dispose();
                resized.Dispose();

                runner.WaitForExit();

                if (!File.Exists(finalFile))
                    throw new Exception("Couldn't get enough VRAM to run the model D:");
                else
                    return new Video(finalFile);
            }
        }

        readonly char s = Path.DirectorySeparatorChar;
        public object workspaceLock = new object();
    }
}
