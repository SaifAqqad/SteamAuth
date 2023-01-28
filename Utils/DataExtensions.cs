using System.Security.Cryptography;
using System.Text;
using ProtectedData = CrossPlatformProtectedData.ProtectedData;

namespace SteamAuth.Utils;

static class DataExtensions
{
    public static byte[]? Protect(this byte[] data, byte[] s_additionalEntropy)
    {
        try
        {
            // Encrypt the data using DataProtectionScope.CurrentUser.
            // The result can be decrypted only by the same current user.
            return ProtectedData.Protect(data, s_additionalEntropy, DataProtectionScope.CurrentUser);
        }
        catch (CryptographicException e)
        {
            Console.WriteLine("Data was not encrypted. An error occurred.");
            Console.WriteLine(e.ToString());
            return null;
        }
    }

    public static byte[]? Unprotect(this byte[] data, byte[] s_additionalEntropy)
    {
        try
        {
            //Decrypt the data using DataProtectionScope.CurrentUser.
            return ProtectedData.Unprotect(data, s_additionalEntropy, DataProtectionScope.CurrentUser);
        }
        catch (CryptographicException e)
        {
            Console.WriteLine("Data was not decrypted. An error occurred.");
            Console.WriteLine(e.ToString());
            return null;
        }
    }

    public static byte[]? ToBytes(this string str)
    {
        return Encoding.UTF8.GetBytes(str);
    }

    public static string? ToStringUtf(this byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

    public static string? ToBase64(this byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }

    public static byte[]? FromBase64(this string str)
    {
        return Convert.FromBase64String(str);
    }

}