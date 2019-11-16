namespace Annium.Extensions.Pooling.Storages
{
    internal class Slot<T>
    {
        public T Item { get; set; }
        public bool IsInUse { get; set; }

        public Slot(T item)
        {
            Item = item;
        }
    }
}