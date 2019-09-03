using Annium.Data.Operations;
using Annium.Localization.Abstractions;

namespace Annium.Extensions.Composition
{
    public class CompositionContext<TValue>
    {
        public TValue Root { get; }
        public string Label { get; }
        public string Field { get; }
        private readonly IResult result;
        private readonly ILocalizer localizer;

        internal CompositionContext(
            TValue root,
            string label,
            string field,
            IResult result,
            ILocalizer localizer
        )
        {
            Root = root;
            Label = label;
            Field = field;
            this.result = result;
            this.localizer = localizer;
        }

        public void Error(string error, params object[] arguments)
        {
            result.Error(Label, localizer[error, arguments]);
        }
    }
}