using System;
using Annium.Data.Operations;

namespace Annium.Extensions.Validation
{
    public class ValidationContext<TValue>
    {
        public TValue Root { get; }
        public string Label { get; }
        public string Name { get; }
        private readonly BooleanResult result;

        internal ValidationContext(
            TValue root,
            string label,
            string name,
            BooleanResult result
        )
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("label must be specified", nameof(label));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("name must be specified", nameof(name));

            Root = root;
            Label = label;
            Name = name;
            this.result = result;
        }

        public void Error(string error)
        {
            result.Error(Label, error);
        }
    }
}