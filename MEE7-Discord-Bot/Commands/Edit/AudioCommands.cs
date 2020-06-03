using Discord;
using Discord.Audio;
using Discord.WebSocket;
using MEE7.Backend.HelperFunctions;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace MEE7.Commands
{
    public class AudioCommands : EditCommandProvider
    {
        public string SpeakDesc = "Let's a fictional character narrate the text you input using the fifteen.ai api. " +
            "Working characters include GLaDOS (Emotions: Homicidal), Twilight Sparkle (Emotions: Happy, Neutral), Fluttershy (Emotions: Neutral), " +
            "Rarity (Emotions: Neutral), Applejack (Emotions: Neutral), Rainbow Dash (Emotions: Neutral), Pinkie Pie (Emotions: Neutral), " +
            "Derpy Hooves (Emotions: Neutral), Solider (Emotions: Neutral), Miss Pauling (Emotions: Neutral), Rise Kujikawa (Emotions: Neutral). " +
            "Default Voice is GLaDOS\n" +
            "Available voices last updated: 03.05.2020";
        public WaveStream Speak(string text, IMessage m, string character = "GLaDOS", string emotion = "Homicidal")
        {
            if (text.Contains('"') || character.Contains('"'))
                return null;

            string payloadText = $"{{\"text\":\"{text}.\",\"character\":\"{character}\",\"emotion\":\"{emotion}\"}}";
            byte[] payload = Encoding.UTF8.GetBytes(payloadText);

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("https://api.fifteen.ai/app/getAudioFile");
            req.Method = "POST";
            req.ContentLength = payloadText.Length;
            req.ContentType = "application/json;charset=UTF-8";
            req.Referer = "https://fifteen.ai/app";
            req.Accept = "application/json, text/plain, */*";
            req.Headers.Add("accept-encoding", "gzip, deflate, br");
            req.Headers.Add("accept-language", "de-DE,de;q=0.9,en-US;q=0.8,en;q=0.7");
            req.Headers.Add("origin", "https://fifteen.ai");
            req.Headers.Add("sec-fetch-dest", "empty");
            req.Headers.Add("sec-fetch-mode", "cors");
            req.Headers.Add("sec-fetch-site", "same-site");
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.132 Safari/537.36";
            using (Stream dataStream = req.GetRequestStream())
                dataStream.Write(payload);
            WebResponse response = req.GetResponse();
            using (Stream dataStream = response.GetResponseStream())
            {
                MemoryStream ms = new MemoryStream();
                dataStream.CopyTo(ms);
                ms.Position = 0;
                return new WaveFileReader(ms);
            }
        }

        public string playAudioDesc = "Plays audio in voicechat";
        public void PlayAudio(WaveStream w, IMessage m)
        {
            SocketGuild g = Program.GetGuildFromChannel(m.Channel);
            ISocketAudioChannel channel = g.VoiceChannels.FirstOrDefault(x => x.Users.Select(y => y.Id).Contains(m.Author.Id));
            if (channel != null)
            {
                try { channel.DisconnectAsync().Wait(); } catch { }

                IAudioClient client = channel.ConnectAsync().Result;
                using (WaveStream naudioStream = WaveFormatConversionStream.CreatePcmStream(w))
                    MultiMediaHelper.SendAudioAsync(client, naudioStream).Wait();

                try { channel.DisconnectAsync().Wait(); } catch { }
            }
            else
                DiscordNETWrapper.SendText("You are not in an AudioChannel on this server!", m.Channel).Wait();

            w.Dispose();
        }

        public string drawAudioDesc = "Draw the samples";
        public Bitmap DrawAudio(WaveStream w, IMessage m)
        {
            var c = new WaveChannel32(w);

            float[] normSamplesMin = new float[1000]; Array.Fill(normSamplesMin, 250);
            float[] normSamplesMax = new float[1000]; Array.Fill(normSamplesMax, 250);
            byte[] buffer = new byte[1024];
            int reader = 0;
            while (c.Position < c.Length)
            {
                reader = c.Read(buffer, 0, buffer.Length);
                for (int i = 0; i < buffer.Length / 4; i++)
                {
                    float sample = BitConverter.ToSingle(buffer, i * 4) * 200 + 250;
                    int index = (int)(((c.Position - buffer.Length) / 4.0 + i) / (c.Length / 4.0) * 1000);
                    if (index >= normSamplesMax.Length)
                        continue;
                    if (normSamplesMax[index] < sample)
                        normSamplesMax[index] = sample;
                    if (normSamplesMin[index] > sample)
                        normSamplesMin[index] = sample;
                }
            }

            int j = 0;
            Bitmap output = new Bitmap(1000, 500);
            using (Graphics graphics = Graphics.FromImage(output))
                graphics.DrawLines(new Pen(System.Drawing.Color.White), Enumerable.
                    Range(0, 1000).
                    Select(x => new Point[] {
                            new Point(j, (int)normSamplesMin[x]),
                            new Point(j++, (int)normSamplesMax[x]),
                    }).
                    SelectMany(x => x).
                    ToArray());

            w.Dispose();
            return output;
        }

        object pitchLock = new object();
        public string pitchDesc = "Adds a Pitch to the sound";
        public WaveStream Pitch(WaveStream w, IMessage m, float PitchFactor)
        {
            string filePath = $"Commands{Path.DirectorySeparatorChar}Edit{Path.DirectorySeparatorChar}pitch.bin";

            lock (pitchLock)
            {
                SmbPitchShiftingSampleProvider pitch = new SmbPitchShiftingSampleProvider(w.ToSampleProvider())
                {
                    PitchFactor = PitchFactor
                };

                WaveFileWriter.CreateWaveFile16(filePath, pitch);
                return new WaveFileReader(filePath);
            }
        }

        object volumeLock = new object();
        public string volumeDesc = "Adds Volume to the sound";
        public WaveStream Volume(WaveStream w, IMessage m, float VolumeFactor)
        {
            string filePath = $"Commands{Path.DirectorySeparatorChar}Edit{Path.DirectorySeparatorChar}volume.bin";

            lock (volumeLock)
            {
                VolumeSampleProvider pitch = new VolumeSampleProvider(w.ToSampleProvider())
                {
                    Volume = VolumeFactor
                };

                WaveFileWriter.CreateWaveFile16(filePath, pitch);
                return new WaveFileReader(filePath);
            }
        }
    }
}
