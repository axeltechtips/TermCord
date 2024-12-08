using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

class Program
{
    private static string token = "";
    private static string currentChannelId = "";
    private static string currentChannelName = "";

    static async Task Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Welcome to TermCord, your virtual terminal for Discord!");
        Console.WriteLine("Type `!help` for available commands.");
        Console.ResetColor();

        while (true)
        {
            Console.Write("\nTermCord> ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input)) continue;

            switch (input.ToLower())
            {
                case var _ when input.StartsWith("!login "):
                    var tokenInput = input.Substring(7).Trim();
                    if (string.IsNullOrWhiteSpace(tokenInput))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error: Please provide a valid token after `!login`.");
                        Console.ResetColor();
                    }
                    else
                    {
                        token = tokenInput;
                        await Login(token);
                    }
                    break;

                case "!logout":
                    Logout();
                    break;

                case "!profile":
                    await FetchProfile();
                    break;

                case "!servers":
                    await FetchServers();
                    break;

                case "!ping":
                    await PingDiscord();
                    break;

                case "!dm":
                    await FetchDMs();
                    break;

                case var _ when input.StartsWith("!send "):
                    var message = input.Substring(6).Trim();
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        Console.WriteLine("Usage: !send <message>");
                    }
                    else
                    {
                        await SendMessage(message);
                    }
                    break;

                case "!clear":
                    Console.Clear();
                    break;

                case "!exit":
                    Environment.Exit(0);
                    break;

                case "!help":
                    ShowHelp();
                    break;

                case var _ when input.StartsWith("!createchannel "):
                    var channelName = input.Substring(15).Trim();
                    if (string.IsNullOrWhiteSpace(channelName))
                    {
                        Console.WriteLine("Usage: !createchannel <channel_name>");
                    }
                    else
                    {
                        await CreateChannel(channelName);
                    }
                    break;

                case var _ when input.StartsWith("!vc "):
                    var serverId = input.Substring(4).Trim();
                    await ListVoiceChannels(serverId);
                    break;

                case var _ when input.StartsWith("!sendfile "):
                    var filePath = input.Substring(10).Trim();
                    if (string.IsNullOrWhiteSpace(filePath))
                    {
                        Console.WriteLine("Usage: !sendfile <file_path>");
                    }
                    else
                    {
                        await SendFile(filePath);
                    }
                    break;

                default:
                    Console.WriteLine("Unknown command. Type `!help` for a list of commands.");
                    break;
            }
        }
    }

    private static async Task Login(string token)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", token);

        var response = await client.GetAsync("https://discord.com/api/v10/users/@me");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<JsonElement>(content);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Logged in as {user.GetProperty("username").GetString()}#{user.GetProperty("discriminator").GetString()}.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Login failed. Please check your token.");
            Console.ResetColor();
        }
    }

    private static void Logout()
    {
        token = "";
        currentChannelId = "";
        currentChannelName = "";
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Logged out successfully.");
        Console.ResetColor();
    }

    private static async Task FetchProfile()
    {
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Please log in first using `!login <token>`.");
            return;
        }

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", token);

        var response = await client.GetAsync("https://discord.com/api/v10/users/@me");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<JsonElement>(content);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Username: {user.GetProperty("username").GetString()}#{user.GetProperty("discriminator").GetString()}");
            Console.WriteLine($"ID: {user.GetProperty("id").GetString()}");
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine("Failed to fetch profile.");
        }
    }

    private static async Task FetchServers()
    {
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Please log in first using `!login <token>`.");
            return;
        }

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", token);

        var response = await client.GetAsync("https://discord.com/api/v10/users/@me/guilds");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var servers = JsonSerializer.Deserialize<JsonElement>(content);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Your Servers:");
            foreach (var server in servers.EnumerateArray())
            {
                Console.WriteLine($"- {server.GetProperty("name").GetString()} (ID: {server.GetProperty("id").GetString()})");
            }
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine("Failed to fetch servers.");
        }
    }

    private static async Task PingDiscord()
    {
        var startTime = DateTime.Now;

        using var client = new HttpClient();
        var response = await client.GetAsync("https://discord.com/api/v10");

        var latency = DateTime.Now - startTime;
        if (response.IsSuccessStatusCode)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Pong! Latency: {latency.TotalMilliseconds} ms");
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine("Failed to ping Discord servers.");
        }
    }

    private static async Task FetchDMs()
    {
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Please log in first using `!login <token>`.");
            return;
        }

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", token);

        var response = await client.GetAsync("https://discord.com/api/v10/users/@me/channels");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var dms = JsonSerializer.Deserialize<JsonElement>(content);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Your Direct Messages:");
            var dmList = new List<(string Id, string Name)>();
            int index = 1;

            foreach (var dm in dms.EnumerateArray())
            {
                if (dm.TryGetProperty("recipients", out JsonElement recipients))
                {
                    var recipientNames = new StringBuilder();
                    foreach (var recipient in recipients.EnumerateArray())
                    {
                        recipientNames.Append(recipient.GetProperty("username").GetString() + ", ");
                    }

                    var formattedNames = recipientNames.ToString().TrimEnd(',', ' ');

                    Console.WriteLine($"{index}. {formattedNames} (ID: {dm.GetProperty("id").GetString()})");
                    dmList.Add((dm.GetProperty("id").GetString(), formattedNames));
                    index++;
                }
            }

            Console.ResetColor();
            Console.WriteLine("Select a DM by typing its number.");
            string selectedInput = Console.ReadLine();
            if (int.TryParse(selectedInput, out int selectedDm) && selectedDm > 0 && selectedDm <= dmList.Count)
            {
                var selected = dmList[selectedDm - 1];
                currentChannelId = selected.Id;
                currentChannelName = selected.Name;
                await ListMessages();
            }
            else
            {
                Console.WriteLine("Invalid selection.");
            }
        }
        else
        {
            Console.WriteLine("Failed to fetch direct messages.");
        }
    }

    private static async Task ListMessages()
    {
        if (string.IsNullOrEmpty(currentChannelId))
        {
            Console.WriteLine("No active DM channel selected.");
            return;
        }

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", token);

        var response = await client.GetAsync($"https://discord.com/api/v10/channels/{currentChannelId}/messages");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var messages = JsonSerializer.Deserialize<JsonElement>(content);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Messages in DM with {currentChannelName}:");
            foreach (var message in messages.EnumerateArray())
            {
                Console.WriteLine($"{message.GetProperty("author").GetProperty("username").GetString()}: {message.GetProperty("content").GetString()}");
            }
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine("Failed to fetch messages.");
        }
    }

    private static async Task SendMessage(string message)
    {
        if (string.IsNullOrEmpty(currentChannelId))
        {
            Console.WriteLine("No active DM channel selected. Use `!dm` to view and select a channel.");
            return;
        }

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", token);

        var payload = new { content = message };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"https://discord.com/api/v10/channels/{currentChannelId}/messages", content);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Message sent successfully.");
        }
        else
        {
            Console.WriteLine("Failed to send message.");
        }
    }

    private static async Task CreateChannel(string channelName)
    {
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Please log in first using `!login <token>`.");
            return;
        }

        Console.WriteLine($"Creating text channel `{channelName}`...");
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", token);

        var payload = new
        {
            name = channelName,
            type = 0 // Text channel type
        };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://discord.com/api/v10/guilds/<your-guild-id>/channels", content);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Channel `{channelName}` created successfully.");
        }
        else
        {
            Console.WriteLine("Failed to create channel.");
        }
    }

    private static async Task SendFile(string filePath)
    {
        if (string.IsNullOrEmpty(currentChannelId))
        {
            Console.WriteLine("No active DM channel selected.");
            return;
        }

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", token);
            var form = new MultipartFormDataContent();
            form.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(filePath)), "file", filePath);

            var response = await client.PostAsync($"https://discord.com/api/v10/channels/{currentChannelId}/messages", form);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("File sent successfully.");
            }
            else
            {
                Console.WriteLine("Failed to send file.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending file: {ex.Message}");
        }
    }

    private static async Task ListVoiceChannels(string serverId)
    {
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Please log in first using `!login <token>`.");
            return;
        }

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", token);

        var response = await client.GetAsync($"https://discord.com/api/v10/guilds/{serverId}/channels");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var channels = JsonSerializer.Deserialize<JsonElement>(content);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Voice Channels in Server:");
            foreach (var channel in channels.EnumerateArray())
            {
                if (channel.GetProperty("type").GetInt32() == 2) // Type 2 = Voice channel
                {
                    Console.WriteLine($"- {channel.GetProperty("name").GetString()} (ID: {channel.GetProperty("id").GetString()})");
                }
            }
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine("Failed to fetch voice channels.");
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Available Commands:");
        Console.WriteLine("!login <token> - Log in with your token.");
        Console.WriteLine("!logout - Log out from your account.");
        Console.WriteLine("!profile - View your profile.");
        Console.WriteLine("!servers - View your servers.");
        Console.WriteLine("!ping - Test latency to Discord.");
        Console.WriteLine("!dm - View your direct messages.");
        Console.WriteLine("!send <message> - Send a message in the current DM.");
        Console.WriteLine("!createchannel <channel_name> - Create a new text channel.");
        Console.WriteLine("!sendfile <file_path> - Send a file in the current DM.");
        Console.WriteLine("!vc <server_id> - View voice channels in a server.");
        Console.WriteLine("!clear - Clear the terminal.");
        Console.WriteLine("!exit - Exit the application.");
    }
}