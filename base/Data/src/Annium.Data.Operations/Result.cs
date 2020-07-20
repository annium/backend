using System.Collections.Generic;
using Annium.Data.Operations.Implementations;

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

        public static IResult<D> New<D>(D data) =>
            new Result<D>(data);

        public static IBooleanResult Success() =>
            new BooleanResult(true);

        public static IBooleanResult Failure() =>
            new BooleanResult(false);

        public static IBooleanResult<D> Success<D>(D data) =>
            new BooleanResult<D>(true, data);

        public static IBooleanResult<D> Failure<D>(D data) =>
            new BooleanResult<D>(false, data);

        public static IStatusResult<S> Status<S>(S status) =>
            new StatusResult<S>(status);

        public static IStatusResult<S, D> Status<S, D>(S status, D data) =>
            new StatusResult<S, D>(status, data);
    }
}