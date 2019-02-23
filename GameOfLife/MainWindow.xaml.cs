using System;
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

        public MainWindow()
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(MyImage, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(MyImage, EdgeMode.Aliased);
        }
    }
}
