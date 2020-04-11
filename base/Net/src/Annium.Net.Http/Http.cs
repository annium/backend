using System;
using Annium.Net.Http.Internal;

namespace Annium.Net.Http
{
    public static class Http
    {
        public static IHttpRequest Open() => new HttpRequest();

        public static IHttpRequest Open(string baseUri) => new HttpRequest(new Uri(baseUri.Trim()));

        public static IHttpRequest Open(Uri baseUri) => new HttpRequest(baseUri);
    }
}