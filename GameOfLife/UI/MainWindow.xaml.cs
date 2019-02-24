using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GameOfLife.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        public MainWindow()
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(MyImage, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(MyImage, EdgeMode.Aliased);
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ((ViewModel) DataContext).Resize.Execute(new Size(ImageHolder.ActualWidth, ImageHolder.ActualHeight));
        }

        private void MyImage_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                ((ViewModel)DataContext).ZoomIn.Execute(null);
            }
            else
            {
                ((ViewModel)DataContext).ZoomOut.Execute(null);
            }
        }
    }
}
