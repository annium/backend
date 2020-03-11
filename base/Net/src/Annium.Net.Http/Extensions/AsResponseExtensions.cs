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
        public static Task<IResponse<string>> AsResponseStringAsync(this IRequest request) =>
            request.ToResponseAsync(Parse.String);

        public static Task<IResponse<ReadOnlyMemory<byte>>> AsResponseMemoryAsync(this IRequest request) =>
            request.ToResponseAsync(Parse.Memory);

        public static Task<IResponse<Stream>> AsResponseStreamAsync(this IRequest request) =>
            request.ToResponseAsync(Parse.Stream);

        public static Task<IResponse<IResult<T>>> AsResponseResultAsync<T>(this IRequest request) =>
            request.ToResponseAsync(Parse.ResultT<T>);

        public static Task<IResponse<T>> AsResponseAsync<T>(this IRequest request) =>
            request.ToResponseAsync(Parse.T<T>);

        private static async Task<IResponse<T>> ToResponseAsync<T>(this IRequest request, Func<HttpContent, Task<T>> parseAsync)
        {
            var response = await request.RunAsync();

            var data = await parseAsync(response.Content);

            return new Response<T>(response, data);
        }
    }
}