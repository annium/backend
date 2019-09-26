using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Storage.Abstractions
{
    public abstract class StorageBase : IStorage
    {
        private static readonly Regex nameRe = new Regex(@"^(?:[A-z0-9]|\.?[A-z0-9]+[A-z0-9-_.]*[A-z0-9]+)$", RegexOptions.Compiled | RegexOptions.Singleline);

        private readonly ILogger logger;

        public StorageBase(
            ILogger logger
        )
        {
            this.logger = logger;
        }

        public Task SetupAsync() => SafeAsync("setup", DoSetupAsync);

        public Task<string[]> ListAsync() => SafeAsync("list", DoListAsync);

        public Task UploadAsync(Stream source, string name) => SafeAsync("upload", () => DoUploadAsync(source, name));

        public Task<Stream> DownloadAsync(string name) => SafeAsync("download", () => DoDownloadAsync(name));

        public Task<bool> DeleteAsync(string name) => SafeAsync("upload", () => DoDeleteAsync(name));

        protected abstract Task DoSetupAsync();

        protected abstract Task<string[]> DoListAsync();

        protected abstract Task DoUploadAsync(Stream source, string name);

        protected abstract Task<Stream> DoDownloadAsync(string name);

        protected abstract Task<bool> DoDeleteAsync(string name);

        protected async Task<T> SafeAsync<T>(string operation, Func<Task<T>> handleAsync)
        {
            try
            {
                logger.Debug($"{operation} start");
                var result = await handleAsync();
                logger.Debug($"{operation} succeed");

                return result;
            }
            catch
            {
                logger.Debug($"{operation} failed");
                throw;
            }
        }

        protected async Task SafeAsync(string operation, Func<Task> handleAsync)
        {
            try
            {
                logger.Debug($"{operation} start");
                await handleAsync();
                logger.Debug($"{operation} succeed");
            }
            catch
            {
                logger.Debug($"{operation} failed");
                throw;
            }
        }

        protected void VerifyName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (!nameRe.IsMatch(name))
                throw new ArgumentException($"Name {name} in invalid");
        }

        protected void VerifyPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (!path.StartsWith('/'))
                throw new ArgumentException($"Path {path} is not absolute");

            if (path.EndsWith('/'))
                throw new ArgumentException($"Path {path} must not end with /");

            foreach (var part in getPathParts())
                if (!nameRe.IsMatch(part))
                    throw new ArgumentException($"Path part {part} has invalid format");

            string[] getPathParts() => path
                .TrimStart('/')
                .Split('/');
        }
    }
}