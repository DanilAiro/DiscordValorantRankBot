using ValorantNET;
using static ValorantNET.Enums;

class Valorant
{
  private static readonly string api_key = Environment.GetEnvironmentVariable("api_key");
  private static string username = "danilairo";
  private static string tag = "000";
  private static bool isFirst = true;

  private static ValorantClient? valorantClient = new(username, tag, Regions.EU);

  public static string GetRankByInfo(string region, string username, string tag)
  {
    string longStr = GetStringRank(region.ToLower(), username.ToLower(), tag.ToLower());
    
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

  public static string GetRankByPUUID(string puuid)
  {
    string longStr = valorantClient.GetMMRByPUUIDAsync(puuid).Result.ToString();

    int startIndex = longStr.IndexOf("currenttierpatched");
    int length = 0;

    if (startIndex == -1)
    {
      return String.Empty;
    }

    foreach (var item in longStr.Substring(startIndex + 22))
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
    return longStr.Substring(startIndex + 22, length);
  }

  public static string GetPUUID(string region, string username, string tag)
  {
    string longStr = GetStringRank(region.ToLower(), username.ToLower(), tag.ToLower());
    
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

  private static string GetStringRank(string region, string username, string tag)
  {
    Task<string>? dirtyString = GetDirtyStringRank(ParseRegion(region), username, tag);

    if (dirtyString != null)
    {
      return dirtyString.Result.ToString();
    }
    else
    {
      return String.Empty;
    }
  }

  private static async Task<string> GetDirtyStringRank(Regions region, string username, string tag)
  {
    try
    {
      using HttpClient client = new HttpClient();
      client.BaseAddress = new Uri("https://api.henrikdev.xyz");
      client.DefaultRequestHeaders.Add("Authorization", api_key);
      return await client.GetStringAsync($"/valorant/v2/mmr/{region}/{username}/{tag}");
    }
    catch (Exception)
    {
      //Console.WriteLine($"{region} - {Valorant.username}#{Valorant.tag} - not found - {DateTime.Now.ToString("h:mm:ss tt")}");
      return String.Empty;
    }
  }

  private static Regions ParseRegion(string region)
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