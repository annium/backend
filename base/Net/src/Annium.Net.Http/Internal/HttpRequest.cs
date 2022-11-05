using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
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
    public Uri Uri => BuildUri();
    public HttpRequestHeaders Headers => _headers;
    public IReadOnlyDictionary<string, StringValues> Params => _parameters;
    public HttpContent? Content { get; private set; }
    public bool IsEnsuringSuccess => _getFailureMessage != null;
    public IHttpContentSerializer ContentSerializer { get; }
    public ILogger<IHttpRequest> Logger { get; }
    private HttpClient _client = DefaultClient;
    private Uri? _baseUri;
    private string? _uri;
    private readonly HttpRequestHeaders _headers;
    private readonly Dictionary<string, StringValues> _parameters = new();
    private Func<IHttpResponse, Task<string>>? _getFailureMessage;
    private readonly IList<Middleware> _middlewares = new List<Middleware>();

    internal HttpRequest(
        IHttpContentSerializer httpContentSerializer,
        ILogger<IHttpRequest> logger,
        Uri baseUri
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
        ILogger<IHttpRequest> logger
    )
    {
        ContentSerializer = httpContentSerializer;
        Logger = logger;
        using var message = new HttpRequestMessage();
        _headers = message.Headers;
    }

    private HttpRequest(
        IHttpContentSerializer httpContentSerializer,
        ILogger<IHttpRequest> logger,
        HttpClient client,
        HttpMethod method,
        Uri? baseUri,
        string? uri,
        HttpRequestHeaders headers,
        IReadOnlyDictionary<string, StringValues> parameters,
        HttpContent? content,
        Func<IHttpResponse, Task<string>>? getFailureMessage,
        IList<Middleware> middlewares
    )
    {
        ContentSerializer = httpContentSerializer;
        Logger = logger;
        _client = client;
        Method = method;
        _baseUri = baseUri;
        _uri = uri;
        using (var message = new HttpRequestMessage()) _headers = message.Headers;
        foreach (var (name, values) in headers)
            _headers.Add(name, values);
        _parameters = parameters.ToDictionary(p => p.Key, p => p.Value);
        Content = content;
        _getFailureMessage = getFailureMessage;
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

    public IHttpRequest Param<T>(string key, List<T> values) => Param(key, values.AsEnumerable());
    public IHttpRequest Param<T>(string key, IList<T> values) => Param(key, values.AsEnumerable());
    public IHttpRequest Param<T>(string key, IReadOnlyList<T> values) => Param(key, values.AsEnumerable());
    public IHttpRequest Param<T>(string key, IReadOnlyCollection<T> values) => Param(key, values.AsEnumerable());
    public IHttpRequest Param<T>(string key, T[] values) => Param(key, values.AsEnumerable());

    public IHttpRequest Param<T>(string key, IEnumerable<T> values)
    {
        _parameters[key] = new StringValues(values.Where(x => x is not null).Select(x => x!.ToString()).ToArray());

        return this;
    }

    public IHttpRequest Param<T>(string key, T value)
    {
        _parameters[key] = value?.ToString() ?? string.Empty;

        return this;
    }

    public IHttpRequest Attach(HttpContent content)
    {
        Content = content;

        return this;
    }

    public IHttpRequest DontEnsureSuccessStatusCode()
    {
        _getFailureMessage = null;

        return this;
    }

    public IHttpRequest EnsureSuccessStatusCode() =>
        EnsureSuccessStatusCode(response => response.Content.ReadAsStringAsync());

    public IHttpRequest EnsureSuccessStatusCode(string message) =>
        EnsureSuccessStatusCode(_ => Task.FromResult(message));

    public IHttpRequest EnsureSuccessStatusCode(Func<IHttpResponse, string> getFailureMessage) =>
        EnsureSuccessStatusCode(response => Task.FromResult(getFailureMessage(response)));

    public IHttpRequest EnsureSuccessStatusCode(Func<IHttpResponse, Task<string>> getFailureMessage)
    {
        _getFailureMessage = getFailureMessage;

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
            _headers,
            _parameters,
            Content,
            _getFailureMessage,
            _middlewares
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<IHttpResponse> RunAsync() => InternalRunAsync(_middlewares.ToArray(), CancellationToken.None);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<IHttpResponse> RunAsync(CancellationToken ct) => InternalRunAsync(_middlewares.ToArray(), ct);

    private async Task<IHttpResponse> InternalRunAsync(Middleware[] middlewares, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return GetRequestCanceledResponse();

        if (middlewares.Length == 0)
            return await InternalRunAsync().ConfigureAwait(false);

        var (middleware, rest) = middlewares;

        Func<Task<IHttpResponse>> next = () => InternalRunAsync(rest, ct);
        var options = new HttpRequestOptions(Method, GetUriFactory().Build(), _parameters, _headers, Content);

        return await middleware(next, this, options).ConfigureAwait(false);
    }

    private async Task<IHttpResponse> InternalRunAsync()
    {
        var message = new HttpRequestMessage { Method = Method, RequestUri = BuildUri() };

        foreach (var (name, values) in _headers)
            message.Headers.Add(name, values);

        message.Content = Content;

        var response = new HttpResponse(await _client.SendAsync(message).ConfigureAwait(false));

        if (response.IsFailure && _getFailureMessage != null)
        {
            var failure = await _getFailureMessage(response).ConfigureAwait(false);
            throw new HttpRequestException(failure);
        }

        return response;
    }

    private IHttpResponse GetRequestCanceledResponse()
    {
        var message = new HttpResponseMessage(HttpStatusCode.RequestTimeout) { ReasonPhrase = "Request canceled" };

        return new HttpResponse(message);
    }

    private Uri BuildUri()
    {
        var factory = GetUriFactory();

        // add manually defined params to queryBuilder
        foreach (var (name, value) in _parameters)
            factory.Param(name, value);

        return factory.Build();
    }

    private UriFactory GetUriFactory()
    {
        var baseUri = _baseUri ?? _client.BaseAddress;
        if (baseUri is null)
        {
            if (string.IsNullOrWhiteSpace(_uri))
                throw new ArgumentException("Request URI is empty");

            return UriFactory.Base(_uri);
        }

        if (string.IsNullOrWhiteSpace(_uri))
            return UriFactory.Base(baseUri);

        return UriFactory.Base(baseUri).Path(_uri);
    }
}