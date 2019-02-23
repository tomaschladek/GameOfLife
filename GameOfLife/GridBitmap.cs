using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GameOfLife
{
    public class GridBitmap
    {
        public WriteableBitmap Source { get; }

        public int Resolution { get; set; }

        public GridBitmap(int resolution)
        {
            Resolution = resolution;
            Source = new WriteableBitmap(
                1000,
                1000,
                96,
                96,
                PixelFormats.Bgr32,
                null);
        }
        // The DrawPixel method updates the WriteableBitmap by using
        // unsafe code to write a pixel into the back buffer.
        internal void DrawPixels(int x, int y, Color color)
        {
            DrawPixels(x, y, color.R, color.G, color.B, Resolution);
        }

        void DrawPixels(int x, int y, int red, int green, int blue, int resolution)
        {
            var columnStart = x / resolution * resolution;
            var rowStart = y / resolution * resolution;

            try
            {
                // Reserve the back buffer for updates.
                Source.Lock();

                for (int rowIndex = 0; rowIndex < resolution; rowIndex++)
                {
                    var row = rowStart + rowIndex;
                    for (int columnIndex = 0; columnIndex < resolution; columnIndex++)
                    {
                        var column = columnStart + columnIndex;
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

        void ErasePixel(Point point)
        {
            byte[] colorData = { 0, 0, 0, 0 }; // B G R

            Int32Rect rect = new Int32Rect(
                    (int) point.X,
                    (int) point.Y,
                    1,
                    1);

            Source.WritePixels(rect, colorData, 4, 0);
        }
    }
}