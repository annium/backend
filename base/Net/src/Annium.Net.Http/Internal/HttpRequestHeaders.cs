using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Annium.Net.Http.Internal;

internal partial class HttpRequest
{
    public IHttpRequest Header(string name, string value)
    {
        Headers.Add(name, value);

        return this;
    }

    public IHttpRequest Header(string name, IEnumerable<string> values)
    {
        Headers.Add(name, values);

        return this;
    }

    public IHttpRequest Authorization(AuthenticationHeaderValue value)
    {
        Headers.Authorization = value;

        return this;
    }
}