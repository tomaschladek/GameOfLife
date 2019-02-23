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

        public GridBitmapWrapper(int resolution)
        {
            Resolution = resolution;
            Width = 1000;
            Height = 1000;
            Source = new WriteableBitmap(
                Width,
                Height,
                96,
                96,
                PixelFormats.Bgr32,
                null);
            DrawAll(Colors.White);
            DrawLines();
        }

        private void DrawLines()
        {
            var color = Colors.AliceBlue;
            var lineThicknes = 1;
            for (var row = 0; row < Width/Resolution; row++)
            {
                DrawPixelsArea(color, 0, Width, row*Resolution, row * Resolution + lineThicknes);
            }
            for (var column = 0; column < Width/Resolution; column++)
            {
                DrawPixelsArea(color, column*Resolution, column * Resolution + lineThicknes, 0, Height);
            }
        }

        private void DrawAll(Color color)
        {
            DrawPixelsArea(color,0,Width,0,Height);
        }

        // The DrawPixel method updates the WriteableBitmap by using
        // unsafe code to write a pixel into the back buffer.
        internal void DrawCell(int x, int y, Color color)
        {
            var columnStart = x / Resolution * Resolution;
            var rowStart = y / Resolution * Resolution;

            DrawPixelsArea(color, columnStart, columnStart+Resolution, rowStart, rowStart+Resolution);
        }

        private void DrawPixelsArea(Color color, int fromColumn, int toColumn, int fromRow, int toRow)
        {
            var red = color.R;
            var green = color.G;
            var blue = color.B;
            try
            {
                // Reserve the back buffer for updates.
                Source.Lock();

                for (int column = fromColumn; column < toColumn; column++)
                {
                    for (int row = fromRow; row < toRow; row++)
                    {
                        DrawPixel(red, green, blue, column, row);
                    }
                }
            }
            finally
            {
                // Release the back buffer and make it available for display.
                Source.Unlock();
            }
        }

        private void DrawPixel(int red, int green, int blue, int column, int row)
        {
            unsafe
            {
                // Get a pointer to the back buffer.
                int pBackBuffer = (int)Source.BackBuffer;

                // Find the address of the pixel to draw.
                pBackBuffer += row * Source.BackBufferStride;
                pBackBuffer += column * 4;

                // Compute the pixel's color.
                int colorData = red << 16; // R
                colorData |= green << 8;   // G
                colorData |= blue << 0;   // B

                // Assign the color data to the pixel.
                *((int*)pBackBuffer) = colorData;
            }

            // Specify the area of the bitmap that changed.
            Source.AddDirtyRect(new Int32Rect(column, row, 1, 1));
        }

        public void ChangeResolution(NodeDto[,] nodes, int resolution)
        {
            Resolution = resolution;
            DrawAll(Colors.White);
            RedrawImage(nodes);
            DrawLines();
        }
        private void RedrawImage(NodeDto[,] nodes)
        {
            for (var row = 0; row < nodes.GetLength(0) && row * (Resolution+1) < Width; row++)
            for (var column = 0; column < nodes.GetLength(1) && column * (Resolution + 1) < Height; column++)
            {
                var node = nodes[row, column];
                DrawCell(row*Resolution,column*Resolution, node?.IsAlive == true ? Colors.CadetBlue : Colors.White);
            }
        }
    }
}