using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterTemplate.Interfaces;
using MasterTemplate.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MasterTemplate.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly AppSettings _appSettings;

        public SettingsViewModel(IPreferencesService preferencesService, IOptions<AppSettings> appSettings)
        {
            _preferencesService = preferencesService;
            _appSettings = appSettings.Value;

            var kalmanFilterData = _preferencesService.Get<KalmanFilterData>(_appSettings.KalmanFilterKey);
            if (kalmanFilterData != null)
            {
                BaseQ = kalmanFilterData.BaseQ;
                SmoothingFactor = kalmanFilterData.SmoothingFactor;
            }
        }

        // Observable properties for Kalman filter settings
        [ObservableProperty]
        private double baseQ = 0;

        [ObservableProperty]
        private double q = 0;

        [ObservableProperty]
        private double smoothingFactor;

        public IPreferencesService _preferencesService { get; }

        // RelayCommand for saving settings
        [RelayCommand]
        public async Task Save()
        {
            KalmanFilterData kalmanFilterData = new()
            {
                BaseQ = BaseQ,
                SmoothingFactor = SmoothingFactor
            };

            _preferencesService.Set(_appSettings.KalmanFilterKey, kalmanFilterData);

            await Toast.Make("Kalman filter settings has been saved.", ToastDuration.Long).Show();
        }
    }
}
