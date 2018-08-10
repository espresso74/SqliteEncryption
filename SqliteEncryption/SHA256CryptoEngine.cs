using System.Text;
using PCLCrypto;

namespace Demo.Encrypt
{
    public static class SHA256CryptoEngine
    {
        public static string SHA256Hash(string data)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            var hasher = WinRTCrypto.HashAlgorithmProvider.OpenAlgorithm(HashAlgorithm.Sha256); 
            byte[] hash = hasher.HashData(byteData); 

            StringBuilder sb = new StringBuilder(64);
            foreach (byte h in hash)
            {
                sb.Append(h.ToString("x2"));
            }

            return sb.ToString();
        }
    }

}
