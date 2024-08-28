using static ValorantNET.Enums;

class Valorant
{
  public static string GetRankByInfo(string longStr)
  {
    int startIndex = longStr.IndexOf("currenttierpatched");
    int length = 0;

    if (startIndex == -1)
    {
      return String.Empty;
    }

    foreach (var item in longStr.Substring(startIndex + 21))
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
      return String.Empty;
    }

    foreach (var item in longStr.Substring(startIndex + 8))
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

  // public static string GetRankByInfo(string region, string username, string tag)
  // {
  //   string longStr = GetStringRank(GetDirtyStringRank(ParseRegion(region.ToLower()), username.ToLower(), tag.ToLower()));
    
  //   int startIndex = longStr.IndexOf("currenttierpatched");
  //   int length = 0;

  //   if (startIndex == -1)
  //   {
  //     return String.Empty;
  //   }

  //   foreach (var item in longStr.Substring(startIndex + 21))
  //   {
  //     if (item != '\"')
  //     {
  //       length++;
  //     }
  //     else
  //     {
  //       break;
  //     }
  //   }
  //   return longStr.Substring(startIndex + 21, length);
  // }

  // public static string GetPUUID(string region, string username, string tag)
  // {
  //   string longStr = GetStringRank(GetDirtyStringRank(ParseRegion(region.ToLower()), username.ToLower(), tag.ToLower()));

  //   int startIndex = longStr.IndexOf("puuid");
  //   int length = 0;

  //   if (startIndex == -1)
  //   {
  //     return String.Empty;
  //   }

  //   foreach (var item in longStr.Substring(startIndex + 8))
  //   {
  //     if (item != '\"')
  //     {
  //       length++;
  //     }
  //     else
  //     {
  //       break;
  //     }
  //   }
  //   return longStr.Substring(startIndex + 8, length);
  // }

  public static string GetStringRank(Task<string>? dirtyString)
  {
    if (dirtyString != null)
    {
      return dirtyString.Result.ToString();
    }
    else
    {
      return String.Empty;
    }
  }

  public static async Task<string> GetDirtyStringRank(Regions region, string username, string tag)
  {
    try
    {
      using HttpClient client = new HttpClient();
      client.BaseAddress = new Uri("https://api.henrikdev.xyz");
      client.DefaultRequestHeaders.Add("Authorization", Secret.apiKey);
      return await client.GetStringAsync($"/valorant/v2/mmr/{region}/{username}/{tag}");
    }
    catch (Exception)
    {
      return String.Empty;
    }
  }

  public static async Task<string> GetDirtyStringRankByPUUID(string region, string puuid)
  {
    try
    {
        using HttpClient client = new HttpClient();
        client.BaseAddress = new Uri("https://api.henrikdev.xyz");
        client.DefaultRequestHeaders.Add("Authorization", Secret.apiKey);
        return await client.GetStringAsync($"/valorant/v1/by-puuid/mmr/{region}/{puuid}");
    }
    catch (Exception)
    {
        return String.Empty;
    }
  }
  
  public static Regions ParseRegion(string region)
  {
    switch (region)
    {
      case "eu":
        return Regions.EU;

      case "na":
        return Regions.NA;
      
      case "kr":
        return Regions.KR;
        
      case "ap":
        return Regions.AP;
        
      default:
        return Regions.EU;
    }
  }
}