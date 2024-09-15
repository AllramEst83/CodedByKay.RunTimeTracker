using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MasterTemplate.Models;
using System.Timers;
using Timer = System.Timers.Timer;

namespace MasterTemplate.ViewModels
{
    public partial class RunSessionViewModel : ObservableObject,
        IRecipient<DistanceUpdateMessage>,
        IRecipient<GoalReachedMessage>
    {

        // Properties to return dynamic colors based on enabled state
        public Color StopBorderColor => IsUnLocked ? Color.FromArgb("#ff4d4d") : GetDimmedColor(Color.FromArgb("#ff4d4d"));
        public Color LockBorderColor => Color.FromArgb("#fff200");
        public Color StartBorderColor => IsUnLocked ? Color.FromArgb("#32ff7e") : GetDimmedColor(Color.FromArgb("#32ff7e"));

        private bool _isUnLocked = true;
        // IsUnLocked property that notifies changes
        public bool IsUnLocked
        {
            get => _isUnLocked;
            set
            {
                SetProperty(ref _isUnLocked, value);
                // Notify that dependent properties have changed
                OnPropertyChanged(nameof(StopBorderColor));
                OnPropertyChanged(nameof(StartBorderColor));
            }
        }

        private readonly Timer _timer;
        private DateTime _startTime;
        private TimeSpan _totalElapsedTime;

        // Observable Properties

        /// <summary>
        /// The total distance traveled by the user.
        /// </summary>
        [ObservableProperty]
        private double distanceTraveled;

        /// <summary>
        /// The elapsed running time in hh:mm:ss format.
        /// </summary>
        [ObservableProperty]
        private string runningTime = "00:00:00";

        /// <summary>
        /// Indicates whether the session is currently running.
        /// </summary>
        [ObservableProperty]
        private bool isRunning;

        /// <summary>
        /// The text displayed on the Lock/Unlock button.
        /// </summary>
        [ObservableProperty]
        private string lockButtonText = "Lock";

        /// <summary>
        /// The target distance for the run session in kilometers.
        /// </summary>
        [ObservableProperty]
        private double targetDistance = 1; // Default target distance in km

        /// <summary>
        /// The text displayed on the Start/Pause/Resume button.
        /// </summary>
        [ObservableProperty]
        private string startButtonText = "Start";



        // Constructor
        public RunSessionViewModel()
        {
            WeakReferenceMessenger.Default.RegisterAll(this);

            // Initialize and configure the timer
            _timer = new Timer(1000); // 1-second intervals
            _timer.Elapsed += OnTimerElapsed;
        }

        // Relay Commands

        /// <summary>
        /// Handles the Start/Pause functionality based on the current state.
        /// </summary>
        [RelayCommand]
        private void StartPauseResume()
        {
            if (!IsRunning)
            {
                StartSession();
            }
            else
            {
                PauseSession();
            }
        }

        /// <summary>
        /// Stops the run session.
        /// </summary>
        [RelayCommand]
        private void StopSession()
        {
            if (IsRunning)
            {
                EndSession();
            }
        }

        /// <summary>
        /// Toggles the lock state, disabling or enabling start and stop buttons.
        /// </summary>
        [RelayCommand]
        private void LockUnlock()
        {
            IsUnLocked = !IsUnLocked;
            LockButtonText = IsUnLocked ? "Lock" : "Unlock";
        }
        /// <summary>
        /// Starts the run session by initiating the timer and setting the start time.
        /// </summary>
        private void StartSession()
        {
            // Adjust start time to consider the total elapsed time if resuming
            _startTime = DateTime.Now - _totalElapsedTime;
            _timer.Start();
            IsRunning = true;
            StartButtonText = "Pause";
        }

        /// <summary>
        /// Pauses the run session by stopping the timer and recording elapsed time.
        /// </summary>
        private void PauseSession()
        {
            _timer.Stop();
            // Accumulate the elapsed time before pausing
            _totalElapsedTime = DateTime.Now - _startTime;
            StartButtonText = "Start";
            IsRunning = false;
        }

        /// <summary>
        /// Ends the run session by resetting the timer and elapsed time.
        /// </summary>
        private void EndSession()
        {
            _timer.Stop();
            IsRunning = false;
            StartButtonText = "Start";
            // Reset total elapsed time
            _totalElapsedTime = TimeSpan.Zero;
            RunningTime = "00:00:00";
        }



        // Timer Elapsed Event Handler
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var elapsed = DateTime.Now - _startTime;
            RunningTime = elapsed.ToString(@"hh\:mm\:ss");
        }

        // Message Handlers

        /// <summary>
        /// Handles distance updates from the LocationService.
        /// </summary>
        /// <param name="message">The distance update message.</param>
        public void Receive(DistanceUpdateMessage message)
        {
            // Ensure UI updates are on the main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DistanceTraveled = message.Value;
            });
        }

        /// <summary>
        /// Handles the event when the run goal is reached.
        /// </summary>
        /// <param name="message">The goal reached message.</param>
        public void Receive(GoalReachedMessage message)
        {
            // Ensure UI updates are on the main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StopSessionCommand.Execute(null);
                // Optionally, notify the user with a popup or alert
                // e.g., Application.Current.MainPage.DisplayAlert("Goal Reached", "Congratulations!", "OK");
            });
        }

        // Cleanup (Optional, if you plan to dispose ViewModels)
        ~RunSessionViewModel()
        {
            WeakReferenceMessenger.Default.Unregister<DistanceUpdateMessage>(this);
            WeakReferenceMessenger.Default.Unregister<GoalReachedMessage>(this);
            _timer?.Dispose();
        }
        
        /// <summary>
        /// Method to calculate the dimmed color.  Adjusts alpha to make it dimmer
        /// </summary>
        /// <param name="originalColor"></param>
        /// <returns></returns>
        private Color GetDimmedColor(Color originalColor)
        {
            return originalColor.WithAlpha(0.6f);
        }
    }
}
