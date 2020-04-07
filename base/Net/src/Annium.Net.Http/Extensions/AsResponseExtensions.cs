using System;
using System.IO;
using System.Net;
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

        public static Task<IResponse<string>> AsResponseStringAsync(this IRequest request, string defaultValue) =>
            request.ToResponseAsync(Parse.String, defaultValue);

        public static Task<IResponse<ReadOnlyMemory<byte>>> AsResponseMemoryAsync(this IRequest request) =>
            request.ToResponseAsync(Parse.Memory);

        public static Task<IResponse<ReadOnlyMemory<byte>>> AsResponseMemoryAsync(this IRequest request, ReadOnlyMemory<byte> defaultValue) =>
            request.ToResponseAsync(Parse.Memory, defaultValue);

        public static Task<IResponse<Stream>> AsResponseStreamAsync(this IRequest request) =>
            request.ToResponseAsync(Parse.Stream);

        public static Task<IResponse<Stream>> AsResponseStreamAsync(this IRequest request, Stream defaultValue) =>
            request.ToResponseAsync(Parse.Stream, defaultValue);

        public static Task<IResponse<IResult<T>>> AsResponseResultAsync<T>(this IRequest request) =>
            request.ToResponseAsync(Parse.ResultT<T>);

        public static Task<IResponse<IResult<T>>> AsResponseResultAsync<T>(this IRequest request, IResult<T> defaultValue) =>
            request.ToResponseAsync(Parse.ResultT<T>, defaultValue);

        public static Task<IResponse<T>> AsResponseAsync<T>(this IRequest request) =>
            request.ToResponseAsync(Parse.T<T>);

        public static Task<IResponse<T>> AsResponseAsync<T>(this IRequest request, T defaultValue) =>
            request.ToResponseAsync(Parse.T<T>, defaultValue);

        private static async Task<IResponse<T>> ToResponseAsync<T>(this IRequest request, Func<HttpContent, Task<T>> parseAsync)
        {
            var response = await request.RunAsync();

            var data = await parseAsync(response.Content);

            return new Response<T>(response, data);
        }

        private static async Task<IResponse<T>> ToResponseAsync<T>(this IRequest request, Func<HttpContent, T, Task<T>> parseAsync, T defaultValue)
        {
            IResponse response;
            try
            {
                response = await request.RunAsync();
            }
            catch (Exception e)
            {
                response = new Response(new HttpResponseMessage(HttpStatusCode.BadGateway) { Content = new StringContent(e.Message) });
            }

            try
            {
                var data = await parseAsync(response.Content, defaultValue);

                return new Response<T>(response, data);
            }
            catch
            {
                return new Response<T>(response, defaultValue);
            }
        }
    }
}