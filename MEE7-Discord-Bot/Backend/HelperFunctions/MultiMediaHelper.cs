using AnimatedGif;
using AnimatedGif.ImageSharp;
using Discord.Audio;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using static MEE7.Commands.Edit.Edit;

namespace MEE7.Backend.HelperFunctions
{
    public static class MultiMediaHelper
    {
        static readonly string youtubeDownloadLock = "";

        private static Process? CreateFFMPEGProcess(string path)
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
                var videoDir = Path.GetDirectoryName(videofile);
                if (videoDir == null)
                    throw new InvalidOperationException("Failed to get video directory.");
                Directory.CreateDirectory(videoDir);
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
        public static Process? GetStreamFromYouTubeVideo(string YoutubeURL, string arguments = "")
        {
            if (!YoutubeURL.StartsWith("https://www.youtube.com/watch?"))
                return null;

            return Process.Start(new ProcessStartInfo()
            {
                FileName = "youtube-dl",
                Arguments = $"{arguments} -o - {YoutubeURL}",
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true
            });
        }
        public static Process? GetAudioStreamFromYouTubeVideo(string YoutubeURL, string audioFormat)
        {
            if (!YoutubeURL.StartsWith("https://www.youtube.com/watch?"))
                return null;

            string filename;
            if (Program.RunningOnLinux) filename = "./youtube-dl";
            else filename = "youtube-dl";

            return Process.Start(new ProcessStartInfo()
            {
                FileName = filename,
                Arguments = $"--audio-format {audioFormat} -o - {YoutubeURL}",
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true
            });
        }
        public static Process? CreateFfmpegOut(string filePath)
        {
            string filename;
            if (Program.RunningOnLinux) filename = "./ffmpeg";
            else filename = "ffmpeg";

            return Process.Start(new ProcessStartInfo
            {
                FileName = filename,
                Arguments = $"-hide_banner -loglevel panic -ac 2 -f s16le -ar 48000 -i {filePath} -acodec pcm_u8 -ar 22050 -f wav -",
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true
            });
        }
        public static async Task SendAudioAsync(IAudioClient audioClient, Stream stream)
        {
            Exception? ex = null;
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
            Exception? ex = null;
            using (Process ffmpeg = CreateFFMPEGProcess(path) ?? throw new InvalidOperationException("Failed to start ffmpeg process."))
            using (AudioOutStream stream = audioClient.CreatePCMStream(AudioApplication.Music))
            {
                try { await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream); }
                catch (Exception e) { ex = e; }
                finally { await stream.FlushAsync(); }
            }

            if (ex != null)
                throw ex;
        }
        public static (int FrameCount, int[] FrameDurations) GetGifFrameInfo(SKImage image)
        {
            using var codec = SKCodec.Create(image.Encode(SKEncodedImageFormat.Gif, 100));

            if (codec == null)
                throw new ArgumentException("The image is not a valid GIF or could not be decoded.");

            int frameCount = codec.FrameCount;
            int[] frameDurations = new int[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                if (codec.GetFrameInfo(i, out SKCodecFrameInfo frameInfo))
                    frameDurations[i] = frameInfo.Duration;
                else
                    frameDurations[i] = 33;
            }

            return (frameCount, frameDurations);
        }
        public static SKBitmap GetGifFrame(SKImage gifImage, int frameIndex)
        {
            using var gifData = gifImage.Encode(SKEncodedImageFormat.Gif, 100);
            using var codec = SKCodec.Create(gifData);

            if (codec == null)
                throw new ArgumentException("The image is not a valid GIF or could not be decoded.");

            // Check if the frame index is valid
            if (frameIndex < 0 || frameIndex >= codec.FrameCount)
                throw new ArgumentOutOfRangeException(nameof(frameIndex), "Frame index is out of range.");

            // Create a bitmap to hold the frame
            if (!codec.GetFrameInfo(frameIndex, out SKCodecFrameInfo frameInfo))
                throw new ArgumentException("Failed to get frame info.");

            var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

            var bitmap = new SKBitmap(frameInfo.FrameRect.Width, frameInfo.FrameRect.Height, SKColorType.Rgba8888, SKAlphaType.Premul);

            // Decode the specific frame into the bitmap
            var opts = new SKCodecOptions(frameIndex);
            var result = codec.GetPixels(info, bitmap.GetPixels(), opts);
            if (result != SKCodecResult.Success)
                throw new InvalidOperationException("Failed to decode the frame.");

            return bitmap;
        }
        public static SKBitmap[] GetGifFrames(SKImage gifImage)
        {
            using var gifData = gifImage.Encode(SKEncodedImageFormat.Gif, 100);
            using var codec = SKCodec.Create(gifData);

            if (codec == null)
                throw new ArgumentException("The image is not a valid GIF or could not be decoded.");

            SKBitmap[] frames = new SKBitmap[codec.FrameCount];

            for (int frameIndex = 0; frameIndex < codec.FrameCount; frameIndex++)
            {
                // Create a bitmap to hold the frame
                if (!codec.GetFrameInfo(frameIndex, out SKCodecFrameInfo frameInfo))
                    throw new ArgumentException("Failed to get frame info.");

                var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

                var bitmap = new SKBitmap(frameInfo.FrameRect.Width, frameInfo.FrameRect.Height, SKColorType.Rgba8888, SKAlphaType.Premul);

                // Decode the specific frame into the bitmap
                var opts = new SKCodecOptions(frameIndex);
                var result = codec.GetPixels(info, bitmap.GetPixels(), opts);
                if (result != SKCodecResult.Success)
                    throw new InvalidOperationException("Failed to decode the frame.");
            }

            return frames;
        }
        public static Gif ImageToGif(SKImage i)
        {
            (_, int[] timings) = GetGifFrameInfo(i);
            var frames = GetGifFrames(i);
            return new Gif(frames, timings);
        }

