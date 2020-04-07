using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Net.Http.Internal;

namespace Annium.Net.Http
{
    public static class AsExtensions
    {
        public static Task<string> AsStringAsync(this IRequest request) =>
            request.ToAsync(Parse.String);

        public static Task<string> AsStringAsync(this IRequest request, string defaultValue) =>
            request.ToAsync(Parse.String, defaultValue);

        public static Task<ReadOnlyMemory<byte>> AsMemoryAsync(this IRequest request) =>
            request.ToAsync(Parse.Memory);

        public static Task<ReadOnlyMemory<byte>> AsMemoryAsync(this IRequest request, ReadOnlyMemory<byte> defaultValue) =>
            request.ToAsync(Parse.Memory, defaultValue);

        public static Task<Stream> AsStreamAsync(this IRequest request) =>
            request.ToAsync(Parse.Stream);

        public static Task<Stream> AsStreamAsync(this IRequest request, Stream defaultValue) =>
            request.ToAsync(Parse.Stream, defaultValue);

        public static Task<IResult> AsResultAsync(this IRequest request) =>
            request.ToAsync(Parse.Result);

        public static Task<IResult> AsResultAsync(this IRequest request, IResult defaultValue) =>
            request.ToAsync(Parse.Result, defaultValue);

        public static Task<IResult<T>> AsResultAsync<T>(this IRequest request) =>
            request.ToAsync(Parse.ResultT<T>);

        public static Task<IResult<T>> AsResultAsync<T>(this IRequest request, IResult<T> defaultValue) =>
            request.ToAsync(Parse.ResultT<T>, defaultValue);

        public static Task<T> AsAsync<T>(this IRequest request) =>
            request.ToAsync(Parse.T<T>);

        public static Task<T> AsAsync<T>(this IRequest request, T defaultValue) =>
            request.ToAsync(Parse.T<T>, defaultValue);

        private static async Task<T> ToAsync<T>(this IRequest request, Func<HttpContent, Task<T>> parseAsync)
        {
            if (!request.IsEnsuringSuccess)
                request.EnsureSuccessStatusCode();

            var response = await request.RunAsync();

            return await parseAsync(response.Content);
        }

        private static async Task<T> ToAsync<T>(this IRequest request, Func<HttpContent, T, Task<T>> parseAsync, T defaultValue)
        {
            try
            {
                if (!request.IsEnsuringSuccess)
                    request.EnsureSuccessStatusCode();

                var response = await request.RunAsync();

                return await parseAsync(response.Content, defaultValue);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}