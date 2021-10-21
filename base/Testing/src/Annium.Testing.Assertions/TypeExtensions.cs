namespace Annium.Testing
{
    public static class TypeExtensions
    {
        public static T As<T>(this object? value, string message = "")
        {
            (value is T).IsTrue(string.IsNullOrEmpty(message) ? $"{value} is {value?.GetType()}, not {typeof(T)}" : message);

            return (T) value!;
        }

        public static T AsExact<T>(this object? value, string message = "")
        {
            (value?.GetType() == typeof(T)).IsTrue(string.IsNullOrEmpty(message) ? $"{value} is {value?.GetType()}, not {typeof(T)}" : message);

            return (T) value!;
        }
    }
}