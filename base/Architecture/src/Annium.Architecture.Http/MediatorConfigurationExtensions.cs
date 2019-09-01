using Annium.Architecture.Http.Internal.PipeHandlers;

namespace Annium.Core.Mediator
{
    public static class MediatorConfigurationExtensions
    {
        public static MediatorConfiguration AddHttpStatusPipeHandler(this MediatorConfiguration cfg)
        {
            cfg.Add(typeof(HttpStatusPipeHandler<,>));

            return cfg;
        }
    }
}