using Annium.Architecture.Http.Internal.PipeHandlers.Request;
using Annium.Architecture.Http.Internal.PipeHandlers.RequestResponse;

namespace Annium.Core.Mediator
{
    public static class MediatorConfigurationExtensions
    {
        public static MediatorConfiguration AddHttpStatusPipeHandler(this MediatorConfiguration cfg)
        {
            cfg.AddHandler(typeof(HttpStatusPipeHandler<>));
            cfg.AddHandler(typeof(HttpStatusPipeHandler<,>));

            return cfg;
        }
    }
}