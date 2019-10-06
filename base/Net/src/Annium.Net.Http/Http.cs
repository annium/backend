using System;
using Annium.Net.Http.Internal;

namespace Annium.Net.Http
{
    public static class Http
    {
        public static IRequest Open() => new Request();

        public static IRequest Open(string baseUri) => new Request(new Uri(baseUri.Trim()));

        public static IRequest Open(Uri baseUri) => new Request(baseUri);
    }
}