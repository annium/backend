using System;
using System.Collections;
using System.Collections.Generic;

namespace Annium.Core.Reflection.Tests.Types.Extensions.ResolveGenericArgumentsByImplementation
{
    public class ParentOther<T1, T2> : Base<T1[], T2, bool, IEnumerable<T1[]>>, IParentOther<T1, T2> where T2 : struct
    {
    }

    public class ParentTwo<T1, T2> : ParentOne<T1, IReadOnlyList<T2>>, IParentTwo<T1, T2> where T1 : struct where T2 : IEnumerable
    {
    }

    public class ParentOne<T1, T2> : Base<List<T2>, T1, int, IEnumerable<List<T2>>>, IParentOne<T1, T2> where T1 : struct
    {
    }

    public class Base<T1, T2, T3, T4> : IBase<T1, T2, T3, T4> where T1 : class where T2 : struct where T4 : IEnumerable<T1>
    {
    }

    public class ParentDictionary<T1, T2> : CustomDictionary<T2, T1> where T2 : notnull
    {
    }

    public class CustomDictionary<T1, T2> : Dictionary<T1, T2> where T1 : notnull
    {
    }

    public class ClassParametrized<T> : ClassBase
    {
        public T X { get; }

        public ClassParametrized(T x)
        {
            X = x;
        }
    }

    public class ClassSimple : ClassBase
    {
    }

    public class ClassBase
    {
    }

    public struct BaseStruct<T1, T2, T3, T4> : IBase<T1, T2, T3, T4> where T1 : class where T2 : struct where T4 : IEnumerable<T1>
    {
    }

    public interface IParentOther<T1, T2> : IBase<T1[], T2, bool, IEnumerable<T1[]>> where T2 : struct
    {
    }

    public interface IParentTwo<T1, T2> : IParentOne<T1, IReadOnlyList<T2>> where T1 : struct where T2 : IEnumerable
    {
    }

    public interface IParentOne<T1, T2> : IBase<List<T2>, T1, int, IEnumerable<List<T2>>> where T1 : struct
    {
    }

    public interface IBase<T1, T2, T3, T4> where T1 : class where T2 : struct where T4 : IEnumerable<T1>
    {
    }

    public struct StructParamatered
    {
        public int X { get; }

        public StructParamatered(int x)
        {
            X = x;
        }
    }

    public struct StructParamaterless
    {
    }

    public struct StructEnumerable : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class ConstrainedComplex<T1, T2, T3, T4> : IGeneric<T1> where T1 : IGeneric<T2, T3> where T3 : IGeneric<T2, T4>
    {
    }

    public interface IGeneric<T>
    {
    }

    public interface IGeneric<T1, T2>
    {
    }

    public interface IClassConstraint<T> where T : class
    {
    }

    public interface IStructConstraint<T> where T : struct
    {
    }

    public interface INewConstraint<T> where T : new()
    {
    }

    public interface IParameterInterfaceConstraint<T> where T : IEnumerable
    {
    }

    public interface IParameterClassConstraint<T> where T : ClassBase
    {
    }

    public interface IParameterStructConstraint<T> where T : IEquatable<T>
    {
    }
}