using Discord.Audio;
using Discord.WebSocket;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using NAudio.Wave.SampleProviders;
using System.Net;

namespace MEE7.Commands
{
    public class AudioCommands : EditCommandProvider
    {
        public string SpeakDesc = "Plays audio in voicechat";
        public WaveStream Speak(string text, SocketMessage m, string character = "GLaDOS")
        {
            if (text.Contains('"') || character.Contains('"'))
                return null;

            string payloadText = $"{{\"text\":\"{text}.\",\"character\":\"{character}\"}}";
            byte[] payload = Encoding.UTF8.GetBytes(payloadText);

            DiscordNETWrapper.SendText("`Speak` works using the api from `fifteen.ai` C:", m.Channel).Wait();

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("https://api.fifteen.ai/app/getAudioFile");
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0";
            req.Method = "POST";
            req.ContentLength = payload.Length;
            req.ContentType = "application/json;charset=UTF-8";
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
        public void PlayAudio(WaveStream w, SocketMessage m)
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
        public Bitmap DrawAudio(WaveStream w, SocketMessage m)
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
                graphics.DrawLines(new Pen(Color.White), Enumerable.
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

        public string pitchDesc = "Adds a Pitch to the sound";
        public WaveStream Pitch(WaveStream w, SocketMessage m, float PitchFactor)
        {
            string filePath = $"Commands{Path.DirectorySeparatorChar}Edit{Path.DirectorySeparatorChar}pitch.bin";

            SmbPitchShiftingSampleProvider pitch = new SmbPitchShiftingSampleProvider(w.ToSampleProvider())
            {
                PitchFactor = PitchFactor
            };

            WaveFileWriter.CreateWaveFile16(filePath, pitch);
            return new WaveFileReader(filePath);
        }

        public string volumeDesc = "Adds Volume to the sound";
        public WaveStream Volume(WaveStream w, SocketMessage m, float VolumeFactor)
        {
            string filePath = $"Commands{Path.DirectorySeparatorChar}Edit{Path.DirectorySeparatorChar}volume.bin";

            VolumeSampleProvider pitch = new VolumeSampleProvider(w.ToSampleProvider())
            {
                Volume = VolumeFactor
            };

            WaveFileWriter.CreateWaveFile16(filePath, pitch);
            return new WaveFileReader(filePath);
        }
    }
}
