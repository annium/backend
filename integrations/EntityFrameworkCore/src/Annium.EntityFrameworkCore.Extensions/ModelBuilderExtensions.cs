using System;
using System.Linq;
using Annium.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Annium.EntityFrameworkCore.Extensions;

public static class ModelBuilderExtensions
{
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

    public static void UseDateTimeUtc(this ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        foreach (var property in entity.GetProperties().Where(x => x.ClrType == typeof(DateTime)))
            property.SetValueConverter(
                new ValueConverter<DateTime, DateTime>(x => x, x => DateTime.SpecifyKind(x, DateTimeKind.Utc))
            );
    }

    public static void UseDeleteBehavior(this ModelBuilder builder, DeleteBehavior behavior)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        foreach (var key in entity.GetForeignKeys())
            key.DeleteBehavior = behavior;
    }
}
