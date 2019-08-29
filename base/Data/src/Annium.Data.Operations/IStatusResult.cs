namespace Annium.Data.Operations
{
    public interface IStatusResult<S, D> : IResultBase<IStatusResult<S, D>>
    {
        S Status { get; }
        D Data { get; }

        void Deconstruct(out S status, out D data);
    }

    public interface IStatusResult<S> : IResultBase<IStatusResult<S>>
    {
        S Status { get; }
    }
}