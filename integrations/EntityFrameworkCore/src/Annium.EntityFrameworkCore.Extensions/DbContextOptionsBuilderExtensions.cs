using Annium.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Annium.EntityFrameworkCore.Extensions;

/// <summary>
/// Extension methods for configuring Entity Framework Core DbContextOptionsBuilder
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Configures the context to use custom entity materializer that supports materialization hooks
    /// </summary>
    /// <param name="builder">The DbContextOptionsBuilder to configure</param>
    /// <returns>The configured DbContextOptionsBuilder for chaining</returns>
    public static DbContextOptionsBuilder UseMaterializationHooks(this DbContextOptionsBuilder builder) =>
        builder.ReplaceService<IEntityMaterializerSource, EntityMaterializerSource>();
}
