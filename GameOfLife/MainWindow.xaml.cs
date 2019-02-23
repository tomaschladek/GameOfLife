using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GameOfLife
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WriteableBitmap writeableBitmap;

        public MainWindow()
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(MyImage, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(MyImage, EdgeMode.Aliased);

            writeableBitmap = new WriteableBitmap(
                1000,
                1000,
                96,
                96,
                PixelFormats.Bgr32,
                null);

            MyImage.Source = writeableBitmap;

            MyImage.MouseMove += i_MouseMove;
            MyImage.MouseLeftButtonDown += i_MouseLeftButtonDown;
            MyImage.MouseRightButtonDown += i_MouseRightButtonDown;

            MouseWheel += w_MouseWheel;
        }

        // The DrawPixel method updates the WriteableBitmap by using
        // unsafe code to write a pixel into the back buffer.
        void DrawPixel(MouseEventArgs e, Color color)
        {
            DrawPixel(e, color.R,color.G,color.B);
        }
        void DrawPixel(MouseEventArgs e, int red, int green, int blue)
        {
            int columnStart = (int)e.GetPosition(MyImage).X;
            int rowStart = (int)e.GetPosition(MyImage).Y;

            try
            {
                // Reserve the back buffer for updates.
                writeableBitmap.Lock();

                for (int rowIndex = 0; rowIndex < 4; rowIndex++)
                {
                    var row = rowStart + rowIndex;
                    for (int columnIndex = 0; columnIndex < 4; columnIndex++)
                    {
                        var column = columnStart + columnIndex;
                        unsafe
                        {
                            // Get a pointer to the back buffer.
                            int pBackBuffer = (int)writeableBitmap.BackBuffer;

                            // Find the address of the pixel to draw.
                            pBackBuffer += row * writeableBitmap.BackBufferStride;
                            pBackBuffer += column * 4;

                            // Compute the pixel's color.
                            int color_data = red << 16; // R
                            color_data |= green << 8;   // G
                            color_data |= blue << 0;   // B

                            // Assign the color data to the pixel.
                            *((int*)pBackBuffer) = color_data;
                        }

                        // Specify the area of the bitmap that changed.
                        writeableBitmap.AddDirtyRect(new Int32Rect(column, row, 1, 1));
                    }
                }
            }
            finally
            {
                // Release the back buffer and make it available for display.
                writeableBitmap.Unlock();
            }
        }

        void ErasePixel(MouseEventArgs e)
        {
            byte[] ColorData = { 0, 0, 0, 0 }; // B G R

            Int32Rect rect = new Int32Rect(
                    (int)(e.GetPosition(MyImage).X),
                    (int)(e.GetPosition(MyImage).Y),
                    1,
                    1);

            writeableBitmap.WritePixels(rect, ColorData, 4, 0);
        }

        void i_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ErasePixel(e);
        }

        void i_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DrawPixel(e,Colors.White);
        }

        void i_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DrawPixel(e, Colors.White);
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                ErasePixel(e);
            }
        }

        void w_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Matrix m = MyImage.RenderTransform.Value;

            if (e.Delta > 0)
            {
                m.ScaleAt(
                    1.5,
                    1.5,
                    e.GetPosition(this).X,
                    e.GetPosition(this).Y);
            }
            else
            {
                m.ScaleAt(
                    1.0 / 1.5,
                    1.0 / 1.5,
                    e.GetPosition(this).X,
                    e.GetPosition(this).Y);
            }

            MyImage.RenderTransform = new MatrixTransform(m);
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            MyImage.Stretch = Stretch.Fill;
        }
    }
}
