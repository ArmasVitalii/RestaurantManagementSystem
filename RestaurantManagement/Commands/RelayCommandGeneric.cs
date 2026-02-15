using System.Windows.Input;

namespace RestaurantManagement.Commands;

public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Predicate<T>? _canExecute;

    public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        if (parameter == null && default(T) == null)
        {
            return _canExecute == null || _canExecute(default!);
        }
        
        if (parameter is T typedParameter)
        {
            return _canExecute == null || _canExecute(typedParameter);
        }
        
        return false;
    }

    public void Execute(object? parameter)
    {
        if (parameter is T typedParameter)
        {
            _execute(typedParameter);
        }
        else if (parameter == null && default(T) == null)
        {
            _execute(default!);
        }
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
} 