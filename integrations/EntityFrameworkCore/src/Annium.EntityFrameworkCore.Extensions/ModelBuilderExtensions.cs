using System;
using System.Linq;
using Annium.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Annium.EntityFrameworkCore.Extensions;

/// <summary>
/// Extension methods for configuring Entity Framework Core ModelBuilder
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies all entity configurations from assemblies containing the DbContext and its interfaces
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <param name="builder">The ModelBuilder to configure</param>
    public static void ApplyConfigurations<TContext>(this ModelBuilder builder)
        where TContext : DbContext
    {
        // deduplicated after the context's own assembly is added, not before: a context implementing an
        // interface declared beside it named that assembly twice, and a configuration applied twice
        // repeats whatever it does - seed data included, where the repeat is a duplicate key
        var configurationAssemblies = typeof(TContext)
            .GetOwnInterfaces()
            .Select(x => x.Assembly)
            .Append(typeof(TContext).Assembly)
            .Distinct()
            .ToArray();

        foreach (var assembly in configurationAssemblies)
            builder.ApplyConfigurationsFromAssembly(assembly);
    }

    /// <summary>
    /// Configures all entity names, property names, and constraint names to use snake_case naming convention
    /// </summary>
    /// <param name="builder">The ModelBuilder to configure</param>
    public static void UseSnakeCase(this ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            // only a root type names a table: a derived type shares its root's, and naming one separately
            // is how a hierarchy is mapped to a table per type instead
            if (entity.BaseType is null)
            {
                entity.SetTableName(entity.GetTableName()?.SnakeCase());
                entity.SetSchema(entity.GetSchema()?.SnakeCase());
            }

            // everything else is declared by the type that owns it, derived types included. Walking only
            // the roots left a hierarchy's own columns in the name they were written with, next to
            // snake-cased ones in the same table
            foreach (var property in entity.GetDeclaredProperties())
                property.SetColumnName(property.GetColumnName().SnakeCase());

            foreach (var key in entity.GetDeclaredKeys())
                key.SetName(key.GetName()?.SnakeCase());

            foreach (var key in entity.GetDeclaredForeignKeys())
                key.SetConstraintName(key.GetConstraintName()?.SnakeCase());

            foreach (var key in entity.GetDeclaredIndexes())
                key.SetDatabaseName(key.GetDatabaseName()?.SnakeCase());
        }
    }

    /// <summary>
    /// Configures all DateTime properties to be treated as UTC when materializing from the database
    /// </summary>
    /// <param name="builder">The ModelBuilder to configure</param>
    public static void UseDateTimeUtc(this ModelBuilder builder)
    {
        // a nullable timestamp is a timestamp: filtering on the CLR type alone left DateTime? columns
        // materializing with an unspecified kind, which a caller has no way to tell apart from a converted one
        foreach (var entity in builder.Model.GetEntityTypes())
        foreach (
            var property in entity
                .GetProperties()
                .Where(x => (Nullable.GetUnderlyingType(x.ClrType) ?? x.ClrType) == typeof(DateTime))
        )
            property.SetValueConverter(
                new ValueConverter<DateTime, DateTime>(x => x, x => DateTime.SpecifyKind(x, DateTimeKind.Utc))
            );
    }

    /// <summary>
    /// Sets the delete behavior for all foreign key relationships in the model
    /// </summary>
    /// <param name="builder">The ModelBuilder to configure</param>
    /// <param name="behavior">The delete behavior to apply to all foreign keys</param>
    public static void UseDeleteBehavior(this ModelBuilder builder, DeleteBehavior behavior)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        foreach (var key in entity.GetForeignKeys())
        {
            // an owned type exists only as part of its owner, so its link to that owner has to keep
            // cascading - anything else leaves the owned row behind when the owner goes. Nothing rejects
            // the change while the model is built, which is what makes setting it a silent way to strand rows
            if (key.IsOwnership)
                continue;

            // a required relationship has nowhere to put a null, and this is refused when the row is
            // deleted rather than now - naming the relationship here beats a constraint violation later
            if (behavior is DeleteBehavior.SetNull or DeleteBehavior.ClientSetNull && key.IsRequired)
                throw new ArgumentException(
                    $"Cannot apply {behavior} to required relationship {entity.DisplayName()} -> "
                        + $"{key.PrincipalEntityType.DisplayName()}: it has nowhere to put a null.",
                    nameof(behavior)
                );

            key.DeleteBehavior = behavior;
        }
    }
}
