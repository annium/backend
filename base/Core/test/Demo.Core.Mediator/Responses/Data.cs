using System.Collections.Generic;
using System.Linq;

namespace Demo.Core.Mediator.Responses
{
    internal class Data<T>
    {
        public int Count => Items.Count();

        public IEnumerable<T> Items { get; }

        public Data(IEnumerable<T> items)
        {
            Items = items;
        }
    }
}