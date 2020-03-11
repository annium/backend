using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Net.Http.Internal
{
    internal static class Parse
    {
        public static Task<string> String(HttpContent x) =>
            x.ReadAsStringAsync();
        public static async Task<ReadOnlyMemory<byte>> Memory(HttpContent x) =>
            new ReadOnlyMemory<byte>(await x.ReadAsByteArrayAsync());
        public static Task<Stream> Stream(HttpContent x) =>
            x.ReadAsStreamAsync();
        public static Task<T> T<T>(HttpContent x) =>
            x.ParseAsync<T>();
        public static Task<IResult> Result(HttpContent x) =>
            x.ParseAsync<IResult>();
        public static Task<IResult<T>> ResultT<T>(HttpContent x) =>
            x.ParseAsync<IResult<T>>();
    }
}