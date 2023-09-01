using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Net.Http.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Net.Http;

public static class AsResponseExtensions
{
    // used in AspNetCore tests
    public static Task<IHttpResponse<IResult>> AsResponseResultAsync(this IHttpRequest request) =>
        request.ToResponseAsync(Parse.Result);

    // not used
    public static Task<IHttpResponse<IResult>> AsResponseResultAsync(this IHttpRequest request, IResult defaultValue) =>
        request.ToResponseAsync(Parse.Result, defaultValue);

    // not used
    public static Task<IHttpResponse<IResult<T>>> AsResponseResultAsync<T>(this IHttpRequest request) =>
        request.ToResponseAsync(Parse.ResultT<T>);

    // not used
    public static Task<IHttpResponse<IResult<T>>> AsResponseResultAsync<T>(this IHttpRequest request, IResult<T> defaultValue) =>
        request.ToResponseAsync(Parse.ResultT, defaultValue);

    // used in lots of places
    public static Task<IHttpResponse<T>> AsResponseAsync<T>(this IHttpRequest request) =>
        request.ToResponseAsync(Parse.T<T>);

    // used in lots of places
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