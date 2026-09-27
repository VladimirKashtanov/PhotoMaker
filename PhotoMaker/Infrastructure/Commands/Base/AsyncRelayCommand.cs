using System.Windows.Input;

namespace PhotoMaker.Client.Infrastructure.Commands.Base
{
    internal abstract class AsyncRelayCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public abstract bool CanExecute(object? parameter);

        public abstract Task ExecuteAsync(object? parameter);

        public async void Execute(object? parameter) =>
            await ExecuteAsync(parameter);
    }
}
