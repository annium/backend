using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public interface IServiceContainer : ICollection<ServiceDescriptor>
    {
        public IServiceCollection Collection { get; }
    }
}