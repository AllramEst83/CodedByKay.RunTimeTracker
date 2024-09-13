#if ANDROID26_0_OR_GREATER
using Android.Content;
using Java.Lang.Annotation;
using Android.Webkit;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterTemplate.Services;
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using MasterTemplate.Models;
using System.Runtime.Versioning;

namespace MasterTemplate.ViewModels
{
    [SupportedOSPlatform("android26.0")]
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private double _targetDistance = 1;
        
        [ObservableProperty]
        private string _clearedDistance = string.Empty;

        [ObservableProperty]
        private string _goalReached = string.Empty;

        [ObservableProperty]
        private bool _isVisble = true;

        [ObservableProperty]
        private bool _isActive = true;


        [RelayCommand]
        private async Task StartTracking()
        {
           var permissionGranted =  await GetLocationPermission();
            if (!permissionGranted)
            {
                return;
            }

            Intent intent = new Intent(Android.App.Application.Context, typeof(LocationService));
            intent.PutExtra("TargetDistance", TargetDistance);
            Android.App.Application.Context.StartService(intent);

            ClearedDistance = "0 km";
            StartListeningForUpdates();
            IsActive = false;
            IsVisble = false;
        }

        [RelayCommand]
        private void StopTracking()
        {
            Intent intent = new(Android.App.Application.Context, typeof(LocationService));
            Android.App.Application.Context.StopService(intent);

            WeakReferenceMessenger.Default.Unregister<DistanceUpdateMessage>(this);
            WeakReferenceMessenger.Default.Unregister<GoalReachedMessage>(this);
            ClearedDistance = string.Empty;
            GoalReached = string.Empty;
            IsActive = true;
            IsVisble = true;
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
                GoalReached = $"Boom! Bra jobbat! {TargetDistance} km avklarad";
            });
        }

        private async Task<bool> GetLocationPermission()
        {
            // Check and request location permission
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status != PermissionStatus.Granted)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
#endif