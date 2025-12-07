using Discord;
using Discord.Interactions;
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
}
