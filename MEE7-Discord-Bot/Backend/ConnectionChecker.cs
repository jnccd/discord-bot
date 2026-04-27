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
    static readonly int ReconnecterCheckIntervalInMinutes = 4;
    static readonly ulong ConnectionCheckChannelId = 1496889462955966535;
    static readonly int ReconnecterForceReconnectIntervalInMinutes = 120;

    static void Reconnect()
    {
        try
        {
            Program.InitClient();
            ConsoleWrapper.WriteLine($"{DateTime.Now:T} Reconnected! Current state is {Program.Client?.ConnectionState}");

            Program.Client?.Log += ClientLogEvent;
        }
        catch (Exception ex)
        {
            ConsoleWrapper.WriteLine($"{DateTime.Now:T} Reconnect failed! Current state is {Program.Client?.ConnectionState}\nError: {ex}");
        }
    }

    static Task ClientLogEvent(LogMessage msg)
    {
        if (msg.Exception.GetType() == typeof(Discord.WebSocket.GatewayReconnectException) || msg.Message.Contains("Discord.WebSocket.GatewayReconnectException"))
        {
            ConsoleWrapper.WriteLine($"{DateTime.Now:T} GatewayReconnectException! Current state is {Program.Client?.ConnectionState}");
            Reconnect();
        }
        return Task.CompletedTask;
    }

    public static void StartReconnectLoop()
    {
        Program.Client?.Log += ClientLogEvent;

        Task.Run(() =>
        {
            Thread.CurrentThread.Name = "Reconnecter";
            ConsoleWrapper.WriteLine($"{DateTime.Now:T} ConnectionChecker Reconnecter startup");

            while (true)
            {
                Task.Delay(ReconnecterForceReconnectIntervalInMinutes * 60000).Wait();

                Console.WriteLine($"{DateTime.Now:T} Force Reconnecting");
                Task.Run(Reconnect);
            }
        });

        Task.Run(() =>
        {
            Thread.CurrentThread.Name = "Reconnect Checker";
            ConsoleWrapper.WriteLine($"{DateTime.Now:T} ConnectionChecker Checker startup");

            while (true)
            {
                Task.Delay(ReconnecterCheckIntervalInMinutes * 60000).Wait();

                // Check what message sending does
                try
                {
                    DiscordNETWrapper.SendText($"Test Message", ConnectionCheckChannelId).Wait();
                    IEnumerable<IMessage> messages = ((ISocketMessageChannel?)Program.Client?.GetChannel(ConnectionCheckChannelId))?.GetMessagesAsync().FlattenAsync().GetAwaiter().GetResult() ?? [];
                    if (messages.Count() == 0)
                        throw new Exception("Where muh message??");
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
                    Reconnect();
                    continue;
                }

                // Reconnect if necessary
                if (Program.Client?.ConnectionState != ConnectionState.Connected)
                {
                    ConsoleWrapper.WriteLine($"{DateTime.Now:T} Connection State is {Program.Client?.ConnectionState}, initiating reconnect");
                    Reconnect();
                    continue;
                }
            }
        });
    }
}