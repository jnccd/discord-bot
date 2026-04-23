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
            Console.WriteLine($"{DateTime.Now:T} ConnectionChecker startup");
            while (true)
            {
                // Check what client reports
                Console.WriteLine($"{DateTime.Now:T} Connection State is: {client.ConnectionState}");

                // Check what message sending does
                try
                {
                    DiscordNETWrapper.SendText($"Test Message", ConnectionCheckChannelId).Wait();
                    IEnumerable<IMessage> messages = ((ISocketMessageChannel)client.GetChannel(ConnectionCheckChannelId)).GetMessagesAsync(int.MaxValue).FlattenAsync().GetAwaiter().GetResult();
                    Console.WriteLine($"{DateTime.Now:T} Got {messages.Count()} message(s)!");
                    foreach (IMessage m in messages)
                    {
                        if (m.Author.Id == client.CurrentUser.Id)
                            m.DeleteAsync().Wait();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{DateTime.Now:T} Sending test message failed! \nError: {ex.Message}");
                }

                // Reconnect if necessary
                if (client.ConnectionState != ConnectionState.Connected)
                {
                    try
                    {
                        client.StopAsync().Wait();
                        client.StartAsync().Wait();
                        Console.WriteLine($"{DateTime.Now:T} Reconnected! Current state is {client.ConnectionState}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{DateTime.Now:T} Reconnect failed! Current state is {client.ConnectionState}\nError: {ex.Message}");
                    }
                }

                Task.Delay(ReconnecterIntervalInMinutes * 60000).Wait();
            }
        });
    }
}