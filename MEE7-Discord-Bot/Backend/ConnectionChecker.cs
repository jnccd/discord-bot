using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MEE7.Backend.HelperFunctions;

namespace MEE7.Backend;

public static class ConnectionChecker
{
    static readonly int ReconnecterIntervalInMinutes = 10;
    static readonly ulong ConnectionCheckChannelId = 1496889462955966535;

    public static void StartReconnectLoop(DiscordSocketClient client)
    {
        Task.Run(() =>
        {
            Thread.CurrentThread.Name = "Reconnecter";
            while (true)
            {
                Thread.Sleep(ReconnecterIntervalInMinutes * 60000);

                // Check what client reports
                ConsoleWrapper.WriteLine($"{DateTime.Now:T} Connection State is: {client.ConnectionState}");

                // Check what message sending does
                try
                {
                    DiscordNETWrapper.SendText($"Test Message", ConnectionCheckChannelId).Wait();
                    IEnumerable<IMessage> messages = ((ISocketMessageChannel)client.GetChannel(ConnectionCheckChannelId)).GetMessagesAsync(int.MaxValue).FlattenAsync().GetAwaiter().GetResult();
                    ConsoleWrapper.WriteLine($"{DateTime.Now:T} Got {messages.Count()} message(s)!");
                    foreach (IMessage m in messages)
                    {
                        if (m.Author.Id == client.CurrentUser.Id)
                            m.DeleteAsync().Wait();
                    }
                }
                catch (Exception ex)
                {
                    ConsoleWrapper.WriteLine($"{DateTime.Now:T} Sending test message failed! \nError: {ex.Message}", ConsoleColor.Red);
                }

                // Reconnect if necessary
                if (client.ConnectionState != ConnectionState.Connected)
                {
                    try
                    {
                        client.StopAsync().Wait();
                        client.StartAsync().Wait();
                        ConsoleWrapper.WriteLine($"{DateTime.Now:T} Reconnected! Current state is {client.ConnectionState}", ConsoleColor.Green);
                    }
                    catch (Exception ex)
                    {
                        ConsoleWrapper.WriteLine($"{DateTime.Now:T} Reconnect failed! Current state is {client.ConnectionState}\nError: {ex.Message}", ConsoleColor.Red);
                    }
                }
            }
        });
    }
}