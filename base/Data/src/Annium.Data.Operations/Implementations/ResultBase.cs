using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Data.Operations.Implementations
{
    internal abstract class ResultBase<T> : IResultBase<T>, IResultBase
        where T : class, IResultBase<T>
    {
        public IEnumerable<string> PlainErrors => _plainErrors;

        public IReadOnlyDictionary<string, IEnumerable<string>> LabeledErrors =>
            _labeledErrors.ToDictionary(pair => pair.Key, pair => pair.Value as IEnumerable<string>);

        public bool HasErrors => _plainErrors.Count > 0 || _labeledErrors.Count > 0;
        private readonly object _locker = new object();
        private readonly HashSet<string> _plainErrors = new HashSet<string>();
        private readonly Dictionary<string, HashSet<string>> _labeledErrors = new Dictionary<string, HashSet<string>>();

        public abstract T Clone();

        public T Clear()
        {
            lock (_locker)
            {
                _plainErrors.Clear();
                _labeledErrors.Clear();
            }

            return (this as T) !;
        }

        public T Error(string error)
        {
            lock (_locker)
                _plainErrors.Add(error);

            return (this as T) !;
        }

        public T Error(string label, string error)
        {
            lock (_locker)
            {
                if (!_labeledErrors.ContainsKey(label))
                    _labeledErrors[label] = new HashSet<string>();
                _labeledErrors[label].Add(error);
            }

            return (this as T) !;
        }

        public T Errors(params string[] errors)
        {
            lock (_locker)
                foreach (var error in errors)
                    _plainErrors.Add(error);

            return (this as T) !;
        }

        public T Errors(IEnumerable<string> errors)
        {
            lock (_locker)
                foreach (var error in errors)
                    _plainErrors.Add(error);

            return (this as T) !;
        }

        public T Errors(params ValueTuple<string, IEnumerable<string>>[] errors)
        {
            lock (_locker)
            {
                foreach (var (label, labelErrors) in errors)
                {
                    if (!_labeledErrors.ContainsKey(label))
                        _labeledErrors[label] = new HashSet<string>();
                    foreach (var error in labelErrors)
                        _labeledErrors[label].Add(error);
                }
            }

            return (this as T) !;
        }

        public T Errors(IReadOnlyCollection<KeyValuePair<string, IEnumerable<string>>> errors)
        {
            lock (_locker)
            {
                foreach (var (label, labelErrors) in errors)
                {
                    if (!_labeledErrors.ContainsKey(label))
                        _labeledErrors[label] = new HashSet<string>();
                    foreach (var error in labelErrors)
                        _labeledErrors[label].Add(error);
                }
            }

            return (this as T) !;
        }

        public T Join(params IResultBase[] results)
        {
            foreach (var result in results)
            {
                Errors(result.PlainErrors);
                Errors(result.LabeledErrors);
            }

            return (this as T) !;
        }

        public T Join(IEnumerable<IResultBase> results)
        {
            foreach (var result in results)
            {
                Errors(result.PlainErrors);
                Errors(result.LabeledErrors);
            }

            return (this as T) !;
        }

        protected void CloneTo(T clone)
        {
            clone.Errors(PlainErrors);
            clone.Errors(LabeledErrors);
        }
    }
}