using System;
using System.Linq.Expressions;
using Annium.Core.Mapper;
using Annium.Core.Primitives;

namespace Annium.Data.Tables;

public interface ITableBuilder<out TR, TW, TM>
    where TR : IEquatable<TR>, ICopyable<TR>
    where TW : notnull
    where TM : notnull
{
    ITableBuilder<TR, TW, TM> Allow(TablePermission permissions);
    ITableBuilder<TR, TW, TM> Key(Expression<Func<TW, object>> getKey);
    ITableBuilder<TR, TW, TM> Keep(Func<TW, bool> isActive);
    ITable<TR, TW> Build(IMapper mapper);
}