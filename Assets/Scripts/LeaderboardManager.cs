using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using PlayFab.EconomyModels;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using System;

public class LeaderboardManager : MonoBehaviour
{ 
    private static LeaderboardManager _instance;
    [SerializeField] private GameObject LB_EntryBar;
    [SerializeField] private Transform LB_EntryParent;
    [SerializeField] private Button LB_RefreshBtn;
    [SerializeField] public GameObject PlayerNameInputWindow;
    [SerializeField] private TMPro.TextMeshProUGUI PlayerNameInput;
    [SerializeField] private int MaxPlayerNameLength = 18;
    private string PlayerDisplayName;

    void Awake()
    {
        _instance = this;
        PlayFabLogin();
        DontDestroyOnLoad(gameObject);
        //LB_EntryText = LB_EntryBar.GetComponentInChildren<TMPro.TextMeshProUGUI>();
    }

    private void Start()
    {
        LB_RefreshBtn.onClick.AddListener(OnRefreshBtnClick);
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

    private void PlayFabLogin()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            } 
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }


    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login successful!");
        string name = null;
        if (result.InfoResultPayload.PlayerProfile != null)
        {
            name = result.InfoResultPayload.PlayerProfile.DisplayName;
            PlayerDisplayName = name;
        }

        if (name == null)
            PlayerNameInputWindow.SetActive(true);
    }


    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("Login failed: " + error.GenerateErrorReport());
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

    void OnRefreshBtnClick()
    {
        if (GameManager.Instance._LastPressTime + GameManager.Instance._PressDelay + 2 > Time.unscaledTime)
            return;
        else
            UpdateLeaderboardUI();
        GameManager.Instance._LastPressTime = Time.unscaledTime;
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
                TMPro.TextMeshProUGUI[] childTexts = lbEntryBarObject.GetComponentsInChildren<TMPro.TextMeshProUGUI>();

                string playerName = TruncateString(item.DisplayName);
                int playerScore = (int)item.StatValue;
                string country = "IL"; // Automate

                // highlight current player entry
                if (item.DisplayName == PlayerDisplayName)
                {
                    var entryHighlight = lbEntryBarObject.GetComponent<Image>();
                    entryHighlight.color = new Color32(180, 180, 180, 130);
                }

                // Convert the score to the desired format 
                string scoreString = FormatScore(playerScore);

                // iterate over prefab text separately
                int i = 0;
                foreach (TMPro.TextMeshProUGUI child in childTexts)
                {
                    switch (i)
                    {
                        case 0:
                            child.text = string.Format("<color=#fff000>{0}.</color> <size=100%>{1}</size><size=80%>{2}</size>", rank, playerName.Substring(0, 1), playerName.Substring(1));
                            break;

                        case 1:
                            child.text = string.Format("<color=#ff0>(<color=#fff>{0}</color>:<color=#fff>{1}</color>:<color=#fff>{2}</color>)</color>", scoreString.Substring(0, 2), scoreString.Substring(3, 2), scoreString.Substring(6));
                            break;

                        case 2:
                            child.text = country;
                            break;

                        default:
                            break;
                    }
                    i++;
                }
                rank++;
            }
        }, error =>
        {
            Debug.LogError("Failed to retrieve leaderboard: " + error.GenerateErrorReport());
        });
    }


    public void ResetLeaderboardEntries()
    {
        if (GameManager.Instance._LastPressTime + GameManager.Instance._PressDelay > Time.unscaledTime)
            return;
        GameManager.Instance._LastPressTime = Time.unscaledTime;

        // Destroy all instantiated LB_EntryBar objects
        foreach (Transform child in LB_EntryParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void OnPlayerSubmitName()
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = PlayerNameInput.text,
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameUpdate, OnError);
    }

    void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("Updated player display name.");
        PlayerDisplayName = result.DisplayName;
        PlayerNameInputWindow.SetActive(false);
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
        if (str == null) { str = "null"; }
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
