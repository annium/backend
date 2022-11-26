using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Annium.Core.Primitives;
using NodaTime;

namespace Annium.Extensions.Jobs.Internal;

internal class IntervalParser : IIntervalParser
{
    private const string EveryMoment = "*";

    public Func<LocalDateTime, Duration> GetDelayResolver(string interval)
    {
        if (interval == Interval.Secondly)
            return _ => Duration.Zero;

        var intervals = interval.Split(' ').Select(e => e.Trim()).ToArray();
        if (intervals.Length != 5)
            throw new ArgumentException("Interval format has 5 parts: second minute hour day day-of-week");

        if (intervals[3] != EveryMoment && intervals[4] != EveryMoment)
            throw new ArgumentException($"Interval format {interval} must not have conditions on both day and day-of-week parts");

        var dateTime = Expression.Variable(typeof(LocalDateTime), "x");
        var expressions = new List<Expression>();
        var parts = new[]
        {
            GetRepeatablePartExpression(
                Property(dateTime, nameof(LocalDateTime.Second)),
                FromLong(nameof(Duration.FromSeconds)),
                "second", intervals[0], 0, 59, 60),
            GetRepeatablePartExpression(
                Property(dateTime, nameof(LocalDateTime.Minute)),
                FromLong(nameof(Duration.FromMinutes)),
                "minute", intervals[1], 0, 59, 60),
            GetRepeatablePartExpression(
                Property(dateTime, nameof(LocalDateTime.Hour)),
                FromInt(nameof(Duration.FromHours)),
                "hour", intervals[2], 0, 23, 24),
            GetRepeatablePartExpression(
                Property(dateTime, nameof(LocalDateTime.Day)),
                FromInt(nameof(Duration.FromDays)),
                "day", intervals[3], 0, 29, 30),
            GetExactPartExpression(
                Expression.Convert(Property(dateTime, nameof(LocalDateTime.DayOfWeek)), typeof(int)),
                FromInt(nameof(Duration.FromDays)),
                "day of week", intervals[4], 0, 6, 7)
        }.OfType<Expression>().ToArray();

        Expression match = parts.Length == 0 ? Expression.Constant(Duration.Zero) : parts[0];
        foreach (var part in parts.Skip(1))
            match = Expression.Add(match, part);

        expressions.Add(match);

        var expression = Expression.Lambda(Expression.Block(expressions), false, dateTime);
        var resolver = (Func<LocalDateTime, Duration>) expression.Compile();

        return resolver;
    }

    private Expression? GetRepeatablePartExpression(
        Expression property,
        Func<Expression, Expression> from,
        string name,
        string interval,
        uint min,
        uint max,
        uint size
    )
    {
        // if every interval - return null
        if (interval == "*")
            return null;

        // if every X
        if (TryAsModulo(property, from, name, interval, min, max, size, out var moduloExpression))
            return moduloExpression;

        // if at constant X
        if (TryAsConst(property, from, name, interval, min, max, size, out var constExpression))
            return constExpression;

        // if at one of X
        if (TryAsList(property, from, name, interval, min, max, size, out var listExpression))
            return listExpression;

        throw new InvalidOperationException($"Can't handle scheduler interval part '{interval}'");
    }

    private Expression? GetExactPartExpression(
        Expression property,
        Func<Expression, Expression> from,
        string name,
        string interval,
        uint min,
        uint max,
        uint size
    )
    {
        // if every interval - return null
        if (interval == "*")
            return null;

        // if at constant X
        if (TryAsConst(property, from, name, interval, min, max, size, out var constExpression))
            return constExpression;

        // if at one of X
        if (TryAsList(property, from, name, interval, min, max, size, out var listExpression))
            return listExpression;

        throw new InvalidOperationException($"Can't handle scheduler interval part '{interval}'");
    }

    private bool TryAsModulo(
        Expression property,
        Func<Expression, Expression> from,
        string name,
        string interval,
        uint min,
        uint max,
        uint size,
        [NotNullWhen(true)] out Expression? result
    )
    {
        result = null;

        if (!interval.StartsWith("*/"))
            return false;

        var moduloString = interval[2..];
        if (!int.TryParse(moduloString, out var value))
            throw new ArgumentException($"'{name}' modulo {value} is not valid");

        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(nameof(interval), value, $"'{name}' modulo {value} must be in [{min};{max}] range");

        if (size % value != 0)
            throw new ArgumentOutOfRangeException(nameof(interval), value, $"'{name}' modulo {value} is not acceptable for range {size}");

        result = from(Expression.Condition(
            Expression.Equal(Expression.Modulo(property, Expression.Constant(value)), Expression.Constant(0)),
            Expression.Constant(0),
            Expression.Subtract(
                Expression.Constant(value),
                Expression.Modulo(property, Expression.Constant(value))
            )
        ));

        return true;
    }

    private bool TryAsConst(
        Expression property,
        Func<Expression, Expression> from,
        string name,
        string interval,
        uint min,
        uint max,
        uint size,
        [NotNullWhen(true)] out Expression? result
    )
    {
        result = null;

        if (!int.TryParse(interval, out var value))
            return false;

        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(nameof(interval), value, $"'{name}' value {value} must be in [{min};{max}] range");

        result = from(Expression.Condition(
            Expression.LessThan(Expression.Constant(value), property),
            Expression.Subtract(Expression.Constant(value + (int) size), property),
            Expression.Subtract(Expression.Constant(value), property)
        ));

        return true;
    }

    private bool TryAsList(
        Expression property,
        Func<Expression, Expression> from,
        string name,
        string interval,
        uint min,
        uint max,
        uint size,
        [NotNullWhen(true)] out Expression? result
    )
    {
        result = null;

        var values = new HashSet<int>();
        foreach (var raw in interval.Split(','))
        {
            if (!int.TryParse(raw, out var value))
                return false;

            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(nameof(interval), value, $"'{name}' value {value} must be in [{min};{max}] range");

            values.Add(value);
        }

        if (values.Count < 2)
            return false;

        var (last, rest) = values.OrderByDescending(x => x).ToArray();
        Expression res = Expression.Condition(
            Expression.LessThan(property, Expression.Constant(last)),
            Expression.Subtract(Expression.Constant(last), property),
            Expression.Subtract(Expression.Constant(rest[^1] + (int) size), property)
        );
        foreach (var point in rest)
            res = Expression.Condition(
                Expression.LessThan(property, Expression.Constant(point)),
                Expression.Subtract(Expression.Constant(point), property),
                res
            );

        result = from(res);

        return true;
    }

    private static MemberExpression Property(ParameterExpression ex, string name) =>
        Expression.Property(ex, typeof(LocalDateTime).GetProperty(name)!);

    private static Func<Expression, Expression> FromInt(string name) =>
        ex => Expression.Call(null, typeof(Duration).GetMethod(name, new[] { typeof(int) })!, ex);

    private static Func<Expression, Expression> FromLong(string name) =>
        ex => Expression.Call(null, typeof(Duration).GetMethod(name, new[] { typeof(long) })!, Expression.Convert(ex, typeof(long)));
}