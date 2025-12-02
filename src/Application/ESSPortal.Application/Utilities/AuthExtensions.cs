using System.Security.Cryptography;
using System.Text;

namespace ESSPortal.Application.Utilities;

internal static class AuthExtensions
{
    public static string SecurityKey(out string hashString)
    {
        string secureRandomString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        using (var sha = SHA512.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(secureRandomString);
            var hash = sha.ComputeHash(bytes);
            hashString = Convert.ToBase64String(hash);

            return hashString;
        }

    }
}
