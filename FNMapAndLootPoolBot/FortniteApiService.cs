using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class FortniteApiService
{
    private static readonly HttpClient _httpClient = new();

    private const string ShopEndpoint = "https://fortnite-api.com/v2/shop?language=en";
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
            var response = await _httpClient.GetAsync(ShopEndpoint);
            var jsonString = await response.Content.ReadAsStringAsync();

            var shopResponse = JsonSerializer.Deserialize<FortniteShopResponse>(
                jsonString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            Console.WriteLine($"Found {shopResponse.Data.Entries.Count} items in total");

            foreach (var entry in shopResponse.Data.Entries)
            {
                string sectionName = entry.Section?.Name ?? "Featured";

                if (entry.Bundle != null && entry.Bundle.Name != null)
                {
                    items.Add(new ShopItem
                    {
                        Name = entry.Bundle.Name,
                        VBucksCost = entry.FinalPrice,
                        SectionName = sectionName
                    });
                }

                if (entry.Items != null)
                {
                    foreach (var itemDetail in entry.Items)
                    {
                        items.Add(new ShopItem
                        {
                            Name = itemDetail.Name ?? "Unknown Item",
                            VBucksCost = entry.FinalPrice,
                            SectionName = sectionName
                        });
                    }
                }
            }

            Console.WriteLine($"Total items collected: {items.Count}");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"API Request Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected Error: {ex.Message}");
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

// Required to parse shop entries
public class BundleInfo
{
    public string Name { get; set; }
}
public class SectionInfo
{
    public string Name { get; set; }
}

public class ShopEntry
{
    public int RegularPrice { get; set; }
    public int FinalPrice { get; set; }
    public BundleInfo Bundle { get; set; }
    public List<ItemDetail> Items { get; set; }
    public SectionInfo Section { get; set; }
}

public class ShopSection
{
    public string Name { get; set; }
    public List<ShopEntry> Entries { get; set; }
}

public class ShopData
{
    public List<ShopEntry> Entries { get; set; }
}

public class FortniteShopResponse
{
    public int Status { get; set; }
    public ShopData Data { get; set; }
}

