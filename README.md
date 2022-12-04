# GetLink
Web server that creates M3U files for Jellyfin TV shows.

## Setup
*These instructions only work for Windows. I haven't tested Linux or macOS yet, but builds are available. I haven't used any Windows-specific features, so it should be fine as long as you have .NET 7 installed.*
1. Download GetLink.exe from the latest release.
2. Put it somewhere safe.
3. Open up a terminal in that area.
4. Type in `GetLink.exe --urls http://localhost:[port] --publicUrl [URL to Jellyfin instance]`
5. Test it out by getting a series ID from your web browser (ex: [...]/web/index.html#!/details?id=SERIESIDHERE00000000000000000000&context=tvshows&serverId=[...]), then open this in VLC: `http://localhost:[port]/GetM3U?seriesId=[series id]&username=[username]&password=[password]` OR `&authToken=[api key]` (if you put in both username, password, and auth token, the auth token takes priority)

## Supports
1 episode per listing: ✅

More than 1 episode per listing: ✅

Timestamps before episode is loaded: ❌

## Screenshots
![Screenshot of VLC, playing the first episode of Breaking Bad](Image0.png)
![Screenshot of VLC, showing most of the Breaking Bad episodes in a list](Image1.png)
