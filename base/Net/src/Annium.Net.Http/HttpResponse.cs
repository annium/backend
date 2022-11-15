using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Annium.Net.Http;

public class HttpResponse<T> : HttpResponse, IHttpResponse<T>
{
    public T Data { get; }

    public HttpResponse(
        IHttpResponse message,
        T data
    ) : base(message)
    {
        Data = data;
    }
}

public class HttpResponse : IHttpResponse
{
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public HttpStatusCode StatusCode { get; }
    public string StatusText { get; }
    public Uri Uri { get; }
    public HttpResponseHeaders Headers { get; }
    public HttpContent Content { get; }

    public HttpResponse(HttpResponseMessage message)
    {
        IsSuccess = message.IsSuccessStatusCode;
        IsFailure = !message.IsSuccessStatusCode;
        StatusCode = message.StatusCode;
        StatusText = message.ReasonPhrase!;
        Uri = message.RequestMessage!.RequestUri!;
        Headers = message.Headers;
        Content = message.Content;
    }

    internal HttpResponse(IHttpResponse response)
    {
        IsSuccess = response.IsSuccess;
        IsFailure = response.IsFailure;
        StatusCode = response.StatusCode;
        StatusText = response.StatusText;
        Uri = response.Uri;
        Headers = response.Headers;
        Content = response.Content;
    }
}