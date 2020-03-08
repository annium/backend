using System;
using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions
{
    public class ConflictException : Exception, IHttpException
    {
        public IResultBase Result { get; }

        public ConflictException(IResultBase result)
        {
            Result = result;
        }
    }
}