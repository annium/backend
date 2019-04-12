using System.IO;
using System.Security.Cryptography;
using PemUtils;

namespace Annium.Security.Cryptography
{
    public class KeyReader
    {
        public RSAParameters ReadRsaKey(Stream stream)
        {
            using(var reader = new PemReader(stream))
            {
                return reader.ReadRsaKey();
            }
        }
    }
}