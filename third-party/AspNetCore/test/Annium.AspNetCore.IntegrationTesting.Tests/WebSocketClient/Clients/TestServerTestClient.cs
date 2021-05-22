using System;
using System.Threading.Tasks;
using Annium.Infrastructure.WebSockets.Client;

namespace Annium.AspNetCore.IntegrationTesting.WebSocketClient.Clients
{
    public class TestServerTestClient
    {
        public DemoClient Demo { get; }
        public bool IsConnected => _client.IsConnected;
        public event Func<Task> ConnectionLost = () => Task.CompletedTask;
        private readonly ITestClient _client;

        public TestServerTestClient(ITestClient client)
        {
            Demo = new DemoClient(client);
            _client = client;
            _client.ConnectionLost += () => ConnectionLost.Invoke();
        }
    }
}