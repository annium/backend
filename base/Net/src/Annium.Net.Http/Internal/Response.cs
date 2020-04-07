using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Annium.Net.Http.Internal
{
    internal class Response<T> : Response, IResponse<T>
    {
        public T Data { get; }

        public Response(
            IResponse message,
            T data
        ) : base(message)
        {
            Data = data;
        }
    }

    internal class Response : IResponse
    {
        public bool IsSuccess { get; }
        public bool IsFailure { get; }
        public HttpStatusCode StatusCode { get; }
        public string StatusText { get; }
        public HttpResponseHeaders Headers { get; }
        public HttpContent Content { get; }

        public Response(HttpResponseMessage message)
        {
            IsSuccess = message.IsSuccessStatusCode;
            IsFailure = !message.IsSuccessStatusCode;
            StatusCode = message.StatusCode;
            StatusText = message.ReasonPhrase;
            Headers = message.Headers;
            Content = message.Content;
        }

        internal Response(IResponse response)
        {
            IsSuccess = response.IsSuccess;
            IsFailure = response.IsFailure;
            StatusCode = response.StatusCode;
            StatusText = response.StatusText;
            Headers = response.Headers;
            Content = response.Content;
        }
    }
}