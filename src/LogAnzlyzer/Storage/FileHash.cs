using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LogAnzlyzer.Storage
{
    public static class FileHash
    {
        public static string Sha256(string path)
        {
            using (var sha = SHA256.Create())
            using (var fs = File.OpenRead(path))
            {
                var bytes = sha.ComputeHash(fs);
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
