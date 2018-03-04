namespace Modix.Services.Cat.Primary
{
    public class Rootobject
    {
        public Datum[] data { get; set; }
        public bool success { get; set; }
        public int status { get; set; }
    }

    public class Datum
    {
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int datetime { get; set; }
        public string cover { get; set; }
        public int cover_width { get; set; }
        public int cover_height { get; set; }
        public object account_url { get; set; }
        public object account_id { get; set; }
        public object privacy { get; set; }
        public object layout { get; set; }
        public int views { get; set; }
        public string link { get; set; }
        public object ups { get; set; }
        public object downs { get; set; }
        public object points { get; set; }
        public int score { get; set; }
        public bool is_album { get; set; }
        public object vote { get; set; }
        public bool? favorite { get; set; }
        public bool nsfw { get; set; }
        public string section { get; set; }
        public object comment_count { get; set; }
        public object favorite_count { get; set; }
        public object topic { get; set; }
        public object topic_id { get; set; }
        public int images_count { get; set; }
        public bool in_gallery { get; set; }
        public bool is_ad { get; set; }
        public object[] tags { get; set; }
        public int ad_type { get; set; }
        public string ad_url { get; set; }
        public bool in_most_viral { get; set; }
        public Image[] images { get; set; }
        public string type { get; set; }
        public bool animated { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int size { get; set; }
        public long bandwidth { get; set; }
        public bool has_sound { get; set; }
        public string mp4 { get; set; }
        public string gifv { get; set; }
        public int mp4_size { get; set; }
        public bool looping { get; set; }
    }

    public class Image
    {
        public string id { get; set; }
        public object title { get; set; }
        public string description { get; set; }
        public int datetime { get; set; }
        public string type { get; set; }
        public bool animated { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int size { get; set; }
        public int views { get; set; }
        public long bandwidth { get; set; }
        public object vote { get; set; }
        public bool favorite { get; set; }
        public object nsfw { get; set; }
        public object section { get; set; }
        public object account_url { get; set; }
        public object account_id { get; set; }
        public bool is_ad { get; set; }
        public bool in_most_viral { get; set; }
        public bool has_sound { get; set; }
        public object[] tags { get; set; }
        public int ad_type { get; set; }
        public string ad_url { get; set; }
        public bool in_gallery { get; set; }
        public string link { get; set; }
        public object comment_count { get; set; }
        public object favorite_count { get; set; }
        public object ups { get; set; }
        public object downs { get; set; }
        public object points { get; set; }
        public object score { get; set; }
        public string mp4 { get; set; }
        public string gifv { get; set; }
        public int mp4_size { get; set; }
        public bool looping { get; set; }
    }
}