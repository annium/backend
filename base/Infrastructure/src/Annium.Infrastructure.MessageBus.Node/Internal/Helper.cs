namespace Annium.Infrastructure.MessageBus.Node.Internal
{
    internal static class Helper
    {
        public static (string, string) GetRequestResponseTopics(string topic) => ($"{topic}-req", $"{topic}-res");
    }
}
