using Microsoft.Extensions.DependencyInjection;
using PhotoMaker.Client.Services.Interfaces;

namespace PhotoMaker.Client.Services
{
    internal static class Registrator
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<IDialogService, ImageFileDialogService>();
            services.AddSingleton<IFileService, ImageFileService>();

            return services;
        }
    }
}
