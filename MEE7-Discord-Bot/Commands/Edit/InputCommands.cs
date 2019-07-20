using Discord;
using Discord.WebSocket;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions.Extensions;
using MEE7.Backend.HelperFunctions;

namespace MEE7.Commands
{
    public partial class Edit : Command
    {
        readonly EditCommand[] InputCommands = new EditCommand[] {
            new EditCommand("lastT", "Gets the last messages text", (SocketMessage m, string a, object o) => {
                return m.Channel.GetMessagesAsync(2).FlattenAsync().Result.Last().Content;
            }, null, typeof(string)),
            new EditCommand("lastP", "Gets the last messages picture", (SocketMessage m, string a, object o) => {
                IMessage lm = m.Channel.GetMessagesAsync(2).FlattenAsync().Result.Last();
                string pic = null;
                if (lm.Attachments.Count > 0 && lm.Attachments.ElementAt(0).Size > 0)
                {
                    if (lm.Attachments.ElementAt(0).Filename.EndsWith(".png"))
                        pic = lm.Attachments.ElementAt(0).Url;
                    else if (lm.Attachments.ElementAt(0).Filename.EndsWith(".jpg"))
                        pic = lm.Attachments.ElementAt(0).Url;
                }
                string picLink = lm.Content.GetPictureLinkInMessage();
                if (string.IsNullOrWhiteSpace(pic) && picLink != null)
                    pic = picLink;
                return pic.GetBitmapFromURL();
            }, null, typeof(Bitmap)),
            new EditCommand("thisT", "Outputs the given arguments", (SocketMessage m, string a, object o) => {
                return a;
            }, null, typeof(string)),
            new EditCommand("thisP", "Gets this messages picture / picture link in the arguments", (SocketMessage m, string a, object o) => {
                return GetPictureLinkFromMessage(m, a).GetBitmapFromURL();
            }, null, typeof(Bitmap)),
            new EditCommand("thisG", "Gets this messages gif / gif link in the arguments", (SocketMessage m, string a, object o) => {
                return GetPictureLinkFromMessage(m, a).GetBitmapsFromGIFURL();
            }, null, typeof(Bitmap[])),
            new EditCommand("thisA", "Gets mp3 or wav audio files attached to this message", (SocketMessage m, string a, object o) => {
                string url = m.Attachments.FirstOrDefault(x => x.Url.EndsWith(".mp3")).Url;
                if (!string.IsNullOrWhiteSpace(url))
                    return url.Getmp3AudioFromURL();

                url = m.Attachments.FirstOrDefault(x => x.Url.EndsWith(".wav")).Url;
                if (!string.IsNullOrWhiteSpace(url))
                    return url.GetwavAudioFromURL();

                url = m.Attachments.FirstOrDefault(x => x.Url.EndsWith(".ogg")).Url;
                if (!string.IsNullOrWhiteSpace(url))
                    return url.GetoggAudioFromURL();

                throw new Exception("No audio file found!");
            }, null, typeof(WaveStream)),
            new EditCommand("profilePicture", "Gets a profile picture", (SocketMessage m, string a, object o) => {
                SocketUser luser = Program.GetUserFromId(Convert.ToUInt64((a as string).Trim(new char[] { ' ', '<', '>', '@', '!' })));
                string avatarURL = luser.GetAvatarUrl(ImageFormat.Png, 512);
                return (string.IsNullOrWhiteSpace(avatarURL) ? luser.GetDefaultAvatarUrl() : avatarURL).GetBitmapFromURL();
            }, null, typeof(Bitmap)),
            new EditCommand("serverPicture", "Gets the server picture from a server id", (SocketMessage m, string a, object o) => {
                return Program.GetGuildFromID(Convert.ToUInt64((a as string).Trim(new char[] { ' ', '<', '>', '@', '!' }))).IconUrl.GetBitmapFromURL();
            }, null, typeof(Bitmap)),
            new EditCommand("emote", "Gets the picture of the emote", (SocketMessage m, string a, object o) => {
                return Program.GetGuildFromChannel(m.Channel).Emotes.FirstOrDefault(x => x.Name.Contains(a) && !x.Animated).Url.GetBitmapFromURL();
            }, null, typeof(Bitmap)),
            new EditCommand("gifEmote", "Gets the pictures of the emote", (SocketMessage m, string a, object o) => {
                return Program.GetGuildFromChannel(m.Channel).Emotes.FirstOrDefault(x => x.Name.Contains(a) && x.Animated).Url.GetBitmapsFromGIFURL();
            }, null, typeof(Bitmap[])),
            new EditCommand("mp3FromYT", "Gets the mp3 of an youtube video, takes the video url as argument", 
                (SocketMessage m, string a, object o) => {
                    MemoryStream mem = new MemoryStream();
                    using (Process P = MultiMediaHelper.GetAudioStreamFromYouTubeVideo(a, "mp3"))
                    {
                        while (true)
                        {
                            Task.Delay(1001).Wait();
                            if (string.IsNullOrWhiteSpace(P.StandardError.ReadLine()))
                                break;
                        }
                        P.StandardOutput.BaseStream.CopyTo(mem);
                        return WaveFormatConversionStream.CreatePcmStream(new StreamMediaFoundationReader(mem));
                    }
            }, null, typeof(WaveStream)),
        };

        private static string GetPictureLinkFromMessage(SocketMessage m, string arguments)
        {
            string pic = null;
            if (m.Attachments.Count > 0 && m.Attachments.ElementAt(0).Size > 0)
            {
                if (m.Attachments.ElementAt(0).Filename.EndsWith(".png"))
                    pic = m.Attachments.ElementAt(0).Url;
                else if (m.Attachments.ElementAt(0).Filename.EndsWith(".jpg"))
                    pic = m.Attachments.ElementAt(0).Url;
                else if (m.Attachments.ElementAt(0).Filename.EndsWith(".gif"))
                    pic = m.Attachments.ElementAt(0).Url;
            }
            string picLink = arguments.GetPictureLinkInMessage();
            if (string.IsNullOrWhiteSpace(pic) && picLink != null)
                pic = picLink;
            return pic;
        }
    }
}
