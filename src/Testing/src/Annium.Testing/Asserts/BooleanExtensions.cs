namespace Annium.Testing
{
    public static class BooleanExtensions
    {
        public static void IsTrue(this bool value, string message = null)
        {
            if (!value)
                throw new AssertionFailedException(message ?? $"{value} != True");
        }

        public static void IsFalse(this bool value, string message = null)
        {
            if (value)
                throw new AssertionFailedException(message ?? $"{value} != False");
        }
    }
}