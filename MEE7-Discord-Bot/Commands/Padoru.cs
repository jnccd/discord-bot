using Discord;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MEE7.Commands
{
    class Padoru : Command
    {
        public override void Execute(IMessage message)
        {
            var padoruDay = new DateTime(DateTime.Now.Year, 12, 25, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            var timeToPadoruDay = padoruDay - DateTime.Now;

            if (timeToPadoruDay.Days > 0)
                DiscordNETWrapper.SendText($"Padoruday returns in {timeToPadoruDay.Days} days :c", message.Channel).Wait();
            else if (timeToPadoruDay.Days < 0)
                DiscordNETWrapper.SendText($"Rejoice! Padoruday has already happened", message.Channel).Wait();
            else
                DiscordNETWrapper.SendText($"It's padoruday! <a:padoru:744966713778372778>", message.Channel).Wait();
        }
    }
}
