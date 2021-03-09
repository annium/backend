using System.Collections.Generic;

namespace Annium.Data.Tables
{
    public static class ChangeEvent
    {
        public static IChangeEvent<T> Init<T>(IReadOnlyCollection<T> values) => new InitEvent<T>(values);
        public static IChangeEvent<T> Add<T>(T value) => new AddEvent<T>(value);
        public static IChangeEvent<T> Update<T>(T oldValue, T newValue) => new UpdateEvent<T>(oldValue, newValue);
        public static IChangeEvent<T> Delete<T>(T value) => new DeleteEvent<T>(value);
    }

    public class InitEvent<T> : IChangeEvent<T>
    {
        public IReadOnlyCollection<T> Values { get; }

        internal InitEvent(IReadOnlyCollection<T> values)
        {
            Values = values;
        }

        public override string ToString() => $"Init {typeof(T).Name} with {Values.Count} value(s)";
    }

    public class AddEvent<T> : IChangeEvent<T>
    {
        public T Value { get; }

        internal AddEvent(T value)
        {
            Value = value;
        }

        public override string ToString() => $"Add {typeof(T).Name} {Value}";
    }

    public class UpdateEvent<T> : IChangeEvent<T>
    {
        public T OldValue { get; }
        public T NewValue { get; }

        internal UpdateEvent(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public override string ToString() => $"Update {typeof(T).Name} {OldValue} -> {NewValue}";
    }

    public class DeleteEvent<T> : IChangeEvent<T>
    {
        public T Value { get; }

        internal DeleteEvent(T value)
        {
            Value = value;
        }

        public override string ToString() => $"Delete {typeof(T).Name} {Value}";
    }

    public interface IChangeEvent<out T>
    {
    }
}