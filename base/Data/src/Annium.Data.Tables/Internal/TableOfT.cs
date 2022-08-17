using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Core.Primitives.Collections.Generic;
using Annium.Logging.Abstractions;
using static Annium.Data.Tables.Internal.TableHelper;

namespace Annium.Data.Tables.Internal;

internal sealed class Table<T> : TableBase<T>, ITable<T>
    where T : IEquatable<T>, ICopyable<T>
{
    public override int Count
    {
        get
        {
            using var _ = DataLocker.Lock();

            return _table.Count;
        }
    }

    public IReadOnlyDictionary<int, T> Source
    {
        get
        {
            using var _ = DataLocker.Lock();

            return _table.ToDictionary();
        }
    }

    private readonly Dictionary<int, T> _table = new();
    private readonly Func<T, int> _getKey;
    private readonly Action<T, T> _update;
    private readonly Func<T, bool> _isActive;

    public Table(
        TablePermission permissions,
        Expression<Func<T, object>> key,
        Func<T, bool> isActive,
        ILogger<Table<T>> logger
    ) : base(permissions, logger)
    {
        _getKey = BuildGetKey(key);
        _update = BuildUpdate<T>(permissions);
        _isActive = isActive;
    }

    public int GetKey(T value) => _getKey(value);

    public void Init(IReadOnlyCollection<T> entries)
    {
        EnsurePermission(TablePermission.Init);

        using var _ = DataLocker.Lock();

        _table.Clear();

        foreach (var entry in entries.Where(_isActive))
        {
            var key = _getKey(entry);
            _table[key] = entry;
        }

        AddEvent(ChangeEvent.Init(_table.Values.ToArray()));
    }

    public void Set(T entry)
    {
        var key = _getKey(entry);

        using var _ = DataLocker.Lock();

        var exists = _table.ContainsKey(key);
        if (!exists)
        {
            EnsurePermission(TablePermission.Add);
            var newValue = _table[key] = entry;
            AddEvent(ChangeEvent.Add(newValue));
        }
        // exists and is inactive
        else if (!_isActive(_table[key]))
        {
            EnsurePermission(TablePermission.Delete);
            _table.Remove(key, out var item);
            AddEvent(ChangeEvent.Delete(item!));
        }
        // exists and is active
        else
        {
            EnsurePermission(TablePermission.Update);
            var oldValue = _table[key].Copy();
            var newValue = _table[key];
            _update(newValue, entry);
            if (!newValue.Equals(oldValue))
                AddEvent(ChangeEvent.Update(oldValue, newValue));
        }

        Cleanup();
    }

    public void Delete(T entry)
    {
        EnsurePermission(TablePermission.Delete);
        var key = _getKey(entry);

        using var _ = DataLocker.Lock();

        if (_table.Remove(key, out var item))
            AddEvent(ChangeEvent.Delete(item));

        Cleanup();
    }

    protected override IReadOnlyCollection<T> Get()
    {
        using var _ = DataLocker.Lock();

        return _table.Values.ToArray();
    }

    private void Cleanup()
    {
        var removed = new List<T>();

        using var _ = DataLocker.Lock();

        var entries = _table.Values.Except(_table.Values.Where(_isActive)).ToArray();
        foreach (var entry in entries)
        {
            var key = _getKey(entry);
            _table.Remove(key, out var item);
            removed.Add(item!);
        }

        AddEvents(removed.Select(ChangeEvent.Delete).ToArray());
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        using var _ = DataLocker.Lock();
        _table.Clear();
    }
}