using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Annium.Net.Http
{
    public partial interface IRequest
    {
        IRequest Header(string name, string value);
        IRequest Header(string name, IEnumerable<string> values);
        IRequest Authorization(AuthenticationHeaderValue value);
    }
}