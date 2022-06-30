using System;
using System.Linq.Expressions;
using Annium.Core.Mapper;
using Annium.Core.Primitives;

namespace Annium.Data.Tables;

public interface ITableBuilder<TR, TW>
    where TR : IEquatable<TR>, ICopyable<TR>
    where TW : notnull
{
    ITableBuilder<TR, TW> Allow(TablePermission permissions);
    ITableBuilder<TR, TW> Key(Expression<Func<TW, object>> getKey);
    ITableBuilder<TR, TW> Keep(Func<TW, bool> isActive);
    ITableBuilder<TR, TW> MapWith(Func<TW, TR> toRead);
    ITable<TR, TW> Build(IMapper mapper);
}