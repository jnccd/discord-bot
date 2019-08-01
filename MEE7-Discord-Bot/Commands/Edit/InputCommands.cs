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
            new EditCommand("lastT", "Gets the last messages text", null, typeof(string), new Argument[0], (SocketMessage m, object[] args, object o) => {
                return m.Channel.GetMessagesAsync(2).FlattenAsync().Result.Last().Content;
            }),
            new EditCommand("lastP", "Gets the last messages picture", null, typeof(Bitmap), new Argument[0], (SocketMessage m, object[] args, object o) => {
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
            }),
            new EditCommand("thisT", "Outputs the given text", null, typeof(string), new Argument[] { new Argument("Text", typeof(string), null) }, 
                (SocketMessage m, object[] args, object o) => {
                
                return args[0];
            }),
            new EditCommand("thisP", "Gets attatched picture / picture from url argument", null, typeof(Bitmap), 
                new Argument[] { new Argument("Picture URL", typeof(string), null) }, 
                (SocketMessage m, object[] args, object o) => {
                
                return GetPictureLinkFromMessage(m, (string)args[0]).GetBitmapFromURL();
            }),
            new EditCommand("thisG", "Gets attatched gif / gif from url argument", null, typeof(Bitmap[]),
                new Argument[] { new Argument("Gif URL", typeof(string), null) },
                (SocketMessage m, object[] args, object o) => {

                return GetPictureLinkFromMessage(m, (string)args[0]).GetBitmapsFromGIFURL();
            }),
            new EditCommand("thisA", "Gets mp3 or wav audio files attached to this message", null, typeof(WaveStream), new Argument[0],
                (SocketMessage m, object[] args, object o) => {
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
            }),
            new EditCommand("profilePicture", "Gets a profile picture", null, typeof(Bitmap),
                new Argument[] { new Argument("User ID / User Mention", typeof(string), null) },
                (SocketMessage m, object[] args, object o) => {

                SocketUser luser = Program.GetUserFromId(Convert.ToUInt64((args[0] as string).Trim(new char[] { ' ', '<', '>', '@', '!' })));
                string avatarURL = luser.GetAvatarUrl(ImageFormat.Png, 512);
                return (string.IsNullOrWhiteSpace(avatarURL) ? luser.GetDefaultAvatarUrl() : avatarURL).GetBitmapFromURL();
            }),
            new EditCommand("serverPicture", "Gets the server picture from a server id", null, typeof(Bitmap),
                new Argument[] { new Argument("Server ID", typeof(string), null) },
                (SocketMessage m, object[] args, object o) => {
                    if (args[0] as string == "")
                        return Program.GetGuildFromChannel(m.Channel).IconUrl.GetBitmapFromURL();
                    else
                        return Program.GetGuildFromID(Convert.ToUInt64((args[0] as string).Trim(new char[] { ' ', '<', '>', '@', '!' }))).IconUrl.GetBitmapFromURL();
            }),
            new EditCommand("emote", "Gets the picture of the emote", null, typeof(Bitmap),
                new Argument[] { new Argument("Emote", typeof(string), null) },
                (SocketMessage m, object[] args, object o) => {

                Emote.TryParse((args[0] as string).Trim(' '), out Emote res);
                if (res != null) return res.Url.GetBitmapFromURL();
                return Program.GetGuildFromChannel(m.Channel).Emotes.FirstOrDefault(x => x.Name.Contains((args[0] as string).Trim(' ', ':'))).Url.GetBitmapFromURL();
            }),
            new EditCommand("gifEmote", "Gets the pictures of the emote", null, typeof(Bitmap[]),
                new Argument[] { new Argument("Emote", typeof(string), null) },
                (SocketMessage m, object[] args, object o) => {

                Emote.TryParse((args[0] as string).Trim(' '), out Emote res);
                if (res != null) return res.Url.GetBitmapsFromGIFURL();
                return Program.GetGuildFromChannel(m.Channel).Emotes.FirstOrDefault(x => x.Name.Contains((args[0] as string).Trim(' ', ':')) && x.Animated).Url.GetBitmapsFromGIFURL();
            }),
            new EditCommand("AudioFromYT", "Gets the mp3 of an youtube video", null, typeof(WaveStream),
                new Argument[] { new Argument("YouTube Video URL", typeof(string), null) },
                (SocketMessage m, object[] args, object o) => {

                    MemoryStream mem = new MemoryStream();
                    using (Process P = MultiMediaHelper.GetAudioStreamFromYouTubeVideo((args[0] as string), "mp3"))
                    {
                        P.StandardOutput.BaseStream.CopyTo(mem);
                        return WaveFormatConversionStream.CreatePcmStream(new StreamMediaFoundationReader(mem));
                    }
            }),
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
