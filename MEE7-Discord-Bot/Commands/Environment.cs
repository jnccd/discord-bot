using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using MEE7.Backend.HelperFunctions.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MEE7.Commands
{
    public class Environment : Command
    {
        public Environment() : base("environment", "Prints bot environment info", false, true)
        {

        }

        public override void Execute(SocketMessage message)
        {
            if (message.Author.Id != Program.Master.Id)
                DiscordNETWrapper.SendText("Sorry but only my Owner is allowed to use this Command.", message.Channel).Wait();
            else
            {
                EmbedBuilder Embed = new EmbedBuilder();
                Embed.WithColor(0, 128, 255);

                Embed.AddFieldDirectly("CurrentDirectory", System.Environment.CurrentDirectory);
                Embed.AddFieldDirectly("Is64BitOperatingSystem", System.Environment.Is64BitOperatingSystem);
                Embed.AddFieldDirectly("MachineName", System.Environment.MachineName);
                Embed.AddFieldDirectly("OSVersion", System.Environment.OSVersion);
                Embed.AddFieldDirectly("ProcessorCount", System.Environment.ProcessorCount);
                Embed.AddFieldDirectly("SystemPageSize", System.Environment.SystemPageSize);
                Embed.AddFieldDirectly("System Start Time", DateTime.Now.Subtract(new TimeSpan(0,0,0,0, System.Environment.TickCount)));
                Embed.AddFieldDirectly("UserDomainName", System.Environment.UserDomainName);
                Embed.AddFieldDirectly("UserInteractive", System.Environment.UserInteractive);
                Embed.AddFieldDirectly("UserName", System.Environment.UserName);
                Embed.AddFieldDirectly("Version", System.Environment.Version);
                Embed.AddFieldDirectly("WorkingSet", System.Environment.WorkingSet);

                DiscordNETWrapper.SendEmbed(Embed, message.Channel).Wait();
            }
        }
    }
}
