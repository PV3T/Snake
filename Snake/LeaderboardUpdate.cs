using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

public class GetFirebaseLeaderboard
{
    private static readonly string FirebaseUrl = "https://snake-board-default-rtdb.firebaseio.com/leaderboard.json";

    public static async Task<List<LeaderboardEntry>> GetLeaderboard()
    {
        using HttpClient client = new();
        var response = await client.GetStringAsync(FirebaseUrl);

        if (!string.IsNullOrEmpty(response))
        {
            var scores = JsonConvert.DeserializeObject<Dictionary<string, LeaderboardEntry>>(response);
            return new List<LeaderboardEntry>(scores.Values);
        }

        return new List<LeaderboardEntry>();
    }
}
