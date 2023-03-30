using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Linq;
using System;

public class LeaderboardManager : MonoBehaviour
{ 
    private static LeaderboardManager _instance;



    void Awake()
    {
        _instance = this;
        AuthenticateWithPlayFab();
    }

    public static LeaderboardManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.Log("LeaderboardManager is null");
            return _instance;
        }
    }

    private void AuthenticateWithPlayFab()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login successful!");
        GetLeaderboard();
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("Login failed: " + error.GenerateErrorReport());
    }

    private void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = "LEVEL1_TIMES",
            StartPosition = 0,
            MaxResultsCount = 10
        };

        PlayFabClientAPI.GetLeaderboard(request, OnGetLeaderboardSuccess, OnGetLeaderboardFailure);
    }

    private void OnGetLeaderboardSuccess(GetLeaderboardResult result)
    {
        Debug.Log("Leaderboard retrieved successfully!");

        foreach (var item in result.Leaderboard)
        {
            string playerName = item.DisplayName;
            string levelTime = MillisecondsToTimeString(item.StatValue);
            string country = "IL";

            Debug.Log(playerName + " " + levelTime + " " + country);
        }
    }


    private void OnGetLeaderboardFailure(PlayFabError error)
    {
        Debug.LogError("Failed to retrieve leaderboard: " + error.GenerateErrorReport());
    }

    void OnError(PlayFabError error)
    {
        Debug.Log("Error while logging/creating account.");
        Debug.Log(error.GenerateErrorReport());
    }

    public void SendLeaderboard(string time)
    {
        int score = TimeStringToMilliseconds(time);
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate{
                    StatisticName = "LEVEL1_TIMES",
                    Value = score
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnError);
    }

    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Sucessfull leaderboard sent.");
    }


    public int TimeStringToMilliseconds(string timeString)
    {
        string[] parts = timeString.Split(':');
        int minutes = int.Parse(parts[0]);
        int seconds = int.Parse(parts[1]);
        int milliseconds = int.Parse(parts[2]);

        int totalMilliseconds = (minutes * 60 * 1000) + (seconds * 1000) + milliseconds;
        return totalMilliseconds;
    }


    public string MillisecondsToTimeString(int milliseconds)
    {
        TimeSpan time = TimeSpan.FromMilliseconds(milliseconds);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", time.Minutes, time.Seconds, time.Milliseconds / 10);
    }


    public void UpdateLeaderboardUI()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = "LEVEL1_TIMES",
            StartPosition = 0,
            MaxResultsCount = 10
        };

        PlayFabClientAPI.GetLeaderboard(request, result =>
        {
            // Clear the leaderboard UI
            GameLogic.Instance.T_LeaderboardEntry.text = "";

            // Loop through the leaderboard data and add it to the UI
            foreach (var item in result.Leaderboard)
            {
                string playerName = item.DisplayName;
                int playerScore = (int)item.StatValue;
                string country = "US"; // Replace with code to get player country

                // Convert the score to the desired format, such as minutes and seconds
                string scoreString = FormatScore(playerScore);

                int rank = 1;
                // Add the player data to the leaderboard UI
                GameLogic.Instance.T_LeaderboardEntry.text += string.Format("{0}. {1} ({2}) - {3}\n", rank++, playerName, scoreString, country);
            }
        }, error =>
        {
            Debug.LogError("Failed to retrieve leaderboard: " + error.GenerateErrorReport());
        });
    }

    // Helper function to format the score as minutes and seconds
    private string FormatScore(int score)
    {
        TimeSpan time = TimeSpan.FromMilliseconds(score);
        return string.Format("{0:D2}:{1:D2}:{2:D3}", time.Minutes, time.Seconds, time.Milliseconds);
    }


    //  PLAYFAB: INT TIME IN SECONDS
    //  Convert player time string to milliseconds (int)
    //  Push to LB
    // When getting from LB to display to UI, convert ms to String format mm:ss:ms


}
