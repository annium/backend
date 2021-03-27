using System;
using System.Linq.Expressions;
using Annium.Core.Mapper;
using Annium.Core.Primitives;

namespace Annium.Data.Tables
{
    public interface ITableBuilder<out TR, TW>
        where TR : IEquatable<TR>, ICopyable<TR>
        where TW : notnull
    {
        ITableBuilder<TR, TW> Allow(TablePermission permissions);
        ITableBuilder<TR, TW> Key(Expression<Func<TW, object>> getKey);
        ITableBuilder<TR, TW> Keep(Func<TW, bool> isActive);
        ITable<TR, TW> Build(IMapper mapper);
    }
}
