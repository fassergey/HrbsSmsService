using Newtonsoft.Json;

namespace SmsService.Models
{
    public class SmsRequest
    {
        public App app { get; set; }
        public Messages[] messages { get; set; }
    }

    public class App
    {
        public string user_id { get; set; }
        public string application_id { get; set; }
    }

    public class Messages
    {
        public string platform_id { get; set; }
        public string pattern_id { get; set; }

        [JsonProperty("params")]
        public Params[] param { get; set; }

        public string[] devices { get; set; }
    }

    public class Params
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
