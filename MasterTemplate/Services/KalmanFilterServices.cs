namespace MasterTemplate.Services
{
    public interface IKalmanFilterService
    {
        void Process(double lat_measurement, double lng_measurement, double accuracy, long timestamp);
        double Latitude { get; }
        double Longitude { get; }
        double Accuracy { get; }
    }

    public class KalmanFilterService : IKalmanFilterService
    {
        public double Latitude => _latitude;
        public double Longitude => _longitude;
        public double Accuracy => Math.Sqrt(_variance);

        private double _latitude;
        private double _longitude;
        private double _variance;
        private const double MinAccuracy = 1; // Minimum accuracy in meters
        private readonly double _q; // Process noise

        public KalmanFilterService()
        {
            _q = 0.0001; // Default process noise
            _variance = -1;
        }

        public void Process(double lat_measurement, double lng_measurement, double accuracy, long timestamp)
        {
            if (accuracy < MinAccuracy)
                accuracy = MinAccuracy;

            if (_variance < 0)
            {
                // First time initialization
                _latitude = lat_measurement;
                _longitude = lng_measurement;
                _variance = accuracy * accuracy;
            }
            else
            {
                // Kalman gain
                double k = _variance / (_variance + accuracy * accuracy);
                // Update estimate with measurement
                _latitude += k * (lat_measurement - _latitude);
                _longitude += k * (lng_measurement - _longitude);
                // Update the variance
                _variance = (1 - k) * _variance + Math.Abs(_latitude - lat_measurement) * _q;
            }
        }
    }
}