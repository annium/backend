using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Annium.Net.Http;

public partial interface IHttpRequest
{
    IHttpRequest Header(string name, string value);
    IHttpRequest Header(string name, IEnumerable<string> values);
    IHttpRequest Authorization(AuthenticationHeaderValue value);
}