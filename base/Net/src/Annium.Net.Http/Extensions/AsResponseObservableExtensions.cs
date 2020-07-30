using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Net.Http.Internal;

namespace Annium.Net.Http
{
    public static class ResponseObservableExtensions
    {
        public static IObservable<IHttpResponse<string>> AsResponseStringObservable(this IHttpRequest request) =>
            request.ToResponseObservable(Parse.String);

        public static IObservable<IHttpResponse<string>> AsResponseStringObservable(this IHttpRequest request, string defaultValue) =>
            request.ToResponseObservable(Parse.String, defaultValue);

        public static IObservable<IHttpResponse<ReadOnlyMemory<byte>>> AsResponseMemoryObservable(this IHttpRequest request) =>
            request.ToResponseObservable(Parse.Memory);

        public static IObservable<IHttpResponse<ReadOnlyMemory<byte>>> AsResponseMemoryObservable(
            this IHttpRequest request,
            ReadOnlyMemory<byte> defaultValue
        ) => request.ToResponseObservable(Parse.Memory, defaultValue);

        public static IObservable<IHttpResponse<Stream>> AsResponseStreamObservable(this IHttpRequest request) =>
            request.ToResponseObservable(Parse.Stream);

        public static IObservable<IHttpResponse<Stream>> AsResponseStreamObservable(this IHttpRequest request, Stream defaultValue) =>
            request.ToResponseObservable(Parse.Stream, defaultValue);

        public static IObservable<IHttpResponse<IResult<T>>> AsResponseResultObservable<T>(this IHttpRequest request) =>
            request.ToResponseObservable(Parse.ResultT<T>);

        public static IObservable<IHttpResponse<IResult<T>>> AsResponseResultObservable<T>(this IHttpRequest request, IResult<T> defaultValue) =>
            request.ToResponseObservable(Parse.ResultT<T>, defaultValue);

        public static IObservable<IHttpResponse<T>> AsResponseObservable<T>(this IHttpRequest request) =>
            request.ToResponseObservable(Parse.T<T>);

        public static IObservable<IHttpResponse<T>> AsResponseObservable<T>(this IHttpRequest request, T defaultValue) =>
            request.ToResponseObservable(Parse.T<T>, defaultValue);

        private static IObservable<IHttpResponse<T>> ToResponseObservable<T>(
            this IHttpRequest request,
            Func<IHttpRequest, HttpContent, Task<T>> parseAsync
        ) => Observable.FromAsync(async () =>
        {
            var response = await request.RunAsync();
            var data = await parseAsync(request, response.Content);

            return new HttpResponse<T>(response, data);
        });

        private static IObservable<IHttpResponse<T>> ToResponseObservable<T>(
            this IHttpRequest request,
            Func<IHttpRequest, HttpContent, T, Task<T>> parseAsync,
            T defaultValue
        ) => Observable.FromAsync(async () =>
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
        });

        public static IObservable<IHttpResponse> AsResponseObservable(this IHttpRequest request) => Observable.FromAsync(() =>
        {
            if (!request.IsEnsuringSuccess)
                request.EnsureSuccessStatusCode();

            return request.RunAsync();
        });
    }
}