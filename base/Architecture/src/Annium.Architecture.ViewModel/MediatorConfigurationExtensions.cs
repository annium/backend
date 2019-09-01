using Annium.Architecture.ViewModel.Internal.PipeHandlers;

namespace Annium.Core.Mediator
{
    public static class MediatorConfigurationExtensions
    {
        public static MediatorConfiguration AddViewMappingHandlers(this MediatorConfiguration cfg)
        {
            cfg.Add(typeof(RequestMappingPipeHandler<,>));
            cfg.Add(typeof(RequestResponseMappingPipeHandler<,>));

            return cfg;
        }
    }
}