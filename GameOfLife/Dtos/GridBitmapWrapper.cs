using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GameOfLife.Dtos
{
    public class GridBitmapWrapper
    {
        public WriteableBitmap Source { get; }

        public int Resolution { get; set; }

        public int Width { get; }
        public int Height { get; }

        private const int LineThicknes = 1;

        public GridBitmapWrapper(int resolution, int width, int height)
        {
            Resolution = resolution;
            Width = width;
            Height = height;
            Source = new WriteableBitmap(
                Width,
                Height,
                96,
                96,
                PixelFormats.Bgr32,
                null);
            ClearDrawAll();
            DrawLines();
        }

        private void DrawLines()
        {
            var color = Colors.AliceBlue;
            for (var row = 0; row < Width/Resolution; row++)
            {
                DrawPixelsArea(color, 0, Width, row*Resolution, row * Resolution + LineThicknes);
            }
            for (var column = 0; column < Width/Resolution; column++)
            {
                DrawPixelsArea(color, column*Resolution, column * Resolution + LineThicknes, 0, Height);
            }
        }

        private void ClearDrawAll()
        {
            DrawPixelsArea(Colors.White, 0, Width, 0, Height);
        }

        // The DrawPixel method updates the WriteableBitmap by using
        // unsafe code to write a pixel into the back buffer.
        internal void DrawCell(int row, int column, Color color)
        {
            var columnStart = column / Resolution * Resolution;
            var rowStart = row / Resolution * Resolution;


            DrawPixelsArea(color, columnStart+ LineThicknes, columnStart+Resolution, rowStart + LineThicknes, rowStart+Resolution);
        }

        private void DrawPixelsArea(Color color, int fromColumn, int toColumn, int fromRow, int toRow)
        {
            if (fromColumn > Width || fromColumn < 0 || fromRow < 0 || fromRow > Height) return;

            try
            {
                // Reserve the back buffer for updates.
                Source.Lock();

                var width = Math.Min(Width, toColumn) - fromColumn;
                var height = Math.Min(Height, toRow) - fromRow;

                int stride = width * 4;
                var pixels = Enumerable
                    .Repeat(new []{ color.B, color.G, color.R, color.A }, height * stride/4)
                    .SelectMany(item => item)
                    .ToArray();
                Source.WritePixels(new Int32Rect(fromColumn, fromRow, width, height), pixels, stride, 0);
            }
            finally
            {
                // Release the back buffer and make it available for display.
                Source.Unlock();
            }
        }

        public void ChangeResolution(BitarrayWrapper nodes, int resolution)
        {
            Resolution = resolution;
            ClearDrawAll();
            RedrawImage(nodes);
            DrawLines();
        }
        private void RedrawImage(BitarrayWrapper nodes)
        {
            for (var index = 0; index < nodes.Length; index++)
            {
                if (nodes[index])
                {
                    var coordinates = nodes.GetCoordinates(index);
                    DrawCell(coordinates.Row * Resolution, coordinates.Column * Resolution, Colors.CadetBlue);
                }
                
            }
        }
    }
}