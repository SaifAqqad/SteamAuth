# SteamAuth

### Usage

```powershell
$ SteamAuth [[--secret] <string>] [--help] [--save] [--version]

        --secret <string>
            Steam TOTP secret

    -?, --help [<boolean>] (-h)
            Displays this help message.

    -s, --save [<boolean>]
            Whether to save the encrypted secret to a config file

        --version [<boolean>]
            Displays version information.
```

###### The code will be copied to your clipboard automatically.

Examples:

` SteamAuth 1234ABC789DE789FG560 -s `

` SteamAuth --save --secret 1234ABC789DE789FG560 `

` SteamAuth 1234ABC789DE789FG560 `

If you've already saved the secret, you can just run `SteamAuth` without any arguments.
