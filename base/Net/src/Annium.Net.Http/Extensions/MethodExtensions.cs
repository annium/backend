using System;
using System.Net.Http;

namespace Annium.Net.Http
{
    public static class MethodExtensions
    {
        public static IRequest Head(this IRequest request, string url) => request.Method(HttpMethod.Head, url);

        public static IRequest Head(this IRequest request, Uri url) => request.Method(HttpMethod.Head, url);

        public static IRequest Get(this IRequest request, string url) => request.Method(HttpMethod.Get, url);

        public static IRequest Get(this IRequest request, Uri url) => request.Method(HttpMethod.Get, url);

        public static IRequest Put(this IRequest request, string url) => request.Method(HttpMethod.Put, url);

        public static IRequest Put(this IRequest request, Uri url) => request.Method(HttpMethod.Put, url);

        public static IRequest Post(this IRequest request, string url) => request.Method(HttpMethod.Post, url);

        public static IRequest Post(this IRequest request, Uri url) => request.Method(HttpMethod.Post, url);

        public static IRequest Delete(this IRequest request, string url) => request.Method(HttpMethod.Delete, url);

        public static IRequest Delete(this IRequest request, Uri url) => request.Method(HttpMethod.Delete, url);
    }
}