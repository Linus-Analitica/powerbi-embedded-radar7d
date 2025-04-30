using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace TemplateAngularCoreSAML.API.Services
{
    public static class CryptographycFunctions
    {
        public static KeyValuePair<bool, string> Encrypt(string Text, string sKey = "test")
        {
            try
            {
                // 1. Declaramos las variables
                var tag = new byte[16];
                var plainData = Encoding.UTF8.GetBytes(Text);
                var cipherBytes = new byte[plainData.Length];
                var nonceBytes = Encoding.UTF8.GetBytes(GenerateSha256Hash("TemplateAngularCoreSaml")[..12]);
                using var cipher = new AesGcm(Encoding.UTF8.GetBytes(sKey)[..16], 16);
                // 2. Encriptamos
                cipher.Encrypt(nonceBytes, plainData, cipherBytes, tag);
                // 3. Convertimos encriptación a formato hexadecimal
                var joinByteArrays = JoinByteArrays(cipherBytes, tag);
                StringBuilder stringBuilder = new();
                foreach (byte b in joinByteArrays)
                {
                    stringBuilder.AppendFormat("{0:X2}", b);
                }
                // 4. Retornamos el resultado
                return new KeyValuePair<bool, string>(true, stringBuilder.ToString());
            }
            catch (Exception ex)
            {
                return new KeyValuePair<bool, string>(false, ex.Message);
            }

        }

        public static KeyValuePair<bool, string> Decrypt(string Text, string sKey = "test")
        {
            try
            {
                // 0. Desconvertimos encriptación del formato hexadecimal
                int len;
                len = Text.Length / 2;
                byte[] encryptedBytes = new byte[len];
                int x, i;
                for (x = 0; x < len; x++)
                {
                    i = Convert.ToInt32(Text.Substring(x * 2, 2), 16);
                    encryptedBytes[x] = (byte)i;
                }
                // 1. Declaramos las variables
                var cipherBytes = encryptedBytes[..^16]; //tag size is 16
                var tag = encryptedBytes[^16..];
                var decryptedData = new byte[cipherBytes.Length];
                var nonceBytes = Encoding.UTF8.GetBytes(GenerateSha256Hash("TemplateAngularCoreSaml")[..12]);
                using var cipher = new AesGcm(Encoding.UTF8.GetBytes(sKey)[..16], 16);
                // 2. Desencriptamos
                cipher.Decrypt(nonceBytes, cipherBytes, tag, decryptedData);
                // 3. Retornamos el resultado
                return new KeyValuePair<bool, string>(true, Encoding.UTF8.GetString(decryptedData));
            }
            catch (Exception ex)
            {
                return new KeyValuePair<bool, string>(false, ex.Message);
            }
        }

        public static string GenerateSha256Hash(string key)
        {
            using SHA256 sha256 = SHA256.Create();
            string hash = GetHash(sha256, key);

            return hash;
        }

        public static bool ValidateClientSecret(string clientApplicationKey, string clientSecret)
        {
            using SHA256 sha256 = SHA256.Create();
            return VerifyHash(sha256, clientApplicationKey, clientSecret);
        }

        private static string GetHash(HashAlgorithm hashAlgorithm, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder stringBuilder = new();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                stringBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return stringBuilder.ToString();
        }

        private static bool VerifyHash(HashAlgorithm hashAlgorithm, string input, string hash)
        {
            var hashOfInput = GetHash(hashAlgorithm, input);
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            return stringComparer.Compare(hashOfInput, hash) == 0;
        }
        private static byte[] JoinByteArrays(byte[] array1, byte[] array2)
        {
            byte[] newArray = new byte[array1.Length + array2.Length];
            Buffer.BlockCopy(array1, 0, newArray, 0, array1.Length);
            Buffer.BlockCopy(array2, 0, newArray, array1.Length, array2.Length);
            return newArray;
        }

        public static SigningCredentials GetSigningCredentials(string secret)
        {
            var test = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)), SecurityAlgorithms.HmacSha256);
            return test;
        }
    }
}
