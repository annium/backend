using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Core.Primitives.Net;

namespace Annium.Core.Internal.Internal
{
    internal class TcpLogger
    {
        private readonly IPEndPoint _endPoint;
        private readonly Encoding _encoding = Encoding.UTF8;
        private Socket _socket;

        private static readonly Scheduler Scheduler = new();

        public TcpLogger(string endpoint)
        {
            _endPoint = IPEndPointExt.Parse(endpoint);
            _socket = CreateSocket();
        }

        public void Write(string message) => Scheduler.Schedule(async () =>
        {
            var msg = _encoding.GetBytes($"{message}{Environment.NewLine}");
            while (true)
            {
                try
                {
                    await Reconnect();
                    await _socket.SendAsync(msg, SocketFlags.None);
                    return;
                }
                catch
                {
                    // ignored
                }
            }
        });

        private async Task Reconnect()
        {
            while (!_socket.Connected)
            {
                try
                {
                    await Task.Delay(100);
                    await _socket.ConnectAsync(_endPoint);
                }
                catch (SocketException)
                {
                    _socket = RecreateSocket(_socket);
                }
            }
        }

        private static Socket CreateSocket()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                Blocking = false,
                SendTimeout = 100
            };

            return socket;
        }

        private static Socket RecreateSocket(Socket socket)
        {
            socket.Close();
            socket.Dispose();

            return CreateSocket();
        }
    }
}