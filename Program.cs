using System.Security.Cryptography;
using System.Text.Json.Nodes;
using Steam = SteamGuard.TOTP;
using SteamAuth.Utils;
using System.Text;

const string STEAMGUARD_FILE = "steamguard.json";

if (!OperatingSystem.IsWindows())
{
    Console.Error.WriteLine("This program only works on Windows");
    return (int)Error.PlatformNotSupported;
}

var steamGuard = new Steam.SteamGuard();

if (args.Length > 0)
{
    return processArgs();
}

return processFile();


int processArgs()
{
    var secret = args[0];
    var shouldSave = args.Length > 1 && args[1].Contains("-s", StringComparison.InvariantCultureIgnoreCase);

    var code = steamGuard.GenerateAuthenticationCode(secret);
    Console.WriteLine(code ?? "Failed to generate code");

    if (code is null)
    {
        return (int)Error.GenerationFailure;
    }

    if (shouldSave)
    {
        var entropy = new byte[20];
        RandomNumberGenerator.Fill(entropy);

        var json = new JsonObject
        {
            ["encryptedSecret"] = secret.ToBytes()?.Protect(entropy)?.ToBase64(),
            ["entropy"] = entropy.ToBase64()
        };

        if (json["encryptedSecret"] is null || json["entropy"] is null)
        {
            Console.Error.WriteLine("Failed to encrypt secret");
            return (int)Error.EncryptionFailure;
        }

        File.WriteAllText(STEAMGUARD_FILE, json.ToString());
    }
    return 0;
}

int processFile()
{
    if (!File.Exists(STEAMGUARD_FILE))
    {
        Console.Error.WriteLine("SteamGuard file not found");
        return (int)Error.ConfigFileNotFound;
    }

    var json = JsonNode.Parse(File.ReadAllText(STEAMGUARD_FILE));
    if (json is null or not JsonObject)
    {
        Console.Error.WriteLine("SteamGuard file is not valid");
        return (int)Error.ConfigFileNotFound;
    }

    var entropy = json["entropy"]!.GetValue<string>();
    var encryptedSecret = json["encryptedSecret"]!.GetValue<string>();

    var secret = encryptedSecret.FromBase64()?.Unprotect(entropy.FromBase64()!)?.ToStringUtf();

    if (secret is null)
    {
        Console.Error.WriteLine("Failed to decrypt secret");
        return (int)Error.DecryptionFailure;
    }

    var code = steamGuard.GenerateAuthenticationCode(secret);

    Console.WriteLine(code ?? "Failed to generate code");
    return code is null ? (int)Error.GenerationFailure : 0;
}

namespace SteamAuth.Utils
{
    enum Error
    {
        PlatformNotSupported = -2,
        ConfigFileNotFound = -3,
        EncryptionFailure = -4,
        DecryptionFailure = -5,
        GenerationFailure = -6,
    }

    public static class Extensions
    {
        public static byte[]? Protect(this byte[] data, byte[] s_additionalEntropy)
        {
            try
            {
                // Encrypt the data using DataProtectionScope.CurrentUser. The result can be decrypted
                // only by the same current user.
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
}