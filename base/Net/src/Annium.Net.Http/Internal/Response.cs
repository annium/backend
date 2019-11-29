using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Annium.Net.Http.Internal
{
    internal class Response<T> : Response, IResponse<T>
    {
        public T Data { get; }

        public Response(IResponse message, T data)
        : base(message)
        {
            Data = data;
        }
    }

    internal class Response : IResponse
    {
        public HttpStatusCode StatusCode { get; }
        public string ReasonPhrase { get; }
        public bool IsSuccessStatusCode { get; }
        public HttpResponseHeaders Headers { get; }
        public HttpContent Content { get; }

        public Response(HttpResponseMessage message)
        {
            StatusCode = message.StatusCode;
            ReasonPhrase = message.ReasonPhrase;
            IsSuccessStatusCode = message.IsSuccessStatusCode;
            Headers = message.Headers;
            Content = message.Content;
        }

        internal Response(IResponse response)
        {
            StatusCode = response.StatusCode;
            ReasonPhrase = response.ReasonPhrase;
            IsSuccessStatusCode = response.IsSuccessStatusCode;
            Headers = response.Headers;
            Content = response.Content;
        }
    }
}