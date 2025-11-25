using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Threading.Tasks;

public class Program
{
    private DiscordSocketClient _client;
    private IConfigurationRoot _config;

    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {
        _config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .AddEnviromentVariables()
            .Build();

        var config = new DiscordSocketConfig { MessageCacheSize = 100, GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent };
        _client = new DiscordSocketClient(config);

        _client.Log += Log;

        string token = _config["DiscordToken"];
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}

