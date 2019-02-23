﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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
        private BitarrayWrapper _nodes;
        private GridBitmapWrapper _image;
        private int _aliveCount;
        private int _duration;
        private int _generationCount;
        private bool _isRunning;
        private CancellationTokenSource _token;
        private string _startLabel = "Start";
        private ESpeed _speed = ESpeed.Normal;
        private Visibility _resetVisibility = Visibility.Collapsed;
        private GenerationProcessor _processor;

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
            ToggleNode = new PositioningCommand(ToggleNodeExecution);
            ResetViewExecution();
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
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var position in result.ChangedPositions)
                    {
                        _image.DrawCell(position.Row, position.Column, position.IsAlive ? Colors.CadetBlue : Colors.White);
                    }

                    AliveCount += result.CounterDif;
                });

                conflictNodes = result.ConflictSet;
                sw.Stop();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    GenerationCount++;
                    Duration = (int)sw.ElapsedMilliseconds;
                });
                await Task.Delay((int)Math.Max(0, GetMaxDelayTime() - sw.ElapsedMilliseconds));
            }
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

            var width = 800;
            var factor = 1;
            _processor = new GenerationProcessor(width / factor);
            _nodes = new BitarrayWrapper(width/factor, width / factor);
            _image = new GridBitmapWrapper(_resolution, width, width);
            GenerationCount = 0;
            AliveCount = 0;

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
                _image.DrawCell(rowCoordinate, columnCoordinate, Colors.CadetBlue);
                _nodes[row,column] = true;
                AliveCount++;
            }
            else 
            {
                _image.DrawCell(rowCoordinate, columnCoordinate, Colors.White);
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