using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Annium.Core.Primitives;
using static Annium.Data.Tables.Internal.TableHelper;

namespace Annium.Data.Tables.Internal
{
    internal sealed class Table<T> : TableBase<T>, ITable<T>
        where T : IEquatable<T>, ICopyable<T>
    {
        public int Count
        {
            get
            {
                lock (DataLocker)
                {
                    return _table.Count;
                }
            }
        }

        private readonly Dictionary<int, T> _table = new Dictionary<int, T>();
        private readonly Func<T, int> _getKey;
        private readonly Action<T, T> _update;
        private readonly Func<T, bool> _isActive;

        public Table(
            TablePermission permissions,
            Expression<Func<T, object>> key,
            Func<T, bool> isActive
        ) : base(permissions)
        {
            _getKey = BuildGetKey(key);
            _update = BuildUpdate<T>(permissions);
            _isActive = isActive;
        }

        public void Init(IReadOnlyCollection<T> entries)
        {
            EnsurePermission(TablePermission.Init);

            IReadOnlyCollection<T> readView;
            lock (DataLocker)
            {
                _table.Clear();

                foreach (var entry in entries.Where(_isActive))
                {
                    var key = _getKey(entry);
                    _table[key] = entry;
                }

                readView = _table.Values.ToArray();
            }

            AddEvent(ChangeEvent.Init(readView));
        }

        public void Set(T entry)
        {
            var key = _getKey(entry);

            lock (DataLocker)
            {
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
                    EnsurePermission(TablePermission.Remove);
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
            }

            Cleanup();
        }

        public void Delete(T entry)
        {
            EnsurePermission(TablePermission.Remove);
            var key = _getKey(entry);

            lock (DataLocker)
                if (_table.Remove(key, out var item))
                    AddEvent(ChangeEvent.Delete(item!));

            Cleanup();
        }

        private IReadOnlyCollection<T> Get()
        {
            lock (DataLocker)
                return _table.Values.ToArray();
        }

        private void Cleanup()
        {
            var removed = new List<T>();

            lock (DataLocker)
            {
                var entries = _table.Values.Except(_table.Values.Where(_isActive)).ToArray();
                foreach (var entry in entries)
                {
                    var key = _getKey(entry);
                    _table.Remove(key, out var item);
                    removed.Add(item!);
                }
            }

            AddEvents(removed.Select(ChangeEvent.Delete).ToArray());
        }

        public void Dispose()
        {
            _table.Clear();
        }

        public IEnumerator<T> GetEnumerator() => Get().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Get().GetEnumerator();
    }
}
