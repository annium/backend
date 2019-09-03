using Annium.Architecture.Mediator.Internal.PipeHandlers;

namespace Annium.Core.Mediator
{
    public static class MediatorConfigurationExtensions
    {
        public static MediatorConfiguration AddExceptionHandler(this MediatorConfiguration cfg)
        {
            cfg.Add(typeof(ExceptionPipeHandler<,>));

            return cfg;
        }

        public static MediatorConfiguration AddCompositionHandler(this MediatorConfiguration cfg)
        {
            cfg.Add(typeof(CompositionPipeHandler<,>));

            return cfg;
        }

        public static MediatorConfiguration AddLoggingHandler(this MediatorConfiguration cfg)
        {
            cfg.Add(typeof(LoggingPipeHandler<,>));

            return cfg;
        }

        public static MediatorConfiguration AddValidationHandler(this MediatorConfiguration cfg)
        {
            cfg.Add(typeof(ValidationPipeHandler<,>));

            return cfg;
        }
    }
}