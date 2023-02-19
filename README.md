# Jellylist
#### (formerly GetLink)
Web server that creates M3U files for TV shows on a Jellyfin instance.

## Setup
*These instructions only work for Windows 10/Server 2016 or later. Linux or macOS have not been tested yet, but builds are available. No Windows-specific features have been used, so it should be fine as long as you have .NET 7 installed. Windows 8.1/Server 2008 R2, macOS 10.15 Catalina or earlier is not supported (rip windows 7). Linux may vary, see https://learn.microsoft.com/en-us/dotnet/core/install/linux.*
1. Download the latest release.
2. Put the folder somewhere safe.
3. Open up a terminal in that area.
4. Type in `Jellylist.exe --urls http://localhost:[port] --jellyfinUrl [URL to Jellyfin instance]`
5. Test it out by getting a series ID from your web browser (ex: `[...]/web/index.html#!/details?id=[series id]&context=tvshows&serverId=[...]`), then open this in VLC: `http://localhost:[port]/GetList?seriesId=[series id]&username=[username]&password=[password]` OR `&authToken=[api key]` (if you put in both username, password, and auth token, the auth token takes priority)

If you want to mass download an entire season with a program like Free Download Manager, or your media player doesn't support EXTM3U, you can add `&returnType=txt` to the end, removing all metadata other than download links.

You don't have to be a manager of the instance for this to work, but your account needs to have download permissions, as your player is downloading the original file.

Note that unless you're using a dedicated API key for the `authToken` parameter, these files will stop working after a while! It's best not to save the m3u files, and rather to put the URL directly in your media player of choice.

## Supports
1 episode per file: ✅

More than 1 episode per file: ✅

Timestamps before episode is loaded: ✅

Music: ❌

## Screenshots
![Screenshot of VLC, playing the first episode of Star vs. the Forces of Evil](Image0.png)
![Screenshot of VLC, showing most of the SvtFoE episodes in a list](Image1.png)