        public static SKBitmap LoadBitmapFromUrl(string imageUrl)
        {
            using var httpClient = new HttpClient();

            // Download the image data as a byte array
            byte[] imageData = httpClient.GetByteArrayAsync(imageUrl).Result;

            // Decode the byte array into an SKBitmap
            using (var stream = new MemoryStream(imageData))
            using (var codec = SKCodec.Create(stream))
            {
                var bitmap = new SKBitmap(
                    codec.Info.Width,
                    codec.Info.Height,
                    SKColorType.Rgba8888,
                    SKAlphaType.Premul
                );

                // Decode the image into the bitmap
                var result = codec.GetPixels(bitmap.Info, bitmap.GetPixels());
                if (result != SKCodecResult.Success)
                    throw new InvalidOperationException("Failed to decode the image.");

                return bitmap;
            }
        }
        public static void SaveGif(Gif gif, Stream s)
        {
            GifEngine ge = new GifEngine(new ImageSharpImageLibrary());
            using var creator = ge.CreateGif(s, -1);
            for (int i = 0; i < gif.Item1.Length; i++)
                using (var image = Image.LoadPixelData<Rgba32>(gif.Item1[i].Bytes, gif.Item1[i].Width, gif.Item1[i].Height))
                    creator.AddFrame(BitmapConverter.Convert(image), gif.Item2[i], GifQuality.Bit8);
        }
        public static void SaveBitmaps(SKBitmap[] bitmaps, Stream s, int delay = 33)
        {
            GifEngine ge = new GifEngine(new ImageSharpImageLibrary());
            using var creator = ge.CreateGif(s, -1);
            for (int i = 0; i < bitmaps.Length; i++)
                using (var image = Image.LoadPixelData<Rgba32>(bitmaps[i].Bytes, bitmaps[i].Width, bitmaps[i].Height))
                    creator.AddFrame(BitmapConverter.Convert(image), delay, GifQuality.Bit8);
        }
        public static Gif LoadGifFromUrl(string gifUrl)
        {
            using var httpClient = new HttpClient();
            byte[] gifData = httpClient.GetByteArrayAsync(gifUrl).Result;

            using var stream = new MemoryStream(gifData);
            using var codec = SKCodec.Create(stream);
            var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

            if (codec == null)
                throw new ArgumentException("The URL does not point to a valid GIF.");

            int frameCount = codec.FrameCount;
            var bitmaps = new SKBitmap[frameCount];
            var timings = new int[frameCount];

            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                if (!codec.GetFrameInfo(frameIndex, out SKCodecFrameInfo frameInfo))
                    throw new ArgumentException("Failed to get frame info.");
                var bitmap = new SKBitmap(
                    frameInfo.FrameRect.Width,
                    frameInfo.FrameRect.Height,
                    info.ColorType,
                    frameInfo.AlphaType
                );

                // Decode the frame
                var opts = new SKCodecOptions(frameIndex);
                var result = codec.GetPixels(info, bitmap.GetPixels(), opts);
                if (result != SKCodecResult.Success)
                    throw new InvalidOperationException("Failed to decode the frame.");

                bitmaps[frameIndex] = bitmap;
                timings[frameIndex] = frameInfo.Duration;
            }

            return new Gif(bitmaps, timings);
        }
    }
}
