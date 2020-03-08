using System;
using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions
{
    public class ForbiddenException : Exception, IHttpException
    {
        public IResult Result { get; }

        public ForbiddenException(IResult result)
        {
            Result = result;
        }
    }
}