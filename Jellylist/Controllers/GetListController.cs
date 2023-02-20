using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

namespace Jellylist.Controllers
{
    public abstract class Shared
    {
        public static async Task<string> MakePostRequest(string url, string data, string embyAuth)
        {
            using var httpClient = new HttpClient();
            var content = new StringContent(data);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:107.0) Gecko/20100101 Firefox/107.0");

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("X-Emby-Authorization", embyAuth);
            request.Content = content;

            var response = await httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }
        
        public static async Task<string> MakeGetRequest(string url, string embyAuth)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("ContentType", "application/json");
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:107.0) Gecko/20100101 Firefox/107.0");
            httpClient.DefaultRequestHeaders.Add("X-Emby-Authorization", embyAuth);

            var response = await httpClient.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class GetAlbumController : ControllerBase
    {
        /// <summary>
        /// Creates an M3U/txt file for an album, and returns the text.
        /// </summary>
        /// <param name="albumId"></param>
        /// <param name="authToken"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        [HttpGet(Name = "GetAlbum")]
        public async Task<string> GetAlbum(string albumId, string? authToken = null, string? username = null,
            string? password = null, string returnType = "m3u")
        {
            string userId;
            
            if (authToken == null) // no auth token
            {
                if (username == null|| password == null) // no user+pass
                {
                    return "No login details specified.";
                }

                /*var url2 = Program.PublicUrl + "/Users/AuthenticateByName";
                var httpRequest2 = (HttpWebRequest)WebRequest.Create(url2);
                httpRequest2.Method = "POST";
                
                httpRequest2.Accept = "application/json";
                httpRequest2.Headers["X-Emby-Authorization"] = "MediaBrowser Client=\"Jellyfin Web\", Device=\"Firefox\", DeviceId=\"TW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NDsgcnY6MTA2LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvMTA2LjB8MTY2Nzk1OTA0NjIxNg11\", Version=\"10.8.5\"";
                httpRequest2.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:107.0) Gecko/20100101 Firefox/107.0";
                httpRequest2.ContentType = "application/json";

                using (var streamWriter = new StreamWriter(httpRequest2.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }
                
                var httpResponse2 = (HttpWebResponse)httpRequest2.GetResponse();
                using var streamReader = new StreamReader(httpResponse2.GetResponseStream());
                var result = streamReader.ReadToEnd();*/
                
                var data = @"{
                                      ""Username"": ""[username]"",
                                      ""Pw"": ""[pass]""
                             }".Replace("[username]", username).Replace("[pass]", password);
                var result = await Shared.MakePostRequest(Program.PublicUrl + "/Users/AuthenticateByName", data, 
                    "MediaBrowser Client=\"Jellyfin Web\", Device=\"Firefox\", DeviceId=\"TW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NDsgcnY6MTA2LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvMTA2LjB8MTY2Nzk1OTA0NjIxNg11\", Version=\"10.8.5\"");
                var test = JsonConvert.DeserializeObject<dynamic>(result);
                authToken = test!.AccessToken;
                userId = test.User.Id;
            }
            else
            {
                var result = await Shared.MakeGetRequest(Program.PublicUrl + "/Users/Me",
                    $"MediaBrowser Client=\"Jellyfin Web\", Device=\"Firefox\", DeviceId=\"TW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NDsgcnY6MTA2LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvMTA2LjB8MTY2Nzk1OTA0NjIxNg11\", Version=\"10.8.5\", Token=\"{authToken}\"");
                var test = JsonConvert.DeserializeObject<dynamic>(result);

                userId = test!.Id;
            }

            var url = Program.PublicUrl + $"/Users/{userId}/Items?ParentId={albumId}&Fields=ItemCounts%2CPrimaryImageAspectRatio%2CBasicSyncInfo%2CCanDelete%2CMediaSourceCount&SortBy=ParentIndexNumber%2CIndexNumber%2CSortName";
            
            switch (returnType)
            {
                case "m3u":
                {
                    string returnstring =
                        """
                        #EXTM3U
                        """;
                
                    try
                    {
                        var result = await Shared.MakeGetRequest(url,
                            $"MediaBrowser Client=\"Jellyfin Web\", Device=\"Firefox\", DeviceId=\"TW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NDsgcnY6MTA2LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvMTA2LjB8MTY2Nzk1OTA0NjIxNg11\", Version=\"10.8.5\", Token=\"{authToken}\"");
                        var test = JsonConvert.DeserializeObject<dynamic>(result);


                        returnstring += $"\n#PLAYLIST:{test!.Items[0].Album}";
                        returnstring += $"\n#EXTALB:{test.Items[0].Album}";
                        returnstring += $"\n#EXTART:{test.Items[0].AlbumArtist}";

                        foreach (var test2 in test.Items)
                        {
                            returnstring += $"\n#EXTINF:{test2.RunTimeTicks / 10000 / 1000 /* seconds */},{test2.Album}, " +
                                            $"Disc {Convert.ToInt32(test2.ParentIndexNumber).ToString().PadLeft(2, '0')}, Track {Convert.ToInt32(test2.IndexNumber).ToString().PadLeft(2, '0')}: " +
                                            $"{test2.Name}";
                            returnstring += "\n" + Program.PublicUrl + $"/Items/{test2.Id}/Download?api_key={authToken}";
                        }

                        return returnstring;
                    }
                    catch (WebException e)
                    {
                        return e.Message.Contains("404") ? "Not an album." : e.Message;
                    }
                }
                case "txt":
                {
                    string returnstring = "";
                
                    try
                    {
                        var result = await Shared.MakeGetRequest(url,
                            $"MediaBrowser Client=\"Jellyfin Web\", Device=\"Firefox\", DeviceId=\"TW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NDsgcnY6MTA2LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvMTA2LjB8MTY2Nzk1OTA0NjIxNg11\", Version=\"10.8.5\", Token=\"{authToken}\"");
                        var test = JsonConvert.DeserializeObject<dynamic>(result);

                        foreach (var test2 in test!.Items)
                        {
                            returnstring += "\n" + Program.PublicUrl + $"/Items/{test2.Id}/Download?api_key={authToken}";
                        }

                        return returnstring;
                    }
                    catch (WebException e)
                    {
                        return e.Message.Contains("404") ? "Not an album." : e.Message;
                    }
                }
                default:
                    return "Invalid type.";
            }
        }
    }

    [ApiController]
    [Route("[controller]")]
    // ReSharper disable once InconsistentNaming
    public class GetTVController : ControllerBase
    {
        /// <summary>
        /// Creates an M3U/txt file for a TV show, and returns the text. 
        /// </summary>
        /// <param name="seriesId">The series ID.</param>
        /// <param name="authToken">An API key. Takes priority over username and password.</param>
        /// <param name="username">The username. Required if an API key isn't specified.</param>
        /// <param name="password">The password. Required if an API key isn't specified, and the user has a password.</param>
        /// <param name="returnType">The return type, either m3u for full metadata or txt for just the links, no other metadata.</param>
        /// <returns></returns>
        [HttpGet(Name = "GetTV")]
        // ReSharper disable once InconsistentNaming
        public async Task<string> GetTV(string seriesId, string? authToken = null, string? username = null, string? password = null, string returnType = "m3u")
        {

            if (authToken == null) // no auth token
            {
                if (username == null || password == null) // no user+pass
                {
                    return "No login details specified.";
                }
                /*var url2 = Program.PublicUrl + "/Users/AuthenticateByName";

                var httpRequest2 = (HttpWebRequest)WebRequest.Create(url2);
                httpRequest2.Method = "POST";

                httpRequest2.Accept = "application/json";
                httpRequest2.Headers["X-Emby-Authorization"] = "MediaBrowser Client=\"Jellyfin Web\", Device=\"Firefox\", DeviceId=\"TW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NDsgcnY6MTA2LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvMTA2LjB8MTY2Nzk1OTA0NjIxNg11\", Version=\"10.8.5\"";
                httpRequest2.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:107.0) Gecko/20100101 Firefox/107.0";
                httpRequest2.ContentType = "application/json";

                var data = @"{
                                      ""Username"": ""[username]"",
                                      ""Pw"": ""[pass]""
                             }".Replace("[username]", username).Replace("[pass]", password);

                using (var streamWriter = new StreamWriter(httpRequest2.GetRequestStream()))
                {
                    streamWriter.Write(data);
                }

                var httpResponse2 = (HttpWebResponse)httpRequest2.GetResponse();
                using var streamReader = new StreamReader(httpResponse2.GetResponseStream());
                var result = streamReader.ReadToEnd();*/
                var data = @"{
                                      ""Username"": ""[username]"",
                                      ""Pw"": ""[pass]""
                             }".Replace("[username]", username).Replace("[pass]", password);
                var result = await Shared.MakePostRequest(Program.PublicUrl + "/Users/AuthenticateByName", data, 
                    "MediaBrowser Client=\"Jellyfin Web\", Device=\"Firefox\", DeviceId=\"TW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NDsgcnY6MTA2LjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvMTA2LjB8MTY2Nzk1OTA0NjIxNg11\", Version=\"10.8.5\"");
                var test = JsonConvert.DeserializeObject<dynamic>(result);
                authToken = test!.AccessToken;
            }

            var url = Program.PublicUrl + $"/Shows/{seriesId}/Episodes?api_key={authToken}";
            
            switch (returnType)
            {
                case "m3u":
                {
                    string returnstring =
                        """
                        #EXTM3U
                        """;

                    try
                    {
                        var result = await Shared.MakeGetRequest(url, "");
                        var test = JsonConvert.DeserializeObject<dynamic>(result);


                        returnstring += $"\n#PLAYLIST:{test!.Items[0].SeriesName}";

                        foreach (var test2 in test.Items)
                        {
                            if (test2.IndexNumberEnd != null) // Is it a multi-episode file?
                            {
                                returnstring += $"\n#EXTINF:{test2.RunTimeTicks / 10000 / 1000 /* seconds */},{test2.SeriesName}, " +
                                                $"S{Convert.ToInt32(test2.ParentIndexNumber).ToString().PadLeft(2, '0') /* season # */}E{Convert.ToInt32(test2.IndexNumber).ToString().PadLeft(2, '0') /* first episode # */}" +
                                                $"-{Convert.ToInt32(test2.IndexNumberEnd).ToString().PadLeft(2, '0') /* last episode # */}: {test2.Name}";
                            }
                            else
                            {
                                returnstring += $"\n#EXTINF:{test2.RunTimeTicks / 10000 / 1000 /* seconds */},{test2.SeriesName}, " +
                                                $"S{Convert.ToInt32(test2.ParentIndexNumber).ToString().PadLeft(2, '0') /* season # */}E{Convert.ToInt32(test2.IndexNumber).ToString().PadLeft(2, '0') /* episode # */}: " +
                                                $"{test2.Name}";
                            }
                            returnstring += "\n" + Program.PublicUrl + $"/Items/{test2.Id}/Download?api_key={authToken}";
                        }

                        return returnstring;
                    }
                    catch (WebException e)
                    {
                        return e.Message.Contains("404") ? "Not a TV show." : e.Message;
                    }
                }
                // only links, no other metadata
                case "txt":
                {
                    string returnstring = "";

                    try
                    {
                        var result = await Shared.MakeGetRequest(url, "");
                        var test = JsonConvert.DeserializeObject<dynamic>(result);

                        if (test == null) return returnstring;
                        foreach (var test2 in test.Items)
                        {
                            returnstring += Program.PublicUrl + $"/Items/{test2.Id}/Download?api_key={authToken}\n";
                        }

                        return returnstring;
                    }
                    catch (WebException e)
                    {
                        return e.Message.Contains("404") ? "Not an album." : e.Message;
                    }
                    
                }
                default:
                    return "Invalid type.";
            }
        }
    }
}