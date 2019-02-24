using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GameOfLife.Dtos;
using GameOfLife.Services;
using Prism.Commands;
using Prism.Mvvm;

namespace GameOfLife.UI
{
    public class ViewModel : BindableBase
    {
        private int _resolutionExponent = 5;
        private int _resolution = 32;
        private const int MaxResolution = 128;
        private const int MinResolution = 4;
        public ICommand ZoomIn { get; set; }
        public ICommand ZoomOut { get; set; }
        public ICommand ToggleNode { get; set; }
        public ICommand ToggleStart { get; set; }
        public ICommand ResetView { get; set; }
        public ICommand ToggleSpeed { get; set; }
        public ICommand Resize { get; set; }
        public ICommand BindingMoveUp { get; set; }
        public ICommand BindingMoveDown { get; set; }
        public ICommand BindingMoveLeft { get; set; }
        public ICommand BindingMoveRight { get; set; }
        private BitarrayWrapper _nodes;
        private GridBitmapWrapper _image;
        private int _aliveCount;
        private int _duration;
        private int _generationCount;
        private bool _isRunning;
        private CancellationTokenSource _token;
        private string _startLabel = "Start";
        private ESpeed _speed = ESpeed.Fast;
        private Visibility _resetVisibility = Visibility.Collapsed;
        private readonly GenerationProcessor _processor;
        private Size _imageSize = new Size(800,800);
        private Point _spaceOffset = new Point(4000,4000);
        private readonly Size _spaceSize = new Size(8000,8000);
        private ConcurrentBag<NodeDto> _conflictNodes;

        public ViewModel()
        {
            ZoomIn = new DelegateCommand(ZoomInExecution);
            ZoomOut = new DelegateCommand(ZoomOutExecution);
            ResetView = new DelegateCommand(() =>
            {
                if (_isRunning)
                {
                    ToggleStartExecution();
                }
                ResetViewExecution();
            });
            ToggleStart = new DelegateCommand(ToggleStartExecution);
            ToggleSpeed = new DelegateCommand(ToggleSpeedExecution);
            BindingMoveUp = new DelegateCommand(() => MoveViewExecution(ExpandDirection.Up));
            BindingMoveDown = new DelegateCommand(() => MoveViewExecution(ExpandDirection.Down));
            BindingMoveLeft = new DelegateCommand(() => MoveViewExecution(ExpandDirection.Left));
            BindingMoveRight = new DelegateCommand(() => MoveViewExecution(ExpandDirection.Right));
            ToggleNode = new MouseClickPointCommand(ToggleNodeExecution);
            Resize = new SizeCommand(ResizeExecution);

            _processor = new GenerationProcessor((int) _spaceSize.Width, (int) _spaceSize.Height);
            ResetViewExecution();
        }

        private void MoveViewExecution(ExpandDirection direction)
        {
            var cellsPerRow = _imageSize.Width / _resolution;
            var cellsPerColumn = _imageSize.Height / _resolution;
            switch (direction)
            {
                case ExpandDirection.Down:
                    _spaceOffset.Y = Math.Min(_spaceSize.Height-_imageSize.Height, Math.Round(cellsPerColumn / 2) + _spaceOffset.Y);
                    break;
                case ExpandDirection.Up:
                    _spaceOffset.Y = Math.Max(0, _spaceOffset.Y - Math.Round(cellsPerColumn / 2));
                    break;
                case ExpandDirection.Left:
                    _spaceOffset.X = Math.Max(0, _spaceOffset.X - Math.Round(cellsPerRow / 2));
                    break;
                case ExpandDirection.Right:
                    _spaceOffset.X = Math.Min(_spaceSize.Width - _imageSize.Width, Math.Round(cellsPerRow / 2) + _spaceOffset.X);
                    break;
            }
            _image.RedrawImage(_conflictNodes,_resolution, _spaceOffset);
            RaisePropertyChanged(nameof(Coordinates));

        }

        private void ResizeExecution(Size obj)
        {
            _imageSize.Width = (int) obj.Width;
            _imageSize.Height = (int) obj.Height;
            RecreateImage();
            _image.RedrawImage(_conflictNodes, _resolution, _spaceOffset);
        }

        private void ToggleSpeedExecution()
        {
            switch (_speed)
            {
                case ESpeed.Normal:
                    _speed = ESpeed.Fast;
                    break;
                case ESpeed.Fast:
                    _speed = ESpeed.Faster;
                    break;
                case ESpeed.Faster:
                    _speed = ESpeed.Normal;
                    break;
            }
            RaisePropertyChanged(nameof(SpeedLabel));
        }

        private void ToggleStartExecution()
        {
            if (!_isRunning)
            {
                _token = new CancellationTokenSource();
                new Task(RunGenerations, _token.Token).Start();
                StartLabel = "Pause";
                ResetVisibility = Visibility.Visible;

            }
            else
            {
                _token.Cancel();
                StartLabel = "Resume";
            }

            _isRunning = !_isRunning;
        }

