using System.ComponentModel;
using Ookii.CommandLine;

namespace SteamAuth
{
    [ParseOptions(
        Mode = ParsingMode.LongShort,
        CaseSensitive = false,
        ArgumentNameTransform = NameTransform.DashCase,
        ValueDescriptionTransform = NameTransform.DashCase
    )]
    [Description("Generates a steam TOTP code using a provided secret.")]
    public class Arguments
    {

        [CommandLineArgument(Position = 0, IsRequired = false)]
        [Description("Steam TOTP secret")]
        public string? Secret { get; set; }

        [CommandLineArgument(IsRequired = false, ShortName = 's')]
        [Description("Whether to save the encrypted secret to a config file")]
        public bool Save { get; set; }

        internal bool IsInvalid { get; private set; }

        public Arguments()
        {
            Secret = null;
            Save = false;
        }

        internal Arguments(bool isInvalid) : this()
        {
            IsInvalid = isInvalid;
        }
    }
}