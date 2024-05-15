using Newtonsoft.Json;

namespace FollowingStuff;

public class Config
{
    [JsonProperty(PropertyName = "api_key")]
    public string ApiKey { get; set; }
}