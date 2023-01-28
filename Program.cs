using System.Text.Json.Nodes;
using SteamGuardTotp = SteamGuard.TOTP.SteamGuard;
using SteamAuth.Utils;
using System.Security.Cryptography;
using TextCopy;
using Ookii.CommandLine;
using Ookii.CommandLine.Terminal;

namespace SteamAuth;

class Program
{
    const string STEAMGUARD_FILE = "steamauth.json";
    private static readonly SteamGuardTotp steamGuard = new();
    private static readonly string steamGuardPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, STEAMGUARD_FILE);
    private static readonly ConsoleWriter console = new();

    private static int Main()
    {
        var args = CommandLineParser.Parse<Arguments>() ?? new(isInvalid: true);
        if (args.IsInvalid)
        {
            return (int)Status.InvalidArguments;
        }

        var result = string.IsNullOrWhiteSpace(args.Secret) ? ProcessFile() : ProcessArgs(args);

        if (result != Status.Success)
        {
            console.WriteLine(result.GetMessage(), TextFormat.ForegroundRed, TextFormat.BoldBright);
        }

        return (int)result;
    }

    private static Status ProcessArgs(Arguments args)
    {
        var secret = args.Secret ?? string.Empty;

        var code = steamGuard.GenerateAuthenticationCode(secret);
        if (code is null)
        {
            return Status.TotpGenerationFailure;
        }

        OutputTotpCode(code);

        if (args.Save)
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
            OutputTotpCode(code);
            return Status.Success;
        }

        return Status.TotpGenerationFailure;
    }

    private static void OutputTotpCode(string code)
    {
        console.WriteLine(code, TextFormat.ForegroundGreen, TextFormat.BoldBright);
        ClipboardService.SetText(code);
    }
}