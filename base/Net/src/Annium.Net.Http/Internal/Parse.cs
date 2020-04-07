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

        public static Task<string> String(HttpContent x, string _) =>
            x.ReadAsStringAsync();

        public static async Task<ReadOnlyMemory<byte>> Memory(HttpContent x) =>
            new ReadOnlyMemory<byte>(await x.ReadAsByteArrayAsync());

        public static async Task<ReadOnlyMemory<byte>> Memory(HttpContent x, ReadOnlyMemory<byte> _) =>
            new ReadOnlyMemory<byte>(await x.ReadAsByteArrayAsync());

        public static Task<Stream> Stream(HttpContent x) =>
            x.ReadAsStreamAsync();

        public static Task<Stream> Stream(HttpContent x, Stream _) =>
            x.ReadAsStreamAsync();

        public static Task<T> T<T>(HttpContent x) =>
            x.ParseAsync<T>();

        public static Task<T> T<T>(HttpContent x, T defaultValue) =>
            x.ParseAsync<T>(defaultValue);

        public static Task<IResult> Result(HttpContent x) =>
            x.ParseAsync<IResult>();

        public static Task<IResult> Result(HttpContent x, IResult defaultValue) =>
            x.ParseAsync<IResult>(defaultValue);

        public static Task<IResult<T>> ResultT<T>(HttpContent x) =>
            x.ParseAsync<IResult<T>>();

        public static Task<IResult<T>> ResultT<T>(HttpContent x, IResult<T> defaultValue) =>
            x.ParseAsync<IResult<T>>(defaultValue);
    }
}