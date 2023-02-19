using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

namespace Jellylist.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GetM3UController : ControllerBase
    {
        /// <summary>
        /// Creates an M3U file, and returns the text. 
        /// </summary>
        /// <param name="seriesId">The series ID.</param>
        /// <param name="authToken">An API key. Takes priority over username and password.</param>
        /// <param name="username">The username. Required if an API key isn't specified.</param>
        /// <param name="password">The password. Required if an API key isn't specified, and the user has a password.</param>
        /// <param name="returnType">The return type, either m3u for full metadata or txt for just the links, no other metadata.</param>
        /// <returns></returns>
        [Obsolete("Replaced with GetList. Endpoint is no longer being updated and may stop working.")]
        [HttpGet(Name = "GetM3U")]
        public string GetM3U(string seriesId, string? authToken = null, string? username = null,
            string? password = null, string returnType = "m3u")
        {
            return new GetListController().GetList(seriesId, authToken, username, password, returnType);
        }
    }
    
    
    [ApiController]
    [Route("[controller]")]
    public class GetListController : ControllerBase
    {
        /// <summary>
        /// Creates an M3U file, and returns the text. 
        /// </summary>
        /// <param name="seriesId">The series ID.</param>
        /// <param name="authToken">An API key. Takes priority over username and password.</param>
        /// <param name="username">The username. Required if an API key isn't specified.</param>
        /// <param name="password">The password. Required if an API key isn't specified, and the user has a password.</param>
        /// <param name="returnType">The return type, either m3u for full metadata or txt for just the links, no other metadata.</param>
        /// <returns></returns>
        [HttpGet(Name = "GetList")]
        public string GetList(string seriesId, string? authToken = null, string? username = null, string? password = null, string returnType = "m3u")
        {

            if (authToken == null) // no auth token
            {
                if (username == null || password == null) // no user+pass
                {
                    return "No login details specified.";
                }
                var url2 = Program.PublicUrl + "/Users/AuthenticateByName";

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
                var result = streamReader.ReadToEnd();
                var test = JsonConvert.DeserializeObject<dynamic>(result);
                authToken = test!.AccessToken;
            }

            var url = Program.PublicUrl + $"/Shows/{seriesId}/Episodes?api_key={authToken}";

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);

            httpRequest.Accept = "application/json";
            if (returnType == "m3u")
            {
                string returnstring =
                """
                #EXTM3U
                """;

                try
                {
                    var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                    using var streamReader = new StreamReader(httpResponse.GetResponseStream());
                    var result = streamReader.ReadToEnd();
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
                    if (e.Message.Contains("404"))
                    {
                        return "Not a TV show.";
                    }
                }
            }
            else if (returnType == "txt") // only links, no other metadata
            {
                string returnstring = "";

                try
                {
                    var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                    using var streamReader = new StreamReader(httpResponse.GetResponseStream());
                    var result = streamReader.ReadToEnd();
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
                    if (e.Message.Contains("404"))
                    {
                        return "Not a TV show.";
                    }
                }
            }
            else
            {
                return "Invalid type.";
            }

            return "An unknown error occurred.";
        }
    }
}