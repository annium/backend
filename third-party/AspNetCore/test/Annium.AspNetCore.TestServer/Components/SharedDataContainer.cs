using System.Collections.Generic;

namespace Annium.AspNetCore.TestServer.Components
{
    public class SharedDataContainer
    {
        public string Value { get; set; } = string.Empty;
        public List<string> Log { get; } = new();
    }
}