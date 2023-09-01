using System.Net.Http;
using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Net.Http.Internal;

internal static class Parse
{
    public static Task<T> T<T>(IHttpRequest request, HttpContent content) =>
        request.ParseAsync<T>(content);

    public static Task<T> T<T>(IHttpRequest request, HttpContent content, T defaultValue) =>
        request.ParseAsync(content, defaultValue);

    public static Task<IResult> Result(IHttpRequest request, HttpContent content) =>
        request.ParseAsync<IResult>(content);

    public static Task<IResult> Result(IHttpRequest request, HttpContent content, IResult defaultValue) =>
        request.ParseAsync(content, defaultValue);

    public static Task<IResult<T>> ResultT<T>(IHttpRequest request, HttpContent content) =>
        request.ParseAsync<IResult<T>>(content);

    public static Task<IResult<T>> ResultT<T>(IHttpRequest request, HttpContent content, IResult<T> defaultValue) =>
        request.ParseAsync(content, defaultValue);
}