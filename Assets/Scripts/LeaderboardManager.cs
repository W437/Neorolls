using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using PlayFab.EconomyModels;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using System;
using Unity.Mathematics;
using static UnityEditor.FilePathAttribute;
using static UnityEngine.Networking.UnityWebRequest;
using PlayFab.CloudScriptModels;
using System.IO;
using System.Linq;

public class LeaderboardManager : MonoBehaviour
{ 
    private static LeaderboardManager _instance;
    [SerializeField] private GameObject LB_EntryBar;
    [SerializeField] private Transform LB_EntryParent;
    [SerializeField] private Button LB_RefreshBtn;
    [SerializeField] public GameObject PlayerNameInputWindow;
    [SerializeField] private TMPro.TextMeshProUGUI PlayerNameInput;
    [SerializeField] private int MaxPlayerNameLength = 16;
    [SerializeField] private Color32[] LBTop5Colors;
    private static string PlayerDisplayName;
    private List<Sprite> flagSprites;
    public GameObject loadingAnimation;

    // Leaderboard names
    public static string LB_Level1 = "LEVEL1_TIMES";
    public static string LB_Level1_Points = "LEVEL1_POINTS";

    void Awake()
    {
        _instance = this;
        PlayFabLogin(SystemInfo.deviceUniqueIdentifier);
        DontDestroyOnLoad(gameObject);
    }


    private void Start()
    {
        PlayerPrefs.GetString("PLAYER_PLAYFAB_NAME", PlayerDisplayName);
        Debug.Log("DisplayName: " + PlayerDisplayName);
        LB_RefreshBtn.onClick.AddListener(OnRefreshBtnClick);
        LoadFlags();

    }


    public static LeaderboardManager Instance
    {
        get
        {
            if(_instance == null)
                Debug.Log("LeaderboardManager is null");
            return _instance;
        }
    }

    private void PlayFabLogin(string id)
    {

        var request = new LoginWithCustomIDRequest
        {
            CustomId = id,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    void GetPlayerProfile(string playFabId, Action<string> callback)
    {
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest()
        {
            PlayFabId = playFabId,
            ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowLocations = true
            }
        },
        result => {
            var countryCode = result.PlayerProfile.Locations[result.PlayerProfile.Locations.Count - 1].CountryCode.ToString();
            callback(countryCode);
        },
        error => Debug.LogError(error.GenerateErrorReport()));
    }

    public void ResetPlayFabLoginID()
    {
        string newId = "";
        for (int i = 0; i < 10; i++)
            newId += (char)UnityEngine.Random.Range(97, 122);
        Debug.Log(newId);
        PlayFabLogin(newId);
    }


    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login successful!");
        string name = null;
        if (result.InfoResultPayload.PlayerProfile != null)
        {
            name = PlayerPrefs.GetString("PLAYER_PLAYFAB_NAME", PlayerDisplayName);
            Debug.Log("name: " + name);
            PlayerDisplayName = PlayerPrefs.GetString("PLAYER_PLAYFAB_NAME", PlayerDisplayName);
            Debug.Log("display name: " + PlayerDisplayName);
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


    public void SendLeaderboardTime(string time, string lb_name)
    {
        int score = TimeStringToMilliseconds(time);
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate{
                    StatisticName = lb_name,
                    Value = score
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnError);
    }

    public void SendLeaderboardPoints(int points, string lb_name)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate{
                    StatisticName = lb_name,
                    Value = points
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnError);
    }

    // 255, 159, 0, 111
    // 200, 125
    // 145. 90
    // 75, 47
    // 32, 20

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

        var request2 = new GetLeaderboardRequest
        {
            StatisticName = "LEVEL1_POINTS",
            StartPosition = 0,
            MaxResultsCount = 100
        };

        loadingAnimation.SetActive(true);

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

