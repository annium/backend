namespace Annium.Testing;

public static class BooleanExtensions
{
    public static void IsTrue(this bool value, string message = "")
    {
        if (!value)
            throw new AssertionFailedException(string.IsNullOrEmpty(message) ? $"{value} != True" : message);
    }

    public static void IsFalse(this bool value, string message = "")
    {
        if (value)
            throw new AssertionFailedException(string.IsNullOrEmpty(message) ? $"{value} != False" : message);
    }
}