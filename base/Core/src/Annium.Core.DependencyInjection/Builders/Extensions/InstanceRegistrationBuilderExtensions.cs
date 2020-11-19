namespace Annium.Core.DependencyInjection
{
    public static class InstanceRegistrationBuilderExtensions
    {
        public static IInstanceRegistrationBuilderTarget AsSelf(this IInstanceRegistrationBuilderBase builder)
        {
            builder.Unit.As(builder.Unit.Type);

            return builder;
        }

        public static IInstanceRegistrationBuilderTarget As<TService>(this IInstanceRegistrationBuilderBase builder)
        {
            builder.Unit.As(typeof(TService));

            return builder;
        }
    }
}