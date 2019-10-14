using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Annium.Net.Base;

namespace Annium.Net.Http.Internal
{
    internal partial class Request : IRequest
    {
        private static readonly HttpClient defaultClient;

        static Request()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            defaultClient = new HttpClient(handler);
        }

        private HttpClient client = defaultClient;

        private Func<HttpClient> createClient;

        private HttpMethod method = HttpMethod.Get;

        private Uri? baseUri;

        private string? uri;

        private readonly HttpRequestHeaders headers;

        private readonly Dictionary<string, string> parameters = new Dictionary<string, string>();

        private HttpContent? content;

        private Func<IResponse, Task<string>>? getFailureMessage;

        internal Request(Uri baseUri) : this()
        {
            this.baseUri = baseUri;
        }

        internal Request()
        {
            createClient = () => client;
            using var message = new HttpRequestMessage();
            headers = message.Headers;
        }

        private Request(
            HttpClient client,
            Func<HttpClient> createClient,
            HttpMethod method,
            Uri? baseUri,
            string? uri,
            HttpRequestHeaders headers,
            IReadOnlyDictionary<string, string> parameters,
            HttpContent? content,
            Func<IResponse, Task<string>>? getFailureMessage
        )
        {
            this.client = client;
            this.createClient = createClient;
            this.method = method;
            this.baseUri = baseUri;
            this.uri = uri;
            using (var message = new HttpRequestMessage()) this.headers = message.Headers;
            foreach (var (name, values) in headers)
                this.headers.Add(name, values);
            this.parameters = parameters.ToDictionary(p => p.Key, p => p.Value);
            this.content = content;
            this.getFailureMessage = getFailureMessage;
        }

        public IRequest Base(Uri baseUri)
        {
            this.baseUri = baseUri;

            return this;
        }

        public IRequest Base(string baseUri) => Base(new Uri(baseUri));

        public IRequest UseClient(HttpClient client)
        {
            this.client = client;

            return this;
        }

        public IRequest UseClientFactory(Func<HttpClient> createClient)
        {
            this.createClient = createClient;

            return this;
        }

        public IRequest Method(HttpMethod method, Uri uri) => Method(method, uri.ToString());

        public IRequest Method(HttpMethod method, string uri)
        {
            this.method = method;
            this.uri = uri;

            return this;
        }

        public IRequest Param<T>(string key, T value)
        {
            parameters[key] = value?.ToString() ?? string.Empty;

            return this;
        }

        public IRequest Content(HttpContent content)
        {
            this.content = content;

            return this;
        }

        public IRequest Configure(
            Action<IRequest, HttpMethod, Uri, IReadOnlyDictionary<string, string>, HttpRequestHeaders, HttpContent?> configure
        )
        {
            configure(this, method, BuildUri(baseUri ?? createClient().BaseAddress), parameters, headers, content);

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
            this.getFailureMessage = getFailureMessage;

            return this;
        }

        public IRequest Clone() =>
            new Request(client, createClient, method, baseUri, uri, headers, parameters, content, getFailureMessage);

        public async Task<IResponse> RunAsync()
        {
            var client = createClient();

            var message = new HttpRequestMessage { Method = method, RequestUri = BuildUri(baseUri ?? client.BaseAddress) };

            foreach (var (name, values) in headers)
                message.Headers.Add(name, values);

            message.Content = content;

            var response = new Response(await client.SendAsync(message));

            if (!response.IsSuccessStatusCode && getFailureMessage != null)
                throw new HttpRequestException(await getFailureMessage(response));

            return response;
        }

        private Uri BuildUri(Uri baseUri)
        {
            var factory = GetUriFactory(baseUri);

            // add manually defined params to queryBuilder
            foreach (var (name, value) in parameters)
                factory.Param(name, value);

            return factory.Build();
        }

        private UriFactory GetUriFactory(Uri baseUri)
        {
            if (baseUri is null)
            {
                if (string.IsNullOrWhiteSpace(uri))
                    throw new ArgumentException($"Request URI is empty");

                return UriFactory.Base(uri);
            }

            if (string.IsNullOrWhiteSpace(uri))
                return UriFactory.Base(baseUri);

            return UriFactory.Base(baseUri).Path(uri);
        }
    }
}