namespace Annium.Core.Mediator
{
    public static class MediatorConfigurationExtensions
    {
        public static MediatorConfiguration AddHttpStatusPipeHandler(this MediatorConfiguration cfg)
        {
            cfg.Add(typeof(Architecture.Http.Internal.PipeHandlers.Request.HttpStatusPipeHandler<>));
            cfg.Add(typeof(Architecture.Http.Internal.PipeHandlers.RequestResponse.HttpStatusPipeHandler<,>));

            return cfg;
        }
    }
}