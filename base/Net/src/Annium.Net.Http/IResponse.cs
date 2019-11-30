using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Annium.Net.Http
{
    public interface IResponse<T> : IResponse
    {
        T Data { get; }
    }

    public interface IResponse
    {
        bool IsSuccess { get; }
        bool IsFailure { get; }
        HttpStatusCode StatusCode { get; }
        string StatusText { get; }
        HttpResponseHeaders Headers { get; }
        HttpContent Content { get; }
    }
}