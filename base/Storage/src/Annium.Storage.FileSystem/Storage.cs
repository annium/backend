using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Annium.Storage.Abstractions;

namespace Annium.Storage.FileSystem
{
    internal class Storage : StorageBase
    {
        private readonly string directory;

        public Storage(
            Configuration configuration,
            ILogger<Storage> logger
        ) : base(logger)
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            VerifyPath(configuration.Directory);

            directory = configuration.Directory;
        }

        protected override Task DoSetupAsync()
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            return Task.CompletedTask;
        }

        protected override Task<string[]> DoListAsync()
        {
            var entries = Directory.GetFiles(directory).Select(e => Path.GetRelativePath(directory, e)).ToArray();

            return Task.FromResult(entries);
        }

        protected override async Task DoUploadAsync(Stream source, string name)
        {
            VerifyName(name);

            var path = Path.Combine(directory, name);
            using (var target = File.Open(Path.Combine(directory, name), FileMode.Create))
            {
                source.Position = 0;
                await source.CopyToAsync(target);
            }
        }

        protected override async Task<Stream> DoDownloadAsync(string name)
        {
            VerifyName(name);

            var path = Path.Combine(directory, name);
            if (!File.Exists(path))
                throw new KeyNotFoundException($"{name} not found in storage");

            using (var source = File.Open(path, FileMode.Open))
            {
                var ms = new MemoryStream();

                source.Position = 0;
                await source.CopyToAsync(ms);
                ms.Position = 0;

                return ms;
            }
        }

        protected override Task<bool> DoDeleteAsync(string name)
        {
            VerifyName(name);

            var path = Path.Combine(directory, name);
            if (!File.Exists(path))
                return Task.FromResult(false);

            File.Delete(path);

            return Task.FromResult(true);
        }
    }
}