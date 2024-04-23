using Discord;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System;
using MPack;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Text;
using Discord.Net.WebSockets;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Drawing;

namespace MEE7.Commands
{
    public class Lighthouse : Command
    {
        Uri uri = new("wss://lighthouse.uni-kiel.de/websocket");
        int msgCounter = 0;
        public static List<Tuple<byte[,,], int>> images = new();
        string username = System.Environment.GetEnvironmentVariable("LIGHTHOUSE_USERNAME");
        string token = System.Environment.GetEnvironmentVariable("LIGHTHOUSE_TOKEN");
        bool gotCreds;
        Exception ex;
        Task lighthouseThread = null;
        CancellationTokenSource cancelTokenSource = new();
        CancellationToken cancelToken;

        public Lighthouse() : base("lighthouse", "Put media on the lighthouse", isExperimental: false, isHidden: true)
        {
            gotCreds = !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(token);
            images.Add(new Tuple<byte[,,], int>(new byte[14, 28, 3], 100));
            cancelToken = cancelTokenSource.Token;
            Program.OnConnected += Program_OnConnected;
        }

        private void Program_OnConnected()
        {
            if (gotCreds)
                StartSenderLoop();
        }

        public override void Execute(IMessage message)
        {
            if (message.Author.Id != Program.Master.Id)
            {
                DiscordNETWrapper.SendText("This command is admin only :P", message.Channel).Wait();
                return;
            }

            var split = message.Content.Split(' ');
            if (split.Length >= 3 && split[1] == "set-token") 
            {
                cancelTokenSource.Cancel();
                lighthouseThread.Wait();

                lock (images)
                {
                    token = split[2];
                    cancelTokenSource = new();
                    cancelToken = cancelTokenSource.Token;
                    StartSenderLoop();
                }
            }

            if (split.Length >= 2 && split[1] == "print-tensor")
            {
                foreach (var image in images)
                {
                    Bitmap b = new(28, 14);
                    for (var y = 0; y < image.Item1.GetLength(0); y++)
                    {
                        for (var x = 0; x < image.Item1.GetLength(1); x++)
                        {
                            b.SetPixel(x, y, System.Drawing.Color.FromArgb(image.Item1[y, x, 0], image.Item1[y, x, 1], image.Item1[y, x, 2]));
                        }
                    }
                    DiscordNETWrapper.SendBitmap(b, message.Channel).Wait();
                }
            }

            if (split.Length >= 2 && split[1] == "print-except")
            {
                if (ex != null)
                    throw ex;
                else
                    DiscordNETWrapper.SendText("Ne exceptions yet", message.Channel);
            }
        }

        public void StartSenderLoop()
        {
            lighthouseThread = Task.Run(async () =>
            {
                Thread.CurrentThread.Name = "Lighthouse Thread";
                while (true)
                {
                    using ClientWebSocket webSocketClient = new();
                    try
                    {
                        int imageCounter = 0, waitTime = 0;
                        Stopwatch sw = new();
                        await webSocketClient.ConnectAsync(uri, default);

                        while (true)
                        {
                            sw.Restart();

                            if (cancelToken.IsCancellationRequested)
                                break;

                            lock (images)
                            {
                                webSocketClient.SendAsync(PackMessage(imageCounter), WebSocketMessageType.Binary, true, default)
                                               .GetAwaiter()
                                               .GetResult();
                                imageCounter = (imageCounter + 1) % images.Count;

                                var bytes = new byte[1024];
                                var result = webSocketClient.ReceiveAsync(bytes, default).Result;
                                //Debug.WriteLine($"Got lighthouse res: {Encoding.UTF8.GetString(bytes, 0, result.Count)}");

                                sw.Stop();
                                waitTime = images[imageCounter].Item2 - (int)sw.ElapsedMilliseconds;
                            }

                            if (cancelToken.IsCancellationRequested)
                                break;

                            await Task.Delay(waitTime > 0 ? waitTime : 1);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.ex = ex;
                        Debug.WriteLine("Lighthouse fail");
                        Debug.WriteLine(ex);
                    }
                    try { await webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", default); } catch { }
                    if (cancelToken.IsCancellationRequested)
                        break;
                    await Task.Delay(1000);
                }
            });
        }

        public ReadOnlyMemory<byte> PackMessage(int imageIndex)
        {
            lock (images)
            {
                var msgpackDict = new MDict
                {
                    {"REID", MToken.From(msgCounter++)},
                    {"AUTH", MToken.From(new MDict {
                            {"USER", username},
                            {"TOKEN", token},
                    })
                                },
                    {"VERB", "PUT"},
                    {"PATH", MToken.From(new[] {
                        "user",
                        username,
                        "model",
                        })
                    },
                    {"META", MToken.From(new MDict { }) },
                    {"PAYL", images[Math.Abs(imageIndex) % images.Count].Item1.Cast<byte>().ToArray()},
                };
                return msgpackDict.EncodeToBytes();
            }
        }
    }
}