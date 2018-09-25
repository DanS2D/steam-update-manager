# Steam Update Manager

A simple command line utility to handle changing the automatic update settings for your Steam games in one fell swoop.

Have a feature request or want to contribute? Open a GitHub issue and I'll see what I can do.
Thank you.

#### Args

`--path`: The path to your Steam `steamapps` folder. This is the folder that contains your Steam `appmanifest_xxx.acf` files. Default (if omitted) is `C:\Program Files (x86)\Steam\steamapps`.
`--disable`: Disables automatic updates for all games found at `--path`. Games will then use the "only update this game when launched" setting in Steam. If you wish to enable automatic updates for all games found at `--path`, simply omit this argument when running the program.
`--dry`: Executes a dry run. This means that you'll see a preview of all the changes the program will make, without it actually doing it. This is a good argument to use if you wish to see what the program will change before using it live. If you want to actually save the update settings, simply omit this argument when running the program.

#### Usage

Below are some usage examples.

NOTE: When running this program in `Live` mode (not using the `dry` argument), the program will close Steam and re-open it when it has finished writing to files. This process is extremely fast.

###### Testing a dry run

Using the default Steam installation path.

`SteamUpdateManager.exe --disable --dry` -- Test what games would get auto updates disabled by running the command in "dry" mode.

###### Enabling auto updates

`SteamUpdateManager.exe` -- Running the command without the `--disable` argument enables auto-updates for all games.

###### Using a custom path

`SteamUpdateManager.exe --path "E:\Games\steamapps" --disable` -- disables auto update for all steam games found at `E:\Games\steamapps`.

#### Building for release

To build for release, execute the following command: `dotnet publish --self-contained -c Release -r win10-x64`. You should substitute `win10-x64` for `ubuntu.16.10-x64` if building for Ubuntu.