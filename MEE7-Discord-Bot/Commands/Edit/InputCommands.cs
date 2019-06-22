using Discord;
using Discord.WebSocket;
using System;
using System.Linq;

namespace MEE7.Commands
{
    public partial class Edit : Command
    {
        readonly EditCommand[] InputCommands = new EditCommand[] {
            new EditCommand("lastT", "Gets the last messages text", (SocketMessage m, string a, object o) => {
                return m.Channel.GetMessagesAsync(2).FlattenAsync().Result.Last().Content;
            }),
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
                string picLink = lm.Content.GetPictureLink();
                if (string.IsNullOrWhiteSpace(pic) && picLink != null)
                    pic = picLink;
                return pic.GetBitmapFromURL();
            }),
            new EditCommand("thisT", "Outputs the given arguments", (SocketMessage m, string a, object o) => {
                return a;
            }),
            new EditCommand("thisP", "Gets this messages picture / picture link in the arguments", (SocketMessage m, string a, object o) => {
                string pic = null;
                if (m.Attachments.Count > 0 && m.Attachments.ElementAt(0).Size > 0)
                {
                    if (m.Attachments.ElementAt(0).Filename.EndsWith(".png"))
                        pic = m.Attachments.ElementAt(0).Url;
                    else if (m.Attachments.ElementAt(0).Filename.EndsWith(".jpg"))
                        pic = m.Attachments.ElementAt(0).Url;
                }
                string picLink = a.GetPictureLink();
                if (string.IsNullOrWhiteSpace(pic) && picLink != null)
                    pic = picLink;
                return pic.GetBitmapFromURL();
            }),
            new EditCommand("thisA", "Gets any audio files attached to this message", (SocketMessage m, string a, object o) => {
                return m.Attachments.First(x => x.Url.EndsWith(".ogg") || x.Url.EndsWith(".wav")  || x.Url.EndsWith(".mp3")); // TODO: fix
            }),
            new EditCommand("profilePicture", "Gets a profile picture", (SocketMessage m, string a, object o) => {
                return Program.GetUserFromId(Convert.ToUInt64((a as string).Trim(new char[] { '<', '>', '@' }))).GetAvatarUrl(ImageFormat.Png, 512).GetBitmapFromURL();
            }),
        };
    }
}
