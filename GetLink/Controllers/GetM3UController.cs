using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.Extensions.WebEncoders.Testing;
using Newtonsoft.Json;

namespace GetLink.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class GetM3UController : ControllerBase
    {
        private readonly ILogger<GetM3UController> _logger;

        public GetM3UController(ILogger<GetM3UController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Creates an M3U file, and returns the text. 
        /// </summary>
        /// <param name="seriesId">The series ID.</param>
        /// <param name="authToken">An API key. Takes priority over username and password.</param>
        /// <param name="username">The username. Required if an API key isn't specified.</param>
        /// <param name="password">The password. Required if an API key isn't specified, and the user has a password.</param>
        /// <returns></returns>
        [HttpGet(Name = "GetM3U")]
        public string GetM3U(string seriesId, string? authToken = null, string? username = null, string? password = null)
        {

            if (authToken == null) // no auth token
            {
                if (username == null || password == null) // no user+pass
                {
                    return "No login details specified.";
                }
                var url2 = Program.publicUrl + "/Users/AuthenticateByName";

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
                using (var streamReader = new StreamReader(httpResponse2.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    var test = JsonConvert.DeserializeObject<dynamic>(result);
                    authToken = test.AccessToken;
                }
            }

            var url = Program.publicUrl + $"/Shows/{seriesId}/Episodes?api_key={authToken}";

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);

            httpRequest.Accept = "application/json";

            string returnstring = 
                """
                #EXTM3U
                """;

            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    var test = JsonConvert.DeserializeObject<dynamic>(result);


                    returnstring += $"\n#PLAYLIST:{test.Items[0].SeriesName}";

                    foreach (var test2 in test.Items)
                    {
                        returnstring += $"\n#EXTINF:0,{test2.SeriesName}, S{Convert.ToInt32(test2.ParentIndexNumber).ToString().PadLeft(2, '0')}E{Convert.ToInt32(test2.IndexNumber).ToString().PadLeft(2, '0')}: {test2.Name}";
                        returnstring += "\n" + Program.publicUrl + $"/Items/{test2.Id}/Download?api_key={authToken}";
                    }
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

            return "An unknown error occurred.";
        }
    }
}