using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class FirebaseLeaderboard
{
    private static readonly string DatabaseUrl = "https://snake-board-default-rtdb.firebaseio.com/leaderboard.json";
    private static readonly HttpClient client = new HttpClient();

    public static async Task AddOrUpdateScoreAsync(LeaderboardEntry entry)
    {
        string userEntryUrl = $"https://snake-board-default-rtdb.firebaseio.com/leaderboard/{entry.UserId}.json";

        HttpResponseMessage getResponse = await client.GetAsync(userEntryUrl);
        string existingJson = await getResponse.Content.ReadAsStringAsync();

        if (getResponse.IsSuccessStatusCode && existingJson != "null")
        {
            var existingEntry = JsonConvert.DeserializeObject<LeaderboardEntry>(existingJson);

            if (entry.Score < existingEntry.Score)
            {
                return;
            }
        }

        string json = JsonConvert.SerializeObject(entry);
        HttpContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PutAsync(userEntryUrl, content);
        response.EnsureSuccessStatusCode();
    }



    public static async Task<List<LeaderboardEntry>> GetLeaderboardAsync()
    {
        HttpResponseMessage response = await client.GetAsync(DatabaseUrl);
        string jsonResponse = await response.Content.ReadAsStringAsync();

        if (jsonResponse == "null")
            return new List<LeaderboardEntry>();

        var scores = JsonConvert.DeserializeObject<Dictionary<string, LeaderboardEntry>>(jsonResponse);
        return new List<LeaderboardEntry>(scores.Values);
    }
}

public class LeaderboardEntry
{
    public string UserId { get; set; }
    public string Username { get; set; }
    public int Score { get; set; }

    public LeaderboardEntry() { }

    public LeaderboardEntry(string userId, string username, int score)
    {
        UserId = userId;
        Username = username;
        Score = score;
    }
}