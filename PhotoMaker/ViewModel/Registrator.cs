using Microsoft.Extensions.DependencyInjection;

namespace PhotoMaker.Client.ViewModel
{
    internal static class Registrator
    {
        public static IServiceCollection RegisterViewModels(this IServiceCollection services)
        {
            services.AddSingleton<MainWindowVM>();

            return services;
        }
    }
}
