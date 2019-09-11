namespace Annium.Core.Mediator
{
    public static class MediatorConfigurationExtensions
    {
        public static MediatorConfiguration AddViewMappingHandlers(this MediatorConfiguration cfg)
        {
            cfg.Add(typeof(Architecture.ViewModel.Internal.PipeHandlers.RequestResponse.MappingPipeHandler<, , ,>));
            cfg.Add(typeof(Architecture.ViewModel.Internal.PipeHandlers.Request.MappingPipeHandler<, ,>));

            return cfg;
        }
    }
}