namespace Annium.Core.Mediator
{
    public static class MediatorConfigurationExtensions
    {
        public static MediatorConfiguration AddModelStatePipeHandler(this MediatorConfiguration cfg)
        {
            cfg.Add(typeof(AspNetCore.Extensions.Internal.PipeHandlers.Request.ModelStatePipeHandler<>));
            cfg.Add(typeof(AspNetCore.Extensions.Internal.PipeHandlers.RequestResponse.ModelStatePipeHandler<,>));

            return cfg;
        }
    }
}