namespace Demo.Core.Mediator.Requests
{
    internal class ListTodosRequest : IRequest
    {
        public string Query { get; }

        public ListTodosRequest(string query)
        {
            Query = query;
        }
    }
}