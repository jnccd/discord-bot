using System;
using System.Drawing;
using MEE7.Backend.HelperFunctions;
using SkiaSharp;

namespace MEE7.Commands.Edit.Resources
{
    // Taken from: https://github.com/TarekVito/Content-Aware-Image-Scaling/tree/master/Content-Aware Image Scaling(Seam Carving)
    class ImageScaler
    {
        SKBitmap userImage;
        int[,] energyImage, minTable;
        SKColor[,] imgColors;
        int newWidth, newHeight;
        const int SCALE_RATE = 1;
        public ImageScaler(SKBitmap userImage, int width, int height)
        {
            this.userImage = userImage;
            newHeight = height;
            newWidth = width;
            fillColors();
            computeEnergy();
        }
        private void fillColors()
        {
            imgColors = new SKColor[userImage.Width, userImage.Height];
            using (var pixmap = userImage.PeekPixels())
            {
                Span<byte> pixels = pixmap.GetPixelSpan();
                for (int y = 0; y < userImage.Height; y++)
                    for (int x = 0; x < userImage.Width; x++)
                    {
                        imgColors[x, y] = pixels.GetPixel(x, y, pixmap.RowBytes);
                    }
            }
        }
        public SKBitmap commitScale()
        {
            computeVert();
            computeHori();
            return createNewImage();
        }
        private SKBitmap createNewImage()
        {
            SKBitmap resultImg = new SKBitmap(newWidth, newHeight);
            using (var pixmap = resultImg.PeekPixels())
            {
                Span<byte> pixels = pixmap.GetPixelSpan();
                for (int y = 0; y < resultImg.Height; y++)
                    for (int x = 0; x < resultImg.Width; x++)
                    {
                        pixels.SetPixel(x, y, pixmap.RowBytes, imgColors[x, y]);
                    }
            }
            return resultImg;
        }
        private SKColor[,] createNewColor(int w, int h)
        {
            SKColor[,] tmp = new SKColor[w, h];
            for (int x = 0; x < w; ++x)
                for (int y = 0; y < h; ++y)
                    tmp[x, y] = imgColors[x, y];
            return tmp;
        }

        private void minSumTableVert()
        {
            int w = energyImage.GetLength(0);
            int h = energyImage.GetLength(1);
            minTable = new int[w, h];
            for (int j = 0; j < h; ++j)
            {
                for (int i = 0; i < w; ++i)
                {
                    minTable[i, j] = energyImage[i, j];
                    if (j != 0)
                        if (i == 0)
                            minTable[i, j] += Math.Min(minTable[i, j - 1], minTable[i + 1, j - 1]);
                        else if (i == w - 1)
                            minTable[i, j] += Math.Min(minTable[i, j - 1], minTable[i - 1, j - 1]);
                        else
                            minTable[i, j] += Math.Min(minTable[i - 1, j - 1], Math.Min(minTable[i, j - 1], minTable[i + 1, j - 1]));
                }
            }
        }
        private void minSumTableHori()
        {
            int w = energyImage.GetLength(0);
            int h = energyImage.GetLength(1);
            minTable = new int[w, h];
            for (int i = 0; i < w; ++i)
            {
                for (int j = 0; j < h; ++j)
                {
                    minTable[i, j] = energyImage[i, j];
                    if (i != 0)
                        if (j == 0)
                            minTable[i, j] += Math.Min(minTable[i - 1, j], minTable[i - 1, j + 1]);
                        else if (j == h - 1)
                            minTable[i, j] += Math.Min(minTable[i - 1, j], minTable[i - 1, j - 1]);
                        else
                            minTable[i, j] += Math.Min(minTable[i - 1, j - 1], Math.Min(minTable[i - 1, j], minTable[i - 1, j + 1]));
                }
            }
        }
        private void decreaseHeight(int rate)
        {
            computeEnergy();
            minSumTableHori();
            for (int r = 0; r < rate; ++r)
            {
                int minCol = 0;
                int w = minTable.GetLength(0);
                int h = minTable.GetLength(1) - r;
                for (int y = 0; y < h; ++y)
                    if (minTable[w - 1, minCol] > minTable[w - 1, y])
                        minCol = y;

                for (int x = w - 1; x >= 0; --x)
                {
                    int inc = 0;
                    for (int y = 0; y < h - 1; ++y)
                    {
                        if (minCol == y)
                            inc = 1;
                        imgColors[x, y] = imgColors[x, y + inc];
                        minTable[x, y] = minTable[x, y + inc];
                    }
                    if (x > 0)
                    {
                        if (minCol > 0 && minTable[x - 1, minCol] > minTable[x - 1, minCol - 1])
                            minCol = minCol - 1;
                        if (minCol < w - 1 && minTable[x - 1, minCol] > minTable[x - 1, minCol])
                            minCol = minCol + 1;
                    }
                }
            }
            imgColors = createNewColor(imgColors.GetLength(0), imgColors.GetLength(1) - rate);
        }
        private void decreaseWidth(int rate)
        {
            computeEnergy();
            minSumTableVert();
            for (int r = 0; r < rate; ++r)
            {
                int minRow = 0;
                int w = minTable.GetLength(0) - r;
                int h = minTable.GetLength(1);
                for (int x = 0; x < w; ++x)
                    if (minTable[minRow, h - 1] > minTable[x, h - 1])
                        minRow = x;

                for (int y = h - 1; y >= 0; --y)
                {
                    int inc = 0;
                    for (int x = 0; x < w - 1; ++x)
                    {
                        if (minRow == x)
                            inc = 1;
                        imgColors[x, y] = imgColors[x + inc, y];
                        minTable[x, y] = minTable[x + inc, y];
                    }
                    if (y > 0)
                    {
                        if (minRow > 0 && minTable[minRow, y - 1] > minTable[minRow - 1, y - 1])
                            minRow = minRow - 1;
                        if (minRow < w - 1 && minTable[minRow, y - 1] > minTable[minRow + 1, y - 1])
                            minRow = minRow + 1;
                    }
                }
            }
            imgColors = createNewColor(imgColors.GetLength(0) - rate, imgColors.GetLength(1));
        }


