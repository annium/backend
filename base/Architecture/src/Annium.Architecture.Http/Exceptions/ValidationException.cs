using System;
using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions
{
    public class ValidationException : Exception, IHttpException
    {
        public IResult Result { get; }

        public ValidationException(IResult result)
        {
            Result = result;
        }
    }
}