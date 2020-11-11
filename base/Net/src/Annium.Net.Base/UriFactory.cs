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

        private readonly Uri? _baseUri;
        private string? _uri;
        private readonly Dictionary<string, string> _parameters = new Dictionary<string, string>();

        private UriFactory(
            Uri? baseUri,
            string? uri,
            IReadOnlyDictionary<string, string> parameters
        )
        {
            if (baseUri != null)
                EnsureAbsolute(baseUri);

            _baseUri = baseUri;
            _uri = uri;
            _parameters = parameters.ToDictionary(p => p.Key, p => p.Value);
        }

        private UriFactory(
            Uri baseUri
        )
        {
            EnsureAbsolute(baseUri);

            _baseUri = baseUri;
        }

        private UriFactory()
        {
        }

        public UriFactory Path(string uri)
        {
            _uri = uri.Trim();

            return this;
        }

        public UriFactory Param<T>(string key, T value)
        {
            _parameters[key] = value?.ToString() ?? string.Empty;

            return this;
        }


        public UriFactory Clone() => new UriFactory(_baseUri, _uri, _parameters);

        public Uri Build()
        {
            var uri = BuildUriBase();
            var qb = new QueryBuilder();

            // if any query in source uri - add it to queryBuilder
            if (!string.IsNullOrWhiteSpace(uri.Query))
                foreach (var (name, value) in QueryHelpers.ParseQuery(uri.Query))
                    qb.Add(name, (IEnumerable<string>) value);

            // add manually defined params to queryBuilder
            foreach (var (name, value) in _parameters)
                qb.Add(name, value);

            return new UriBuilder(uri) { Query = qb.ToString() }.Uri;
        }

        private Uri BuildUriBase()
        {
            if (_baseUri is null)
            {
                if (string.IsNullOrWhiteSpace(_uri))
                    throw new UriFormatException("Request URI is empty");

                var uri = new Uri(_uri);
                EnsureAbsolute(uri);

                return uri;
            }

            if (string.IsNullOrWhiteSpace(_uri) || _uri == "/")
                return _baseUri;

            if (!_uri.StartsWith("/"))
            {
                if (Uri.TryCreate(_uri, UriKind.Absolute, out _))
                    throw new UriFormatException("Both base and path are absolute URI");

                return new Uri($"{_baseUri.ToString().TrimEnd('/')}/{_uri.TrimStart('/')}");
            }

            var sb = new StringBuilder();
            sb.Append($"{_baseUri.Scheme}://");
            sb.Append(_baseUri.Host);
            if (!_baseUri.IsDefaultPort)
                sb.Append($":{_baseUri.Port}");

            return new Uri($"{sb}/{_uri.TrimStart('/')}");
        }

        private void EnsureAbsolute(Uri uri)
        {
            if (!uri.IsAbsoluteUri)
                throw new UriFormatException($"URI {uri} is not absolute");
        }
    }
}