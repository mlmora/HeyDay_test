using HeyDay.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace HeyDay.Controllers
{

    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("api/search")]
        [Produces("application/json")]
        public IActionResult Twitter_request(string query, int max_results = 20, string pagination_token = null, bool only_photos = false)
        {
            //ignore null values
            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            //if searched text is empyt
            if (String.IsNullOrEmpty(query))
                return BadRequest(JsonConvert.SerializeObject(new { message = "Bad request. Query is missing!" }));

            //max_results cannot be higher than 100
            if (max_results > 100)
                max_results = 100;


            if (only_photos==true)
                query += " has:images";

            var link = $@"https://api.twitter.com/2/tweets/search/recent?query={query} -is:retweet&tweet.fields=attachments,author_id,created_at&max_results={max_results}&expansions=attachments.media_keys&media.fields=preview_image_url,type,url";

            if (pagination_token != null)
                link += $"&pagination_token={pagination_token}";

            try
            {
                // get the reponse from twitter
                var twitter_data = GetRequestAsync(link).Result;
                JObject json = JObject.Parse(twitter_data);

                // if no data returned, send a simple response
                if (json["data"] == null)
                    return Ok(JsonConvert.SerializeObject(new { message = "No data found." }));

                // convert from string response to Tweet/Meta/Media objects
                var tweets = JsonConvert.DeserializeObject<List<Tweet>>(json["data"].ToString());
                var meta_info = JsonConvert.DeserializeObject<Meta>(json["meta"].ToString());

                var media = new List<Media>();
                if (json["includes"] != null)
                    media = JsonConvert.DeserializeObject<List<Media>>(json["includes"]["media"].ToString());

                // for each tweet, get the images details (url,type,preview_image_url)
                foreach (var tweet in tweets)
                {
                    if (tweet.attachments.media_keys != null)
                        tweet.images = GetImages(tweet.attachments, media);

                    //delete the attachements to not increase the json reponse
                    tweet.attachments = null;
                }

                //group tweets data and the next token data
                var response = new { tweets = tweets, next_token = meta_info.next_token };


                return Ok(JsonConvert.SerializeObject(response, Formatting.Indented, jsonSettings));
            }
            catch
            {
                return BadRequest(JsonConvert.SerializeObject(new { message = "Bad request." }));
            }

        }

        private List<Media> GetImages(Attachments attachments,List<Media> medias)
        {
            List<Media> all_media=new List<Media>();
            foreach (var media_key in attachments.media_keys)
            {
                var media = medias.Where(m => m.media_key == media_key).FirstOrDefault();
                if (media != null)
                    all_media.Add(media);
            }
            return all_media;

        }

        public async Task<string> GetRequestAsync(string uri)
        {
            var client = new HttpClient();
            //getting the token from appsettings.json
            var twitterToken = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("TwitterSettings")["TwitterToken"];

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", twitterToken);
            HttpResponseMessage response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }
    }
}
