using System;
using System.Windows;
using System.Windows.Input;

namespace GameOfLife.UI
{
    public class MouseClickPointCommand : ICommand
    {
        private readonly Action<Point> _execution;

        public MouseClickPointCommand(Action<Point> execution)
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

    public class SizeCommand : ICommand
    {
        private readonly Action<Size> _execution;

        public SizeCommand(Action<Size> execution)
        {
            _execution = execution;
        }

        public void Execute(object parameter)
        {
            _execution(parameter is Size size ? size : default(Size));
        }

        public bool CanExecute(object parameter) { return true; }

        public event EventHandler CanExecuteChanged;
    }
}