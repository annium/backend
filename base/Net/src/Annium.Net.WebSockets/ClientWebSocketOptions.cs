using System;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets
{
    public class ClientWebSocketOptions
    {
        public bool ReconnectOnFailure { get; set; }
        public Func<Task>? BeforeReconnect { get; set; }
        public Func<Task>? AfterReconnect { get; set; }
    }
}