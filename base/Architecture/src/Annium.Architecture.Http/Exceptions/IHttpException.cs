using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions
{
    public interface IHttpException
    {
        public IResult Result { get; }
    }
}