using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Linq;
using System;
using PlayFab.EconomyModels;
using UnityEngine.SocialPlatforms.Impl;
using System.Drawing;

public class LeaderboardManager : MonoBehaviour
{ 
    private static LeaderboardManager _instance;
    [SerializeField] private GameObject LB_EntryBar;
    [SerializeField] private Transform LB_EntryParent;

    [SerializeField] private int MaxPlayerNameLength = 11;
    void Awake()
    {
        _instance = this;
        AuthenticateWithPlayFab();
        //LB_EntryText = LB_EntryBar.GetComponentInChildren<TMPro.TextMeshProUGUI>();
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


    public void UpdateLeaderboardUI()
    {
        ResetLeaderboardEntries();
        var request = new GetLeaderboardRequest
        {
            StatisticName = "LEVEL1_TIMES",
            StartPosition = 0,
            MaxResultsCount = 100
        };

        PlayFabClientAPI.GetLeaderboard(request, result =>
        {
            // Ascending to Descending order (because PlayFab doesn't provide order setting)
            result.Leaderboard.Reverse();

            int rank = 1;

            // Loop through the leaderboard data and add it to the UI
            foreach (var item in result.Leaderboard)
            {
                var lbEntryBarObject = Instantiate(LB_EntryBar, LB_EntryParent);

                string playerName = TruncateString(item.DisplayName);
                int playerScore = (int)item.StatValue;
                string country = "US"; // Automate

                // Convert the score to the desired format 
                string scoreString = FormatScore(playerScore);

                var lbEntryText = lbEntryBarObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                lbEntryText.text = string.Format("<color=#fff000>{0}.</color> {1} (<color=#fff123>{2}</color>) - {3}", rank, playerName, scoreString, country);

                rank++;
            }
        }, error =>
        {
            Debug.LogError("Failed to retrieve leaderboard: " + error.GenerateErrorReport());
        });
    }


    public void ResetLeaderboardEntries()
    {
        // Destroy all instantiated LB_EntryBar objects
        foreach (Transform child in LB_EntryParent.transform)
        {
            Destroy(child.gameObject);
        }
    }



    private string FormatScore(int score)
    {
        TimeSpan time = TimeSpan.FromMilliseconds(score);
        return string.Format("{0:D2}:{1:D2}:{2:D3}", time.Minutes, time.Seconds, time.Milliseconds);
    }


    private string GetFlagIconUrl(string countryCode)
    {
        return "https://flagicons.lipis.dev/flags/4x3/" + countryCode.ToLower() + "/.svg";
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


    private string TruncateString(string str)
    {
        if (str.Length > MaxPlayerNameLength)
            return str.Substring(0, MaxPlayerNameLength - 2) + "..";
        else
            return str;
    }


    //  PLAYFAB: INT TIME IN SECONDS
    //  Convert player time string to milliseconds (int)
    //  Push to LB
    // When getting from LB to display to UI, convert ms to String format mm:ss:ms


}
