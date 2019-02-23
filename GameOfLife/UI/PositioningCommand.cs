using System;
using System.Windows;
using System.Windows.Input;

namespace GameOfLife.UI
{
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