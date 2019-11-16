using System;
namespace Annium.Extensions.Pooling
{
    public class CacheReference<TValue> : IDisposable
    where TValue : notnull
    {
        public TValue Value { get; private set; }
        private readonly Action dispose;

        public CacheReference(
            TValue value,
            Action dispose
        )
        {
            Value = value;
            this.dispose = dispose;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
                return;

            if (disposing)
            {
                Value = default!;
                dispose();
            }

            disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}