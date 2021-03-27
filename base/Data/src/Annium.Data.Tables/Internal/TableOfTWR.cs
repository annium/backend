using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Annium.Core.Mapper;
using Annium.Core.Primitives;
using static Annium.Data.Tables.Internal.TableHelper;

namespace Annium.Data.Tables.Internal
{
    internal sealed class Table<TR, TW> : TableBase<TR>, ITable<TR, TW>
        where TR : IEquatable<TR>, ICopyable<TR>
        where TW : notnull
    {
        public int Count
        {
            get
            {
                lock (DataLocker)
                {
                    return _readTable.Count;
                }
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

        public void Init(IReadOnlyCollection<TW> entries)
        {
            EnsurePermission(TablePermission.Init);

            IReadOnlyCollection<TR> readView;
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

                readView = _readTable.Values.ToArray();
            }

            AddEvent(ChangeEvent.Init(readView));
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
                    EnsurePermission(TablePermission.Remove);
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
            }

            Cleanup();
        }

        public void Delete(TW entry)
        {
            EnsurePermission(TablePermission.Remove);
            var key = _getKey(entry);

            lock (DataLocker)
                if (_writeTable.Remove(key))
                {
                    _readTable.Remove(key, out var item);
                    AddEvent(ChangeEvent.Delete(item!));
                }

            Cleanup();
        }

        private IReadOnlyCollection<TR> Get()
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
            }

            AddEvents(removed.Select(ChangeEvent.Delete).ToArray());
        }

        public void Dispose()
        {
            _writeTable.Clear();
            _readTable.Clear();
        }

        public IEnumerator<TR> GetEnumerator() => Get().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Get().GetEnumerator();
    }
}