using Discord;
using Discord.Interactions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

// NOTE: You must also create the FortniteApiService and related models!

public class FortniteModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly FortniteApiService _apiService;

    // The service is automatically provided by the Interaction Service
    public FortniteModule(FortniteApiService apiService)
    {
        _apiService = apiService;
    }

    [SlashCommand("map", "Shows the current Battle Royale map image.")]
    public async Task GetMapCommand()
    {
        // Acknowledge the command immediately to avoid timeout
        await DeferAsync(ephemeral: false);

        // 1. Get the map URL using the API service
        string mapImageUrl = await _apiService.GetMapImageUrlAsync();

        if (mapImageUrl.StartsWith("Error"))
        {
            await FollowupAsync($"Error: {mapImageUrl}");
            return;
        }

        // 2. Build and send the Embed
        var embed = new EmbedBuilder()
            .WithTitle("🗺️ Current Battle Royale Map")
            .WithColor(new Color(0, 150, 255))
            .WithImageUrl(mapImageUrl)
            .WithFooter("Via Fortnite-API.com");

        await FollowupAsync(embed: embed.Build());
    }

    [SlashCommand("shop", "Shows the current Item Shop.")]
    public async Task GetShopCommand()
    {
        await DeferAsync(ephemeral: false); // Setting ephemeral to false to make it visible to everyone

        try
        {
            var shopItems = await _apiService.GetCurrentShopAsync();

            if (shopItems == null || !shopItems.Any())
            {
                await FollowupAsync("The item shop is currently empty or failed to load. Try again later!");
                return;
            }

            var sections = shopItems.GroupBy(i => i.SectionName).ToList();

            var embed = new EmbedBuilder()
                .WithTitle("🛍️ Fortnite Item Shop (Today)")
                .WithDescription($"Total items found: **{shopItems.Count}**")
                .WithColor(new Color(255, 100, 0));

            foreach (var section in sections)
            {
                var sectionContent = section
                    .Select(item => $"• **{item.Name}** ({item.VBucksCost} V-Bucks)")
                    .ToList();

                string fieldContent = string.Join("\n", sectionContent);

                if (fieldContent.Length > 1024)
                {
                    fieldContent = fieldContent.Substring(0, 1000) + "\n... (truncated)";
                }

                embed.AddField($"___{section.Key} (Items: {section.Count()})___", fieldContent);
            }

            embed.WithFooter("Prices are in V-Bucks. Fortnite-API.com");

            await FollowupAsync(embed: embed.Build());
        }
        catch (Exception ex)
        {
            await FollowupAsync($"An error occurred while fetching the shop data: {ex.Message}", ephemeral: false);
        }
    }

    [SlashCommand("status", "Displays the status of the Fortnite servers.")]
    public async Task GetStatusCommand()
    {
        await DeferAsync(ephemeral: false);

        var status = await _apiService.GetServiceStatusAsync();

        if (status == null)
        {
            await FollowupAsync("Failed to fetch the server status. Please try again later or contact a developer for support.");
            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle("Epic Games Server Status")
            .WithColor(GetColorFromIndicator(status.Indicator))
            .WithDescription($"**{status.Description}**")
            .WithCurrentTimestamp();

        var fortniteComponents = status.Components
            .Where(c => c.Name.Contains("Fortnite", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if(fortniteComponents.Any())
        {
            foreach (var component in fortniteComponents)
            {
                string statusEmoji = GetEmojiFromStatus(component.Status);
                string formattedStatus = component.Status.Replace("_", " ");
                embed.AddField(component.Name, $"{statusEmoji} {char.ToUpper(formattedStatus[0]) + formattedStatus.Substring(1)}", inline: true);
            }
        }
        else
        {
            foreach (var component in status.Components.Take(10))
            {
                string statusEmoji = GetEmojiFromStatus(component.Status);
                string formattedStatus = component.Status.Replace("_", " ");
                embed.AddField(component.Name, $"{statusEmoji} {char.ToUpper(formattedStatus[0]) + formattedStatus.Substring(1)}", inline: true);
            }
        }

        if (status.HasIncidents)
        {
            embed.AddField("Active Incidents: ", $"{status.IncidentCount} incident(s) currently active.", inline: false);
        }

        embed.WithFooter("Status via status.epicgames.com");

        await FollowupAsync(embed: embed.Build());

    }

    [SlashCommand("news", "Displays the latest news in Fortnite including BR, STW and Creative modes.")]
    public async Task GetNewsCommand()
    {
        await DeferAsync(ephemeral: false);
        
        var newsItems = await _apiService.GetFortniteNewsAsync();

        if (newsItems == null || !newsItems.Any())
        {
            await FollowupAsync("No news is available at the moment. Please try again later.");
            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle("Fortnite News")
            .WithColor(new Color(0, 150, 255))
            .WithCurrentTimestamp();

        foreach (var news in newsItems.Take(3)) // This limits neews items to a max of 3 to appear on the embed
        {
            string body = news.Body.Length > 500
                ? news.Body.Substring(0, 500) + "..."
                : news.Body;

            embed.AddField(news.Title, body, inline: false);
        }

        if (!string.IsNullOrEmpty(newsItems.First().Image))
        {
            embed.WithThumbnailUrl(newsItems.First().Image);
        }

        embed.WithFooter("Data from Fortnite-API.com");

        await FollowupAsync(embed: embed.Build());
    }

    [SlashCommand("cosmetic", "Searches for a fortnite cosmetic item.")]
    public async Task GetCosmeticCommand(
        [Summary("name", "the cosmetic item that you wish to search for.")] string itemName)
    {
        await DeferAsync(ephemeral: false);

        var cosmetic = await _apiService.SearchCosmeticAsync(itemName);

        if (cosmetic == null)
        {
            await FollowupAsync($"Could not find a cosmetic name with **{itemName}**. Please try again and enter a correct cosmetic name.");
            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle($"{cosmetic.Name}")
            .WithDescription(cosmetic.Description ?? "No description available.")
            .WithColor(GetRarityColor(cosmetic.Rarity?.Value));

        if (cosmetic.Type?.DisplayValue != null)
        {
            embed.AddField("Type", cosmetic.Type.DisplayValue, inline: true);
        }
        if (cosmetic.Rarity?.DisplayValue != null)
        {
            embed.AddField("Rarity", cosmetic.Rarity.DisplayValue, inline: true);
        }
        if (cosmetic.Introduction?.Text != null)
        {
            embed.AddField("Introduced", cosmetic.Introduction.Text, inline: true);
        }

        if (!string.IsNullOrEmpty(cosmetic.Images.Icon))
        {
            embed.WithImageUrl(cosmetic.Images?.Icon);
        }
        embed.WithFooter("Data from Fornite-API.com.");

        await FollowupAsync(embed: embed.Build());
    }

    private Color GetColorFromIndicator(string indicator)
    {
        return indicator switch
        {
            "none" => Color.Green,
            "minor" => Color.Gold,
            "major" => Color.Orange,
            "critical" => Color.Red,
            _ => Color.LighterGrey,
        };
    }

    private string GetEmojiFromStatus(string status)
    {
        return status switch
        {
            "operational" => "🟢",
            "degraded_performance" => "🟡",
            "partial_outage" => "🟠",
            "major_outage" => "🔴",
            _ => "⚪"
        };
    }

    // Rarity Colors
    private Color GetRarityColor(string rarity)
    {
        return rarity?.ToLower() switch
        {
            "legendary" => new Color(211, 120, 65),
            "epic" => new Color(177, 91, 226),
            "rare" => new Color(73, 172, 242),
            "uncommon" => new Color(96, 170, 58),
            "common" => new Color(190, 190, 190),
            _ => Color.Default
        };
    }
}
