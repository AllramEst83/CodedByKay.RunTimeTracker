namespace MasterTemplate.Services
{
    /// <summary>
    /// Kalman filter service for processing latitude, longitude, and accuracy.
    /// </summary>
    public interface IKalmanFilterService
    {
        /// <summary>
        /// Processes the measurements and updates latitude, longitude, and accuracy.
        /// </summary>
        /// <param name="lat_measurement">Latitude measurement.</param>
        /// <param name="lng_measurement">Longitude measurement.</param>
        /// <param name="accuracy">Accuracy of the measurement.</param>
        /// <param name="timestamp">Timestamp of the measurement.</param>
        void Process(double lat_measurement, double lng_measurement, double accuracy, long timestamp);

        /// <summary>
        /// Gets the current filtered latitude value.
        /// </summary>
        double Latitude { get; }

        /// <summary>
        /// Gets the current filtered longitude value.
        /// </summary>
        double Longitude { get; }

        /// <summary>
        /// Gets the current accuracy of the filter.
        /// </summary>
        double Accuracy { get; }
    }

    public class KalmanFilterService : IKalmanFilterService
    {
        /// <summary>
        /// Gets the current filtered latitude value.
        /// </summary>
        public double Latitude => _latitude;

        /// <summary>
        /// Gets the current filtered longitude value.
        /// </summary>
        public double Longitude => _longitude;

        /// <summary>
        /// Gets the current accuracy of the filter.
        /// </summary>
        public double Accuracy => Math.Sqrt(_variance);

        private double _latitude;
        private double _longitude;
        private double _variance;
        private const double MinAccuracy = 1; // Minimum accuracy in meters

        // Key Considerations:
        // - _baseQ: Base process noise, controls how much the filter smooths out the measurements.
        //   Increase this value to reduce over-smoothing (default = 0.001). Increasing it makes the filter trust new data more.
        private readonly double _baseQ;

        // - _q: Dynamic process noise, adjusts based on the difference between measurements. 
        //   Larger changes result in less smoothing. Adjusts with diff * 0.0001.
        private double _q;

        // - _smoothingFactor: Controls how much trust is placed on the measurements. 
        //   Values closer to 1 give more weight to the measurements (default = 0.9).
        private readonly double _smoothingFactor;

        public KalmanFilterService()
        {
            _baseQ = 0.01; // Slightly increased process noise for improved accuracy
            _variance = -1;
            _smoothingFactor = 0.9; // Smoothing factor: 0.9 means trust measurements slightly more
        }

        /// <summary>
        /// Processes latitude, longitude, and accuracy data using the Kalman filter.
        /// </summary>
        /// <param name="lat_measurement">Latitude measurement.</param>
        /// <param name="lng_measurement">Longitude measurement.</param>
        /// <param name="accuracy">Accuracy of the measurement.</param>
        /// <param name="timestamp">Timestamp of the measurement.</param>
        public void Process(double lat_measurement, double lng_measurement, double accuracy, long timestamp)
        {
            if (accuracy < MinAccuracy)
                accuracy = MinAccuracy;

            if (_variance < 0)
            {
                // First time initialization
                _latitude = lat_measurement;
                _longitude = lng_measurement;
                _variance = Math.Pow(accuracy * 0.5, 2); // Trust initial measurements slightly more
            }
            else
            {
                // Calculate the difference between measurements
                double lat_diff = Math.Abs(lat_measurement - _latitude);
                double lng_diff = Math.Abs(lng_measurement - _longitude);
                double diff = lat_diff + lng_diff;

                // Dynamic process noise based on how much the measurements change
                _q = _baseQ + (diff * 0.0001);

                // Kalman gain
                double k = (_variance / (_variance + (accuracy * accuracy * _smoothingFactor)));

                // Update estimate with measurement
                _latitude += k * (lat_measurement - _latitude);
                _longitude += k * (lng_measurement - _longitude);

                // Update the variance (use dynamic _q)
                _variance = (1 - k) * _variance + _q;
            }
        }
    }
}
