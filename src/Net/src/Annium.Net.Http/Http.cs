using System;

namespace Annium.Net.Http
{
    public static class Http
    {
        public static IRequest Open(string baseUri = null) =>
            Open(string.IsNullOrWhiteSpace(baseUri) ? null : new Uri(baseUri.Trim()));

        public static IRequest Open(Uri baseUri) => new Request(baseUri);
    }
}