        private async void RunGenerations()
        {
            var sw = new Stopwatch();
            while (!_token.IsCancellationRequested)
            {
                sw.Restart();

                var result = _processor.Execute(_nodes, _resolution, _conflictNodes);

                DrawPositions(result.ChangedPositions, result.CounterDif);

                _conflictNodes = result.ConflictSet;

                sw.Stop();

                DrawStatusLine(sw.ElapsedMilliseconds);
                await Task.Delay((int)Math.Max(0, GetMaxDelayTime() - sw.ElapsedMilliseconds));
            }
        }

        private void DrawStatusLine(long milisecons)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GenerationCount++;
                Duration = (int)milisecons;
            });
        }

        private void DrawPositions(ConcurrentBag<NodeDto> changedPositions, int counterDif)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var position in changedPositions)
                {
                    _image.DrawCellWithCoordinates(position.Row, position.Column, position.IsAlive,_spaceOffset);
                }

                AliveCount += counterDif;
            });
        }

        private int GetMaxDelayTime()
        {
            switch (_speed)
            {
                case ESpeed.Normal:
                    return 100;
                case ESpeed.Fast:
                    return 50;
            }
            return 0;
        }

        private void ResetViewExecution()
        {
            ResetVisibility = Visibility.Collapsed;
            StartLabel = "Start";
            GenerationCount = 0;
            AliveCount = 0;
            _nodes = new BitarrayWrapper((int) _spaceSize.Width,(int) _spaceSize.Height);
            _conflictNodes = new ConcurrentBag<NodeDto>();

            RecreateImage();
        }

        private void RecreateImage()
        {
            _image = new GridBitmapWrapper(_resolution, new Size(_imageSize.Width, _imageSize.Height), _spaceOffset);
            RaisePropertyChanged(nameof(Source));
        }

        private void ToggleNodeExecution(Point point)
        {
            var column = (int) (point.X / _resolution + _spaceOffset.X);
            var row = (int) (point.Y / _resolution + _spaceOffset.Y);
            var columnCoordinate = (int) (point.X / _resolution * _resolution + _spaceOffset.X*_resolution);
            var rowCoordinate = (int) (point.Y / _resolution * _resolution + _spaceOffset.Y * _resolution);
            if (!_nodes[row,column])
            {
                _image.DrawCellWithCoordinates(rowCoordinate, columnCoordinate, true,_spaceOffset);
                _nodes[row,column] = true;
                AliveCount++;
            }
            else
            {
                _image.DrawCellWithCoordinates(rowCoordinate, columnCoordinate, false, _spaceOffset);
                _nodes[row, column] = false;
                AliveCount--;
            }
            _processor.AddSurroundingToCollection(_nodes,_conflictNodes,(int) (row*_spaceSize.Width+column));
        }

        private void ZoomInExecution()
        {
            ResolutionExponent++;
        }

        private void SetResolution()
        {
            var resolution = Math.Pow(2, _resolutionExponent);
            var resolutionWithinBounds = (int) Math.Max(MinResolution,Math.Min(resolution, MaxResolution));
            SetProperty(ref _resolution, resolutionWithinBounds);
            RaisePropertyChanged(nameof(Zoom));
            _image.RedrawImage(_conflictNodes, resolutionWithinBounds, _spaceOffset);
        }

        private void ZoomOutExecution()
        {
            ResolutionExponent--;
        }

        public string Zoom => Math.Round(Math.Log(_resolution,2)).ToString(CultureInfo.InvariantCulture);

        public int ResolutionExponent
        {
            get => _resolutionExponent;
            set
            {
                SetProperty(ref _resolutionExponent, value);
                SetResolution();
            }
        }

        public WriteableBitmap Source
        {
            get => _image.Source;
        }

        public int AliveCount
        {
            get => _aliveCount;
            set => SetProperty(ref _aliveCount, value);
        }
        public int Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }
        public int GenerationCount
        {
            get => _generationCount;
            set => SetProperty(ref _generationCount, value);
        }
        public string StartLabel
        {
            get => _startLabel;
            set => SetProperty(ref _startLabel, value);
        }
        public string Coordinates
        {
            get => $"{_spaceOffset.X}, {_spaceOffset.Y}";
        }
        public string SpeedLabel
        {
            get
            {
                switch (_speed)
                {
                    case ESpeed.Normal:
                        return ">";
                    case ESpeed.Fast:
                        return ">>";
                    case ESpeed.Faster:
                        return ">>>";
                }

                return "?";
            }
        }

        public Visibility ResetVisibility
        {
            get => _resetVisibility;
            set => SetProperty(ref _resetVisibility, value);
        }
    }

    public enum ESpeed
    {
        Normal,Fast,Faster
    }
}