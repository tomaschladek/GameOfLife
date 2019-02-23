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
        private int _resolution = 32;
        private const int MaxResolution = 128;
        private const int MinResolution = 4;
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
            _nodes = new NodeDto[width/10,width/10];
            _image = new GridBitmapWrapper(_resolution, 1000, 1000);
        }

        private void ToggleNodeExecution(Point point)
        {
            var xIndex = (int) point.X/_resolution;
            var yIndex = (int) point.Y / _resolution;
            var xCoordinate = xIndex * _resolution;
            var yCoordinate = yIndex * _resolution;
            if (_nodes[xIndex, yIndex] == null)
            {
                _image.DrawCell(xCoordinate, yCoordinate, Colors.CadetBlue);
                _nodes[xIndex, yIndex] = new NodeDto(true);
            }
            else if (_nodes[xIndex, yIndex].IsAlive)
            {
                _image.DrawCell(xCoordinate, yCoordinate, Colors.White);
                _nodes[xIndex, yIndex].IsAlive = false;
            }
            else
            {
                _image.DrawCell(xCoordinate, yCoordinate, Colors.CadetBlue);
                _nodes[xIndex, yIndex].IsAlive = true;
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