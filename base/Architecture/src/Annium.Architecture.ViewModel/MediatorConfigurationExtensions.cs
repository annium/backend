namespace Annium.Core.Mediator
{
    public static class MediatorConfigurationExtensions
    {
        public static MediatorConfiguration AddViewMappingHandlers(this MediatorConfiguration cfg)
        {
            cfg.Add(typeof(Architecture.ViewModel.Internal.PipeHandlers.Request.MappingEnumerablePipeHandler<, , , ,>));
            cfg.Add(typeof(Architecture.ViewModel.Internal.PipeHandlers.Request.MappingSinglePipeHandler<, ,>));
            cfg.Add(typeof(Architecture.ViewModel.Internal.PipeHandlers.Response.MappingEnumerablePipeHandler<, , , ,>));
            cfg.Add(typeof(Architecture.ViewModel.Internal.PipeHandlers.Response.MappingSinglePipeHandler<, ,>));

            return cfg;
        }
    }
}