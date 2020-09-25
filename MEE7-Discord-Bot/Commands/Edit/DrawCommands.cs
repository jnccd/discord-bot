using Discord;
using MEE7.Backend.HelperFunctions;
using System.Drawing;
using static MEE7.Commands.Edit.Edit;
using Color = System.Drawing.Color;

namespace MEE7.Commands.Edit
{
    class DrawCommands : EditCommandProvider
    {
        public string DrawTextDesc = "Draw Text into pic";
        public Bitmap DrawText(Bitmap b, IMessage m, string text, Color c, int x = 0, int y = 0, float fontSize = 72, string font = "Arial", int maxX = 99999, int maxY = 99999)
        {
            using Graphics g = Graphics.FromImage(b);

            g.DrawString(text, new Font(font, fontSize), new SolidBrush(c), new Rectangle(x, y, maxX, maxY));

            return b;
        }

        public string DrawRectDesc = "Draw Rect into pic";
        public Bitmap DrawRect(Bitmap b, IMessage m, Rectangle r, Color c)
        {
            using Graphics g = Graphics.FromImage(b);

            g.DrawRectangle(new Pen(c), r);

            return b;
        }

        public string WhiteDesc = "Get my skin color";
        public Color White(EditNull n, IMessage m)
        {
            return Color.White;
        }

        public string RedDesc = "Get red";
        public Color Red(EditNull n, IMessage m)
        {
            return Color.Red;
        }

        public string ColDesc = "Get a color from rgb";
        public Color Col(EditNull n, IMessage m, byte r, byte g, byte b)
        {
            return Color.FromArgb(r, g, b);
        }

        public string ColNameDesc = "Get a color from name";
        public Color ColName(EditNull n, IMessage m, string name)
        {
            return Color.FromName(name);
        }

        public string lerpDesc = "Lerp two colors";
        public Color Lerp(EditNull n, IMessage m, Color a, Color b, float l)
        {
            return a.Lerp(b, l);
        }
    }
}
