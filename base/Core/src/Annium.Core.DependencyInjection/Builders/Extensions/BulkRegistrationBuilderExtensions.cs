namespace Annium.Core.DependencyInjection
{
    public static class BulkRegistrationBuilderExtensions
    {
        public static IBulkRegistrationBuilderTarget AsSelf(this IBulkRegistrationBuilderBase builder)
        {
            foreach (var unit in builder.Units)
                unit.As(unit.Type);

            return builder;
        }

        public static IBulkRegistrationBuilderTarget As<TService>(this IBulkRegistrationBuilderBase builder)
        {
            foreach (var unit in builder.Units)
                unit.As(typeof(TService));

            return builder;
        }
    }
}