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
        HttpStatusCode StatusCode { get; }
        string ReasonPhrase { get; }
        bool IsSuccessStatusCode { get; }
        HttpResponseHeaders Headers { get; }
        HttpContent Content { get; }
    }
}