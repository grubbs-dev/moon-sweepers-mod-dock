# Code signing status

This project is MIT licensed. Current builds are unsigned. No SignPath approval, signing certificate or signed release is claimed.

The public GitHub Actions workflow builds the standalone launcher from repository source. No game binaries, mods or third-party patcher are bundled. The .NET Windows desktop runtime is included by dotnet publish.

Free-signing application: https://signpath.org/ (Apply).

Project: https://github.com/grubbs-dev/moon-sweepers-mod-dock

Description for application:

Moon Sweepers Mod Dock is an MIT-licensed Windows desktop launcher and local BepInEx DLL manager for Moon Sweepers. It lets users select their game directory, import standalone mod DLLs, enable/disable them by renaming files, open logs and launch the game. It does not bundle game assets, mod binaries or the community patcher. Public GitHub Actions builds provide traceable release artifacts. The initial unsigned executable is blocked by Windows Smart App Control on the development PC. We request signing for the launcher and its own built components.

After approval, configure the signing provider's GitHub integration and protect signing credentials using repository secrets and restricted release permissions. Do not upload private signing keys to the repository.
