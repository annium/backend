namespace Annium.Data.Operations
{
    public interface IBooleanResult<D> : IResultBase<IBooleanResult<D>>
    {
        bool IsSuccess { get; }
        bool IsFailure { get; }
        D Data { get; }

        void Deconstruct(out bool succeed, out D data);
    }

    public interface IBooleanResult : IResultBase<IBooleanResult>
    {
        bool IsSuccess { get; }
        bool IsFailure { get; }
    }
}