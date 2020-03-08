using System;
using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions
{
    public class NotFoundException : Exception, IHttpException
    {
        public IResult Result { get; }

        public NotFoundException(IResult result)
        {
            Result = result;
        }
    }
}