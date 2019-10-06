using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Annium.Net.Http.Internal
{
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
    }
}