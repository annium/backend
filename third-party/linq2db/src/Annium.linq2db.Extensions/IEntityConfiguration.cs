using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions
{
    public interface IEntityConfiguration<TEntity>
        where TEntity : class
    {
        void Configure(EntityMappingBuilder<TEntity> builder);
    }
}