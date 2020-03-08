using System;
using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions
{
    public class ForbiddenException : Exception, IHttpException
    {
        public IResultBase Result { get; }

        public ForbiddenException(IResultBase result)
        {
            Result = result;
        }
    }
}