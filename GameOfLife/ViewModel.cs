using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Prism.Commands;
using Prism.Mvvm;

namespace GameOfLife
{
    public class ViewModel : BindableBase
    {
        private int _resolution = 8;
        private const int MaxResolution = 32;
        private const int MinResolution = 2;
        public ICommand ZoomIn { get; set; }
        public ICommand ZoomOut { get; set; }
        public ICommand ToggleNode { get; set; }
        private ISet<NodeDto> _nodes = new HashSet<NodeDto>();
        private GridBitmap _image;

        public ViewModel()
        {
            ZoomIn = new DelegateCommand(ZoomInExecution);
            ZoomOut = new DelegateCommand(ZoomOutExecution);
            ToggleNode = new PositioningCommand(ToggleNodeExecution);
            _image = new GridBitmap(_resolution);
        }

        private void ToggleNodeExecution(Point point)
        {
            _image.DrawPixels((int) point.X,(int) point.Y,Colors.White);
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
            _image.Resolution = newResolution;
        }

        private void ZoomOutExecution()
        {
            var newResolution = Math.Max(_resolution/2,MinResolution);
            SetResolution(newResolution);
        }

        public string Zoom => Math.Round(Math.Log(_resolution,2)).ToString();

        public WriteableBitmap Source
        {
            get => _image.Source;
        }
    }

    public class NodeDto
    {
        public PositionDto X { get; set; }
        public bool IsAlive { get; set; }

        public NodeDto(PositionDto x, bool isAlive)
        {
            X = x;
            IsAlive = isAlive;
        }
    }

    public class PositionDto
    {
        public int X { get; set; }
        public int Y { get; set; }

        public PositionDto(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
    public class PositioningCommand : ICommand
    {
        private readonly Action<Point> _execution;

        public PositioningCommand(Action<Point> execution)
        {
            _execution = execution;
        }

        public void Execute(object parameter)
        {
            Point mousePos = Mouse.GetPosition((IInputElement)parameter);
            _execution(mousePos);
        }

        public bool CanExecute(object parameter) { return true; }

        public event EventHandler CanExecuteChanged;
    }
}