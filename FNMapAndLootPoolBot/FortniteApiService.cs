using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class FortniteApiService
{
    private static readonly HttpClient _httpClient = new();

    private const string ShopEndpoint = "https://fortnite-api.com/v2/shop/br";
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

    public async Task<List<ShopItem>> GetCurrentShopAsync()
    {
        var items = new List<ShopItem>();

        try
        {
            var jsonString = await _httpClient.GetStringAsync(ShopEndpoint);

            var shopResponse = JsonSerializer.Deserialize<FortniteShopResponse>(
                jsonString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            // Loop through all sections (e.g., Featured, Daily, Special Offers)
            if (shopResponse?.Data?.Sections != null)
            {
                foreach (var section in shopResponse.Data.Sections)
                {
                    // The API names the section in its 'name' property
                    string sectionName = section.Name ?? "Uncategorized";

                    // Loop through all entries in that section
                    if (section.Entries != null)
                    {
                        foreach (var entry in section.Entries)
                        {
                            // A single entry can contain multiple items (e.g., bundles)
                            if (entry.Items != null)
                            {
                                foreach (var itemDetail in entry.Items)
                                {
                                    items.Add(new ShopItem
                                    {
                                        Name = itemDetail.Name,
                                        VBucksCost = entry.RegularPrice,
                                        SectionName = sectionName
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"API Request Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Shop Deserialization Error: {ex.Message}");
        }

        return items;
    }

}

public class ShopItem
{
    public string Name { get; set; }
    public int VBucksCost { get; set; }
    public string SectionName { get; set; }
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

public class ItemDetail
{
    public string Name { get; set; }
}

public class ShopEntry
{
    public int RegularPrice { get; set; }
    public List<ItemDetail> Items
    {
        get; set;
    }
}

public class ShopSection
{
    public string Name { get; set; }
    public List<ShopEntry> Entries { get; set; }
}

public class ShopData
{
    public List<ShopSection> Sections { get; set; }
}

public class FortniteShopResponse
{
    public int Status { get; set; }
    public ShopData Data { get; set; }
}

