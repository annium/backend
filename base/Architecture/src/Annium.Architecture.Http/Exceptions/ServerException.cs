using System;
using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions
{
    public class ServerException : Exception, IHttpException
    {
        public IResult Result { get; }

        public ServerException(IResult result)
        {
            Result = result;
        }
    }
}