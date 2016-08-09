using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PCLCrypto;

namespace GPSOAuthSharp
{
    internal class GoogleKeyUtils
    {
        // BitConverter has different endianness, hence the Reverse()
        public static RSAParameters KeyFromB64(string b64Key)
        {
            byte[] decoded = Convert.FromBase64String(b64Key);
            int modLength = BitConverter.ToInt32(decoded.Take(4).Reverse().ToArray(), 0);
            byte[] mod = decoded.Skip(4).Take(modLength).ToArray();
            int expLength = BitConverter.ToInt32(decoded.Skip(modLength + 4).Take(4).Reverse().ToArray(), 0);
            byte[] exponent = decoded.Skip(modLength + 8).Take(expLength).ToArray();
            var rsaKeyInfo = new RSAParameters
            {
                Modulus = mod,
                Exponent = exponent
            };
            return rsaKeyInfo;
        }

        // Python version returns a string, but we use byte[] to get the same results
        public static byte[] KeyToStruct(RSAParameters key)
        {
            byte[] modLength = { 0x00, 0x00, 0x00, 0x80 };
            byte[] mod = key.Modulus;
            byte[] expLength = { 0x00, 0x00, 0x00, 0x03 };
            byte[] exponent = key.Exponent;
            return DataTypeUtils.CombineBytes(modLength, mod, expLength, exponent);
        }

        public static Dictionary<string, string> ParseAuthResponse(string text)
        {
            var split = text.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            return split.Select(line => line.Split(new[] {'='}, 2)).ToDictionary(parts => parts[0], parts => parts[1]);
        }

        public static string CreateSignature(string email, string password, RSAParameters key)
        {
            IAsymmetricKeyAlgorithmProvider alg = WinRTCrypto.AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithm.RsaOaepSha1);
            var importedKey = alg.ImportParameters(key);
            var hasher = WinRTCrypto.HashAlgorithmProvider.OpenAlgorithm(HashAlgorithm.Sha1);
            byte[] prefix = { 0x00 };
            byte[] keyArray = KeyToStruct(key);
            byte[] something = Encoding.UTF8.GetBytes(email + "\x00" + password);
            byte[] hash = hasher.HashData(keyArray).Take(4).ToArray();
            byte[] encrypted = WinRTCrypto.CryptographicEngine.Encrypt(importedKey, something);
            byte[] combinedBytes = DataTypeUtils.CombineBytes(prefix, hash, encrypted);
            return DataTypeUtils.UrlSafeBase64(combinedBytes);
        }
    }
}
