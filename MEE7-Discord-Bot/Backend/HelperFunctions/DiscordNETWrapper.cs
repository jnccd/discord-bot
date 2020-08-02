using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TweetSharp;

namespace MEE7.Backend.HelperFunctions
{
    public static class DiscordNETWrapper
    {
        public static async Task<IUserMessage> SendFile(string path, IMessageChannel Channel, string text = "")
        {
            Saver.SaveChannel(Channel);
            return await Channel.SendFileAsync(path, text);
        }
        public static async Task<IUserMessage> SendFile(Stream stream, IMessageChannel Channel, string fileEnd, string fileName = "", string text = "")
        {
            Saver.SaveChannel(Channel);
            if (fileName == "")
                fileName = DateTime.Now.ToBinary().ToString();
            stream.Position = 0;
            return await Channel.SendFileAsync(stream, fileName + "." + fileEnd.TrimStart('.'), text);
        }
        public static async Task<IUserMessage> SendBitmap(Bitmap bmp, IMessageChannel Channel, string text = "")
        {
            Saver.SaveChannel(Channel);
            MemoryStream stream = new MemoryStream();
            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return await SendFile(stream, Channel, "png", "", text);
        }
        public static async Task<List<IUserMessage>> SendText(string text, IMessageChannel Channel)
        {
            List<IUserMessage> sendMessages = new List<IUserMessage>();
            Saver.SaveChannel(Channel);
            if (text.Length < 2000)
                sendMessages.Add(await Channel.SendMessageAsync(text));
            else
            {
                while (text.Length > 0)
                {
                    int subLength = Math.Min(1999, text.Length);
                    string sub = text.Substring(0, subLength);
                    sendMessages.Add(await Channel.SendMessageAsync(sub));
                    text = text.Remove(0, subLength);
                }
            }
            return sendMessages;
        }
        public static async Task<List<IUserMessage>> SendText(string text, ulong ChannelID)
        {
            return await DiscordNETWrapper.SendText(text, (ISocketMessageChannel)Program.GetChannelFromID(ChannelID));
        }
        public static async Task<List<IUserMessage>> SendEmbed(EmbedBuilder Embed, IMessageChannel Channel, string text = "")
        {
            if (Embed == null)
                return new List<IUserMessage>();

            if (Embed.Color == null && !(Channel is IDMChannel))
                Embed.Color = Program.GetGuildFromChannel(Channel).GetUser(Program.GetSelf().Id).GetDisplayColor();

            List<IUserMessage> sendMessages = new List<IUserMessage>();
            if ((Embed.Fields == null || Embed.Fields.Count < 25) && Embed.Length < 6000)
                sendMessages.Add(await Channel.SendMessageAsync(text, false, Embed.Build()));
            else if (Embed.Length >= 6000)
            {
                List<EmbedFieldBuilder> Fields = new List<EmbedFieldBuilder>(Embed.Fields);
                while (Fields.Count > 0)
                {
                    EmbedBuilder eb = new EmbedBuilder
                    {
                        Color = Embed.Color,
                        Description = Embed.Description,
                        Author = Embed.Author,
                        Footer = Embed.Footer,
                        ImageUrl = Embed.ImageUrl,
                        ThumbnailUrl = Embed.ThumbnailUrl,
                        Timestamp = Embed.Timestamp,
                        Title = Embed.Title,
                        Url = Embed.Url
                    };
                    for (int i = 0; i < 6 && Fields.Count > 0; i++)
                    {
                        eb.Fields.Add(Fields[0]);
                        Fields.RemoveAt(0);
                    }
                    sendMessages.Add(await Channel.SendMessageAsync(text, false, eb.Build()));
                }
            }
            else
            {
                List<EmbedFieldBuilder> Fields = new List<EmbedFieldBuilder>(Embed.Fields);
                while (Fields.Count > 0)
                {
                    EmbedBuilder eb = new EmbedBuilder
                    {
                        Color = Embed.Color,
                        Description = Embed.Description,
                        Author = Embed.Author,
                        Footer = Embed.Footer,
                        ImageUrl = Embed.ImageUrl,
                        ThumbnailUrl = Embed.ThumbnailUrl,
                        Timestamp = Embed.Timestamp,
                        Title = Embed.Title,
                        Url = Embed.Url
                    };
                    for (int i = 0; i < 25 && Fields.Count > 0; i++)
                    {
                        eb.Fields.Add(Fields[0]);
                        Fields.RemoveAt(0);
                    }
                    sendMessages.Add(await Channel.SendMessageAsync(text, false, eb.Build()));
                }
            }
            Saver.SaveChannel(Channel);
            return sendMessages;
        }

        public static IUser ParseUser(string userText, IMessageChannel c)
        {
            return ParseUser(userText, c.GetUsersAsync().FlattenAsync().Result);
        }

        public static IUser ParseUser(string userText, IEnumerable<IUser> possibleUsers = null)
        {
            try
            {
                if (userText.Contains('@') && !userText.Contains('<') && !userText.Contains('>'))
                {
                    TwitterUser user = Program.twitterService.GetUserProfileFor(new GetUserProfileForOptions(){ ScreenName = userText.Trim(' ', '@') });
                    return new TwitterDiscordUser(user);
                }
                else if (userText.Contains('@'))
                {
                    return Program.GetUserFromId(Convert.ToUInt64(userText.Trim(new char[] { ' ', '<', '>', '@', '!' })));
                }
                else if (userText.Contains('#'))
                {
                    return possibleUsers.First(x => x.ToString() == userText);
                }
                else if (userText.All(x => char.IsDigit(x)))
                {
                    return Program.GetUserFromId(ulong.Parse(userText));
                }
                else
                {
                    List<Tuple<string, IUser>> users = new List<Tuple<string, IUser>>();

                    foreach (var user in possibleUsers)
                    {
                        if (user is SocketGuildUser)
                        {
                            users.Add(new Tuple<string, IUser>((user as SocketGuildUser).Nickname, user));
                        }
                        users.Add(new Tuple<string, IUser>(user.Username, user));
                    }

                    return users.MinElement(x => userText.ModifiedLevenshteinDistance(x.Item1)).Item2;
                }
            }
            catch
            {
                return null;
            }
        }

        public static EmbedBuilder CreateEmbedBuilder(string TitleText = "", string DescText = "", string ImgURL = "", IUser Author = null, string ThumbnailURL = "")
        {
            EmbedBuilder e = new EmbedBuilder();
            e.WithDescription(DescText);
            e.WithImageUrl(ImgURL);
            e.WithTitle(TitleText);
            if (Author != null)
                e.WithAuthor(Author);
            e.WithThumbnailUrl(ThumbnailURL);
            return e;
        }

        public static IEnumerable<IMessage> EnumerateMessages(IMessageChannel channel)
        {
            ulong lastMessageID = 0;
            IEnumerable<IMessage> messages;

            while (true)
            {
                try
                {
                    if (lastMessageID == 0)
                        messages = channel.GetMessagesAsync().FlattenAsync().Result.OfType<IMessage>();
                    else
                        messages = channel.GetMessagesAsync(lastMessageID, Direction.Before, 100).FlattenAsync().Result.OfType<IMessage>();
                }
                catch
                {
                    break;
                }

                if (messages.Count() == 0)
                    break;

                foreach (var message in messages)
                    yield return message;

                lastMessageID = messages.Last().Id;
            }
        }
    }
}
