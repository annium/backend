using System;
using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions
{
    public class ValidationException : Exception, IHttpException
    {
        public IResultBase Result { get; }

        public ValidationException(IResultBase result)
        {
            Result = result;
        }
    }
}