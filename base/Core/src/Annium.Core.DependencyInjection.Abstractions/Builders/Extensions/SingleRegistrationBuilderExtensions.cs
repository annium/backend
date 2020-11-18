namespace Annium.Core.DependencyInjection
{
    public static class SingleRegistrationBuilderExtensions
    {
        public static ISingleRegistrationBuilderTarget AsSelf(this ISingleRegistrationBuilderBase builder)
        {
            builder.Unit.As(builder.Unit.Type);

            return builder;
        }

        public static ISingleRegistrationBuilderTarget As<TService>(this ISingleRegistrationBuilderBase builder)
        {
            builder.Unit.As(typeof(TService));

            return builder;
        }
    }
}