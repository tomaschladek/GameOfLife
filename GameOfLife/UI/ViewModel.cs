using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GameOfLife.Dtos;
using Prism.Commands;
using Prism.Mvvm;

namespace GameOfLife.UI
{
    public class ViewModel : BindableBase
    {
        private int _resolution = 8;
        private const int MaxResolution = 32;
        private const int MinResolution = 2;
        public ICommand ZoomIn { get; set; }
        public ICommand ZoomOut { get; set; }
        public ICommand ToggleNode { get; set; }
        private NodeDto[,] _nodes;
        private GridBitmapWrapper _image;

        public ViewModel()
        {
            ZoomIn = new DelegateCommand(ZoomInExecution);
            ZoomOut = new DelegateCommand(ZoomOutExecution);
            ToggleNode = new PositioningCommand(ToggleNodeExecution);
            var width = 1000;
            _nodes = new NodeDto[width,width];
            _image = new GridBitmapWrapper(_resolution);
        }

        private void ToggleNodeExecution(Point point)
        {
            var x = (int) point.X/_resolution * _resolution;
            var y = (int) point.Y / _resolution * _resolution;
            if (_nodes[x, y] == null)
            {
                _image.DrawCell(x, y, Colors.CadetBlue);
                _nodes[x, y] = new NodeDto(true);
            }
            else if (_nodes[x, y].IsAlive)
            {
                _image.DrawCell(x, y, Colors.White);
                _nodes[x, y].IsAlive = false;
            }
            else
            {
                _image.DrawCell(x, y, Colors.CadetBlue);
                _nodes[x, y].IsAlive = true;
            }
        }

        private void ZoomInExecution()
        {
            var newResolution = Math.Min(_resolution * 2, MaxResolution);
            SetResolution(newResolution);
        }

        private void SetResolution(int newResolution)
        {
            SetProperty(ref _resolution, newResolution);
            RaisePropertyChanged(nameof(Zoom));
            _image.ChangeResolution(_nodes,newResolution);
        }

        private void ZoomOutExecution()
        {
            var newResolution = Math.Max(_resolution/2,MinResolution);
            SetResolution(newResolution);
        }

        public string Zoom => Math.Round(Math.Log(_resolution,2)).ToString(CultureInfo.InvariantCulture);

        public WriteableBitmap Source
        {
            get => _image.Source;
        }
    }
}