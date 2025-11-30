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
            .WithFooter("Data from Fortnite-API.com");

        await FollowupAsync(embed: embed.Build());
    }
}
