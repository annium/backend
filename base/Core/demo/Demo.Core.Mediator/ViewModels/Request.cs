namespace Demo.Core.Mediator.ViewModels
{
    internal class Request<T>
    {
        public string Value { get; }

        public Request(string value)
        {
            Value = value;
        }
    }
}