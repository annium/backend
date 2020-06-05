using System.Linq;
using Annium.Core.Reflection;
using Annium.Extensions.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Annium.Core.DependencyInjection
{
    public static class ModelBuilderExtensions
    {
        public static void ApplyConfigurations<TContext>(this ModelBuilder builder)
            where TContext : DbContext
        {
            var configurationAssemblies = typeof(TContext).GetOwnInterfaces().Select(x => x.Assembly).ToArray();

            foreach (var assembly in configurationAssemblies)
                builder.ApplyConfigurationsFromAssembly(assembly);
        }

        public static void UseSnakeCase(this ModelBuilder builder)
        {
            foreach (var entity in builder.Model.GetEntityTypes())
            {
                if (entity.BaseType is null)
                    entity.SetTableName(entity.GetTableName().SnakeCase());

                foreach (var property in entity.GetProperties())
                    property.SetColumnName(property.GetColumnName().SnakeCase());

                foreach (var key in entity.GetKeys())
                    key.SetName(key.GetName().SnakeCase());

                foreach (var key in entity.GetForeignKeys())
                    key.SetConstraintName(key.GetConstraintName().SnakeCase());

                foreach (var key in entity.GetIndexes())
                    key.SetName(key.GetName().SnakeCase());
            }
        }

        public static void UseDeleteBehavior(this ModelBuilder builder, DeleteBehavior behavior)
        {
            foreach (var entity in builder.Model.GetEntityTypes())
            foreach (var key in entity.GetForeignKeys())
                key.DeleteBehavior = behavior;
        }
    }
}