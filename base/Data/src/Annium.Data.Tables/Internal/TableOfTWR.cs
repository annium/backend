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

internal sealed class Table<TR, TW> : TableBase<TR>, ITable<TR, TW>
    where TR : IEquatable<TR>, ICopyable<TR>
    where TW : notnull
{
    public override int Count
    {
        get
        {
            lock (DataLocker)
                return _readTable.Count;
        }
    }

    public IReadOnlyDictionary<int, TW> Source
    {
        get
        {
            lock (DataLocker)
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
        Func<TW, TR> toRead,
        ILogger<Table<TR, TW>> logger
    ) : base(permissions, logger)
    {
        _getKey = BuildGetKey(key);
        _update = BuildUpdate<TW>(permissions);
        _isActive = isActive;
        _toRead = toRead;
    }

    public int GetKey(TW value) => _getKey(value);

    public void Init(IReadOnlyCollection<TW> entries)
    {
        EnsurePermission(TablePermission.Init);

        lock (DataLocker)
        {
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
    }

    public void Set(TW entry)
    {
        var key = _getKey(entry);

        lock (DataLocker)
        {
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
    }

    public void Delete(TW entry)
    {
        EnsurePermission(TablePermission.Delete);
        var key = _getKey(entry);

        lock (DataLocker)
        {
            if (_writeTable.Remove(key))
            {
                _readTable.Remove(key, out var item);
                AddEvent(ChangeEvent.Delete(item!));
            }

            Cleanup();
        }
    }

    protected override IReadOnlyCollection<TR> Get()
    {
        lock (DataLocker)
            return _readTable.Values.ToArray();
    }

    private void Cleanup()
    {
        var removed = new List<TR>();

        lock (DataLocker)
        {
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
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        lock (DataLocker)
        {
            _writeTable.Clear();

            _readTable.Clear();
        }
    }
}