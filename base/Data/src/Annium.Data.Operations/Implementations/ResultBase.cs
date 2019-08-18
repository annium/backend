using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Data.Operations
{
    public abstract class ResultBase<T> : IResult<T> where T : ResultBase<T>
    {
        public IEnumerable<string> PlainErrors => plainErrors;

        public IReadOnlyDictionary<string, IEnumerable<string>> LabeledErrors =>
        labeledErrors.ToDictionary(pair => pair.Key, pair => pair.Value as IEnumerable<string>);

        public bool HasErrors => plainErrors.Count > 0 || labeledErrors.Count > 0;

        private HashSet<string> plainErrors = new HashSet<string>();

        private Dictionary<string, HashSet<string>> labeledErrors = new Dictionary<string, HashSet<string>>();

        public T Error(string error)
        {
            lock(plainErrors)
            plainErrors.Add(error);

            return this as T;
        }

        public T Error(string label, string error)
        {
            lock(labeledErrors)
            {
                if (!labeledErrors.ContainsKey(label))
                    labeledErrors[label] = new HashSet<string>();
                labeledErrors[label].Add(error);
            }

            return this as T;
        }

        public T Errors(params string[] errors)
        {
            lock(plainErrors)
            foreach (var error in errors)
                plainErrors.Add(error);

            return this as T;
        }

        public T Errors(IEnumerable<string> errors)
        {
            lock(plainErrors)
            foreach (var error in errors)
                plainErrors.Add(error);

            return this as T;
        }

        public T Errors(params ValueTuple<string, IEnumerable<string>>[] errors)
        {
            lock(labeledErrors)
            {
                foreach (var(label, labelErrors) in errors)
                {
                    if (!labeledErrors.ContainsKey(label))
                        labeledErrors[label] = new HashSet<string>();
                    foreach (var error in labelErrors)
                        labeledErrors[label].Add(error);
                }
            }

            return this as T;
        }

        public T Errors(IReadOnlyCollection<KeyValuePair<string, IEnumerable<string>>> errors)
        {
            lock(labeledErrors)
            {
                foreach (var(label, labelErrors) in errors)
                {
                    if (!labeledErrors.ContainsKey(label))
                        labeledErrors[label] = new HashSet<string>();
                    foreach (var error in labelErrors)
                        labeledErrors[label].Add(error);
                }
            }

            return this as T;
        }

        public T Join(params IResult[] results)
        {
            foreach (var result in results)
            {
                Errors(result.PlainErrors);
                Errors(result.LabeledErrors);
            }

            return this as T;
        }

        public T Join(IEnumerable<IResult> results)
        {
            foreach (var result in results)
            {
                Errors(result.PlainErrors);
                Errors(result.LabeledErrors);
            }

            return this as T;
        }
    }
}