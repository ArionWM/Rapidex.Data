using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Rapidex
{
    public static class HashHelper
    {
        //http://peterkellner.net/2010/11/24/efficiently-generating-sha256-checksum-for-files-using-csharp/
        public static string GetShaChecksum(Stream stream)
        {
            using (var bufferedStream = new BufferedStream(stream, 1024 * 128))
            {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(bufferedStream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        private static MurmurHash2UInt32Hack _hasher = new MurmurHash2UInt32Hack();

        public static int GetStableHashCode(byte[] data)
        {
            data.NotNull();
            uint hashu = _hasher.Hash(data);
            int hash;
            hash = unchecked((int)hashu);
            return hash;
        }

        /// <summary>
        /// Unchanged hashcode creator for strings
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int GetStableHashCode(this string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text);
            return GetStableHashCode(data);
        }
    }
}