        private void computeHori()
        {
            int H = newHeight;
            if (userImage.Height < SCALE_RATE)
            { decreaseWidth(userImage.Height - newHeight); return; }
            while (userImage.Height > H)
            {
                H += SCALE_RATE;
                decreaseHeight(SCALE_RATE);
            }
            decreaseHeight(H - userImage.Height);
        }
        private void computeVert()
        {
            if (userImage.Width < SCALE_RATE)
            { decreaseWidth(userImage.Width - newWidth); return; }
            int W = newWidth;
            while (userImage.Width > W)
            {
                W += SCALE_RATE;
                decreaseWidth(SCALE_RATE);
            }
            decreaseWidth(W - userImage.Width);
        }


        private void computeEnergy()
        {
            int w = imgColors.GetLength(0);
            int h = imgColors.GetLength(1);
            energyImage = new int[w, h];
            for (int i = 0; i < w; ++i)
                for (int j = 0; j < h; ++j)
                {
                    int val = 0;
                    if (j == 0)
                        val += colorDist(imgColors[i, j], imgColors[i, j + 1]);
                    else if (j == h - 1)
                        val += colorDist(imgColors[i, j], imgColors[i, j - 1]);
                    else
                        val += colorDist(imgColors[i, j - 1], imgColors[i, j + 1]);

                    if (i == 0)
                        val += colorDist(imgColors[i, j], imgColors[i + 1, j]);
                    else if (i == w - 1)
                        val += colorDist(imgColors[i, j], imgColors[i - 1, j]);
                    else
                        val += colorDist(imgColors[i - 1, j], imgColors[i + 1, j]);
                    energyImage[i, j] = val;
                }
        }
        public SKBitmap getEnergyImage()
        {
            double maxVal = 0;
            int w = energyImage.GetLength(0);
            int h = energyImage.GetLength(1);
            for (int i = 0; i < w; ++i)
                for (int j = 0; j < h; ++j)
                    maxVal = Math.Max(maxVal, energyImage[i, j]);

            SKBitmap energyResult = new SKBitmap(w, h);
            using (var pixmap = energyResult.PeekPixels())
            {
                Span<byte> pixels = pixmap.GetPixelSpan();
                for (int i = 0; i < energyResult.Height; i++)
                    for (int j = 0; j < energyResult.Width; j++)
                    {
                        byte map = (byte)(energyImage[i, j] / maxVal * 255.0);
                        energyResult.SetPixel(i, j, new SKColor(map, map, map));
                    }
            }
            return energyResult;
        }
        private int colorDist(SKColor A, SKColor B)
        {
            int red = A.Red - B.Red;
            int green = A.Green - B.Green;
            int blue = A.Blue - B.Blue;
            double sum = red * red + green * green + blue * blue;
            return (int)Math.Sqrt(sum);
        }
    }
}
