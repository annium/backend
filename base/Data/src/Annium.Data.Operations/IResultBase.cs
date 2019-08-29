using System;
using System.Collections.Generic;

namespace Annium.Data.Operations
{
    public interface IResultBase<T> : IReadOnlyResultBase<T>
    {
        T Clear();

        T Error(string error);

        T Error(string label, string error);

        T Errors(params string[] errors);

        T Errors(IEnumerable<string> errors);

        T Errors(params ValueTuple<string, IEnumerable<string>>[] errors);

        T Errors(IReadOnlyCollection<KeyValuePair<string, IEnumerable<string>>> errors);

        T Join(params IResultBase[] results);

        T Join(IEnumerable<IResultBase> results);
    }

    public interface IReadOnlyResultBase<T> : IResultBase
    {
        T Clone();
    }

    public interface IResultBase
    {
        IEnumerable<string> PlainErrors { get; }
        IReadOnlyDictionary<string, IEnumerable<string>> LabeledErrors { get; }
        bool HasErrors { get; }
    }
}