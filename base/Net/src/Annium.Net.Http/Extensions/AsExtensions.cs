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

        public static Task<ReadOnlyMemory<byte>> AsMemoryAsync(this IRequest request) =>
            request.ToAsync(Parse.Memory);

        public static Task<Stream> AsStreamAsync(this IRequest request) =>
            request.ToAsync(Parse.Stream);

        public static Task<IResult> AsResultAsync(this IRequest request) =>
            request.ToAsync(Parse.Result);

        public static Task<IResult<T>> AsResultAsync<T>(this IRequest request) =>
            request.ToAsync(Parse.ResultT<T>);

        public static Task<T> AsAsync<T>(this IRequest request) =>
            request.ToAsync(Parse.T<T>);

        private static async Task<T> ToAsync<T>(this IRequest request, Func<HttpContent, Task<T>> parseAsync)
        {
            if (!request.IsEnsuringSuccess)
                request.EnsureSuccessStatusCode();

            var response = await request.RunAsync();

            return await parseAsync(response.Content);
        }
    }
}