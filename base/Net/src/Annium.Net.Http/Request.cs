using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;

namespace Annium.Net.Http
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

        private readonly IDictionary<string, string> parameters = new Dictionary<string, string>();

        private HttpContent? content;

        private Func<IResponse, Task<string>> ? getFailureMessage;

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
            IDictionary<string, string> parameters,
            HttpContent? content,
            Func<IResponse, Task<string>> ? getFailureMessage
        )
        {
            this.client = client;
            this.createClient = createClient;
            this.method = method;
            this.baseUri = baseUri;
            this.uri = uri;
            using(var message = new HttpRequestMessage()) this.headers = message.Headers;
            foreach (var(name, values) in headers)
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

            var message = new HttpRequestMessage { Method = method };

            // evaluate URI
            var uri = buildUri();
            Uri buildUri()
            {
                var baseUri = this.baseUri ?? client.BaseAddress;

                // if null or relative base
                if (baseUri is null || !baseUri.IsAbsoluteUri)
                {
                    if (string.IsNullOrWhiteSpace(this.uri))
                        throw new ArgumentException($"Request URI is empty");

                    return new Uri(this.uri);
                }

                if (string.IsNullOrWhiteSpace(this.uri))
                    return baseUri;

                return new Uri($"{baseUri.ToString().TrimEnd('/')}/{this.uri?.TrimStart('/')}");
            }

            // build query
            var queryBuilder = new QueryBuilder();
            // if any query in source uri - add it to queryBuilder
            if (!string.IsNullOrWhiteSpace(uri.Query))
                foreach (var(name, value) in QueryHelpers.ParseQuery(uri.Query))
                    queryBuilder.Add(name, (IEnumerable<string>) value);
            // add manually defined params to queryBuilder
            foreach (var(name, value) in parameters)
                queryBuilder.Add(name, value);

            message.RequestUri = new UriBuilder(uri) { Query = queryBuilder.ToString() }.Uri;

            foreach (var(name, values) in headers)
                message.Headers.Add(name, values);

            message.Content = content;

            var response = new Response(await client.SendAsync(message));

            if (!response.IsSuccessStatusCode && getFailureMessage != null)
                throw new HttpRequestException(await getFailureMessage(response));

            return response;
        }
    }
}