using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions
{
    public interface IHttpException
    {
        public IResultBase Result { get; }
    }
}