using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class FortniteApiService
{
    private static readonly HttpClient _httpClient = new();

    private const string ShopEndpoint = "https://fortnite-api.com/v2/shop?language=en";
    private const string MapEndpoint = "https://fortnite-api.com/v1/map";
    private const string StatusEndpoint = "https://status.epicgames.com/api/v2/summary.json"; // This is the status endpoint
    private const string NewsEndpoint = "https://fortnite-api.com/v2/news/br?language=en"; // Setting the language to english for this endpoint ensures that we get data that is in english
    private const string CosmeticSearchEndpoint = "https://fortnite-api.com/v2/cosmetics/br/search";

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

    public async Task<EpicGamesStatus> GetServiceStatusAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(StatusEndpoint);
            var jsonString = await response.Content.ReadAsStringAsync();

            var statusResponse = JsonSerializer.Deserialize<EpicStatusResponse>(
                jsonString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (statusResponse.Status != null)
            {
                return new EpicGamesStatus
                {
                    Indicator = statusResponse.Status.Indicator,
                    Description = statusResponse.Status.Description,
                    Components = statusResponse.Components,
                    HasIncidents = statusResponse.Incidents?.Count > 0,
                    IncidentCount = statusResponse.Incidents?.Count ?? 0
                };
            }
            return null;
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Status API Error: {ex.Message}");
            return null;
        }
        
    }

    public async Task<List<NewsItem>> GetFortniteNewsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(NewsEndpoint);
            var jsonString = await response.Content.ReadAsStringAsync();

            var newsResponse = JsonSerializer.Deserialize<FortniteNewsResponse>(
                jsonString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

            if (newsResponse?.Data.Motds != null)
            {
                return newsResponse.Data.Motds.Select(motd => new NewsItem
                {
                    Title = motd.Title,
                    Body = motd.Body,
                    Image = motd.Image
                }).ToList();
            }

            return new List<NewsItem>();
        }

        catch (Exception ex)
        {
            Console.WriteLine($"API Error: {ex.Message}");
            return new List<NewsItem>();
        }
    }

    public async Task<CosmeticItem> SearchCosmeticAsync(string name)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{CosmeticSearchEndpoint}?name={Uri.EscapeDataString(name)}&language=en");
            var jsonString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Cosmetic Search API Error: {response.StatusCode}");
                return null;
            }

            var cosmeticResponse = JsonSerializer.Deserialize<FortniteCosmeticResponse>(
                jsonString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            return cosmeticResponse.Data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cosmetic Search API Error: {ex.Message}");
            return null;
        }
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

// Classes for Epic Games Status API
public class EpicGamesStatus
{
    public string Indicator { get; set; }
    public string Description { get; set; }
    public List<StatusComponent> Components { get; set; }
    public bool HasIncidents { get; set; }
    public int IncidentCount { get; set; }
}

public class StatusComponent
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
}

public class StatusInfo
{
    public string Indicator { get; set; }
    public string Description { get; set; }
}

public class StatusIncident
{
    public string Name { get; set; }
    public string Status { get; set; }
    public string Impact { get; set; }
}

public class EpicStatusResponse
{
    public StatusInfo Status { get; set; }

    public List<StatusComponent> Components { get; set; }
    public List<StatusIncident> Incidents { get; set; }
}

// Classes for News API
public class NewsItem
{
    public string Title { get; set; }
    public string Body { get; set; }
    public string Image { get; set; }
}

public class NewsMotd
{
    public string Title { get; set; }
    public string Body { get; set; }
    public string Image { get; set; }
}

public class NewsData
{
    public List<NewsMotd> Motds { get; set; }
}

public class FortniteNewsResponse
{
    public int Status { get; set; }
    public NewsData Data { get; set; }
}

// Classes for Cosmetic Search

public class CosmeticItem
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public CosmeticType Type { get; set; }
    public CosmeticRarity Rarity { get; set; }
    public CosmeticImages Images { get; set; }
    public CosmeticIntroduction Introduction { get; set; }
}

public class CosmeticType
{
    public string Value { get; set; }
    public string DisplayValue { get; set; }
}

public class CosmeticRarity
{
    public string Value { get; set; }
    public string DisplayValue { get; set; }
}


public class CosmeticImages
{
    public string Icon { get; set; }
    public string Featured { get; set; }
}

public class CosmeticIntroduction
{
    public string Chapter { get; set; }
    public string Season { get; set; }
    public string Text { get; set; }
}

public class FortniteCosmeticResponse
{
    public int Status { get; set; }
    public CosmeticItem Data { get; set; }
}