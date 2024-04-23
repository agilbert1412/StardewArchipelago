using System;
using System.Security.Cryptography;
using System.Text;

namespace StardewArchipelago.Extensions
{
    public static class HashExtensions
    {
        public static int GetHash(this string text)
        {
            if (text == null)
            {
                return 0;
            }

            using HashAlgorithm algorithm = SHA256.Create();
            var bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(text));
            var intValue = BitConverter.ToInt32(bytes, 0);
            return intValue;
        }
    }
}
