using System;
using System.Collections.Generic;

namespace Annium.Data.Operations
{
    public interface IResult<T> : IResult where T : IResult<T>
    {
        T Error(string error);

        T Error(string label, string error);

        T Errors(params string[] errors);

        T Errors(IEnumerable<string> errors);

        T Errors(params ValueTuple<string, string>[] errors);

        T Errors(IReadOnlyCollection<KeyValuePair<string, string>> errors);

        T Join(params IResult[] results);

        T Join(IEnumerable<IResult> results);
    }

    public interface IResult
    {
        IEnumerable<string> PlainErrors { get; }

        IReadOnlyDictionary<string, string> LabeledErrors { get; }

        bool HasErrors { get; }
    }
}