using Discord.WebSocket;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MEE7.Commands
{
    public partial class Edit : Command
    {
        readonly EditCommand[] AudioCommands = new EditCommand[] {
            new EditCommand("drawAudio", "Draw the samples", (SocketMessage m, string a, object o) => {

                WaveStream w = o as WaveStream;
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

                return output;

            }, typeof(WaveStream), typeof(Bitmap)),
        };
    }
}
