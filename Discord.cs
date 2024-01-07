using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Data;
using System.Data.SQLite;

internal class Discord
{
  private static DiscordClient? Client { get; set; }
  private static readonly string[] ranks = [
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
    ];
  private static DiscordRole? valorantRole { get; set; }
  private static DiscordMember? Member { get; set; }
  private static DiscordGuild? Guild { get; set; }
  private static DiscordChannel? Channel { get; set; }

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

    // Добавить команду для удаления себя из БД

    Client.MessageCreated += async (s, e) =>
    {
      string dt = DateTime.Now.ToString("h:mm:ss tt");
      string connectionString = "Data Source=data.sqlite;Version=3;";
      Member = e.Guild.Members[e.Author.Id];
      Guild = e.Guild;

      foreach (var role in Guild.Roles.Values)
      {
        if (role.Name == "Valorant Member")
        {
          valorantRole = role;
          break;
        }
      }

      foreach (var channel in Guild.Channels.Values)
      {
        if (channel.Name == "ранг" || channel.Name == "rank")
        {
          Channel = channel;
        }
      }

      await using (var connection = new SQLiteConnection(connectionString))
      {
        connection.Open();
        
        if (e.Message.Channel.Name == "ранг" || e.Message.Channel.Name == "rank")
        {
          Console.WriteLine(e.Message + " - " + dt);
          string[] strs = e.Message.Content.Split('#');

          string rank = String.Empty;

          if (strs.Length == 2)
          {
            rank = Valorant.GetRankByInfo("eU", strs[0], strs[1]);
          }
          else if (strs.Length == 3)
          {
            rank = Valorant.GetRankByInfo(strs[2], strs[0], strs[1]);
          }

          string sql = $@"SELECT *
                          FROM ranks
                          WHERE discord_user_id = {Member.Id}
                          AND discord_server_id = {Guild.Id}";
          
          var command = new SQLiteCommand(sql, connection);
          SQLiteDataReader reader = command.ExecuteReader();

          if (!String.IsNullOrEmpty(rank))
          {
            if (!reader.HasRows)
            {
              SendMessage($"{e.Message.Content} user added with rank - {rank}");
              // ранг распознан, пользователь не распознан
              if (strs.Length == 2)
              {
                sql = $@"INSERT INTO ranks VALUES(
                        {Guild.Id}, 
                        {Member.Id}, 
                        '{Valorant.GetPUUID("eU", strs[0], strs[1])}', 
                        '{rank}', 
                        '{e.Message.Content}')";
              }
              else if (strs.Length == 3)
              {
                sql = $@"INSERT INTO ranks VALUES(
                      {Guild.Id}, 
                      {Member.Id}, 
                      '{Valorant.GetPUUID(strs[2], strs[0], strs[1])}', 
                      '{rank}', 
                      '{e.Message.Content}')";
              }
              command = new SQLiteCommand(sql, connection);
              command.ExecuteNonQueryAsync();
              GiveMemberRole();
              UpdateRole(rank);
            }
            else 
            {
              // ранг, пользователь и сервер распознаны
              while (reader.Read())
              {
                var oldRank = reader.GetValue(3);
                var currentRank = Valorant.GetRankByPUUID($"{reader.GetValue(2)}");
                if (!oldRank.Equals(currentRank) && !currentRank.Equals(String.Empty))
                {
                  SendMessage($"{Member.DisplayName} user updated with rank -  {currentRank}");
                  sql = $@"UPDATE ranks 
                            SET valorant_rank = '{currentRank}'
                            WHERE discord_user_id = {Member.Id} 
                            AND discord_server_id = {Guild.Id}";
                  command = new SQLiteCommand(sql, connection);
                  command.ExecuteNonQueryAsync();
                  GiveMemberRole();
                  UpdateRole($"{reader.GetValue(3)}");
                }
              }
            }
          }
          else
          {
            if (reader.HasRows)
            {
              while (reader.Read())
              {
                // ранг не распознан, пользователь и сервер распознаны
                var oldRank = $"{reader.GetValue(3)}";
                var currentRank = Valorant.GetRankByPUUID($"{reader.GetValue(2)}");
                if (!currentRank.Equals(oldRank) && !currentRank.Equals(String.Empty))
                {
                  SendMessage($"{Member.DisplayName} user updated with rank - {currentRank}");
                  sql = $@"UPDATE ranks 
                            SET valorant_rank = '{currentRank}'
                            WHERE discord_user_id = {Member.Id} 
                            AND discord_server_id = {Guild.Id}";
                  command = new SQLiteCommand(sql, connection);
                  command.ExecuteNonQueryAsync();
                  GiveMemberRole();
                  UpdateRole($"{reader.GetValue(3)}");
                }
              }
            }
            else
            {
              // ранг, пользователь и сервер не распознаны
              SendMessage($"{e.Message.Content} Valorant user not found");
            }
          }
        }
          
        UpdateAllRoles(connection);
      }
    };

    await Client.ConnectAsync();
    await Task.Delay(-1);
  }

  private static Task Client_Ready(DiscordClient sender, ReadyEventArgs args)
  {
    return Task.CompletedTask;
  }

  private static void GiveMemberRole()
  {
    var roles = Member.Roles;

    foreach (var role in roles)
    {
      if (role == valorantRole)
      {
        return;
      }
    }

    Thread.Sleep(1000);

    Member.GrantRoleAsync(valorantRole);
    Console.WriteLine($"{Member.DisplayName} - gives - {valorantRole.Name}");
  }

  private static void UpdateRole(string newRank)
  {
    var roles = Member.Roles;

    foreach (var role in roles)
    {
      foreach (var rank in ranks)
      {
        if (role.Name == rank)
        {
          Member.RevokeRoleAsync(role);
          Console.WriteLine($"{Member.DisplayName} - remove - {role.Name}");
        }
      }
    }

    Thread.Sleep(1000);

    foreach (var role in Guild.Roles.Values)
    {
      if (role.Name == newRank)
      {
        Member.GrantRoleAsync(role);
        Console.WriteLine($"{Member.DisplayName} - gives - {role.Name}");
        break;
      }
    }
  }

  private static void UpdateAllRoles(SQLiteConnection c)
  {
    var sql = $@"SELECT *
                  FROM ranks
                  WHERE discord_server_id = {Guild.Id}";
    var command = new SQLiteCommand(sql, c);
    var reader = command.ExecuteReader();

    if (reader.HasRows)
    {
      while (reader.Read())
      {
        var oldRank = reader.GetValue(3);
        var currentRank = Valorant.GetRankByPUUID($"{reader.GetValue(2)}");
        System.Console.WriteLine($"Test: {reader.GetValue(4)} - oldRank: {oldRank} - currentRank: {currentRank}");
        if (!oldRank.Equals(currentRank) && !currentRank.Equals(String.Empty))
        {
          SendMessage($"{reader.GetValue(4)} user updated with rank - {currentRank}");
          Member = Guild.Members[ulong.Parse($"{reader.GetValue(1)}")];
          UpdateRole(currentRank);

          sql = $@"UPDATE ranks 
                  SET valorant_rank = '{currentRank}'
                  WHERE discord_user_id = {Member.Id} 
                  AND discord_server_id = {Guild.Id}";
          command = new SQLiteCommand(sql, c);
          command.ExecuteNonQueryAsync();
        }
      }
    }
  }

  private static void SendMessage(string text)
  {
    if (!Member.IsBot)
    {
      Client.SendMessageAsync(Channel, text);
    }
  }
}