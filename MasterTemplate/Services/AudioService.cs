using System.Collections.Concurrent;
using Android.Media;

namespace MasterTemplate.Services
{
    public interface IAudioService
    {
        void PlayAudioFile(string fileName);
        void ReleaseMediaPlayer();
        void DestroyMediaPlayer();
    }

    public class AudioService : IAudioService
    {
        private MediaPlayer? _mediaPlayer = null;
        private readonly ConcurrentQueue<string> _audioQueue = new();
        private readonly object _lock = new();
        private bool _isPlaying = false;

        /// <summary>
        /// Plays an audio file. If another audio is currently playing, the file will be queued.
        /// </summary>
        /// <param name="fileName">The name of the audio file to play.</param>
        public void PlayAudioFile(string fileName)
        {
            _audioQueue.Enqueue(fileName);
            StartPlaybackIfNeeded();
        }

        private void StartPlaybackIfNeeded()
        {
            lock (_lock)
            {
                if (!_isPlaying && _audioQueue.TryDequeue(out string? nextFile))
                {
                    PlayAudio(nextFile);
                }
            }
        }

        private void PlayAudio(string fileName)
        {
            try
            {
                _isPlaying = true;
                _mediaPlayer?.Release();
                _mediaPlayer = new MediaPlayer();

                var appContext = Android.App.Application.Context;
                if (appContext != null)
                {
                    var audioAssetStream = appContext.Assets?.OpenFd(fileName);
                    if (audioAssetStream != null && _mediaPlayer != null)
                    {
                        _mediaPlayer.SetDataSource(audioAssetStream.FileDescriptor, audioAssetStream.StartOffset, audioAssetStream.Length);
                        _mediaPlayer.Prepare();
                        _mediaPlayer.Start();

                        // Subscribe to events
                        _mediaPlayer.Completion += MediaPlayer_Completion;
                        _mediaPlayer.Error += MediaPlayer_Error;
                    }
                    else
                    {
                        Android.Util.Log.Error("AudioService", "Audio asset stream or media player is null.");
                        _isPlaying = false;
                        StartPlaybackIfNeeded();
                    }
                }
                else
                {
                    Android.Util.Log.Error("AudioService", "Application context is null.");
                    _isPlaying = false;
                    StartPlaybackIfNeeded();
                }
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("AudioService", $"Error playing audio file {fileName}: {ex.Message}");
                _isPlaying = false;
                StartPlaybackIfNeeded();
            }
        }

        private void MediaPlayer_Completion(object sender, EventArgs e)
        {
            _mediaPlayer?.Reset();
            _isPlaying = false;
            StartPlaybackIfNeeded();
        }

        private void MediaPlayer_Error(object sender, MediaPlayer.ErrorEventArgs e)
        {
            Android.Util.Log.Error("AudioService", $"MediaPlayer error: {e.What}");
            _mediaPlayer?.Reset();
            _isPlaying = false;
            StartPlaybackIfNeeded();
        }

        public void ReleaseMediaPlayer()
        {
            lock (_lock)
            {
                _mediaPlayer?.Release();
                _mediaPlayer = null;
                _isPlaying = false;
            }
        }

        public void DestroyMediaPlayer()
        {
            lock (_lock)
            {
                _mediaPlayer?.Dispose();
                _mediaPlayer = null;
                _isPlaying = false;
            }
        }
    }
}
