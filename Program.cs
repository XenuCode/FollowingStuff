using System.Net;
using Newtonsoft.Json;
using Serilog;

namespace FollowingStuff;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("Starting...");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogFiles", "Log.txt"),
                rollingInterval: RollingInterval.Infinite,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
            .CreateLogger();
        Log.Information("Loading config");

        var config = JsonConvert.DeserializeObject<Config>(new StreamReader("config.json").ReadToEnd());
        var orders = JsonConvert.DeserializeObject<Orders>(new StreamReader("orders.json").ReadToEnd());
        if (config is null) Log.Error("Error loading config file");
        if (config is null) Log.Error("Error loading orders file");

        var x = 0;
        foreach (var order in orders.OrdersList)
        {
            foreach (var link in order.Links)
            {
                var response = OrderFun(config.ApiKey, order.Service, link, order.Min,order.Max);
                if (response.ResponseCode is not HttpStatusCode.OK)
                {
                    Log.Error("Fail for: {Link}", link);
                    Log.Debug("Error message: {Error}", response.ResponseContent);
                }
                else
                {
                    Log.Information("Success for: {service},{Link}", order.Service, link);
                    x++;
                }
            }
            
        }

        int y = 0;
        foreach (Order order in orders.OrdersList)
        {
            y += order.Links.Count;
        }
        Console.WriteLine(
            $"---- {x}/{y} completed successfully ----\nRead the Log file for more information");
        Log.Information("---- {success}/{fail} completed successfully ----", x, y);
        Log.Information("Finished, shutting down");
        Console.WriteLine("Press eny key to close");
        Console.ReadKey();
    }


    private static Response OrderFun(string apiKey, int service, string link, int min,int max)
    {
        try
        {
            var apiUrl = "https://justanotherpanel.com/api/v2";
            var request = new Dictionary<string, string>
            {
                ["key"] = apiKey,
                ["action"] = "add",
                ["service"] = service.ToString(),
                ["link"] = link,
                ["quantity"] = Random.Shared.Next(min,max).ToString()
            };
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/4.0 (compatible; MSIE 5.01; Windows NT 5.0)");
                var content = new FormUrlEncodedContent(request);
                var response = httpClient.PostAsync(apiUrl, content).Result;
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

internal class Response
{
    public Response(HttpStatusCode responseCode, string responseContent)
    {
        ResponseCode = responseCode;
        ResponseContent = responseContent;
    }

    public HttpStatusCode ResponseCode { get; }
    public string ResponseContent { get; }
}

internal class Order
{
    public Order(int min,int max, List<string> links, int service)
    {
        Min = min;
        Max = max;
        Links = links;
        Service = service;
    }

    [JsonProperty(PropertyName = "min")]
    public int Min { get; set; }
    [JsonProperty(PropertyName = "max")]
    public int Max { get; set; }

    [JsonProperty(PropertyName = "links")]
    public List<string> Links { get; set; }

    [JsonProperty(PropertyName = "service")]
    public int Service { get; set; }
}

internal class Orders
{
    [JsonProperty(PropertyName = "orders")]
    public List<Order> OrdersList = new();
}