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
        var configurationAssemblies = typeof(TContext)
            .GetOwnInterfaces()
            .Select(x => x.Assembly)
            .Distinct()
            .Append(typeof(TContext).Assembly)
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
        foreach (var entity in builder.Model.GetEntityTypes().Where(x => x.BaseType is null))
        {
            entity.SetTableName(entity.GetTableName()?.SnakeCase());
            entity.SetSchema(entity.GetSchema()?.SnakeCase());

            foreach (var property in entity.GetProperties())
                property.SetColumnName(property.GetColumnName().SnakeCase());

            foreach (var key in entity.GetKeys())
                key.SetName(key.GetName()?.SnakeCase());

            foreach (var key in entity.GetForeignKeys())
                key.SetConstraintName(key.GetConstraintName()?.SnakeCase());

            foreach (var key in entity.GetIndexes())
                key.SetDatabaseName(key.GetDatabaseName()?.SnakeCase());
        }
    }

    /// <summary>
    /// Configures all DateTime properties to be treated as UTC when materializing from the database
    /// </summary>
    /// <param name="builder">The ModelBuilder to configure</param>
    public static void UseDateTimeUtc(this ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        foreach (var property in entity.GetProperties().Where(x => x.ClrType == typeof(DateTime)))
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
            key.DeleteBehavior = behavior;
    }
}
