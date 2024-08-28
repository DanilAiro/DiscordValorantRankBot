using System.Data.SQLite;

string filePath = "/home/danila/autostart/data.sqlite";

if (!File.Exists(filePath))
{
  SQLiteConnection.CreateFile(filePath);

  string connectionString = $"Data Source={filePath};Version=3;";
  SQLiteConnection connection = new(connectionString);
  connection.Open();

  string sql = @"CREATE TABLE ranks (
                discord_server_id INTEGER NOT NULL, 
                discord_user_id INTEGER NOT NULL, 
                valorant_puuid TEXT NOT NULL, 
                valorant_region TEXT NOT NULL, 
                valorant_rank TEXT NOT NULL, 
                valorant_user TEXT NOT NULL, 
                valorant_tag TEXT NOT NULL, 
                discord_message TEXT NOT NULL,
                UNIQUE (discord_server_id, discord_user_id, valorant_puuid))";

  var command = new SQLiteCommand(sql, connection);
  command.ExecuteNonQuery();
}

await Discord.WaitMessage();

