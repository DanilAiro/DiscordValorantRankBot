class Valorant
{
  private static readonly string endpoint = "https://api.henrikdev.xyz";

  public static string GetRankByInfo(string longStr)
  {
    int startIndex = longStr.IndexOf("currenttierpatched");
    int length = 0;

    if (startIndex == -1)
    {
      return string.Empty;
    }

    foreach (var item in longStr[(startIndex + 21)..])
    {
      if (item != '\"')
      {
        length++;
      }
      else
      {
        break;
      }
    }
    return longStr.Substring(startIndex + 21, length);
  }

  public static string GetPUUID(string longStr)
  {
    int startIndex = longStr.IndexOf("puuid");
    int length = 0;

    if (startIndex == -1)
    {
      return string.Empty;
    }

    foreach (var item in longStr[(startIndex + 8)..])
    {
      if (item != '\"')
      {
        length++;
      }
      else
      {
        break;
      }
    }
    return longStr.Substring(startIndex + 8, length);
  }

  public static string GetStringRank(Task<string>? dirtyString)
  {
    return dirtyString != null ? dirtyString.Result.ToString() : string.Empty;
  }

  public static async Task<string> GetDirtyStringRank(string region, string username, string tag)
  {
    try
    {
      using HttpClient client = new();
      client.BaseAddress = new Uri(endpoint);
      client.DefaultRequestHeaders.Add("Authorization", Secret.apiKey);
      return await client.GetStringAsync($"/valorant/v2/mmr/{region}/{username}/{tag}");
    }
    catch (Exception e)
    {
      Console.WriteLine(DateTime.Now.ToString("h:mm:ss tt") + " - " + e.Message);
      return string.Empty;
    }
  }

  public static async Task<string> GetDirtyStringRankByPUUID(string region, string puuid)
  {
    try
    {
        using HttpClient client = new();
        client.BaseAddress = new Uri(endpoint);
        client.DefaultRequestHeaders.Add("Authorization", Secret.apiKey);
        return await client.GetStringAsync($"/valorant/v1/by-puuid/mmr/{region}/{puuid}");
    }
    catch (Exception e)
    {
      Console.WriteLine(DateTime.Now.ToString("h:mm:ss tt") + " - " + e.Message);
      return string.Empty;
    }
  }
}