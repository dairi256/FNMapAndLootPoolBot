using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class FortniteApiService
{
    private static readonly HttpClient _httpClient = new();
    private const string MapEndpoint = "https://fortnite-api.com/v1/map";

    public async Task<string> GetMapImageUrlAsync()
    {
        try
        {
            var jsonString = await _httpClient.GetStringAsync(MapEndpoint);

            var mapResponse = JsonSerializer.Deserialize<FortniteMapResponse>(
                jsonString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (mapResponse?.Data?.Images?.Pois != null)
            {
                string originalUrl = mapResponse.Data.Images.Pois;

                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // Generates a timestamp to fix the map issue.

                string cacheBusterUrl = $"{originalUrl}?t={timestamp}";

                return cacheBusterUrl;
            }

            if (mapResponse?.Data?.Images?.Pois != null)
            {
                return mapResponse.Data.Images.Pois;
            }

            return "Error: Map URL not found in API response structure.";
        }
        catch (HttpRequestException ex)
        {
            return $"Error fetching data from API: {ex.Message}";
        }
    }

    public class MapImages
    {
        public string Pois { get; set; }
    }

    public class MapData
    {
        public MapImages Images { get; set; }
    }

    public class FortniteMapResponse
    {
        public int Status { get; set; }
        public MapData Data { get; set; }
    }
}