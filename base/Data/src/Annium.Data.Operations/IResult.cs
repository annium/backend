namespace Annium.Data.Operations
{
    public interface IResult<D> : IResultBase<IResult<D>>
    {
        D Data { get; }
    }

    public interface IResult : IResultBase<IResult>
    {
    }
}