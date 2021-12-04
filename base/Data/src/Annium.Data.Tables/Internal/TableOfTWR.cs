using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using Annium.Core.Primitives;
using Annium.Core.Primitives.Collections.Generic;
using static Annium.Data.Tables.Internal.TableHelper;

namespace Annium.Data.Tables.Internal;

internal sealed class Table<TR, TW> : TableBase<TR>, ITable<TR, TW>
    where TR : IEquatable<TR>, ICopyable<TR>
    where TW : notnull
{
    public override int Count
    {
        get
        {
            using var _ = DataLocker.Lock();
            return _readTable.Count;
        }
    }

    public IReadOnlyDictionary<int, TW> Source
    {
        get
        {
            using var _ = DataLocker.Lock();
            return _writeTable.ToDictionary();
        }
    }

    private readonly Dictionary<int, TW> _writeTable = new();
    private readonly Dictionary<int, TR> _readTable = new();
    private readonly Func<TW, int> _getKey;
    private readonly Action<TW, TW> _update;
    private readonly Func<TW, bool> _isActive;
    private readonly Func<TW, TR> _toRead;

    public Table(
        TablePermission permissions,
        Expression<Func<TW, object>> key,
        Func<TW, bool> isActive,
        IMapper mapper
    ) : base(permissions)
    {
        _getKey = BuildGetKey(key);
        _update = BuildUpdate<TW>(permissions);
        _isActive = isActive;
        _toRead = x => mapper.Map<TR>(x);
    }

    public int GetKey(TW value) => _getKey(value);

    public void Init(IReadOnlyCollection<TW> entries)
    {
        EnsurePermission(TablePermission.Init);

        using var _ = DataLocker.Lock();

        _writeTable.Clear();
        _readTable.Clear();

        foreach (var entry in entries.Where(_isActive))
        {
            var key = _getKey(entry);
            _writeTable[key] = entry;
            _readTable[key] = _toRead(entry);
        }

        AddEvent(ChangeEvent.Init(_readTable.Values.ToArray()));
    }

    public void Set(TW entry)
    {
        var key = _getKey(entry);

        using var _ = DataLocker.Lock();

        var exists = _writeTable.ContainsKey(key);
        if (!exists)
        {
            EnsurePermission(TablePermission.Add);
            var newValue = _readTable[key] = _toRead(_writeTable[key] = entry);
            AddEvent(ChangeEvent.Add(newValue));
        }
        // exists and is inactive
        else if (!_isActive(_writeTable[key]))
        {
            EnsurePermission(TablePermission.Delete);
            _writeTable.Remove(key);
            _readTable.Remove(key, out var item);
            AddEvent(ChangeEvent.Delete(item!));
        }
        // exists and is active
        else
        {
            EnsurePermission(TablePermission.Update);
            var oldValue = _readTable[key];
            _update(_writeTable[key], entry);
            var newValue = _readTable[key] = _toRead(_writeTable[key]);
            if (!newValue.Equals(oldValue))
                AddEvent(ChangeEvent.Update(oldValue, newValue));
        }

        Cleanup();
    }

    public void Delete(TW entry)
    {
        EnsurePermission(TablePermission.Delete);
        var key = _getKey(entry);

        using var _ = DataLocker.Lock();

        if (_writeTable.Remove(key))
        {
            _readTable.Remove(key, out var item);
            AddEvent(ChangeEvent.Delete(item!));
        }

        Cleanup();
    }

    protected override IReadOnlyCollection<TR> Get()
    {
        using var _ = DataLocker.Lock();

        return _readTable.Values.ToArray();
    }

    private void Cleanup()
    {
        var removed = new List<TR>();

        using var _ = DataLocker.Lock();

        var entries = _writeTable.Values.Except(_writeTable.Values.Where(_isActive)).ToArray();

        foreach (var entry in entries)
        {
            var key = _getKey(entry);
            _writeTable.Remove(key);
            _readTable.Remove(key, out var item);
            removed.Add(item!);
        }

        AddEvents(removed.Select(ChangeEvent.Delete).ToArray());
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        using var _ = DataLocker.Lock();
        _writeTable.Clear();

        _readTable.Clear();
    }
}