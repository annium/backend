using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Annium.Net.Http.Internal;

internal class HttpResponse<T> : HttpResponse, IHttpResponse<T>
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

internal class HttpResponse : IHttpResponse
{
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public HttpStatusCode StatusCode { get; }
    public string StatusText { get; }
    public Uri Uri { get; }
    public HttpResponseHeaders Headers { get; }
    public HttpContent Content { get; }

    public HttpResponse(Uri uri, HttpResponseMessage message)
    {
        IsSuccess = message.IsSuccessStatusCode;
        IsFailure = !message.IsSuccessStatusCode;
        StatusCode = message.StatusCode;
        StatusText = message.ReasonPhrase ?? string.Empty;
        Uri = uri;
        Headers = message.Headers;
        Content = message.Content;
    }

    public HttpResponse(Uri uri, HttpRequestException exception)
    {
        IsSuccess = false;
        IsFailure = true;
        StatusCode = HttpStatusCode.ServiceUnavailable;
        StatusText = "Connection refused";
        Uri = uri;
        using (var message = new HttpResponseMessage()) Headers = message.Headers;
        Content = new StringContent(exception.Message);
    }

    protected HttpResponse(IHttpResponse response)
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