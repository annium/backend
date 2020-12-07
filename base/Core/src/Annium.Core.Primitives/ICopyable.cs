namespace Annium.Core.Primitives
{
    public interface ICopyable<out T>
    {
        T Copy();
    }
}