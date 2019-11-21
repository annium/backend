using System;
using System.Net.Http;

namespace Annium.Net.Http
{
    public static class MethodExtensions
    {
        public static IRequest Head(this IRequest request, string url) => request.With(HttpMethod.Head, url);

        public static IRequest Head(this IRequest request, Uri url) => request.With(HttpMethod.Head, url);

        public static IRequest Get(this IRequest request, string url) => request.With(HttpMethod.Get, url);

        public static IRequest Get(this IRequest request, Uri url) => request.With(HttpMethod.Get, url);

        public static IRequest Put(this IRequest request, string url) => request.With(HttpMethod.Put, url);

        public static IRequest Put(this IRequest request, Uri url) => request.With(HttpMethod.Put, url);

        public static IRequest Post(this IRequest request, string url) => request.With(HttpMethod.Post, url);

        public static IRequest Post(this IRequest request, Uri url) => request.With(HttpMethod.Post, url);

        public static IRequest Delete(this IRequest request, string url) => request.With(HttpMethod.Delete, url);

        public static IRequest Delete(this IRequest request, Uri url) => request.With(HttpMethod.Delete, url);
    }
}