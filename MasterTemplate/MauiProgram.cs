using CommunityToolkit.Maui;
using MasterTemplate.Helper;
using MasterTemplate.Interfaces;
using MasterTemplate.Models;
using MasterTemplate.Pages;
using MasterTemplate.Platforms.Android;
using MasterTemplate.Services;
using MasterTemplate.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;

namespace MasterTemplate
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureMauiHandlers(handlers =>
                {
                    handlers.AddHandler(typeof(AppShell), typeof(AppShellRenderer));
                })
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });


            var assembly = typeof(MauiProgram).Assembly;
            string configFileName;

#if DEBUG
            configFileName = "MasterTemplate.appsettings.Local.json";
#else
configFileName = "MasterTemplate.appsettings.json";
#endif

            using (Stream? stream = assembly.GetManifestResourceStream(configFileName))
            {
                if (stream != null)
                {
                    var config = new ConfigurationBuilder()
                        .AddJsonStream(stream)
                        .Build();

                    builder.Configuration.AddConfiguration(config);
                }
                else
                {
                    throw new NullReferenceException("Stream can not be null.");
                }
            }

            builder.Services.AddOptions<AppSettings>()
                    .Bind(builder.Configuration.GetSection("ApplicationSettings"));

            builder.Services

                //Services
                .AddSingleton<IUserSecureStorageService, UserSecureStorageService>()
                .AddSingleton<IPreferencesService, PreferencesService>()
                .AddSingleton(AudioManager.Current)
                .AddSingleton<IKalmanFilterService, KalmanFilterService>()
                .AddSingleton<IAudioService, AudioService>()

#if ANDROID21_0_OR_GREATER
                //ViewModels
                .AddSingleton<MainViewModel>()
                  .AddSingleton<SettingsViewModel>()
#endif
                //Pages
                .AddSingleton<MainPage>()
                .AddSingleton<SettingsPage>();
#if DEBUG
            builder.Logging.AddDebug();
#endif
            var app = builder.Build();
            ServiceHelper.Services = app.Services;
            return app;
        }
    }
}
