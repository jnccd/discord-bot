using Discord;
using MEE7.Backend.HelperFunctions;
using MEE7.Commands.MessageDB;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using static MEE7.Commands.Edit.Edit;

namespace MEE7.Commands.Edit
{
    class MessageDBCommands : EditCommandProvider
    {
        public string GetDBDesc = "Get that DB";
        public DBGuild GetDB(EditNull n, IMessage m) => (Program.GetCommandInstance("MessageDB") as MessageDB.MessageDB).GetGuild(m);

        public string GetAllMessagesDesc = "Get those messages";
        public List<DBMessage> GetAllMessages(DBGuild dbGuild, IMessage m)
        {
            List<DBMessage> allGuildMessages = new List<DBMessage>();
            foreach (var channel in dbGuild.TextChannels)
                allGuildMessages.AddRange(channel.Messages);

            if (allGuildMessages.Count == 0)
                throw new Exception("No messages :/");

            return allGuildMessages;
        }

        public string FilterMessagesFromDesc = "Get those messages";
        public List<DBMessage> FilterMessagesFrom(List<DBMessage> messages, IMessage m, IUser user)
        {
            List<DBMessage> allGuildMessages = new List<DBMessage>();
            allGuildMessages.Clear();
            foreach (var message in messages)
                if (message.AuthorId == user.Id)
                    allGuildMessages.Add(message);

            if (allGuildMessages.Count == 0)
                throw new Exception("No messages :/");

            return allGuildMessages;
        }

        public string GetMessagesFromChannelDesc = "Get those messages";
        public List<DBMessage> GetMessagesFromChannel(DBGuild dbGuild, IMessage m, string channelName)
        {
            List<DBMessage> messages = new List<DBMessage>();
            foreach (var message in dbGuild.TextChannels.FirstOrDefault(x => x.Name == channelName).Messages)
                messages.Add(message);

            if (messages.Count == 0)
                throw new Exception("No messages :/");

            return messages;
        }

        public string CountMessagesDesc = "Plot those messages over time";
        public long CountMessages(List<DBMessage> messages, IMessage m) => messages.Count;

        public string PlotMessagesDesc = "Plot those messages over time";
        public Bitmap PlotMessages(List<DBMessage> messages, IMessage m)
        {
            var messagesGroupedByDay = messages.
                OrderBy(x => x.Timestamp).
                Select(x => new Tuple<DBMessage, string>(x, x.Timestamp.ToShortDateString())).
                GroupBy(x => x.Item2).
                Select(x => new Tuple<string, int>(x.Key, x.Count())).
                ToArray();

            Plot plt = new Plot();
            plt.PlotScatter(
                messagesGroupedByDay.
                Select(x => DateTime.Parse(x.Item1).ToOADate()).
                ToArray(),
                messagesGroupedByDay.
                Select(x => (double)x.Item2).
                ToArray());
            plt.Ticks(dateTimeX: true);
            plt.Legend();
            plt.Title("Activity over Time");
            plt.YLabel("Messages send on that day");
            plt.XLabel("Day");

            return plt.GetBitmap();
        }

        public string PlotMessagesIntervalDesc = "Plot those messages over time";
        public Bitmap PlotMessagesInterval(List<DBMessage> messages, IMessage m, DateTime intrStart, DateTime intrEnd)
        {
            var messagesGroupedByDay = messages.
                Where(x => x.Timestamp > intrStart && x.Timestamp < intrEnd).
                OrderBy(x => x.Timestamp).
                Select(x => new Tuple<DBMessage, string>(x, x.Timestamp.ToShortDateString())).
                GroupBy(x => x.Item2).
                Select(x => new Tuple<string, int>(x.Key, x.Count())).
                ToArray();

            Plot plt = new Plot();
            plt.PlotScatter(
                messagesGroupedByDay.
                Select(x => DateTime.Parse(x.Item1).ToOADate()).
                ToArray(),
                messagesGroupedByDay.
                Select(x => (double)x.Item2).
                ToArray());
            plt.Ticks(dateTimeX: true);
            plt.Legend();
            plt.Title("Activity over Time");
            plt.YLabel("Messages send on that day");
            plt.XLabel("Day");

            return plt.GetBitmap();
        }

        public string GetUserDesc = "Gets a user";
        public IUser GetUser(EditNull n, IMessage m, IUser u) => u;
    }
}
