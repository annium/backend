using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Primitives;
using Annium.Net.Base;

namespace Annium.Net.Http.Internal
{
    internal partial class HttpRequest : IHttpRequest
    {
        private delegate Task<IHttpResponse> Middleware(
            Func<Task<IHttpResponse>> next,
            IHttpRequest request,
            HttpRequestOptions options
        );

        private static readonly HttpClient DefaultClient = new HttpClient();

        public HttpMethod Method { get; private set; } = HttpMethod.Get;
        public Uri Uri => GetUriFactory().Build();
        public IReadOnlyDictionary<string, string> Params => _parameters;
        public HttpContent? Content { get; private set; }
        public bool IsEnsuringSuccess => _getFailureMessage != null;
        public IHttpContentSerializer ContentSerializer { get; }
        private HttpClient _client = DefaultClient;
        private Uri? _baseUri;
        private string? _uri;
        private readonly HttpRequestHeaders _headers;
        private readonly Dictionary<string, string> _parameters = new Dictionary<string, string>();
        private Func<IHttpResponse, Task<string>>? _getFailureMessage;
        private readonly IList<Middleware> _middlewares = new List<Middleware>();

        internal HttpRequest(
            IHttpContentSerializer httpContentSerializer,
            Uri baseUri
        ) : this(
            httpContentSerializer
        )
        {
            _baseUri = baseUri;
        }

        internal HttpRequest(
            IHttpContentSerializer httpContentSerializer
        )
        {
            ContentSerializer = httpContentSerializer;
            using var message = new HttpRequestMessage();
            _headers = message.Headers;
        }

        private HttpRequest(
            IHttpContentSerializer httpContentSerializer,
            HttpClient client,
            HttpMethod method,
            Uri? baseUri,
            string? uri,
            HttpRequestHeaders headers,
            IReadOnlyDictionary<string, string> parameters,
            HttpContent? content,
            Func<IHttpResponse, Task<string>>? getFailureMessage
        )
        {
            ContentSerializer = httpContentSerializer;
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

        public IHttpRequest Attach(HttpContent content)
        {
            Content = content;

            return this;
        }

        public IHttpRequest EnsureSuccessStatusCode() =>
            EnsureSuccessStatusCode(response => response.Content.ReadAsStringAsync());

        public IHttpRequest EnsureSuccessStatusCode(string message) =>
            EnsureSuccessStatusCode(response => Task.FromResult(message));

        public IHttpRequest EnsureSuccessStatusCode(Func<IHttpResponse, string> getFailureMessage) =>
            EnsureSuccessStatusCode(response => Task.FromResult(getFailureMessage(response)));

        public IHttpRequest EnsureSuccessStatusCode(Func<IHttpResponse, Task<string>> getFailureMessage)
        {
            _getFailureMessage = getFailureMessage;

            return this;
        }

        public IHttpRequest Clone() =>
            new HttpRequest(ContentSerializer, _client, Method, _baseUri, _uri, _headers, _parameters, Content, _getFailureMessage);

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
                throw new HttpRequestException(await _getFailureMessage(response).ConfigureAwait(false));

            return response;
        }

        private IHttpResponse GetRequestCanceledResponse()
        {
            var message = new HttpResponseMessage(HttpStatusCode.RequestTimeout);
            message.ReasonPhrase = "Request canceled";

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
}