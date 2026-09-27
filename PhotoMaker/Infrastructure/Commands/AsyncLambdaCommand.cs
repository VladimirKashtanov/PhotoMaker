using PhotoMaker.Client.Infrastructure.Commands.Base;
using System.Windows.Input;

namespace PhotoMaker.Client.Infrastructure.Commands
{
    internal class AsyncLambdaCommand : AsyncRelayCommand
    {
        private readonly Func<object?, Task> _execute;
        private readonly Func<object?, bool>? _canExecute;
        private bool _isExecuting;

        public AsyncLambdaCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public override bool CanExecute(object? parameter) =>
            !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);

        public override async Task ExecuteAsync(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            try
            {
                _isExecuting = true;
                CommandManager.InvalidateRequerySuggested();
                await _execute(parameter);
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
