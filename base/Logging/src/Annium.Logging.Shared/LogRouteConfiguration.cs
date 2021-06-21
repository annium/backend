namespace Annium.Logging.Shared
{
    public class LogRouteConfiguration
    {
        public int SendDelay { get; }

        public LogRouteConfiguration(int sendDelay)
        {
            SendDelay = sendDelay;
        }
    }
}