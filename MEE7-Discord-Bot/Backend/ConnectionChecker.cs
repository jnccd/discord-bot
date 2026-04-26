using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MEE7.Backend.HelperFunctions;

namespace MEE7.Backend;

public static class ConnectionChecker
{
    static readonly int ReconnecterCheckIntervalInMinutes = 10;
    static readonly ulong ConnectionCheckChannelId = 1496889462955966535;
    static readonly int ReconnecterForceReconnectIntervalInMinutes = 120;

    static void Reconnect()
    {
        try
        {
            Program.InitClient();
            ConsoleWrapper.WriteLine($"{DateTime.Now:T} Reconnected! Current state is {Program.Client?.ConnectionState}");
        }
        catch (Exception ex)
        {
            ConsoleWrapper.WriteLine($"{DateTime.Now:T} Reconnect failed! Current state is {Program.Client?.ConnectionState}\nError: {ex}");
        }
    }

    public static void StartReconnectLoop()
    {
        Program.Client?.Log += (LogMessage msg) =>
        {
            if (msg.Exception.GetType() == typeof(Discord.WebSocket.GatewayReconnectException) || msg.Message.Contains("Discord.WebSocket.GatewayReconnectException"))
            {
                ConsoleWrapper.WriteLine($"{DateTime.Now:T} GatewayReconnectException! Current state is {Program.Client?.ConnectionState}");
                Reconnect();
            }
            return Task.CompletedTask;
        };

        // Task.Run(() =>
        // {
        //     Thread.CurrentThread.Name = "Reconnecter";
        //     ConsoleWrapper.WriteLine($"{DateTime.Now:T} ConnectionChecker startup");

        //     while (true)
        //     {
        //         Task.Delay(ReconnecterForceReconnectIntervalInMinutes * 60000).Wait();

        //         Console.WriteLine($"{DateTime.Now:T} Force Reconnecting");
        //         Task.Run(Reconnect);
        //     }
        // });

        // Task.Run(() =>
        // {
        //     Thread.CurrentThread.Name = "Signal Sender";
        //     ConsoleWrapper.WriteLine($"{DateTime.Now:T} ConnectionChecker startup");

        //     while (true)
        //     {
        //         Task.Delay(60000).Wait();

        //         Program.MainThread.Interrupt();
        //     }
        // });

        Task.Run(() =>
        {
            Thread.CurrentThread.Name = "Reconnect Checker";
            ConsoleWrapper.WriteLine($"{DateTime.Now:T} ConnectionChecker startup");

            while (true)
            {
                // Check what client reports
                //ConsoleWrapper.WriteLine($"{DateTime.Now:T} Connection State is: {Program.Client?.ConnectionState}");

                // Check what message sending does
                try
                {
                    DiscordNETWrapper.SendText($"Test Message", ConnectionCheckChannelId).Wait();
                    IEnumerable<IMessage> messages = ((ISocketMessageChannel?)Program.Client?.GetChannel(ConnectionCheckChannelId))?.GetMessagesAsync(int.MaxValue).FlattenAsync().GetAwaiter().GetResult() ?? [];
                    //ConsoleWrapper.WriteLine($"{DateTime.Now:T} Got {messages.Count()} message(s)!");
                    foreach (IMessage m in messages)
                    {
                        if (m.Author.Id == Program.Client?.CurrentUser.Id)
                            m.DeleteAsync().Wait();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{DateTime.Now:T} Sending test message failed! \nError: {ex}");
                }

                // Reconnect if necessary
                if (Program.Client?.ConnectionState != ConnectionState.Connected)
                {
                    Reconnect();
                }

                Task.Delay(ReconnecterCheckIntervalInMinutes * 60000).Wait();
            }
        });
    }
}