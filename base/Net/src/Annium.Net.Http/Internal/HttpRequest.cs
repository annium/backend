using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Base;
using Microsoft.Extensions.Primitives;

namespace Annium.Net.Http.Internal;

internal partial class HttpRequest : IHttpRequest
{
    private delegate Task<IHttpResponse> Middleware(
        Func<Task<IHttpResponse>> next,
        IHttpRequest request,
        HttpRequestOptions options
    );

    private static readonly HttpClient DefaultClient = new();

    public HttpMethod Method { get; private set; } = HttpMethod.Get;
    public Uri Uri => Helper.BuildUri(_client, _baseUri, _uri, _parameters);
    public HttpRequestHeaders Headers { get; }
    public IReadOnlyDictionary<string, StringValues> Params => _parameters;
    public HttpContent? Content { get; private set; }
    public IHttpContentSerializer ContentSerializer { get; }
    public ILogger Logger { get; }
    private Uri Path => Helper.GetUriFactory(_client, _baseUri, _uri).Build();
    private HttpClient _client = DefaultClient;
    private Uri? _baseUri;
    private string? _uri;
    private TimeSpan _timeout = TimeSpan.FromSeconds(30);
    private readonly Dictionary<string, StringValues> _parameters = new();
    private readonly List<Middleware> _middlewares = new();

    internal HttpRequest(
        Uri baseUri,
        IHttpContentSerializer httpContentSerializer,
        ILogger logger
    ) : this(
        httpContentSerializer,
        logger
    )
    {
        Logger = logger;
        _baseUri = baseUri;
    }

    internal HttpRequest(
        IHttpContentSerializer httpContentSerializer,
        ILogger logger
    )
    {
        ContentSerializer = httpContentSerializer;
        Logger = logger;
        using var message = new HttpRequestMessage();
        Headers = message.Headers;
    }

    private HttpRequest(
        IHttpContentSerializer httpContentSerializer,
        ILogger logger,
        HttpClient client,
        HttpMethod method,
        Uri? baseUri,
        string? uri,
        HttpRequestHeaders headers,
        IReadOnlyDictionary<string, StringValues> parameters,
        HttpContent? content,
        List<Middleware> middlewares
    )
    {
        ContentSerializer = httpContentSerializer;
        Logger = logger;
        _client = client;
        Method = method;
        _baseUri = baseUri;
        _uri = uri;
        using (var message = new HttpRequestMessage()) Headers = message.Headers;
        foreach (var (name, values) in headers)
            Headers.Add(name, values);
        _parameters = parameters.ToDictionary(p => p.Key, p => p.Value);
        Content = content;
        _middlewares = middlewares;
    }

    public IHttpRequest Base(Uri baseUri)
    {
        _baseUri = baseUri;

        return this;
    }

    public IHttpRequest Base(string baseUri) => Base(new Uri(baseUri));

    public IHttpRequest UseClient(HttpClient client)
    {
        _client = client;

        return this;
    }

    public IHttpRequest With(HttpMethod method, Uri uri) => With(method, uri.ToString());

    public IHttpRequest With(HttpMethod method, string uri)
    {
        Method = method;
        _uri = uri;

        return this;
    }

    public IHttpRequest Param<T>(string key, T value)
    {
        _parameters[key] = value?.ToString() ?? string.Empty;

        return this;
    }

    public IHttpRequest Param<T>(string key, List<T> values)
    {
        return Param(key, values.AsEnumerable());
    }

    public IHttpRequest Param<T>(string key, IList<T> values)
    {
        return Param(key, values.AsEnumerable());
    }

    public IHttpRequest Param<T>(string key, IReadOnlyList<T> values)
    {
        return Param(key, values.AsEnumerable());
    }

    public IHttpRequest Param<T>(string key, IReadOnlyCollection<T> values)
    {
        return Param(key, values.AsEnumerable());
    }

    public IHttpRequest Param<T>(string key, T[] values)
    {
        return Param(key, values.AsEnumerable());
    }

    public IHttpRequest Param<T>(string key, IEnumerable<T> values)
    {
        var parameters =
        (
            from value in values
            where value is not null
            select value.ToString()
        ).ToArray();

        _parameters[key] = new StringValues(parameters);

        return this;
    }

    public IHttpRequest Attach(HttpContent content)
    {
        Content = content;

        return this;
    }

    public IHttpRequest Clone() =>
        new HttpRequest(
            ContentSerializer,
            Logger,
            _client,
            Method,
            _baseUri,
            _uri,
            Headers,
            _parameters,
            Content,
            _middlewares
        );

    public IHttpRequest Timeout(TimeSpan timeout)
    {
        _timeout = timeout;

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<IHttpResponse> RunAsync(CancellationToken ct = default)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(
            new CancellationTokenSource(_timeout).Token,
            ct
        );

        return _middlewares.Count == 0
            ? InternalRunAsync(cts.Token)
            : InternalRunAsync(0, cts.Token);
    }

    private async Task<IHttpResponse> InternalRunAsync(int middlewareIndex, CancellationToken ct)
    {
        if (middlewareIndex >= _middlewares.Count)
            return await InternalRunAsync(ct).ConfigureAwait(false);

        if (ct.IsCancellationRequested)
            return new HttpResponse(
                Uri,
                new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                {
                    ReasonPhrase = "Request canceled"
                }
            );

        var options = new HttpRequestOptions(Method, Path, _parameters, Headers, Content);
        var middleware = _middlewares[middlewareIndex];

        return await middleware(
            () => InternalRunAsync(middlewareIndex + 1, ct),
            this,
            options
        ).ConfigureAwait(false);
    }

    private async Task<IHttpResponse> InternalRunAsync(CancellationToken ct)
    {
        var uri = Uri;
        var requestMessage = new HttpRequestMessage { Method = Method, RequestUri = uri };

        foreach (var (name, values) in Headers)
            requestMessage.Headers.Add(name, values);

        requestMessage.Content = Content;

        try
        {
            var responseMessage = await _client.SendAsync(requestMessage, ct).ConfigureAwait(false);
            var response = new HttpResponse(uri, responseMessage);

            return response;
        }
        catch (HttpRequestException e)
        {
            return new HttpResponse(
                false,
                uri,
                HttpStatusCode.ServiceUnavailable,
                "Connection refused",
                e.Message
            );
        }
        catch (TaskCanceledException e)
        {
            return new HttpResponse(
                false,
                uri,
                HttpStatusCode.GatewayTimeout,
                "Request canceled",
                e.Message
            );
        }
    }
}

file static class Helper
{
    public static Uri BuildUri(
        HttpClient client,
        Uri? baseUri,
        string? uri,
        IReadOnlyDictionary<string, StringValues> parameters
    )
    {
        var factory = GetUriFactory(client, baseUri, uri);

        // add manually defined params to queryBuilder
        foreach (var (name, value) in parameters)
            factory.Param(name, value);

        return factory.Build();
    }

    public static UriFactory GetUriFactory(
        HttpClient client,
        Uri? baseUri,
        string? uri
    )
    {
        var baseAddress = baseUri ?? client.BaseAddress;
        if (baseAddress is null)
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentException("Request URI is empty");

            return UriFactory.Base(uri);
        }

        if (string.IsNullOrWhiteSpace(uri))
            return UriFactory.Base(baseAddress);

        return UriFactory.Base(baseAddress).Path(uri);
    }
}