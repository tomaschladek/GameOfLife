using System.Windows;
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
    }
}
