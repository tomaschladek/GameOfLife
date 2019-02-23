using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace GameOfLife
{
    public class ViewModel : BindableBase
    {
        private double _resolution = 8;
        private const int MaxResolution = 32;
        private const int MinResolution = 2;
        public ICommand ZoomIn { get; set; }
        public ICommand ZoomOut { get; set; }
        public ICommand AddNode { get; set; }
        private ISet<NodeDto> _nodes = new HashSet<NodeDto>();

        public ViewModel()
        {
            ZoomIn = new DelegateCommand(ZoomInExecution);
            ZoomOut = new DelegateCommand(ZoomOutExecution);
            AddNode = new PositioningCommand(AddNodeExecution);
        }

        private void AddNodeExecution(Point point)
        {
            
        }

        private void ZoomInExecution()
        {
            var newResolution = Math.Round(Math.Min(_resolution*2,MaxResolution));
            SetProperty(ref _resolution, newResolution);
            RaisePropertyChanged(nameof(Zoom));
        }

        private void ZoomOutExecution()
        {
            var newResolution = Math.Round(Math.Max(_resolution/2,MinResolution));
            SetProperty(ref _resolution, newResolution);
            RaisePropertyChanged(nameof(Zoom));
        }

        public string Zoom => Math.Round(Math.Log(_resolution,2)).ToString();
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