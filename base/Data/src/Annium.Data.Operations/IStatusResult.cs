namespace Annium.Data.Operations
{
    public interface IStatusResult<S, D> : IResultBase<IStatusResult<S, D>>, IDataResultBase<D>
    {
        S Status { get; }

        void Deconstruct(out S status, out D data);
    }

    public interface IStatusResult<S> : IResultBase<IStatusResult<S>>, IResultBase
    {
        S Status { get; }
    }
}