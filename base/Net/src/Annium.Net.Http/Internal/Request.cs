using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Annium.Extensions.Primitives;
using Annium.Net.Base;

namespace Annium.Net.Http.Internal
{
    internal partial class Request : IRequest
    {
        private delegate Task<IResponse> Middleware(Func<Task<IResponse>> next, IRequest request, RequestOptions options);

        private static readonly HttpClient DefaultClient;

        static Request()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                MaxConnectionsPerServer = 16
            };

            DefaultClient = new HttpClient(handler);
        }

        public HttpMethod Method { get; private set; } = HttpMethod.Get;
        public Uri Uri => GetUriFactory().Build();
        public IReadOnlyDictionary<string, string> Params => _parameters;
        public HttpContent? Content { get; private set; }
        public bool IsEnsuringSuccess => _getFailureMessage != null;
        private HttpClient _client = DefaultClient;
        private Uri? _baseUri;
        private string? _uri;
        private readonly HttpRequestHeaders _headers;
        private readonly Dictionary<string, string> _parameters = new Dictionary<string, string>();
        private Func<IResponse, Task<string>>? _getFailureMessage;
        private readonly IList<Middleware> _middlewares = new List<Middleware>();

        internal Request(Uri baseUri) : this()
        {
            _baseUri = baseUri;
        }

        internal Request()
        {
            using var message = new HttpRequestMessage();
            _headers = message.Headers;
        }

        private Request(
            HttpClient client,
            HttpMethod method,
            Uri? baseUri,
            string? uri,
            HttpRequestHeaders headers,
            IReadOnlyDictionary<string, string> parameters,
            HttpContent? content,
            Func<IResponse, Task<string>>? getFailureMessage
        )
        {
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

        public IRequest Base(Uri baseUri)
        {
            _baseUri = baseUri;

            return this;
        }

        public IRequest Base(string baseUri) => Base(new Uri(baseUri));

        public IRequest UseClient(HttpClient client)
        {
            _client = client;

            return this;
        }

        public IRequest With(HttpMethod method, Uri uri) => With(method, uri.ToString());

        public IRequest With(HttpMethod method, string uri)
        {
            Method = method;
            _uri = uri;

            return this;
        }

        public IRequest Param<T>(string key, T value)
        {
            _parameters[key] = value?.ToString() ?? string.Empty;

            return this;
        }

        public IRequest Attach(HttpContent content)
        {
            Content = content;

            return this;
        }

        public IRequest EnsureSuccessStatusCode() =>
            EnsureSuccessStatusCode(response => response.Content.ReadAsStringAsync());

        public IRequest EnsureSuccessStatusCode(string message) =>
            EnsureSuccessStatusCode(response => Task.FromResult(message));

        public IRequest EnsureSuccessStatusCode(Func<IResponse, string> getFailureMessage) =>
            EnsureSuccessStatusCode(response => Task.FromResult(getFailureMessage(response)));

        public IRequest EnsureSuccessStatusCode(Func<IResponse, Task<string>> getFailureMessage)
        {
            _getFailureMessage = getFailureMessage;

            return this;
        }

        public IRequest Clone() =>
            new Request(_client, Method, _baseUri, _uri, _headers, _parameters, Content, _getFailureMessage);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<IResponse> RunAsync() => InternalRunAsync(_middlewares.ToArray());

        private async Task<IResponse> InternalRunAsync(Middleware[] middlewares)
        {
            if (middlewares.Length == 0)
                return await InternalRunAsync().ConfigureAwait(false);

            var (middleware, rest) = middlewares;

            Func<Task<IResponse>> next = () => InternalRunAsync(rest);
            var options = new RequestOptions(Method, GetUriFactory().Build(), _parameters, _headers, Content);

            return await middleware(next, this, options).ConfigureAwait(false);
        }

        private async Task<IResponse> InternalRunAsync()
        {
            var message = new HttpRequestMessage { Method = Method, RequestUri = BuildUri() };

            foreach (var (name, values) in _headers)
                message.Headers.Add(name, values);

            message.Content = Content;

            var response = new Response(await _client.SendAsync(message).ConfigureAwait(false));

            if (response.IsFailure && _getFailureMessage != null)
                throw new HttpRequestException(await _getFailureMessage(response).ConfigureAwait(false));

            return response;
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