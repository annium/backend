using System;

namespace Annium.Net.Http
{
    public interface IHttpRequestFactory
    {
        IHttpRequest Get();
        IHttpRequest Get(string baseUri);
        IHttpRequest Get(Uri baseUri);
    }
}