using System.Collections.Generic;

namespace Annium.Data.Operations
{
    public static class Result
    {
        public static IResult Join(params IResult[] results) =>
            new Implementations.Result().Join(results);

        public static IResult Join(IEnumerable<IResult> results) =>
            new Implementations.Result().Join(results);

        public static IResult New() =>
            new Implementations.Result();

        public static IBooleanResult Success() =>
            new Implementations.BooleanResult(true);

        public static IBooleanResult Failure() =>
            new Implementations.BooleanResult(false);

        public static IBooleanResult<D> Success<D>(D data) =>
            new Implementations.BooleanResult<D>(true, data);

        public static IBooleanResult<D> Failure<D>(D data) =>
            new Implementations.BooleanResult<D>(false, data);

        public static IStatusResult<S> New<S>(S status) =>
            new Implementations.StatusResult<S>(status);

        public static IStatusResult<S, D> New<S, D>(S status, D data) =>
            new Implementations.StatusResult<S, D>(status, data);
    }
}