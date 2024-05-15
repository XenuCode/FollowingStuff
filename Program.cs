using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace FollowingStuff;

class Program
{
    static void Main(string[] args)
    {
        Config config = JsonConvert.DeserializeObject<Config>( new StreamReader("config.json").ReadToEnd())!;
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogFiles", "Log.txt"),
                rollingInterval: RollingInterval.Infinite,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
            .CreateLogger();
        Log.Information("SPINNING UP...");
        
        
        /*
        Response response = Order(config.ApiKey, "1960", "some", 1000);
        if(response.ResponseCode != HttpStatusCode.Accepted)
            Log.Error(response.ResponseCode.ToString());*/
    }


    public static Response Order(string apiKey, string service, string link, long amount)
    {
        try
        {
            string _apiUrl = "https://justanotherpanel.com/api/2";
            var request = new Dictionary<string, string>()
            {
                ["key"] = apiKey,
                ["action"] = "add",
                ["service"] = service,
                ["link"] = link,
                ["quantity"] = amount.ToString()
            };
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/4.0 (compatible; MSIE 5.01; Windows NT 5.0)");
                var content = new FormUrlEncodedContent(request);
                var response = httpClient.PostAsync(_apiUrl, content).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;

                return new Response(response.StatusCode, response.Content.ReadAsStringAsync().Result);
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            Log.Fatal("FATAL ERROR OCCURED, CANCELLING");
            throw;
        }
    }
}

class Response
{
    public Response( HttpStatusCode responseCode, string responseContent)
    {
        ResponseCode = responseCode;
        ResponseContent = responseContent;
    }

    public  HttpStatusCode ResponseCode { get; }
    public string ResponseContent { get;}
}

class Order
{
    [JsonProperty(PropertyName = "amount")]
    public long Amount { get; set; }
    [JsonProperty(PropertyName = "link")]
    public string Link { get; set; }
    [JsonProperty(PropertyName = "type")]
    public string Type { get; set; }
}
