using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        private Point _spaceOffset = new Point(0,0);
        private readonly Size _spaceSize = new Size(8000,8000);

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
            BindingMoveUp = new DelegateCommand(() => MoveViewExecution(false,null));
            BindingMoveDown = new DelegateCommand(() => MoveViewExecution(true,null));
            BindingMoveLeft = new DelegateCommand(() => MoveViewExecution(null,false));
            BindingMoveRight = new DelegateCommand(() => MoveViewExecution(null,true));
            ToggleNode = new MouseClickPointCommand(ToggleNodeExecution);
            Resize = new SizeCommand(ResizeExecution);

            _processor = new GenerationProcessor((int) _spaceSize.Width, (int) _spaceSize.Height);
            ResetViewExecution();
        }

        private void MoveViewExecution(bool? isHorizontal, bool? isVertical)
        {
            
        }

        private void ResizeExecution(Size obj)
        {
            _imageSize.Width = (int) obj.Width;
            _imageSize.Height = (int) obj.Height;
            RecreateImage();
            _image.RedrawImage(_nodes, _resolution);
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
            var conflictNodes = _processor.CreateConflictSet(_nodes);
            while (!_token.IsCancellationRequested)
            {
                sw.Restart();

                var result = _processor.Execute(_nodes, _resolution, conflictNodes);

                DrawPositions(result.ChangedPositions, result.CounterDif);

                conflictNodes = result.ConflictSet;

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

        private void DrawPositions(System.Collections.Concurrent.ConcurrentBag<NodeDto> changedPositions, int counterDif)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var position in changedPositions)
                {
                    _image.DrawCell(position.Row, position.Column, position.IsAlive);
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

            RecreateImage();
        }

        private void RecreateImage()
        {
            _image = new GridBitmapWrapper(_resolution, (int) _imageSize.Width, (int) _imageSize.Height);
            RaisePropertyChanged(nameof(Source));
        }

        private void ToggleNodeExecution(Point point)
        {
            var column = (int) point.X/_resolution;
            var row = (int) point.Y / _resolution;
            var columnCoordinate = column * _resolution;
            var rowCoordinate = row * _resolution;
            if (!_nodes[row,column])
            {
                _image.DrawCell(rowCoordinate, columnCoordinate, true);
                _nodes[row,column] = true;
                AliveCount++;
            }
            else
            {
                _image.DrawCell(rowCoordinate, columnCoordinate, false);
                _nodes[row, column] = false;
                AliveCount--;
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
            _image.RedrawImage(_nodes,newResolution);
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