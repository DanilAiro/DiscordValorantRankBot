using ValorantNET;
using static ValorantNET.Enums;


class Valorant
{
  public static string GetRank(string region, string username, string tag)
  {
    string longStr = GetStringRank(region.ToLower(), username.ToLower(), tag.ToLower());
    int startIndex = longStr.IndexOf("currenttierpatched");
    int length = 0;

    if (startIndex == -1)
    {
      return "";
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

  private static string GetStringRank(string region, string username, string tag)
  {
    Task<string>? dirtyString = GetDirtyStringRank(ParseRegion(region), username, tag);

    if (dirtyString != null)
    {
      return dirtyString.Result.ToString();
    }
    else
    {
      return "";
    }
  }

  private static async Task<string> GetDirtyStringRank(Regions region, string username, string tag)
  {
    try
    {
      using HttpClient client = new HttpClient();
      client.BaseAddress = new Uri("https://api.henrikdev.xyz");
      return await client.GetStringAsync($"/valorant/v1/mmr/{region}/{username}/{tag}");
    }
    catch (Exception)
    {
      Console.WriteLine($"{region} - {username}#{tag} - not found - {DateTime.Now.ToString("h:mm:ss tt")}");
      return "";
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