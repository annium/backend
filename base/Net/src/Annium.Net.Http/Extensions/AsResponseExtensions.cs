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
        public static Task<IHttpResponse<string>> AsResponseStringAsync(this IHttpRequest request) =>
            request.ToResponseAsync(Parse.String);

        public static Task<IHttpResponse<string>> AsResponseStringAsync(this IHttpRequest request, string defaultValue) =>
            request.ToResponseAsync(Parse.String, defaultValue);

        public static Task<IHttpResponse<ReadOnlyMemory<byte>>> AsResponseMemoryAsync(this IHttpRequest request) =>
            request.ToResponseAsync(Parse.Memory);

        public static Task<IHttpResponse<ReadOnlyMemory<byte>>> AsResponseMemoryAsync(this IHttpRequest request, ReadOnlyMemory<byte> defaultValue) =>
            request.ToResponseAsync(Parse.Memory, defaultValue);

        public static Task<IHttpResponse<Stream>> AsResponseStreamAsync(this IHttpRequest request) =>
            request.ToResponseAsync(Parse.Stream);

        public static Task<IHttpResponse<Stream>> AsResponseStreamAsync(this IHttpRequest request, Stream defaultValue) =>
            request.ToResponseAsync(Parse.Stream, defaultValue);

        public static Task<IHttpResponse<IResult>> AsResponseResultAsync(this IHttpRequest request) =>
            request.ToResponseAsync(Parse.Result);

        public static Task<IHttpResponse<IResult>> AsResponseResultAsync(this IHttpRequest request, IResult defaultValue) =>
            request.ToResponseAsync(Parse.Result, defaultValue);

        public static Task<IHttpResponse<IResult<T>>> AsResponseResultAsync<T>(this IHttpRequest request) =>
            request.ToResponseAsync(Parse.ResultT<T>);

        public static Task<IHttpResponse<IResult<T>>> AsResponseResultAsync<T>(this IHttpRequest request, IResult<T> defaultValue) =>
            request.ToResponseAsync(Parse.ResultT, defaultValue);

        public static Task<IHttpResponse<T>> AsResponseAsync<T>(this IHttpRequest request) =>
            request.ToResponseAsync(Parse.T<T>);

        public static Task<IHttpResponse<T>> AsResponseAsync<T>(this IHttpRequest request, T defaultValue) =>
            request.ToResponseAsync(Parse.T, defaultValue);

        private static async Task<IHttpResponse<T>> ToResponseAsync<T>(
            this IHttpRequest request,
            Func<IHttpRequest, HttpContent, Task<T>> parseAsync
        )
        {
            var response = await request.RunAsync();

            var data = await parseAsync(request, response.Content);

            return new HttpResponse<T>(response, data);
        }

        private static async Task<IHttpResponse<T>> ToResponseAsync<T>(
            this IHttpRequest request,
            Func<IHttpRequest, HttpContent, T, Task<T>> parseAsync,
            T defaultValue
        )
        {
            IHttpResponse response;
            try
            {
                response = await request.RunAsync();
            }
            catch (Exception e)
            {
                response = new HttpResponse(new HttpResponseMessage(HttpStatusCode.BadGateway) { Content = new StringContent(e.Message) });
            }

            try
            {
                var data = await parseAsync(request, response.Content, defaultValue);

                return new HttpResponse<T>(response, data);
            }
            catch
            {
                return new HttpResponse<T>(response, defaultValue);
            }
        }
    }
}