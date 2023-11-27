using DSharpPlus;
using DSharpPlus.EventArgs;

internal class Discord
{
  private static DiscordClient? Client { get; set; }
  public static async Task WaitMessage()
  {
    var discordConfig = new DiscordConfiguration()
    {
      Intents = DiscordIntents.All,
      Token = Secret.token,
      TokenType = TokenType.Bot,
      AutoReconnect = true
    };

    Client = new DiscordClient(discordConfig);

    Client.Ready += Client_Ready;

    Client.MessageCreated += async (s, e) =>
    {
      using CancellationTokenSource cts = new();
      ulong chanelId = Secret.chanelId;
      string dt = DateTime.Now.ToString("h:mm:ss tt");

      if (e.Message.Channel.Name == "ранг")
      {
        Console.WriteLine(e.Message + " " + dt);
        string[] strs = e.Message.Content.Split(' ');
        Valorant.GetRank("eU", "danilairo", "000");


      }

    };

    await Client.ConnectAsync();
    await Task.Delay(-1);
  }

  private static Task Client_Ready(DiscordClient sender, ReadyEventArgs args)
  {
    return Task.CompletedTask;
  }
}