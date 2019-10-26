using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;

namespace Annium.Net.Base
{
    public class UriFactory
    {
        public static UriFactory Base(Uri baseUri) => new UriFactory(baseUri);

        public static UriFactory Base(string baseUri) => new UriFactory(new Uri(baseUri));

        public static UriFactory Base() => new UriFactory();

        private readonly Uri? baseUri;
        private string? uri;
        private readonly Dictionary<string, string> parameters = new Dictionary<string, string>();

        private UriFactory(
            Uri? baseUri,
            string? uri,
            IReadOnlyDictionary<string, string> parameters
        )
        {
            if (baseUri != null)
                EnsureAbsolute(baseUri);

            this.baseUri = baseUri;
            this.uri = uri;
            this.parameters = parameters.ToDictionary(p => p.Key, p => p.Value);
        }

        private UriFactory(
            Uri baseUri
        )
        {
            EnsureAbsolute(baseUri);

            this.baseUri = baseUri;
        }

        private UriFactory() { }

        public UriFactory Path(string uri)
        {
            this.uri = uri.Trim();

            return this;
        }

        public UriFactory Param<T>(string key, T value)
        {
            parameters[key] = value?.ToString() ?? string.Empty;

            return this;
        }


        public UriFactory Clone() => new UriFactory(baseUri, uri, parameters);

        public Uri Build()
        {
            var uri = BuildUriBase();
            var qb = new QueryBuilder();

            // if any query in source uri - add it to queryBuilder
            if (!string.IsNullOrWhiteSpace(uri.Query))
                foreach (var (name, value) in QueryHelpers.ParseQuery(uri.Query))
                    qb.Add(name, (IEnumerable<string>)value);

            // add manually defined params to queryBuilder
            foreach (var (name, value) in parameters)
                qb.Add(name, value);

            return new UriBuilder(uri) { Query = qb.ToString() }.Uri;
        }

        private Uri BuildUriBase()
        {
            if (baseUri is null)
            {
                if (string.IsNullOrWhiteSpace(this.uri))
                    throw new UriFormatException($"Request URI is empty");

                var uri = new Uri(this.uri);
                EnsureAbsolute(uri);

                return uri;
            }

            if (string.IsNullOrWhiteSpace(uri) || uri == "/")
                return baseUri;

            if (!uri.StartsWith("/"))
            {
                if (Uri.TryCreate(uri, UriKind.Absolute, out _))
                    throw new UriFormatException($"Both base and path are absolute URI");

                return new Uri($"{baseUri.ToString().TrimEnd('/')}/{uri.ToString().TrimStart('/')}");
            }

            var sb = new StringBuilder();
            sb.Append($"{baseUri.Scheme}://");
            sb.Append(baseUri.Host);
            if (!baseUri.IsDefaultPort)
                sb.Append($":{baseUri.Port}");

            return new Uri($"{sb.ToString()}/{uri.ToString().TrimStart('/')}");
        }

        private void EnsureAbsolute(Uri uri)
        {
            if (!uri.IsAbsoluteUri)
                throw new UriFormatException($"URI {uri} is not absolute");
        }
    }
}