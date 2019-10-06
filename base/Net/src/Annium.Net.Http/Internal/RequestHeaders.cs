using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Annium.Net.Http.Internal
{
    internal partial class Request
    {
        public IRequest Header(string name, string value)
        {
            headers.Add(name, value);

            return this;
        }

        public IRequest Header(string name, IEnumerable<string> values)
        {
            headers.Add(name, values);

            return this;
        }

        public IRequest Authorization(AuthenticationHeaderValue value)
        {
            headers.Authorization = value;

            return this;
        }
    }
}