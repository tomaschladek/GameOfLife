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

        public Size Size { get; }

        private const int LineThicknes = 1;

        public GridBitmapWrapper(int resolution, Size size, Point offset)
        {
            Resolution = resolution;
            Size = size;
            Source = new WriteableBitmap(
                (int) Size.Width,
                (int) Size.Height,
                96,
                96,
                PixelFormats.Bgr32,
                null);
            ClearImage();
            DrawLines(offset);
        }

        private void DrawLines(Point offset)
        {
            var color = Colors.AliceBlue;
            var dominantColor = Colors.Aquamarine;

            var shiftRow = (int) offset.Y % 5;
            var shiftColumn = (int) offset.X % 5;
            for (var row = 0; row < Size.Height/Resolution; row++)
            {
                DrawPixelsArea((row + shiftRow) % 5 != 0 ? color : dominantColor, 0, (int) Size.Width, row*Resolution, row * Resolution + LineThicknes);
            }
            for (var column = 0; column < Size.Width /Resolution; column++)
            {
                DrawPixelsArea((column + shiftColumn) % 5 != 0 ? color : dominantColor, column*Resolution, column * Resolution + LineThicknes, 0, (int) Size.Height);
            }
        }

        private void ClearImage()
        {
            DrawPixelsArea(Colors.White, 0, (int) Size.Width, 0, (int) Size.Height);
        }

        void DrawCell(int row, int column, bool isAlive)
        {
            var columnStart = column / Resolution * Resolution;
            var rowStart = row / Resolution * Resolution;
            var color = isAlive ? Colors.CadetBlue : Colors.White;

            DrawPixelsArea(color, columnStart+ LineThicknes, columnStart+Resolution, rowStart + LineThicknes, rowStart+Resolution);
        }


        internal void DrawCellWithCoordinates(int row, int column, bool isAlive, Point offset)
        {
            DrawCell((int)(row - offset.Y * Resolution), (int)(column - offset.X * Resolution), isAlive);
        }

        private void DrawPixelsArea(Color color, int fromColumn, int toColumn, int fromRow, int toRow)
        {
            if (fromColumn > Size.Width || fromColumn < 0 || fromRow < 0 || fromRow > Size.Height) return;

            try
            {
                // Reserve the back buffer for updates.
                Source.Lock();

                var width = (int) Math.Min(Size.Width, toColumn) - fromColumn;
                var height = (int)Math.Min(Size.Height, toRow) - fromRow;

                var stride = width * 4;
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

        public void RedrawImage(BitarrayWrapper nodes, int resolution, Point spaceOffset)
        {
            Resolution = resolution;
            ClearImage();
            DrawImage(nodes, spaceOffset);
            DrawLines(spaceOffset);
        }

        private void DrawImage(BitarrayWrapper nodes, Point spaceOffset)
        {
            for (var index = 0; index < nodes.Length; index++)
            {
                if (nodes[index])
                {
                    var coordinates = nodes.GetCoordinates(index);
                    DrawCell((int) (coordinates.Row * Resolution - spaceOffset.Y* Resolution), (int) (coordinates.Column * Resolution - spaceOffset.X* Resolution), true);
                }
            }
        }
    }
}