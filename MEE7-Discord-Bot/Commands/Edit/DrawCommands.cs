using Discord;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Color = System.Drawing.Color;

namespace MEE7.Commands.Edit
{
    class DrawCommands : EditCommandProvider
    {
        public string DrawTextDesc = "Draw Text into pic";
        public Bitmap DrawText(Bitmap b, IMessage m, string text, Color c, int x, int y, float fontSize = 72, int maxX = 99999, int maxY = 99999)
        {
            using Graphics g = Graphics.FromImage(b);

            g.DrawString(text, new Font("Arial", fontSize), new SolidBrush(c), new Rectangle(x, y, maxX, maxY));

            return b;
        }
    }
}
