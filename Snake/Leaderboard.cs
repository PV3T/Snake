using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class FirebaseLeaderboard
{
    private static readonly string DatabaseUrl = "https://snake-board-default-rtdb.firebaseio.com/leaderboard.json";
    private static readonly HttpClient client = new HttpClient();

    // Add a new score to the leaderboard
    public static async Task AddOrUpdateScoreAsync(LeaderboardEntry entry)
    {
        // Use UserId as the unique key
        string userEntryUrl = $"https://snake-board-default-rtdb.firebaseio.com/leaderboard/{entry.UserId}.json";

        // Check if the user already has a score
        HttpResponseMessage getResponse = await client.GetAsync(userEntryUrl);
        string existingJson = await getResponse.Content.ReadAsStringAsync();

        // If the entry exists, deserialize the existing score and compare
        if (getResponse.IsSuccessStatusCode && existingJson != "null")
        {
            var existingEntry = JsonConvert.DeserializeObject<LeaderboardEntry>(existingJson);

            // If the new score is higher than the existing score, update it
            if (entry.Score < existingEntry.Score)
            {
                return; // Don't update if the new score is not higher
            }
        }

        // Convert the new entry to JSON
        string json = JsonConvert.SerializeObject(entry);
        HttpContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Use PUT to update the score (or create if it doesn't exist)
        HttpResponseMessage response = await client.PutAsync(userEntryUrl, content);
        response.EnsureSuccessStatusCode();
    }



    // Get leaderboard scores
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
    public string UserId { get; set; }  // Unique UserId
    public string Username { get; set; } // Username to display
    public int Score { get; set; }

    public LeaderboardEntry() { }

    public LeaderboardEntry(string userId, string username, int score)
    {
        UserId = userId;
        Username = username;
        Score = score;
    }
}