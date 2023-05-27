using LinqToDB.Configuration;

namespace Annium.linq2db.Extensions.Models;

/// <summary>
/// Configuration container per <see cref="TConnection"/>
/// </summary>
/// <param name="Options">Connection options</param>
/// <typeparam name="TConnection">Connection type, configuration is specific for</typeparam>
public sealed record Config<TConnection>(LinqToDBConnectionOptions Options);