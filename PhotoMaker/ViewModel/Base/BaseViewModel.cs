using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PhotoMaker.Client.ViewModel.Base
{
    public class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? PropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }


        protected virtual bool SetValue<T>(T currentValue, T newValue, Action<T> setter, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(currentValue, newValue)) return false;
            setter(newValue);
            OnPropertyChanged(propertyName);
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private bool _disposed;
        protected virtual void Dispose(bool Disposing)
        {
            if (!Disposing || _disposed) return;
            _disposed = true;
        }
    }
}
