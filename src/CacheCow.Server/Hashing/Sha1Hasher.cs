using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CacheCow.Server
{
    /// <summary>
    /// Sha1 impl
    /// </summary>
    public class Sha1Hasher : IHasher
    {
        private readonly SHA256 _sha1 = SHA256.Create();

        public string ComputeHash(byte[] bytes)
        {
            return Convert.ToBase64String(_sha1.ComputeHash(bytes));
        }

        public void Dispose()
        {
            _sha1.Dispose();
        }
    }
}
