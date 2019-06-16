using System;
using Annium.Data.Operations;

namespace Annium.Extensions.Validation
{
    public class ValidationContext<TValue>
    {
        public TValue Root { get; }
        public string Label { get; }
        public string Field { get; }
        internal BooleanResult Result { get; }

        internal ValidationContext(
            TValue root,
            string label,
            string name,
            BooleanResult result
        )
        {
            Root = root;
            Label = label;
            Field = name;
            Result = result;
        }

        public void Error(string error)
        {
            Result.Error(Label, error);
        }
    }
}