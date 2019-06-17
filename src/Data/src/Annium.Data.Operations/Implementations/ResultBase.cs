using System;
using System.Collections.Generic;

namespace Annium.Data.Operations
{
    public abstract class ResultBase<T> : IResult<T> where T : ResultBase<T>
    {
        public IEnumerable<string> PlainErrors => plainErrors;

        public IReadOnlyDictionary<string, string> LabeledErrors => labeledErrors;

        public bool HasErrors => plainErrors.Count > 0 || labeledErrors.Count > 0;

        private HashSet<string> plainErrors = new HashSet<string>();

        private Dictionary<string, string> labeledErrors = new Dictionary<string, string>();

        public T Error(string error)
        {
            lock(plainErrors)
            plainErrors.Add(error);

            return this as T;
        }

        public T Error(string label, string error)
        {
            lock(labeledErrors)
            labeledErrors[label] = error;

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

        public T Errors(params ValueTuple<string, string>[] errors)
        {
            lock(labeledErrors)
            foreach (var(label, error) in errors)
                labeledErrors[label] = error;

            return this as T;
        }

        public T Errors(IReadOnlyCollection<KeyValuePair<string, string>> errors)
        {
            lock(labeledErrors)
            foreach (var(label, error) in errors)
                labeledErrors[label] = error;

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