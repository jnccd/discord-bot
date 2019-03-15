using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TestDiscordBot.Config;

namespace TestDiscordBot.Commands
{
    public class PatchNotes : Command
    {
        public PatchNotes() : base("togglePatchNotes", "Get annoying messages", false)
        {

        }

        public override void OnConnected()
        {
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string url = "https://github.com/niklasCarstensen/Discord-Bot";
            WebRequest req = HttpWebRequest.Create(url);
            req.Timeout = 10000;
            WebResponse W = req.GetResponse();
            using (StreamReader sr = new StreamReader(W.GetResponseStream()))
            {
                string html = sr.ReadToEnd();
                string message = html.GetEverythingBetween("<a data-pjax=\"true\" title=\"", "\" class=\"message\"");
                string link = "https://github.com/niklasCarstensen/Discord-Bot/commit/" + html.GetEverythingBetween("href=\"/niklasCarstensen/Discord-Bot/commit/", "\">");

                if (message != Config.Config.Data.LastCommitMessage && message != "" && message != "Projektdateien hinzufügen." && message != "GITIGNORE und GITATTRIBUTES hinzufügen.")
                {
                    Config.Config.Data.LastCommitMessage = message;
                    Config.Config.Save();

                    foreach (ulong id in Config.Config.Data.PatchNoteSubscribedChannels)
                    {
                        try
                        {
                            EmbedBuilder Embed = new EmbedBuilder();
                            Embed.WithColor(0, 128, 255);
                            Embed.AddField("Patch Notes:", message + "\n[Link to the github-commit.](" + link + ")");
                            Embed.WithThumbnailUrl("https://community.canvaslms.com/community/image/2043/2.png?a=1646");
#if !DEBUG
                            Global.SendEmbed(Embed, (ISocketMessageChannel)Program.GetChannelFromID(id)).Wait();
#else
                            Global.ConsoleWriteLine("Patch Notes:" + message + "\n[Link to the github-commit.](" + link + ")", ConsoleColor.Cyan);
#endif
                        }
                        catch (Exception e) {
                            Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red);
                        }
                    }
                }
            }
        }
        bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public override async Task Execute(SocketMessage commandmessage)
        {
            if (commandmessage.Channel is SocketGuildChannel)
            {
                if (commandmessage.Author.Id == Program.GetGuildFromChannel(commandmessage.Channel).OwnerId || commandmessage.Author.Id == Global.Master.Id)
                {
                    if (Config.Config.Data.PatchNoteSubscribedChannels.Contains(commandmessage.Channel.Id))
                    {
                        Config.Config.Data.PatchNoteSubscribedChannels.Remove(commandmessage.Channel.Id);
                        await Global.SendText("Canceled Patch Note subscription for this channel!", commandmessage.Channel);
                    }
                    else
                    {
                        Config.Config.Data.PatchNoteSubscribedChannels.Add(commandmessage.Channel.Id);
                        await Global.SendText("Subscribed to Patch Notes!", commandmessage.Channel);
                    }
                }
                else
                {
                    await Global.SendText("Only the server/bot owner is authorized to use this command!", commandmessage.Channel);
                }
            }
            else
            {
                await Global.SendText("You can't use this in DMs", commandmessage.Channel);
            }
        }
    }
}
