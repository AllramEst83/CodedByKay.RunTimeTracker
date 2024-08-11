#if ANDROID26_0_OR_GREATER
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Locations;
using System.Threading.Tasks;
using System;
using Microsoft.Maui.ApplicationModel;
using Android.Media;
using CommunityToolkit.Mvvm.Messaging;
using MasterTemplate.Models;
using System.Security;
using Android.Content.PM;
using System.Runtime.Versioning;

namespace MasterTemplate.Services
{
    /// <summary>
    /// A foreground service that tracks the user's location and distance traveled during a run.
    /// </summary>
    [Service(ForegroundServiceType = ForegroundService.TypeLocation)]
    [SupportedOSPlatform("android26.0")]
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

        /// <summary>
        /// Creates a notification channel for the foreground service.
        /// </summary>
        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelName = "Location Service";
                var channelDescription = "Notification channel for location service";
                NotificationChannel channel = new(ChannelId, channelName, NotificationImportance.Low)
                {
                    Description = channelDescription
                };

                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager?.CreateNotificationChannel(channel);
            }
        }

        /// <summary>
        /// Called when the service is first created.
        /// </summary>
        public override void OnCreate()
        {
            base.OnCreate();
            _locationManager = (LocationManager?)GetSystemService(LocationService);

            ResetState();
        }

        /// <summary>
        /// Binds the service to an intent.
        /// </summary>
        /// <param name="intent">The intent to bind to.</param>
        /// <returns>An IBinder object that the client can use to communicate with the service.</returns>
        public override IBinder? OnBind(Intent? intent)
        {
            return null;
        }

        /// <summary>
        /// Starts the service when the start command is issued.
        /// </summary>
        /// <param name="intent">The intent supplied to startService(Intent).</param>
        /// <param name="flags">Additional data about this start request.</param>
        /// <param name="startId">A unique integer representing this specific request to start.</param>
        /// <returns>The start command result.</returns>
        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            if (!IsPermissionGranted())
            {
                StopSelf(); // Stop service if permissions are not granted
                return StartCommandResult.NotSticky;
            }

            _mediaPlayer ??= new MediaPlayer();

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

            ResetState();

            if (intent == null)
            {
                StopSelf();
                return StartCommandResult.NotSticky;
            }

            _targetDistance = intent.GetDoubleExtra("TargetDistance", 0);
            _locationProvider = LocationManager.GpsProvider;

            try
            {
                _locationManager.RequestLocationUpdates(_locationProvider, 500, 1, this);
            }
            catch (SecurityException)
            {
                StopSelf();
                return StartCommandResult.NotSticky;
            }

            return StartCommandResult.Sticky;
        }

        /// <summary>
        /// Checks if location permissions are granted.
        /// </summary>
        /// <returns>True if permissions are granted; otherwise, false.</returns>
        private bool IsPermissionGranted()
        {
            var fineLocationPermission = Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>().Result;

            return fineLocationPermission == PermissionStatus.Granted;
        }

        /// <summary>
        /// Handles changes to the user's location.
        /// </summary>
        /// <param name="location">The new location of the user.</param>
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

                // Send updated distance to the activity via WeakReferenceMessenger
                WeakReferenceMessenger.Default.Send(new DistanceUpdateMessage(_totalDistance));
            }

            _lastLocation = location;
        }

        /// <summary>
        /// Plays an audio file.
        /// </summary>
        /// <param name="fileName">The name of the audio file to play.</param>
        private void PlayAudioFile(string fileName)
        {
            try
            {
                _mediaPlayer?.Reset();

                var audioAssetStream = Android.App.Application.Context.Assets.OpenFd(fileName);
                if (audioAssetStream != null && _mediaPlayer != null)
                {
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

        /// <summary>
        /// Cleans up the service when it is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            _locationManager?.RemoveUpdates(this);
            _mediaPlayer?.Release();
            _mediaPlayer = null;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu) // API level 33 (Android 13)
            {
                StopForeground(StopForegroundFlags.Remove);
            }
            else
            {
                StopForeground(true); // For older API levels
            }

            StopSelf(); // Stop the service itself

            base.OnDestroy();
        }


        /// <summary>
        /// Resets the state of the service.
        /// </summary>
        private void ResetState()
        {
            _pepTalk1Notified = false;
            _halfwayNotified = false;
            _pepTalk2Notified = false;
            _endNotified = false;
            _totalDistance = 0;
            _lastLocation = null;
        }

        public void OnProviderDisabled(string provider) { }
        public void OnProviderEnabled(string provider) { }
        public void OnStatusChanged(string? provider, [GeneratedEnum] Availability status, Bundle? extras) { }
    }
}
#endif
