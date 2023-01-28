namespace SteamAuth.Utils;
enum Status
{
    Success = 0,
    PlatformNotSupported = -2,
    ConfigFileNotFound = -3,
    EncryptionFailure = -4,
    DecryptionFailure = -5,
    TotpGenerationFailure = -6
}

static class StatusExtensions
{
    public static string GetMessage(this Status status)
    {
        return status switch
        {
            Status.Success => "",
            Status.TotpGenerationFailure => "Failed to generate code",
            Status.EncryptionFailure => "Failed to encrypt secret",
            Status.DecryptionFailure => "Failed to decrypt secret",
            Status.ConfigFileNotFound => "SteamGuard file not found or invalid",
            _ => "Unknown error"
        };
    }
}