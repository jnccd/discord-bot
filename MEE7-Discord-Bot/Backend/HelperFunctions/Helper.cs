using AnimatedGif;
using AnimatedGif.ImageSharp;
using Discord.Audio;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static MEE7.Commands.Edit.Edit;

namespace MEE7.Backend.HelperFunctions
{
    public static class Helper
    {
        public static void TimedLock(object @lock, int timeoutMs, Action eitherWayAction) => TimedLock(@lock, timeoutMs, eitherWayAction, eitherWayAction);
        public static void TimedLock(object @lock, int timeoutMs, Action inLockAction, Action onTimeout)
        {
            bool lockTaken = false;
            string outerStackTrace = new StackTrace().ToString();

            try
            {
                Monitor.TryEnter(@lock, timeoutMs, ref lockTaken);

                if (lockTaken)
                {
                    inLockAction();
                }
                else
                {
                    onTimeout();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Helper.Lock: {outerStackTrace}\n{ex}");
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(@lock);
                }
            }
        }
    }
}
