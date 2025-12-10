using Annium.Data.Models;
using Annium.linq2db.Extensions.Internal.Extensions;
using LinqToDB.Data;
using LinqToDB.Internal.SqlQuery;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

/// <summary>
/// Extension methods for DataConnection to handle automatic created/updated time management in SQL queries.
/// </summary>
public static class DataConnectionExtensions
{
    /// <summary>
    /// Processes SQL statements to automatically inject created/updated time values based on query type.
    /// </summary>
    /// <param name="dc">The data connection.</param>
    /// <param name="statement">The SQL statement to process.</param>
    /// <param name="timeProvider">The time provider for current timestamp.</param>
    /// <returns>The processed SQL statement with time fields added where appropriate.</returns>
    public static SqlStatement ProcessCreatedUpdatedTimeQuery(
        this DataConnection dc,
        SqlStatement statement,
        ITimeProvider timeProvider
    )
    {
        return statement switch
        {
            SqlInsertStatement st => dc.ProcessInsertTimeQuery(st, timeProvider),
            SqlUpdateStatement st => dc.ProcessUpdateTimeQuery(st, timeProvider),
            SqlInsertOrUpdateStatement st => dc.ProcessInsertOrUpdateTimeQuery(st, timeProvider),
            _ => statement,
        };
    }

    /// <summary>
    /// Processes INSERT queries to add created and updated time values.
    /// </summary>
    /// <param name="cn">The data connection.</param>
    /// <param name="statement">The SQL INSERT statement.</param>
    /// <param name="timeProvider">The time provider for current timestamp.</param>
    /// <returns>The processed SQL statement with time fields added.</returns>
    private static SqlStatement ProcessInsertTimeQuery(
        this DataConnection cn,
        SqlInsertStatement statement,
        ITimeProvider timeProvider
    )
    {
        // get data type
        var dataType = statement.Insert.Into?.ObjectType;

        // weird, but possible according to nullability spec
        if (dataType is null)
            return statement;

        // basic and fast way to ensure processability
        if (!dataType.IsAssignableTo(typeof(ICreatedTimeEntity)))
            return statement;

        // get table schema descriptor
        var descriptor = cn.MappingSchema.GetEntityDescriptor(dataType);

        // resolve CreatedAt column descriptor
        var createdTime = descriptor.Columns.FindColumn(nameof(ICreatedTimeEntity.CreatedAt));

        // if source is additionally updatable
        var updatedTime = dataType.IsAssignableTo(typeof(ICreatedUpdatedTimeEntity))
            ? descriptor.Columns.FindColumn(nameof(ICreatedUpdatedTimeEntity.UpdatedAt))
            : null;

        var stmt = statement.CloneWithoutParams();
        var insert = stmt.Insert;
        var now = timeProvider.Now;

        insert.SetValue(createdTime, now);

        if (updatedTime is not null)
            insert.SetValue(updatedTime, now);

        return stmt;
    }

    /// <summary>
    /// Processes UPDATE queries to add updated time values.
    /// </summary>
    /// <param name="cn">The data connection.</param>
    /// <param name="statement">The SQL UPDATE statement.</param>
    /// <param name="timeProvider">The time provider for current timestamp.</param>
    /// <returns>The processed SQL statement with updated time field added.</returns>
    private static SqlStatement ProcessUpdateTimeQuery(
        this DataConnection cn,
        SqlUpdateStatement statement,
        ITimeProvider timeProvider
    )
    {
        // get data type
        var dataType = statement.Update.Table?.ObjectType;

        // weird, but possible according to nullability spec
        if (dataType is null)
            return statement;

        // basic and fast way to ensure processability
        if (!dataType.IsAssignableTo(typeof(ICreatedUpdatedTimeEntity)))
            return statement;

        // get table schema descriptor
        var descriptor = cn.MappingSchema.GetEntityDescriptor(dataType);

        // resolve UpdatedAt column descriptor
        var updatedTime = descriptor.Columns.FindColumn(nameof(ICreatedUpdatedTimeEntity.UpdatedAt));

        var stmt = statement.CloneWithoutParams();
        var update = stmt.Update;
        update.SetValue(updatedTime, timeProvider.Now);

        return stmt;
    }

    /// <summary>
    /// Processes INSERT OR UPDATE (UPSERT) queries to manage both created and updated time values appropriately.
    /// </summary>
    /// <param name="cn">The data connection.</param>
    /// <param name="statement">The SQL INSERT OR UPDATE statement.</param>
    /// <param name="timeProvider">The time provider for current timestamp.</param>
    /// <returns>The processed SQL statement with time fields managed for both insert and update operations.</returns>
    private static SqlStatement ProcessInsertOrUpdateTimeQuery(
        this DataConnection cn,
        SqlInsertOrUpdateStatement statement,
        ITimeProvider timeProvider
    )
    {
        // get data type (generally it's impossible to have different tables in insert/update clauses, so simply use insert one to get object type)
        var dataType = statement.Insert.Into?.ObjectType;

        // weird, but possible according to nullability spec
        if (dataType is null)
            return statement;

        // basic and fast way to ensure processability
        if (!dataType.IsAssignableTo(typeof(ICreatedTimeEntity)))
            return statement;

        // get table schema descriptor
        var descriptor = cn.MappingSchema.GetEntityDescriptor(dataType);

        // resolve CreatedAt column descriptor
        var createdTime = descriptor.Columns.FindColumn(nameof(ICreatedTimeEntity.CreatedAt));

        // if source is additionally updatable
        var updatedTime = dataType.IsAssignableTo(typeof(ICreatedUpdatedTimeEntity))
            ? descriptor.Columns.FindColumn(nameof(ICreatedUpdatedTimeEntity.UpdatedAt))
            : null;

        var stmt = statement.CloneWithoutParams();
        var insert = stmt.Insert;
        var update = stmt.Update;
        var now = timeProvider.Now;

        insert.SetValue(createdTime, now);
        update.IgnoreValue(createdTime);

        if (updatedTime is not null)
        {
            insert.SetValue(updatedTime, now);
            update.SetValue(updatedTime, now);
        }

        return stmt;
    }
}
