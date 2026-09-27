using Microsoft.Extensions.DependencyInjection;

namespace PhotoMaker.Client.ViewModel
{
    internal class ViewModelLocator
    {
        public MainWindowVM MainWindowVM => App.Host.Services.GetRequiredService<MainWindowVM>();
    }
}
