namespace Annium.Data.Operations
{
    public interface IBooleanResult<T, D> : IBooleanResult<T> where T : IBooleanResult<T, D>
    {
        D Data { get; }

        void Deconstruct(out bool succeed, out D data);
    }

    public interface IBooleanResult<T> : IResult<T> where T : IBooleanResult<T>
    {
        bool IsSuccess { get; }

        bool IsFailure { get; }
    }
}