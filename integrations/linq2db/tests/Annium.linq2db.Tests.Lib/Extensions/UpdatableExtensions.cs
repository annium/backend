using Annium.Data.Models;
using LinqToDB;
using LinqToDB.Linq;
using NodaTime;

namespace Annium.linq2db.Tests.Lib.Extensions;

public static class UpdatableExtensions
{
    public static IUpdatable<TEntity> WithUpdatedAt<TEntity>(this IUpdatable<TEntity> source)
        where TEntity : ICreatedUpdatedTimeEntity
    {
        return source.Set(x => x.UpdatedAt, default(Instant));
    }
}
