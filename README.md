# Moon Sweepers Mod Dock

A Windows x64 desktop launcher and DLL manager for Moon Sweepers with a starfield theme.

Source license: MIT. Current releases are unsigned and may be blocked by Windows Smart App Control. See SIGNING.md for the signing preparation status. Source: https://github.com/grubbs-dev/moon-sweepers-mod-dock

## Use

Extract the portable ZIP into its own folder and open MoonSweepers.ModDock.exe. Select the folder containing MoonSweepers.exe. The app detects BepInEx and the required MoonSweepers ModEngine patcher; download buttons open their project pages. Those dependencies must be installed separately.

Add DLL imports standalone DLL mods into BepInEx/plugins/<DLL name>. For mods with assets, ZIP packages or other dependencies, follow the mod's full installation instructions manually, then Refresh. Existing DLL names are not overwritten.

Disable appends .disabled to a DLL; Enable restores its .dll extension. Shared dependency DLLs appear in the list too, so consult mod instructions before disabling them. Changes require the game to be closed. Open plugins and View log help with manual installs and troubleshooting.

Launch starts the selected game directory. It creates steam_appid.txt with app ID 5335510 if missing, to support launching a copied install. Steam must be running with ownership of the game.

Settings are stored under %LOCALAPPDATA%/MoonSweepersModDock/settings.json. The portable distribution includes the .NET runtime. BepInEx, patcher, game files and mod DLLs are not bundled.

## Build

dotnet publish MoonSweepers.ModDock.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ../dist/ModDock

## Verification

Release build and portable publishing completed successfully. On the development PC, Windows Application Control blocked the new executable before launch; interactive layout and behavior testing could not be completed. No security policy changes were made.
