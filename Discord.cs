using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

internal class Discord
{
  private static DiscordClient? Client { get; set; }
  public static async Task WaitMessage()
  {
    var discordConfig = new DiscordConfiguration()
    {
      Intents = DiscordIntents.All,
      Token = Secret.diskordToken,
      TokenType = TokenType.Bot,
      AutoReconnect = true
    };

    Client = new DiscordClient(discordConfig);

    Client.Ready += Client_Ready;

    Client.MessageCreated += async (s, e) =>
    {
      ulong chanelId = Secret.discordChanelId;
      string dt = DateTime.Now.ToString("h:mm:ss tt");

      if (e.Message.Channel.Name == "ранг" || e.Message.Channel.Name == "rank")
      {
        Console.WriteLine(e.Message + " - " + dt);
        string[] strs = e.Message.Content.Split(' ');

        string rank = String.Empty; ;

        if (strs.Length == 2)
        {
          rank = Valorant.GetRank("eU", strs[0], strs[1]);
        }
        else if (strs.Length == 3)
        {
          rank = Valorant.GetRank(strs[2], strs[0], strs[1]);
        }

        System.Console.WriteLine(rank);

        if (!String.IsNullOrEmpty(rank))
        {
          UpdateRole(e, rank);
        }


      }

    };

    await Client.ConnectAsync();
    await Task.Delay(-1);
  }

  private static Task Client_Ready(DiscordClient sender, ReadyEventArgs args)
  {
    return Task.CompletedTask;
  }

  private static void UpdateRole(MessageCreateEventArgs e, string newRank)
  {
    // RemoveRole(e.Guild.Members[e.Author.Id]);
    var member = e.Guild.Members[e.Author.Id];

    string[] ranks = {
      "Iron 1",
      "Iron 2",
      "Iron 3",
      "Bronze 1",
      "Bronze 2",
      "Bronze 3",
      "Silver 1",
      "Silver 2",
      "Silver 3",
      "Gold 1",
      "Gold 2",
      "Gold 3",
      "Platinum 1",
      "Platinum 2",
      "Platinum 3",
      "Diamond 1",
      "Diamond 2",
      "Diamond 3",
      "Ascedant 1",
      "Ascedant 2",
      "Ascedant 3",
      "Immortal 1",
      "Immortal 2",
      "Immortal 3",
      "Radiant"
    };

    var roles = member.Roles;

    foreach (var role in roles)
    {
      foreach (var rank in ranks)
      {
        if (role.Name == rank)
        {
          member.RevokeRoleAsync(role);
          Console.WriteLine($"{member.DisplayName} - remove - {role.Name}");
        }
      }
    }

    Thread.Sleep(1000);

    foreach (var role in e.Guild.Roles.Values)
    {
      if (role.Name == newRank)
      {
        // GiveRole(e.Guild.Members[e.Author.Id], role);
        member.GrantRoleAsync(role);
        Console.WriteLine($"{member.DisplayName} - gives - {role.Name}");
        break;
      }
    }
  }

  private static void RemoveRole(DiscordMember member)
  {
    string[] ranks = {
      "Iron 1",
      "Iron 2",
      "Iron 3",
      "Bronze 1",
      "Bronze 2",
      "Bronze 3",
      "Silver 1",
      "Silver 2",
      "Silver 3",
      "Gold 1",
      "Gold 2",
      "Gold 3",
      "Platinum 1",
      "Platinum 2",
      "Platinum 3",
      "Diamond 1",
      "Diamond 2",
      "Diamond 3",
      "Ascedant 1",
      "Ascedant 2",
      "Ascedant 3",
      "Immortal 1",
      "Immortal 2",
      "Immortal 3",
      "Radiant"
    };

    var roles = member.Roles;

    foreach (var role in roles)
    {
      foreach (var rank in ranks)
      {
        if (role.Name == rank)
        {
          member.RevokeRoleAsync(role);
          Console.WriteLine($"{member.DisplayName} - remove - {role.Name}");
        }
      }
    }
  }

    private static async void GiveRole(DiscordMember member, DiscordRole role)
    {
      await member.GrantRoleAsync(role);
      Console.WriteLine($"{member.DisplayName} - gives - {role.Name}");
    }
}