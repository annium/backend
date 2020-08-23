namespace Demo.Core.Mediator.ViewModels
{
    internal class Response<T>
    {
        public string Value { get; }

        public Response(string value)
        {
            Value = value;
        }
    }
}