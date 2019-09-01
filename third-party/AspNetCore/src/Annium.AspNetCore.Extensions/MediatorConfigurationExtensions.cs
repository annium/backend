using Annium.AspNetCore.Extensions.Internal.PipeHandlers;

namespace Annium.Core.Mediator
{
    public static class MediatorConfigurationExtensions
    {
        public static MediatorConfiguration AddModelStatePipeHandler(this MediatorConfiguration cfg)
        {
            cfg.Add(typeof(ModelStatePipeHandler<,>));

            return cfg;
        }
    }
}