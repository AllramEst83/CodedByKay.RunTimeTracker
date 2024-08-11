#if ANDROID21_0_OR_GREATER
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Locations;
using System.Threading.Tasks;
using System;
using Android.Media;
using Microsoft.Maui.ApplicationModel;
using CommunityToolkit.Mvvm.Messaging;
using MasterTemplate.Models;
using System.Security;
using Android.Content.PM;

namespace MasterTemplate.Services
{
    [Service (ForegroundServiceType = ForegroundService.TypeLocation)]
    public class LocationService : Service, ILocationListener
    {
        private LocationManager? _locationManager = null;
        private string? _locationProvider = null;
        private Android.Locations.Location? _lastLocation = null;
        private MediaPlayer? _mediaPlayer = null;
        private double _targetDistance;
        private double _totalDistance;
        private const int NotificationId = 1000;
        private const string ChannelId = "location_service_channel";

        private bool _pepTalk1Notified = false;
        private bool _halfwayNotified = false;
        private bool _pepTalk2Notified = false;
        private bool _endNotified = false;

        // Create Notification Channel for Android 8.0+
        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelName = "Location Service";
                var channelDescription = "Notification channel for location service";
                var channel = new NotificationChannel(ChannelId, channelName, NotificationImportance.Low)
                {
                    Description = channelDescription
                };

                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }
        }

        public override void OnCreate()
        {
            base.OnCreate();
            _locationManager = (LocationManager?)GetSystemService(LocationService);

            // Reset state
            _pepTalk1Notified = false;
            _halfwayNotified = false;
            _pepTalk2Notified = false;
            _endNotified = false;
        }

        public override IBinder? OnBind(Intent? intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            if (!IsPermissionGranted())
            {
                StopSelf(); // Stop service if permissions are not granted
                return StartCommandResult.NotSticky;
            }

            CreateNotificationChannel();

            var notification = new Notification.Builder(this, ChannelId)
                .SetContentTitle("Tracking Your Run")
                .SetContentText("Your run is being tracked.")
                .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)
                .SetOngoing(true)
                .Build();

            StartForeground(NotificationId, notification);

            if (_locationManager == null)
            {
                _locationManager = (LocationManager?)GetSystemService(LocationService);
                if (_locationManager == null)
                {
                    StopSelf(); // Stop the service if location manager is not available
                    return StartCommandResult.NotSticky;
                }
            }

            _lastLocation = null;
            _totalDistance = 0;
            _pepTalk1Notified = false;
            _halfwayNotified = false;
            _pepTalk2Notified = false;
            _endNotified = false;

            _targetDistance = intent.GetDoubleExtra("TargetDistance", 0);
            _locationProvider = LocationManager.GpsProvider;

            try
            {
                _locationManager.RequestLocationUpdates(_locationProvider, 500, 1, this);
            }
            catch (SecurityException)
            {
                StopSelf();
            }

            return StartCommandResult.Sticky;
        }

        private bool IsPermissionGranted()
        {
            var fineLocationPermission = Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>().Result;

            return fineLocationPermission == PermissionStatus.Granted;
        }


        public void OnLocationChanged(Android.Locations.Location location)
        {
            if (_lastLocation != null)
            {
                _totalDistance += location.DistanceTo(_lastLocation) / 1000.0; // Convert to kilometers

                // Calculate progress as a percentage of the total distance
                double progressPercentage = (_totalDistance / _targetDistance) * 100;

                // Play pep talk 1 at 25% progress
                if (progressPercentage >= 25 && !_pepTalk1Notified)
                {
                    PlayAudioFile("pep_talk1.m4a");
                    _pepTalk1Notified = true;
                }

                // Play halfway audio at 50% progress
                if (progressPercentage >= 50 && !_halfwayNotified)
                {
                    PlayAudioFile("half_way.m4a");
                    _halfwayNotified = true;
                }

                // Play pep talk 2 at 75% progress
                if (progressPercentage >= 75 && !_pepTalk2Notified)
                {
                    PlayAudioFile("pep_talk2.m4a");
                    _pepTalk2Notified = true;
                }

                // Play end audio at 100% progress
                if (progressPercentage >= 100 && !_endNotified)
                {
                    PlayAudioFile("end.m4a");
                    _endNotified = true;

                    WeakReferenceMessenger.Default.Send(new GoalReachedMessage());
                }

                // Log the calculated distance for debugging
                Android.Util.Log.Debug("LocationService", $"Distance Traveled: {_totalDistance} km");

                // Send updated distance to the activity via WeakReferenceMessenger
                WeakReferenceMessenger.Default.Send(new DistanceUpdateMessage(_totalDistance));
            }

            _lastLocation = location;
        }

        private void PlayAudioFile(string fileName)
        {
            try
            {
                _mediaPlayer?.Release(); // Release any previous media player instance

                var audioAssetStream = Android.App.Application.Context.Assets.OpenFd(fileName);
                if (audioAssetStream != null)
                {
                    _mediaPlayer = new MediaPlayer();
                    _mediaPlayer.SetDataSource(audioAssetStream.FileDescriptor, audioAssetStream.StartOffset, audioAssetStream.Length);
                    _mediaPlayer.Prepare();
                    _mediaPlayer.Start();
                }
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("LocationService", $"Error playing audio file {fileName}: {ex.Message}");
            }
        }

        public override void OnDestroy()
        {
            _locationManager?.RemoveUpdates(this);
            _mediaPlayer?.Release();
            _mediaPlayer = null;

            StopForeground(true); // Stop the foreground service
            StopSelf(); // Stop the service itself

            base.OnDestroy();
        }

        public void OnProviderDisabled(string provider) { }
        public void OnProviderEnabled(string provider) { }
        public void OnStatusChanged(string? provider, [GeneratedEnum] Availability status, Bundle? extras) { }
    }
}
#endif
