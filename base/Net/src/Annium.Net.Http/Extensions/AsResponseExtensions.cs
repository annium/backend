using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Net.Http.Internal;

namespace Annium.Net.Http
{
    public static class AsResponseExtensions
    {
        public static Task<IResponse<string>> AsStringAsync(this IRequest request) =>
            request.ToResponseAsync(Parse.String);

        public static Task<IResponse<ReadOnlyMemory<byte>>> AsMemoryAsync(this IRequest request) =>
            request.ToResponseAsync(Parse.Memory);

        public static Task<IResponse<Stream>> AsStreamAsync(this IRequest request) =>
            request.ToResponseAsync(Parse.Stream);

        public static Task<IResponse<IResult<T>>> AsResultAsync<T>(this IRequest request) =>
            request.ToResponseAsync(Parse.ResultT<T>);

        public static Task<IResponse<T>> AsAsync<T>(this IRequest request) =>
            request.ToResponseAsync(Parse.T<T>);

        private static async Task<IResponse<T>> ToResponseAsync<T>(this IRequest request, Func<HttpContent, Task<T>> parseAsync)
        {
            if (!request.IsEnsuringSuccess)
                request.EnsureSuccessStatusCode();

            var response = await request.RunAsync();
            var data = await parseAsync(response.Content);

            return new Response<T>(response, data);
        }
    }
}