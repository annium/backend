using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using NodaTime;

namespace Annium.Extensions.Jobs.Internal;

internal class IntervalParser : IIntervalParser
{
    public Func<LocalDateTime, Duration> GetDelayResolver(string interval)
    {
        var intervals = interval.Split(' ').Select(e => e.Trim()).ToArray();
        if (intervals.Length != 5)
            throw new ArgumentException("Interval format has 5 parts: second minute hour day dayOfWeek");

        var dateTime = Expression.Variable(typeof(LocalDateTime));
        var expressions = new List<Expression>();
        expressions.Add(Expression.Call(
            null,
            DurationFromMethod<long>(nameof(Duration.FromMilliseconds)),
            Expression.Modulo(
                Expression.Subtract(
                    Expression.Constant(1000L),
                    Expression.Convert(LocalDateTimeProperty(dateTime, nameof(LocalDateTime.Millisecond)), typeof(long))
                ),
                Expression.Constant(1000L)
            )
        ));

        var expression = Expression.Lambda(Expression.Block(expressions), false, dateTime);
        var resolver = (Func<LocalDateTime, Duration>) expression.Compile();

        return resolver;
    }


    private Expression GetPartExpression(
        Expression dateTime,
        Expression property,
        string name,
        string interval,
        uint min,
        uint max
    )
    {
        // if every interval - return null
        if (interval == "*")
            return Expression.Empty();

        // if const - test equality
        if (uint.TryParse(interval, out var value))
            if (value >= min && value <= max)
                return Expression.Equal(property, Expression.Constant((int) value));
            else
                throw new ArgumentOutOfRangeException(nameof(interval), value, $"'{name}' must be in [{min};{max}] range");

        // if list - handle with "or" equality
        if (Regex.IsMatch(interval, "([0-9]{1,2})(?:,[0-9]{1,2})+"))
        {
            var values = interval.Split(',').Select(uint.Parse).ToArray();
            if (values.Any(v => v < min || v > max))
                throw new ArgumentOutOfRangeException(nameof(interval), $"'{name}' must be in [{min};{max}] range");

            var result = Expression.Equal(property, Expression.Constant((int) values[0]));
            foreach (var orValue in values.Skip(1))
                result = Expression.Or(
                    result,
                    Expression.Equal(property, Expression.Constant((int) orValue))
                );

            return result;
        }

        throw new InvalidOperationException($"Can't handle scheduler interval part '{interval}'");
    }

    private static MemberExpression LocalDateTimeProperty(ParameterExpression ex, string name) =>
        Expression.Property(ex, typeof(LocalDateTime).GetProperty(name)!);

    private static MethodInfo DurationFromMethod<T>(string name) =>
        typeof(Duration).GetMethod(name, new[] { typeof(T) })!;
}