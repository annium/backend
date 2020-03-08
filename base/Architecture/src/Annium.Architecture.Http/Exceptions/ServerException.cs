using System;
using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions
{
    public class ServerException : Exception, IHttpException
    {
        public IResultBase Result { get; }

        public ServerException(IResultBase result)
        {
            Result = result;
        }
    }
}