using Android.Content;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterTemplate.Services;
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using MasterTemplate.Models;

namespace MasterTemplate.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        // Existing properties
        [ObservableProperty]
        private double targetDistance = 1;

        [ObservableProperty]
        private string clearedDistance = string.Empty;

        [ObservableProperty]
        private string goalReached = string.Empty;

        [ObservableProperty]
        private bool isVisible = true;

        [ObservableProperty]
        private bool isActive = true;

        // New properties
        [ObservableProperty]
        private bool isTracking = false;

        public bool IsNotTracking => !IsTracking;

        [ObservableProperty]
        private string displayTargetDistance = string.Empty;

        [RelayCommand]
        private async Task StartTracking()
        {
            var permissionGranted = await GetLocationPermission();
            if (!permissionGranted)
            {
                return;
            }

            Intent intent = new(Android.App.Application.Context, typeof(LocationService));
            intent.PutExtra("TargetDistance", TargetDistance);
            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                Android.App.Application.Context.StartForegroundService(intent);
            }
            else
            {
                Android.App.Application.Context.StartService(intent);
            }

            ClearedDistance = "0 km";
            DisplayTargetDistance = $"{TargetDistance} km";
            StartListeningForUpdates();
            IsActive = false;
            IsVisible = false;
            IsTracking = true;
            OnPropertyChanged(nameof(IsNotTracking));
        }

        [RelayCommand]
        private void StopTracking()
        {
            Intent intent = new Intent(Android.App.Application.Context, typeof(LocationService));
            Android.App.Application.Context.StopService(intent);

            WeakReferenceMessenger.Default.Unregister<DistanceUpdateMessage>(this);
            WeakReferenceMessenger.Default.Unregister<GoalReachedMessage>(this);

            ClearedDistance = string.Empty;
            GoalReached = string.Empty;
            DisplayTargetDistance = string.Empty;
            IsActive = true;
            IsVisible = true;
            IsTracking = false;
            OnPropertyChanged(nameof(IsNotTracking));
        }

        private void StartListeningForUpdates()
        {
            WeakReferenceMessenger.Default.Register<DistanceUpdateMessage>(this, (recipient, message) =>
            {
                double distanceInMeters = message.Value * 1000;

                if (distanceInMeters < 1000)
                {
                    ClearedDistance = $"{distanceInMeters:F2} m";
                }
                else
                {
                    ClearedDistance = $"{message.Value:F2} km";
                }
            });

            WeakReferenceMessenger.Default.Register<GoalReachedMessage>(this, (recipient, message) =>
            {
                GoalReached = $"Boom! Great job! {TargetDistance} km completed!";
            });
        }

        private async Task<bool> GetLocationPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            return status == PermissionStatus.Granted;
        }
    }
}
