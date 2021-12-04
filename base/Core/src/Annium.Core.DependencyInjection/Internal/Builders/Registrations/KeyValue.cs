namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

public sealed record KeyValue<TKey, TValue>(TKey Key, TValue Value);