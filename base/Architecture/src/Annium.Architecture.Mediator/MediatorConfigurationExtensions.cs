using Annium.Architecture.Mediator.Internal.PipeHandlers;
using Annium.Architecture.Mediator.Internal.PipeHandlers.Request;
using Annium.Architecture.Mediator.Internal.PipeHandlers.RequestResponse;

namespace Annium.Core.Mediator
{
    public static class MediatorConfigurationExtensions
    {
        public static MediatorConfiguration AddCompositionHandler(this MediatorConfiguration cfg)
        {
            cfg.Add(typeof(CompositionPipeHandler<>));
            cfg.Add(typeof(CompositionPipeHandler<,>));

            return cfg;
        }

        public static MediatorConfiguration AddExceptionHandler(this MediatorConfiguration cfg)
        {
            cfg.Add(typeof(ExceptionPipeHandler<>));
            cfg.Add(typeof(ExceptionPipeHandler<,>));

            return cfg;
        }

        public static MediatorConfiguration AddLoggingHandler(this MediatorConfiguration cfg)
        {
            cfg.Add(typeof(LoggingPipeHandler<,>));

            return cfg;
        }

        public static MediatorConfiguration AddValidationHandler(this MediatorConfiguration cfg)
        {
            cfg.Add(typeof(ValidationPipeHandler<>));
            cfg.Add(typeof(ValidationPipeHandler<,>));

            return cfg;
        }
    }
}