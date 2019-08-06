namespace Annium.Testing
{
    public static class ArrayExtensions
    {
        public static T At<T>(this T[] value, int key)
        {
            var total = value.Length;
            (0 <= key && key < total).IsTrue($"Index `{key}` is out of bounds [0,{total-1}]");

            return value[key];
        }

        public static T[] Has<T>(this T[] value, int count)
        {
            var total = value.Length;
            total.IsEqual(count, $"Array expected to have `{count}` items, but has `{total}` items");

            return value;
        }

        public static T[] IsEmpty<T>(this T[] value)
        {
            var total = value.Length;
            total.IsEqual(0, $"Array expected to be empty, but has `{total}` items");

            return value;
        }
    }
}