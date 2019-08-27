using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Annium.Extensions.Validation
{
    public static class BaseRuleExtensions
    {
        public static IRuleBuilder<TValue, string> Required<TValue>(
            this IRuleBuilder<TValue, string> rule,
            string message = null
        ) => rule.Add((context, value) =>
        {
            if (string.IsNullOrWhiteSpace(value))
                context.Error(message ?? "Value is required");
        });

        public static IRuleBuilder<TValue, TField> Required<TValue, TField>(
            this IRuleBuilder<TValue, TField> rule,
            string message = null
        ) => rule.Add((context, value) =>
        {
            if (Equals(value, default(TField)))
                context.Error(message ?? "Value is required");
        });

        public static IRuleBuilder<TValue, TField> Equal<TValue, TField>(
            this IRuleBuilder<TValue, TField> rule,
            TField target,
            string message = null
        ) => rule.Add((context, value) =>
        {
            if (!Equals(value, target))
                context.Error(message ?? "Value is not equal to given");
        });

        public static IRuleBuilder<TValue, TField> In<TValue, TField>(
            this IRuleBuilder<TValue, TField> rule,
            IEnumerable<TField> targets,
            string message = null
        ) => rule.Add((context, value) =>
        {
            if (!targets.Any(target => Equals(value, target)))
                context.Error(message ?? "Value is not in given");
        });

        public static IRuleBuilder<TValue, TField> Equal<TValue, TField>(
            this IRuleBuilder<TValue, TField> rule,
            Func<TValue, TField> target,
            string message = null
        ) => rule.Add((context, value) =>
        {
            if (!Equals(value, target(context.Root)))
                context.Error(message ?? "Value is not equal to given");
        });

        public static IRuleBuilder<TValue, TField> NotEqual<TValue, TField>(
            this IRuleBuilder<TValue, TField> rule,
            TField target,
            string message = null
        ) => rule.Add((context, value) =>
        {
            if (Equals(value, target))
                context.Error(message ?? "Value is equal to given");
        });

        public static IRuleBuilder<TValue, TField> NotIn<TValue, TField>(
            this IRuleBuilder<TValue, TField> rule,
            IEnumerable<TField> targets,
            string message = null
        ) => rule.Add((context, value) =>
        {
            if (targets.Any(target => Equals(value, target)))
                context.Error(message ?? "Value is in given");
        });

        public static IRuleBuilder<TValue, TField> NotEqual<TValue, TField>(
            this IRuleBuilder<TValue, TField> rule,
            Func<TValue, TField> target,
            string message = null
        ) => rule.Add((context, value) =>
        {
            if (Equals(value, target(context.Root)))
                context.Error(message ?? "Value is equal to given");
        });

        public static IRuleBuilder<TValue, string> Length<TValue>(
            this IRuleBuilder<TValue, string> rule,
            int minLength,
            int maxLength,
            string message = null
        ) => rule.Add((context, value) =>
        {
            if (value?.Length < minLength)
                context.Error(message ?? "Value length is less, than {0}", minLength);

            if (value?.Length > maxLength)
                context.Error(message ?? "Value length is greater, than {0}", maxLength);
        });

        public static IRuleBuilder<TValue, string> MinLength<TValue>(
            this IRuleBuilder<TValue, string> rule,
            int minLength,
            string message = null
        ) => rule.Add((context, value) =>
        {
            if (value?.Length < minLength)
                context.Error(message ?? "Value length is less, than {0}", minLength);
        });

        public static IRuleBuilder<TValue, string> MaxLength<TValue>(
            this IRuleBuilder<TValue, string> rule,
            int maxLength,
            string message = null
        ) => rule.Add((context, value) =>
        {
            if (value?.Length > maxLength)
                context.Error(message ?? "Value length is greater, than {0}", maxLength);
        });

        public static IRuleBuilder<TValue, TField> Between<TValue, TField>(
            this IRuleBuilder<TValue, TField> rule,
            TField min,
            TField max,
            string message = null
        ) where TField : IComparable<TField> => rule.Add((context, value) =>
        {
            if (value?.CompareTo(min) == -1)
                context.Error(message ?? "Value is less, than given minimum");

            if (value?.CompareTo(max) == 1)
                context.Error(message ?? "Value is greater, than given maximum");
        });

        public static IRuleBuilder<TValue, TField> LessThan<TValue, TField>(
            this IRuleBuilder<TValue, TField> rule,
            TField min,
            string message = null
        ) where TField : IComparable<TField> => rule.Add((context, value) =>
        {
            if (value?.CompareTo(min) >= 0)
                context.Error(message ?? "Value is greater, than given maximum");
        });

        public static IRuleBuilder<TValue, TField> LessThanOrEqual<TValue, TField>(
            this IRuleBuilder<TValue, TField> rule,
            TField min,
            string message = null
        ) where TField : IComparable<TField> => rule.Add((context, value) =>
        {
            if (value?.CompareTo(min) > 0)
                context.Error(message ?? "Value is greater, than given maximum");
        });

        public static IRuleBuilder<TValue, TField> GreaterThan<TValue, TField>(
            this IRuleBuilder<TValue, TField> rule,
            TField max,
            string message = null
        ) where TField : IComparable<TField> => rule.Add((context, value) =>
        {
            if (value?.CompareTo(max) <= 0)
                context.Error(message ?? "Value is less, than given minimum");
        });

        public static IRuleBuilder<TValue, TField> GreaterThanOrEqual<TValue, TField>(
            this IRuleBuilder<TValue, TField> rule,
            TField max,
            string message = null
        ) where TField : IComparable<TField> => rule.Add((context, value) =>
        {
            if (value?.CompareTo(max) < 0)
                context.Error(message ?? "Value is less, than given minimum");
        });

        public static IRuleBuilder<TValue, string> Matches<TValue>(
            this IRuleBuilder<TValue, string> rule,
            string regex,
            string message = null
        ) => rule.Add((context, value) =>
        {
            var re = new Regex(regex, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
            if (value != null && !re.IsMatch(value))
                context.Error(message ?? "Value doesn't match specified regex");
        });

        public static IRuleBuilder<TValue, string> Email<TValue>(
            this IRuleBuilder<TValue, string> rule,
            string message = null
        ) => rule.Add((context, value) =>
        {
            var index = value?.IndexOf('@') ?? -1;
            if (index < 1 || index >= value.Length - 1)
                context.Error(message ?? "Value is not an email");
        });

        public static IRuleBuilder<TValue, TField> Enum<TValue, TField>(
            this IRuleBuilder<TValue, TField> rule,
            string message = null
        )
        {
            var type = typeof(TField);
            if (!type.IsEnum)
                throw new ArgumentException($"{type.FullName} is not Enum type");

            return rule.Add((context, value) =>
            {
                if (!System.Enum.IsDefined(type, value))
                    context.Error(message ?? "Value is not in expected range");
            });
        }

        private static bool Equals<TField>(TField x, TField y) =>
            EqualityComparer<TField>.Default.Equals(x, y);
    }
}