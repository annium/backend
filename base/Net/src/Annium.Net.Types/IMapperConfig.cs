using System;

namespace Annium.Net.Types;

public interface IMapperConfig
{
    #region base

    IMapperConfig RegisterBaseType(Type type, string name);
    bool IsBaseType(Type type);

    #endregion

    #region ignored

    IMapperConfig RegisterIgnored(Predicate<Type> matcher);
    bool IsIgnored(Type type);

    #endregion

    #region known

    IMapperConfig RegisterKnown(Predicate<Type> matcher);
    bool IsKnown(Type type);

    #endregion

    #region array

    IMapperConfig RegisterArray(Type type);
    bool IsArray(Type type);

    #endregion

    #region record

    IMapperConfig RegisterRecord(Type type);
    bool IsRecord(Type type);

    #endregion
}