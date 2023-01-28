using System.Text.Json.Nodes;
using SteamGuardTotp = SteamGuard.TOTP.SteamGuard;
using SteamAuth.Utils;
using System.Security.Cryptography;

namespace SteamAuth;

class Program
{
    const string STEAMGUARD_FILE = "steamauth.json";
    private static readonly SteamGuardTotp steamGuard = new();
    private static readonly string steamGuardPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, STEAMGUARD_FILE);

    private static int Main(string[] args)
    {
        var result = args.Length == 0 ? ProcessFile() : ProcessArgs(args);

        if (result != Status.Success)
        {
            Console.Error.WriteLine(result.GetMessage());
        }

        return (int)result;
    }

    private static Status ProcessArgs(string[] args)
    {
        var secret = args[0];
        var shouldSave = args.Length > 1 && args[1].Contains("-s", StringComparison.InvariantCultureIgnoreCase);

        var code = steamGuard.GenerateAuthenticationCode(secret);
        if (code is null)
        {
            return Status.TotpGenerationFailure;
        }

        Console.WriteLine(code);

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
                return Status.EncryptionFailure;
            }

            File.WriteAllText(steamGuardPath, json.ToString());
        }

        return Status.Success;
    }

    private static Status ProcessFile()
    {
        if (!File.Exists(steamGuardPath))
        {
            return Status.ConfigFileNotFound;
        }

        var json = JsonNode.Parse(File.ReadAllText(steamGuardPath));
        if (json is null or not JsonObject)
        {
            return Status.ConfigFileNotFound;
        }

        var entropy = json["entropy"]!.GetValue<string>();
        var encryptedSecret = json["encryptedSecret"]!.GetValue<string>();

        var secret = encryptedSecret.FromBase64()?.Unprotect(entropy.FromBase64()!)?.ToStringUtf();

        if (secret is null)
        {
            return Status.DecryptionFailure;
        }

        var code = steamGuard.GenerateAuthenticationCode(secret);
        if (code is not null)
        {
            Console.WriteLine(code);
            return Status.Success;
        }

        return Status.TotpGenerationFailure;
    }
}