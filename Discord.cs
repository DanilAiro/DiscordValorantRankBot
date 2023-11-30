using DSharpPlus;
using DSharpPlus.EventArgs;
using System.Data.SQLite;

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
      string dt = DateTime.Now.ToString("h:mm:ss tt");

      if (e.Message.Channel.Name == "ранг" || e.Message.Channel.Name == "rank")
      {
        string connectionString = "Data Source=data.sqlite;Version=3;";
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

        await using (var connection = new SQLiteConnection(connectionString))
        {
          connection.Open();
          string sql = $@"SELECT *
                          FROM ranks
                          WHERE discord_user_id = {e.Author.Id}
                          AND discord_user_id = {e.Guild.Id}";
          
          var command = new SQLiteCommand(sql, connection);
          SQLiteDataReader reader = command.ExecuteReader();

          if (!String.IsNullOrEmpty(rank))
          {
            if (!reader.HasRows)
            {
              Console.WriteLine($"Situation 1. {e.Author.Username} user added - {rank}");
              // ранг распознан, пользователь не распознан
              if (strs.Length == 2)
              {
                sql = $@"INSERT INTO ranks VALUES(
                        {e.Guild.Id}, 
                        {e.Message.Author.Id}, 
                        '{Valorant.GetPUUID("eU", strs[0], strs[1])}', 
                        '{rank}', 
                        '{e.Message.Content}')";
              }
              else if (strs.Length == 3)
              {
                sql = $@"INSERT INTO ranks VALUES(
                      {e.Guild.Id}, 
                      {e.Message.Author.Id}, 
                      '{Valorant.GetPUUID(strs[2], strs[0], strs[1])}', 
                      '{rank}', 
                      '{e.Message.Content}')";
              }
              command = new SQLiteCommand(sql, connection);
              command.ExecuteNonQueryAsync();
              UpdateRole(e, rank);
            }
            else 
            {
              // ранг, пользователь и сервер распознаны
              while (reader.Read())
              {
                if (ulong.Parse($"{reader.GetValue(0)}") == e.Guild.Id && ulong.Parse($"{reader.GetValue(1)}") == e.Author.Id)
                {
                  var oldRank = reader.GetValue(3);
                  var currentRank = Valorant.GetRankByPUUID($"{reader.GetValue(2)}");
                  Console.WriteLine($"Situation 2. {e.Author.Username} user updated - {currentRank}");
                  if (!oldRank.Equals(currentRank))
                  {
                    sql = $@"UPDATE ranks 
                            SET valorant_rank = '{currentRank}'
                            WHERE discord_user_id = {e.Author.Id} 
                            AND discord_server_id = {e.Guild.Id}";
                    command = new SQLiteCommand(sql, connection);
                    command.ExecuteNonQueryAsync();
                    UpdateRole(e, $"{reader.GetValue(3)}");
                  }
                  break;
                }
                else if (ulong.Parse($"{reader.GetValue(1)}") == e.Author.Id && ulong.Parse($"{reader.GetValue(0)}") != e.Guild.Id)
                {
                  // ранг и пользователь распознаны, сервер не распознан
                  Console.WriteLine($"Situation 3. {e.Author.Username} user added - {rank}");
                  sql = $@"INSERT INTO ranks VALUES(
                          {e.Guild.Id}, 
                          {e.Author.Id}, 
                          '{reader.GetValue(2)}', 
                          '{reader.GetValue(3)}', 
                          '{reader.GetValue(4)}')";
                  command = new SQLiteCommand(sql, connection);
                  command.ExecuteNonQueryAsync();
                  UpdateRole(e, $"{reader.GetValue(3)}");
                  break;
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
                if (ulong.Parse($"{reader.GetValue(0)}") == e.Guild.Id && ulong.Parse($"{reader.GetValue(1)}") == e.Author.Id)
                {
                  var oldRank = reader.GetValue(3).ToString();
                  var currentRank = Valorant.GetRankByPUUID($"{reader.GetValue(2)}");
                  Console.WriteLine($"Situation 4. {e.Author.Username} user updated - {currentRank}");
                  if (!currentRank.Equals(oldRank))
                  {
                    sql = $@"UPDATE ranks 
                            SET valorant_rank = '{currentRank}'
                            WHERE discord_user_id = {e.Author.Id}
                            AND discord_server_id = {e.Guild.Id}";
                    command = new SQLiteCommand(sql, connection);
                    command.ExecuteNonQueryAsync();
                    UpdateRole(e, $"{reader.GetValue(3)}");
                  }
                  break;
                }
                else if (ulong.Parse($"{reader.GetValue(0)}") != e.Guild.Id && ulong.Parse($"{reader.GetValue(1)}") == e.Author.Id)
                {
                  var oldRank = $"{reader.GetValue(3)}";
                  var currentRank = Valorant.GetRankByPUUID($"{reader.GetValue(2)}");
                  if (!currentRank.Equals(oldRank))
                  {
                    Console.WriteLine($"Situation 5. {e.Author.Username} user updated - {currentRank}");
                    // ранг и сервер не распознаны, пользователь распознан. 
                    sql = $@"INSERT INTO ranks VALUES(
                            {e.Guild.Id}, 
                            {e.Author.Id}, 
                            '{reader.GetValue(2)}', 
                            '{currentRank}', 
                            '{reader.GetValue(4)}')";
                    command = new SQLiteCommand(sql, connection);
                    command.ExecuteNonQueryAsync();
                    UpdateRole(e, $"{currentRank}");
                  }
                  else
                  {
                    Console.WriteLine($"Situation 6. {e.Author.Username} user added - {reader.GetValue(3)}");
                    sql = $@"INSERT INTO ranks VALUES(
                            {e.Guild.Id}, 
                            {e.Author.Id}, 
                            '{reader.GetValue(2)}', 
                            '{reader.GetValue(3)}', 
                            '{reader.GetValue(4)}')";
                    command = new SQLiteCommand(sql, connection);
                    command.ExecuteNonQueryAsync();
                    UpdateRole(e, $"{reader.GetValue(3)}");
                  }
                  break;
                }
              }
            }
            else
            {
              Console.WriteLine($"Situation 6. {e.Author.Username} user not found");
              // ранг, пользователь и сервер не распознаны
            }
          }
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
    var member = e.Guild.Members[e.Author.Id];

    string[] ranks = [
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
        member.GrantRoleAsync(role);
        Console.WriteLine($"{member.DisplayName} - gives - {role.Name}");
        break;
      }
    }
  } 
}