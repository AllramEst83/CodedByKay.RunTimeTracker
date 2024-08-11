using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MasterTemplate.Models
{
    public class DistanceUpdateMessage : ValueChangedMessage<double>
    {
        public DistanceUpdateMessage(double distance) : base(distance)
        {
        }
    }
}
