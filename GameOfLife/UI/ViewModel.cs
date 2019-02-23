using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
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
        public ICommand ToggleStart { get; set; }
        public ICommand ResetView { get; set; }
        private NodeDto[,] _nodes;
        private GridBitmapWrapper _image;
        private int _aliveCount;
        private int _duration;
        private int _generationCount;
        private bool _isRunning;
        private CancellationTokenSource _token;
        private string _startLabel = "Start";
        private Visibility _resetVisibility = Visibility.Collapsed;

        public ViewModel()
        {
            ZoomIn = new DelegateCommand(ZoomInExecution);
            ZoomOut = new DelegateCommand(ZoomOutExecution);
            ResetView = new DelegateCommand(ResetViewExecution);
            ToggleStart = new DelegateCommand(ToggleStartExecution);
            ToggleNode = new PositioningCommand(ToggleNodeExecution);
            var width = 800;
            var factor = 10;
            _nodes = new NodeDto[width / factor, width / factor];
            _image = new GridBitmapWrapper(_resolution, width, width);
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
            var tempArray = new bool[_nodes.GetLength(0), _nodes.GetLength(1)];
            while (!_token.IsCancellationRequested)
            {
                Stopwatch sw = new Stopwatch();

                sw.Start();

                Parallel.For(0, _nodes.GetLength(0), row =>
                {
                    Parallel.For(0, _nodes.GetLength(1), column =>
                    {
                        tempArray[row,column] = IsAlive(row,column);
                    });
                });
                Parallel.For(0, _nodes.GetLength(0), row =>
                {
                    Parallel.For(0, _nodes.GetLength(1), column =>
                    {
                        if (tempArray[row, column] != (_nodes[row,column]?.IsAlive ?? false))
                        {
                            if (_nodes[row, column] == null)
                            {
                                _nodes[row, column] = new NodeDto(tempArray[row, column]);
                            }
                            else
                            {
                                _nodes[row, column].IsAlive = tempArray[row, column];
                            }
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (tempArray[row, column])
                                {
                                    _image.DrawCell(row * _resolution, column * _resolution, Colors.CadetBlue);
                                    AliveCount++;
                                }
                                else
                                {
                                    _image.DrawCell(row * _resolution, column * _resolution, Colors.White);
                                    AliveCount--;
                                }
                            });
                        }
                    });
                });

                sw.Stop();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    GenerationCount++;
                    Duration = (int) sw.ElapsedMilliseconds;
                });
                await Task.Delay((int) Math.Max(0,100-sw.ElapsedMilliseconds));
            }
        }

        private bool IsAlive(int row, int column)
        {
            var counter = 0;
            for (int rowShift = -1; rowShift < 2 && counter < 4; rowShift++)
            for (int colShift = -1; colShift < 2 && counter < 4; colShift++)
            {
                if (rowShift == 0 && colShift == 0) continue;
                if (row + rowShift >= 0
                    && row + rowShift < _nodes.GetLength(0)
                    && column + colShift >= 0
                    && column + colShift < _nodes.GetLength(1)
                    && _nodes[row + rowShift, column + colShift] != null
                    && _nodes[row + rowShift, column + colShift].IsAlive)
                {
                    counter++;
                }

            }

            if (_nodes[row, column]?.IsAlive == true)
            {
                return counter == 2 || counter == 3;
            }

            return counter == 3;

        }

        private void ResetViewExecution()
        {
            var width = 800;
            _nodes = new NodeDto[width, width];
            _image = new GridBitmapWrapper(_resolution, width, width);
            ResetVisibility = Visibility.Collapsed;
            RaisePropertyChanged(nameof(Source));
            StartLabel = "Start";
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
                AliveCount++;
            }
            else if (_nodes[xIndex, yIndex].IsAlive)
            {
                _image.DrawCell(xCoordinate, yCoordinate, Colors.White);
                _nodes[xIndex, yIndex].IsAlive = false;
                AliveCount--;
            }
            else
            {
                _image.DrawCell(xCoordinate, yCoordinate, Colors.CadetBlue);
                _nodes[xIndex, yIndex].IsAlive = true;
                AliveCount++;
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
        public Visibility ResetVisibility
        {
            get => _resetVisibility;
            set => SetProperty(ref _resetVisibility, value);
        }
    }
}