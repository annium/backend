using Annium.Data.Operations;
using Annium.Localization.Abstractions;

namespace Annium.Extensions.Validation
{
    public class ValidationContext<TValue>
    {
        public TValue Root { get; }
        public string Label { get; }
        public string Field { get; }
        internal IBooleanResult Result { get; }
        private readonly ILocalizer localizer;

        internal ValidationContext(
            TValue root,
            string label,
            string name,
            IBooleanResult result,
            ILocalizer localizer
        )
        {
            Root = root;
            Label = label;
            Field = name;
            Result = result;
            this.localizer = localizer;
        }

        public void Error(string error, params object[] arguments)
        {
            Result.Error(Label, localizer[error, arguments]);
        }
    }
}