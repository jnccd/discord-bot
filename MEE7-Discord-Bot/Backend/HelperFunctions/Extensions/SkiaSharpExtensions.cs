using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using SkiaSharp;

namespace MEE7.Backend.HelperFunctions
{
    public static class SkiaSharpExtensions
    {
        public static SKBitmap CropImage(this SKBitmap source, SKRect section, bool dispose = true)
        {
            SKBitmap bmp = new SKBitmap((int)section.Width, (int)section.Height);

            using (SKCanvas canvas = new SKCanvas(bmp))
            {
                canvas.DrawBitmap(source, section);
            }

            if (dispose)
                source.Dispose();

            return bmp;
        }
        public static int GetGrayScale(this SKColor c) => (c.Red + c.Green + c.Blue) / 3;
        public static int GetColorDist(this SKColor c, SKColor C) => Math.Abs(c.Red - C.Red) + Math.Abs(c.Green - C.Green) + Math.Abs(c.Blue - C.Blue);
        public static SKColor HsvToRgb(this Vector3 HSV) // from https://stackoverflow.com/questions/1335426/is-there-a-built-in-c-net-system-api-for-hsv-to-rgb
        {
            double h = HSV.X;
            double S = HSV.Y;
            double V = HSV.Z;

            byte Clamp(int i)
            {
                if (i < 0) return 0;
                if (i > 255) return 255;
                return (byte)i;
            }

            double H = h;
            while (H < 0) { H += 360; }
            ;
            while (H >= 360) { H -= 360; }
            ;
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            return new SKColor(
                Clamp((int)(R * 255.0)),
                Clamp((int)(G * 255.0)),
                Clamp((int)(B * 255.0)));
        }
        public static float GetValue(this SKColor c)
        {
            return new float[] { c.Red / 255f, c.Green / 255f, c.Blue / 255f }.Max();
        }
        public static SKBitmap RotateImage(this SKBitmap b, float AngleInDegrees, SKPoint RotationOrigin)
        {
            SKBitmap re = new SKBitmap(b.Width, b.Height);
            using (SKCanvas canvas = new SKCanvas(re))
            {
                canvas.Translate(RotationOrigin.X, RotationOrigin.Y);
                canvas.RotateDegrees(AngleInDegrees);
                canvas.Translate(-RotationOrigin.X, -RotationOrigin.Y);
                canvas.DrawBitmap(b, new SKPoint(0, 0));
            }
            return re;
        }
        public static SKPoint RotatePointAroundPoint(this SKPoint P, SKPoint RotationOrigin, double angle)
        {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            return new SKPoint((int)(cos * (P.X - RotationOrigin.X) - sin * (P.Y - RotationOrigin.Y) + RotationOrigin.X),
                                (int)(sin * (P.X - RotationOrigin.X) + cos * (P.Y - RotationOrigin.Y) + RotationOrigin.Y));
        }
        public static SKBitmap Stretch(this SKBitmap originalBitmap, int newWidth, int newHeight)
        {
            // Create a new bitmap with the desired dimensions
            var stretchedBitmap = new SKBitmap(newWidth, newHeight, originalBitmap.ColorType, originalBitmap.AlphaType);

            // Create a canvas to draw on the new bitmap
            using (var canvas = new SKCanvas(stretchedBitmap))
            {
                // Define the source and destination rectangles
                SKRect sourceRect = new SKRect(0, 0, originalBitmap.Width, originalBitmap.Height);
                SKRect destRect = new SKRect(0, 0, newWidth, newHeight);

                // Draw the original bitmap, stretched to the new size
                canvas.DrawBitmap(originalBitmap, sourceRect, destRect);
            }

            return stretchedBitmap;
        }
        public static void SetPixel(this Span<byte> pixels, int x, int y, int rowBytes, SKColor c)
        {
            int offset = y * rowBytes + x * 4; // Assuming RGBA8888 format
            pixels[offset + 0] = c.Red;
            pixels[offset + 1] = c.Green;
            pixels[offset + 2] = c.Blue;
            pixels[offset + 3] = c.Alpha;
        }
        public static SKColor GetPixel(this Span<byte> pixels, int x, int y, int rowBytes)
        {
            int offset = y * rowBytes + x * 4; // Assuming RGBA8888 format
            return new SKColor(pixels[offset + 0], pixels[offset + 1], pixels[offset + 2], pixels[offset + 3]);
        }
        public static SKColor Lerp(this SKColor s, SKColor t, float k) // from http://www.java2s.com/example/csharp/system.drawing/lerp-between-two-color.html
        {
            static float keepInIntv(float f)
            {
                if (f < 0) f = 0;
                if (f > 255) f = 255;
                return f;
            }

            var bk = (1 - k);
            var a = s.Alpha * bk + t.Alpha * k;
            var r = s.Red * bk + t.Red * k;
            var g = s.Green * bk + t.Green * k;
            var b = s.Blue * bk + t.Blue * k;
            a = keepInIntv(a);
            r = keepInIntv(r);
            g = keepInIntv(g);
            b = keepInIntv(b);
            return new SKColor((byte)r, (byte)g, (byte)b, (byte)a);
        }
        public static void ColorToHSV(this SKColor color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.Red, Math.Max(color.Green, color.Blue));
            int min = Math.Min(color.Red, Math.Min(color.Green, color.Blue));

            hue = color.Hue;
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }
    }
}
