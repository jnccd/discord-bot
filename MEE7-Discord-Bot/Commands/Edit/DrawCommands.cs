using Discord;
using MEE7.Backend.HelperFunctions;
using SkiaSharp;
using static MEE7.Commands.Edit.Edit;

namespace MEE7.Commands.Edit
{
    class DrawCommands : EditCommandProvider
    {
        public string DrawTextDesc = "Draw Text into pic";
        public SKBitmap DrawText(SKBitmap b, IMessage m, string text, SKColor c, int x = 0, int y = 0, float fontSize = 72, string font = "Arial", int maxX = 99999, int maxY = 99999)
        {
            using SKCanvas canvas = new SKCanvas(b);
            SKFont skFont = new SKFont(SKTypeface.FromFamilyName(font), fontSize);
            using SKPaint paint = new SKPaint()
            {
                Color = c,
            };

            canvas.DrawText(text, new SKPoint(x, y), skFont, paint);

            return b;
        }

        public string DrawCircleDesc = "Draw Circle into pic";
        public SKBitmap DrawCircle(SKBitmap b, IMessage m, float cx, float cy, float radius, SKColor c)
        {
            using SKCanvas canvas = new SKCanvas(b);

            canvas.DrawCircle(new SKPoint(cx, cy), radius, new SKPaint { Color = c });

            return b;
        }

        public string DrawRectDesc = "Draw Rect into pic";
        public SKBitmap DrawRect(SKBitmap b, IMessage m, SKRect r, SKColor c)
        {
            using SKCanvas canvas = new SKCanvas(b);

            canvas.DrawRect(r, new SKPaint { Color = c });

            return b;
        }

        public string RectDesc = "Get Rect";
        public SKRect Rect(EditNull n, IMessage m, int x, int y, int w, int h)
        {
            return new SKRect(x, y, x + w, y + h);
        }

        public string WhiteDesc = "Get my skin color";
        public SKColor White(EditNull n, IMessage m)
        {
            return SKColor.Parse("FFFFFF");
        }

        public string RedDesc = "Get red";
        public SKColor Red(EditNull n, IMessage m)
        {
            return SKColor.Parse("FF0000");
        }

        public string ColDesc = "Get a color from rgb";
        public SKColor Col(EditNull n, IMessage m, byte r, byte g, byte b)
        {
            return new SKColor(r, g, b);
        }

        public string ColNameDesc = "Get a color from name";
        public SKColor ColName(EditNull n, IMessage m, string name)
        {
            return SKColor.Parse(name);
        }

        public string lerpDesc = "Lerp two colors";
        public SKColor Lerp(EditNull n, IMessage m, SKColor a, SKColor b, float l)
        {
            return a.Lerp(b, l);
        }
    }
}
