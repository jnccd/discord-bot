using Discord;
using Discord.Audio;
using Discord.Rest;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions.Extensions;
using MEE7.Commands;
using MEE7.Configuration;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace MEE7.Backend.HelperFunctions
{
    public static class MultiMediaHelper
    {
        static readonly string youtubeDownloadLock = "";

        private static Process CreateFFMPEGProcess(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -filter:a \"volume = 0.05\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
        public static string DownloadVideoFromYouTube(string YoutubeURL)
        {
            if (!YoutubeURL.StartsWith("https://www.youtube.com/watch?"))
                return "";

            lock (youtubeDownloadLock)
            {
                string videofile = $"Downloads{Path.DirectorySeparatorChar}YoutubeVideo.mp4";
                Directory.CreateDirectory(Path.GetDirectoryName(videofile));
                if (File.Exists(videofile))
                {
                    int i = 0;
                    while (true)
                    {
                        if (File.Exists(videofile) && new FileInfo(videofile).IsFileLocked())
                            videofile = $"Downloads{Path.DirectorySeparatorChar}YoutubeVideo{++i}.mp4";
                        else
                        {
                            File.Delete(videofile);
                            break;
                        }
                    }
                }

                bool worked = false;
                $"youtube-dl.exe -f mp4 -o \"{videofile}\" {YoutubeURL}".RunAsConsoleCommand(25, () => { },
                    (s, e) => { if (s != null) worked = true; }, (StreamWriter w) => w.Write("e"));

                if (worked)
                    return videofile;
                else
                    return "";
            }
        }
        public static Process GetAudioStreamFromYouTubeVideo(string YoutubeURL, string audioFormat)
        {
            if (!YoutubeURL.StartsWith("https://www.youtube.com/watch?"))
                return null;

            return Process.Start(new ProcessStartInfo()
            {
                FileName = "youtube-dl",
                Arguments = $"--audio-format {audioFormat} -o - {YoutubeURL}",
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true
            });
        }
        public static Process CreateFfmpegOut()
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -ac 2 -f s16le -ar 48000 -i file.bin -acodec pcm_u8 -ar 22050 -f wav -",
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true
            });
        }
        public static async Task SendAudioAsync(IAudioClient audioClient, Stream stream)
        {
            Exception ex = null;
            using (AudioOutStream audioStream = audioClient.CreatePCMStream(AudioApplication.Music))
            {
                try { await stream.CopyToAsync(audioStream); }
                catch (Exception e) { ex = e; }
                finally { await audioStream.FlushAsync(); }
            }

            if (ex != null)
                throw ex;
        }
        public static async Task SendAudioAsync(IAudioClient audioClient, string path)
        {
            Exception ex = null;
            using (Process ffmpeg = CreateFFMPEGProcess(path))
            using (AudioOutStream stream = audioClient.CreatePCMStream(AudioApplication.Music))
            {
                try { await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream); }
                catch (Exception e) { ex = e; }
                finally { await stream.FlushAsync(); }
            }

            if (ex != null)
                throw ex;
        }
    }
}
