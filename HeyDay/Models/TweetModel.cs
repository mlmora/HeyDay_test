using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace HeyDay.Models
{
    public class Attachments
    {
        public List<string> media_keys { get; set; }
    }

    public class Tweet
    {
        public string author_id { get; set; }
        public string text { get; set; }
        public string id { get; set; }
        public DateTime created_at { get; set; }

        public Attachments attachments { get; set; }
        public List<Media> images { get; set; }

        public Tweet()
        {
            this.attachments = new Attachments();
        }
    }
    public class Media
    {
        public string media_key { get; set; }
        public string type { get; set; }
        public string url { get; set; }
        public string preview_image_url { get; set; }
    }


    public class Meta
    {
        public string newest_id { get; set; }
        public string oldest_id { get; set; }
        public string result_count { get; set; }
        public string next_token { get; set; }
    }


}
