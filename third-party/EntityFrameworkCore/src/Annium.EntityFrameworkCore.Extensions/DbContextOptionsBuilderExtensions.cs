using Annium.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Annium.Core.DependencyInjection;

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseMaterializationHooks(this DbContextOptionsBuilder builder) =>
        builder.ReplaceService<IEntityMaterializerSource, EntityMaterializerSource>();
}