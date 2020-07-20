namespace Annium.Data.Operations
{
    public interface IBooleanResult<D> : IResultBase<IBooleanResult<D>>, IDataResultBase<D>
    {
        bool IsSuccess { get; }
        bool IsFailure { get; }

        void Deconstruct(out bool succeed, out D data);
    }

    public interface IBooleanResult : IResultBase<IBooleanResult>, IResultBase
    {
        bool IsSuccess { get; }
        bool IsFailure { get; }
    }
}