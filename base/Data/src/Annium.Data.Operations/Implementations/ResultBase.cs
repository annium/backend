using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Data.Operations.Implementations
{
    internal abstract class ResultBase<T> : IResultBase<T> where T : class, IResultBase<T>
    {
        public IEnumerable<string> PlainErrors => plainErrors;

        public IReadOnlyDictionary<string, IEnumerable<string>> LabeledErrors =>
            labeledErrors.ToDictionary(pair => pair.Key, pair => pair.Value as IEnumerable<string>);

        public bool HasErrors => plainErrors.Count > 0 || labeledErrors.Count > 0;
        private readonly HashSet<string> plainErrors = new HashSet<string>();
        private readonly Dictionary<string, HashSet<string>> labeledErrors = new Dictionary<string, HashSet<string>>();

        protected ResultBase()
        {
        }

        public abstract T Clone();

        public T Clear()
        {
            plainErrors.Clear();
            labeledErrors.Clear();

            return (this as T) !;
        }

        public T Error(string error)
        {
            plainErrors.Add(error);

            return (this as T) !;
        }

        public T Error(string label, string error)
        {
            lock (labeledErrors)
            {
                if (!labeledErrors.ContainsKey(label))
                    labeledErrors[label] = new HashSet<string>();
                labeledErrors[label].Add(error);
            }

            return (this as T) !;
        }

        public T Errors(params string[] errors)
        {
            lock (plainErrors)
                foreach (var error in errors)
                    plainErrors.Add(error);

            return (this as T) !;
        }

        public T Errors(IEnumerable<string> errors)
        {
            lock (plainErrors)
                foreach (var error in errors)
                    plainErrors.Add(error);

            return (this as T) !;
        }

        public T Errors(params ValueTuple<string, IEnumerable<string>>[] errors)
        {
            lock (labeledErrors)
            {
                foreach (var (label, labelErrors) in errors)
                {
                    if (!labeledErrors.ContainsKey(label))
                        labeledErrors[label] = new HashSet<string>();
                    foreach (var error in labelErrors)
                        labeledErrors[label].Add(error);
                }
            }

            return (this as T) !;
        }

        public T Errors(IReadOnlyCollection<KeyValuePair<string, IEnumerable<string>>> errors)
        {
            lock (labeledErrors)
            {
                foreach (var (label, labelErrors) in errors)
                {
                    if (!labeledErrors.ContainsKey(label))
                        labeledErrors[label] = new HashSet<string>();
                    foreach (var error in labelErrors)
                        labeledErrors[label].Add(error);
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