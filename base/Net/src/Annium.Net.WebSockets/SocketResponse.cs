namespace Annium.Net.WebSockets
{
    public class SocketResponse<T>
    {
        public bool IsSocketOpen { get; }
        public T Data { get; }

        public SocketResponse(bool isSocketOpen, T data)
        {
            IsSocketOpen = isSocketOpen;
            Data = data;
        }
    }
}