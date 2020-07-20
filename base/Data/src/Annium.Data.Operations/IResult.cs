namespace Annium.Data.Operations
{
    public interface IResult<D> : IResultBase<IResult<D>>, IDataResultBase<D>
    {
    }

    public interface IResult : IResultBase<IResult>, IResultBase
    {
    }
}