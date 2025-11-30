using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    private DiscordSocketClient _client;
    private IConfigurationRoot _config;
    private InteractionService _interactions;
    private IServiceProvider _services;

    public static Task Main(string[] args) => new Program().MainAsync();

    private IServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(x => new InteractionService(_client))
            .AddSingleton<FortniteApiService>()
            .BuildServiceProvider();
    }

    public async Task ClientReady()
    {
        await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        if (ulong.TryParse(_config["TestGuildId"], out ulong guildId))
        {
            await _interactions.RegisterCommandsToGuildAsync(guildId);
            Console.WriteLine($"Registered commands to Guild: {guildId}");
        }
    }

    public async Task OnInteractionCreated(SocketInteraction interaction)
    {
        var context = new SocketInteractionContext(_client, interaction);
        await _interactions.ExecuteCommandAsync(context, _services);
    }

    public async Task MainAsync()
    {
        _config = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddUserSecrets<Program>()
        .AddEnvironmentVariables()
        .Build();

        var config = new DiscordSocketConfig { /* ... */ };
        _client = new DiscordSocketClient(config);

        _client.Log += Log;

        _services = ConfigureServices();
        _interactions = _services.GetRequiredService<InteractionService>();

        _client.Ready += ClientReady;
        _client.InteractionCreated += OnInteractionCreated;

        string token = _config["DiscordToken"];
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        using var scope = _services.CreateScope();

        await Task.Delay(Timeout.Infinite);
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}