                var entryHighlight = lbEntryBarObject.GetComponent<Image>();
                // Top 5 entries color grading
                switch (rank)
                {
                    case 1:
                        entryHighlight.color = LBTop5Colors[rank-1];
                        break;
                    case 2:
                        entryHighlight.color = LBTop5Colors[rank - 1];
                        break;
                    case 3:
                        entryHighlight.color = LBTop5Colors[rank - 1];
                        break;
                    case 4:
                        entryHighlight.color = LBTop5Colors[rank - 1];
                        break;
                    case 5:
                        entryHighlight.color = LBTop5Colors[rank - 1];
                        break;
                    default:
                        break;
                }

                // highlight current player entry
                if (item.DisplayName == PlayerPrefs.GetString("PLAYER_PLAYFAB_NAME", PlayerDisplayName))
                {
                    Debug.Log("inside display: " + PlayerDisplayName);
                    entryHighlight.color = new Color32(180, 180, 180, 130);
                }

                // Convert the score to the desired format 
                string scoreString = FormatScore(playerScore);

                // iterate over prefab text separately
                int childIndex = 0;
                foreach (TMPro.TextMeshProUGUI child in childTexts)
                {
                    int points = 0;
                    switch (childIndex)
                    {
                        // Rank & name
                        case 0:
                            child.text = string.Format("<color=#fff000>{0}.</color> <size=100%>{1}</size><size=80%>{2}</size>", rank, playerName.Substring(0, 1), playerName.Substring(1));
                            break;

                        // Time & POINTS
                        case 1:
                            // get points lb val for player by id
                            GetPlayerLeaderboardData(item.PlayFabId, LB_Level1_Points, result =>
                            {
                                points = result;
                                child.text = string.Format("<color=#ff0>(<color=#fff>{0}</color>:<color=#fff>{1}</color>:<color=#fff>{2}</color>)</color> <color=#fff000> <size=80%>{3}<color=#fff>", scoreString.Substring(0, 2), scoreString.Substring(3, 2), scoreString.Substring(6), points);
                            });

                            break;

                        case 2:
                            GetPlayerProfile(item.PlayFabId, countryCode => {

                                Sprite flagSprite = flagSprites.Find(s => s.name == countryCode + "@2x");

                                if (flagSprite != null)
                                {
                                    var flagObject = lbEntryBarObject.transform.Find("Country/FlagImg");
                                    var prefabFlagImg = flagObject.GetComponent<RawImage>();
                                    prefabFlagImg.texture = flagSprite.texture;
                                }

                                else
                                {
                                    Debug.LogError($"Failed to find flag sprite for country code: {countryCode}");
                                }
                                child.text = countryCode; 
                            });

                            break;

                        default:
                            break;
                    }
                    childIndex++;
                }
                rank++;
            }
            loadingAnimation.SetActive(false);
        }, error => 
        {
            Debug.LogError("Failed to retrieve leaderboard: " + error.GenerateErrorReport());
        });

    }


    public void GetPlayerLeaderboardData(string playerId, string lbName, Action<int> callback)
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = lbName,
            PlayFabId = playerId,
            MaxResultsCount = 1
        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, result =>
        {
            var playerData = result.Leaderboard.FirstOrDefault();
            if (playerData != null)
            {
                int playerScore = (int)playerData.StatValue;
                callback(playerScore);
            }
            else
            {
                Debug.Log("Player not found in leaderboard.");
            }
        }, error =>
        {
            Debug.LogError("Failed to get player leaderboard data: " + error.GenerateErrorReport());
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
        PlayerDisplayName = result.DisplayName;
        PlayerPrefs.SetString("PLAYER_PLAYFAB_NAME", PlayerDisplayName);
        Debug.Log("changed display: " + PlayerDisplayName);
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

    void LoadFlags()
    {
        flagSprites = new List<Sprite>();

        // Load all of the flag sprites in the "Flags" folder
        Sprite[] sprites = Resources.LoadAll<Sprite>(@"Flags" + System.IO.Path.AltDirectorySeparatorChar);

        // Add the flag sprites to the list
        flagSprites.AddRange(sprites);
    }

    //  PLAYFAB: INT TIME IN SECONDS
    //  Convert player time string to milliseconds (int)
    //  Push to LB
    // When getting from LB to display to UI, convert ms to String format mm:ss:ms


}
