using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Annium.Storage.Abstractions;

namespace Annium.Storage.InMemory
{
    public class InMemoryStorage : StorageBase
    {
        private IDictionary<string, byte[]> storage = new Dictionary<string, byte[]>();

        public InMemoryStorage(ILogger<InMemoryStorage> logger) : base(logger) { }

        protected override Task DoSetupAsync() => Task.CompletedTask;

        protected override Task<string[]> DoListAsync() => Task.FromResult(storage.Keys.ToArray());

        protected override async Task DoUploadAsync(Stream source, string name)
        {
            VerifyName(name);

            using(var ms = new MemoryStream())
            {
                ms.Position = 0;
                await source.CopyToAsync(ms);
                storage[name] = ms.ToArray();
            }
        }

        protected override Task<Stream> DoDownloadAsync(string name)
        {
            VerifyName(name);

            if (!storage.ContainsKey(name))
                throw new KeyNotFoundException($"{name} not found in storage");

            return Task.FromResult<Stream>(new MemoryStream(storage[name]));
        }

        protected override Task<bool> DoDeleteAsync(string name)
        {
            VerifyName(name);

            if (!storage.ContainsKey(name))
                return Task.FromResult(false);

            storage.Remove(name);

            return Task.FromResult(true);
        }
    }
}