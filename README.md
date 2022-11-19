# GetLink
Web server that creates M3U files for Jellyfin TV shows.

## Setup
*This only works for Windows. I haven't tested Linux or macOS yet, but builds are available.*
1. Download GetLink.exe from the latest release.
2. Put it somewhere safe.
3. Open up a terminal in that area.
4. Type in `GetLink.exe --urls http://localhost:[port] --publicUrl [URL to Jellyfin instance]`
5. Test it out by getting a series ID from your web browser (ex: [...]/web/index.html#!/details?id=8b578923c105ee3a6039330ceb4cbe94&context=tvshows&serverId=[...]), then open this in VLC: `http://localhost:[port]/GetM3U?seriesId=[series id]&username=[username]&password=[password]` OR `&authToken=[api key]` (if you put in both username, password, and auth token, the auth token takes priority)

## Screenshots
![Screenshot of VLC, playing the first episode of Breaking Bad](Image0.png)
![Screenshot of VLC, showing most of the Breaking Bad episodes in a list](Image1.png)
