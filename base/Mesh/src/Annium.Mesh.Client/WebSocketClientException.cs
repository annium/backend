using System;
using Annium.Data.Operations;

namespace Annium.Mesh.Client;

public class WebSocketClientException : Exception
{
    public IResultBase Result { get; }

    public WebSocketClientException(IResultBase result)
    {
        Result = result;
    }
}