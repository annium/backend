namespace Annium.Data.Operations
{
    public interface IStatusResult<T, S, D> : IStatusResult<T, S> where T : IStatusResult<T, S, D>
    {
        D Data { get; }

        void Deconstruct(out S status, out D data);
    }

    public interface IStatusResult<T, S> : IResult<T> where T : IStatusResult<T, S>
    {
        S Status { get; }
    }
}