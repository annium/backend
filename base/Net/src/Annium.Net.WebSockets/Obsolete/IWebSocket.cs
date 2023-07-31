using System;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets.Obsolete;

[Obsolete]
public interface IWebSocket : ISendingReceivingWebSocket
{
    Task DisconnectAsync();
}