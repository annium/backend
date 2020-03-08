using System;
using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions
{
    public class ConflictException : Exception, IHttpException
    {
        public IResult Result { get; }

        public ConflictException(IResult result)
        {
            Result = result;
        }
    }
}