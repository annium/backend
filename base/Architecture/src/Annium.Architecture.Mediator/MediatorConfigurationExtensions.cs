namespace Annium.Core.Mediator
{
    public static class MediatorConfigurationExtensions
    {
        public static MediatorConfiguration AddCompositionHandler(this MediatorConfiguration cfg)
        {
            cfg.Add(typeof(Architecture.Mediator.Internal.PipeHandlers.Request.CompositionPipeHandler<>));
            cfg.Add(typeof(Architecture.Mediator.Internal.PipeHandlers.RequestResponse.CompositionPipeHandler<,>));

            return cfg;
        }

        public static MediatorConfiguration AddExceptionHandler(this MediatorConfiguration cfg)
        {
            cfg.Add(typeof(Architecture.Mediator.Internal.PipeHandlers.Request.ExceptionPipeHandler<>));
            cfg.Add(typeof(Architecture.Mediator.Internal.PipeHandlers.RequestResponse.ExceptionPipeHandler<,>));

            return cfg;
        }

        public static MediatorConfiguration AddLoggingHandler(this MediatorConfiguration cfg)
        {
            cfg.Add(typeof(Architecture.Mediator.Internal.PipeHandlers.LoggingPipeHandler<,>));

            return cfg;
        }

        public static MediatorConfiguration AddValidationHandler(this MediatorConfiguration cfg)
        {
            cfg.Add(typeof(Architecture.Mediator.Internal.PipeHandlers.Request.ValidationPipeHandler<>));
            cfg.Add(typeof(Architecture.Mediator.Internal.PipeHandlers.RequestResponse.ValidationPipeHandler<,>));

            return cfg;
        }
    }
}