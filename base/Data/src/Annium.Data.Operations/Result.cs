using System.Collections.Generic;

namespace Annium.Data.Operations
{
    public static class Result
    {
        public static BooleanResult Join(params IResult[] results) =>
            new BooleanResult(true).Join(results);

        public static BooleanResult Join(IEnumerable<IResult> results) =>
            new BooleanResult(true).Join(results);

        public static BooleanResult New() =>
            new BooleanResult(true);

        public static BooleanResult Success() =>
            new BooleanResult(true);

        public static BooleanResult Failure() =>
            new BooleanResult(false);

        public static BooleanResult<D> Success<D>(D data) =>
            new BooleanResult<D>(true, data);

        public static BooleanResult<D> Failure<D>(D data) =>
            new BooleanResult<D>(false, data);

        public static StatusResult<S> New<S>(S status) =>
            new StatusResult<S>(status);

        public static StatusResult<S, D> New<S, D>(S status, D data) =>
            new StatusResult<S, D>(status, data);
    }
}