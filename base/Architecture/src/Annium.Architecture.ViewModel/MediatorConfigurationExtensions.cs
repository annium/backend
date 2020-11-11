using Annium.Architecture.ViewModel.Internal.PipeHandlers.Request;

namespace Annium.Core.Mediator
{
    public static class MediatorConfigurationExtensions
    {
        public static MediatorConfiguration AddViewMappingHandlers(this MediatorConfiguration cfg)
        {
            cfg.Add(typeof(MappingEnumerablePipeHandler<,,>));
            cfg.Add(typeof(MappingSinglePipeHandler<,,>));
            cfg.Add(typeof(Architecture.ViewModel.Internal.PipeHandlers.Response.MappingEnumerablePipeHandler<,,>));
            cfg.Add(typeof(Architecture.ViewModel.Internal.PipeHandlers.Response.MappingSinglePipeHandler<,,>));

            return cfg;
        }
    }
}