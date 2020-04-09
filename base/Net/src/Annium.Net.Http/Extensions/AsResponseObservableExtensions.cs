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
        public static IObservable<IResponse<string>> AsResponseStringObservable(this IRequest request) =>
            request.ToResponseObservable(Parse.String);

        public static IObservable<IResponse<string>> AsResponseStringObservable(this IRequest request, string defaultValue) =>
            request.ToResponseObservable(Parse.String, defaultValue);

        public static IObservable<IResponse<ReadOnlyMemory<byte>>> AsResponseMemoryObservable(this IRequest request) =>
            request.ToResponseObservable(Parse.Memory);

        public static IObservable<IResponse<ReadOnlyMemory<byte>>> AsResponseMemoryObservable(this IRequest request, ReadOnlyMemory<byte> defaultValue) =>
            request.ToResponseObservable(Parse.Memory, defaultValue);

        public static IObservable<IResponse<Stream>> AsResponseStreamObservable(this IRequest request) =>
            request.ToResponseObservable(Parse.Stream);

        public static IObservable<IResponse<Stream>> AsResponseStreamObservable(this IRequest request, Stream defaultValue) =>
            request.ToResponseObservable(Parse.Stream, defaultValue);

        public static IObservable<IResponse<IResult<T>>> AsResponseResultObservable<T>(this IRequest request) =>
            request.ToResponseObservable(Parse.ResultT<T>);

        public static IObservable<IResponse<IResult<T>>> AsResponseResultObservable<T>(this IRequest request, IResult<T> defaultValue) =>
            request.ToResponseObservable(Parse.ResultT<T>, defaultValue);

        public static IObservable<IResponse<T>> AsResponseObservable<T>(this IRequest request) =>
            request.ToResponseObservable(Parse.T<T>);

        public static IObservable<IResponse<T>> AsResponseObservable<T>(this IRequest request, T defaultValue) =>
            request.ToResponseObservable(Parse.T<T>, defaultValue);

        private static IObservable<IResponse<T>> ToResponseObservable<T>(
            this IRequest request,
            Func<HttpContent, Task<T>> parseAsync
        ) => Observable.FromAsync(async () =>
        {
            var response = await request.RunAsync();
            var data = await parseAsync(response.Content);

            return new Response<T>(response, data);
        });

        private static IObservable<IResponse<T>> ToResponseObservable<T>(
            this IRequest request,
            Func<HttpContent, T, Task<T>> parseAsync,
            T defaultValue
        ) => Observable.FromAsync(async () =>
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
        });

        public static IObservable<IResponse> AsResponseObservable(this IRequest request) => Observable.FromAsync(() =>
        {
            if (!request.IsEnsuringSuccess)
                request.EnsureSuccessStatusCode();

            return request.RunAsync();
        });
    }
}