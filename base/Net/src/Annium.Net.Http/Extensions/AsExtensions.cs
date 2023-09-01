using System;
using System.Net.Http;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Net.Http.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Net.Http;

public static class AsExtensions
{
    // not used
    public static Task<IResult> AsResultAsync(this IHttpRequest request) =>
        request.ToAsync(Parse.Result);

    // not used
    public static Task<IResult> AsResultAsync(this IHttpRequest request, IResult defaultValue) =>
        request.ToAsync(Parse.Result, defaultValue);

    // used in AspNetCore tests
    public static Task<IResult<T>> AsResultAsync<T>(this IHttpRequest request) =>
        request.ToAsync(Parse.ResultT<T>);

    // not used
    public static Task<IResult<T>> AsResultAsync<T>(this IHttpRequest request, IResult<T> defaultValue) =>
        request.ToAsync(Parse.ResultT, defaultValue);

    // used in lots of places
    public static Task<T> AsAsync<T>(this IHttpRequest request) =>
        request.ToAsync(Parse.T<T>);

    // used in lots of places
    public static Task<T> AsAsync<T>(this IHttpRequest request, T defaultValue) =>
        request.ToAsync(Parse.T, defaultValue);

    private static async Task<T> ToAsync<T>(
        this IHttpRequest request,
        Func<IHttpRequest, HttpContent, Task<T>> parseAsync
    )
    {
        if (!request.IsEnsuringSuccess)
            request.EnsureSuccessStatusCode();

        var response = await request.RunAsync();

        return await parseAsync(request, response.Content);
    }

    private static async Task<T> ToAsync<T>(
        this IHttpRequest request,
        Func<IHttpRequest, HttpContent, T, Task<T>> parseAsync,
        T defaultValue
    )
    {
        try
        {
            if (!request.IsEnsuringSuccess)
                request.EnsureSuccessStatusCode();

            var response = await request.RunAsync();

            return await parseAsync(request, response.Content, defaultValue);
        }
        catch
        {
            return defaultValue;
        }
    }
}