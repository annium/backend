using System;
using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions
{
    public class NotFoundException : Exception, IHttpException
    {
        public IResultBase Result { get; }

        public NotFoundException(IResultBase result)
        {
            Result = result;
        }
    }
}