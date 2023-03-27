using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    private static MenuController _instance;
    [SerializeField] private Button PlayButton;
    [SerializeField] private Button SettingsButton;
    [SerializeField] private Button AboutButton;
    [SerializeField] private Button SettingsBackBtn;
    [SerializeField] private Button AboutBackBtn;
    [SerializeField] private Button StatsBtn;
    [SerializeField] private Button QuitGameBtn;
    [SerializeField] private Button DeletePlayerDataBtn;
    [SerializeField] private GameObject MenuView;
    [SerializeField] private GameObject SettingsView;
    [SerializeField] private GameObject AboutView;
    [SerializeField] private GameObject StatsView;
    [SerializeField] public Slider VolumeSlider;
    [SerializeField] public Toggle MenuMusicToggle;
    [SerializeField] public AudioSource MenuAudio;
    [SerializeField] private GameObject[] Logo;
    [SerializeField] private GameObject LogoGO;

    // Stats
    [SerializeField] private TMPro.TextMeshProUGUI HighScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI TotalDistanceText;
    [SerializeField] private TMPro.TextMeshProUGUI TotalDeathsText;
    [SerializeField] private TMPro.TextMeshProUGUI PlaytimeText;
    [SerializeField] private TMPro.TextMeshProUGUI TotalPointsText;

    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        // Load Settings
        if (PlayerPrefs.GetInt("MenuMusicToggle", 1) == 0)
        {
            MenuAudio.Stop();
            MenuMusicToggle.isOn = false;
        }
        else
        {
            MenuAudio.Play();
            MenuMusicToggle.isOn = true;
        }

        MenuAudio.volume = PlayerPrefs.GetFloat("MAIN_VOLUME", 0.8f);
        VolumeSlider.value = PlayerPrefs.GetFloat("MAIN_VOLUME", 0.8f);

        // Button listeners
        SettingsButton.onClick.AddListener(delegate{ OnMainMenuButtonClick(SettingsView); });
        AboutButton.onClick.AddListener(delegate{ OnMainMenuButtonClick(AboutView); });
        SettingsBackBtn.onClick.AddListener(delegate{ OnBackButtonClick(SettingsView); });
        AboutBackBtn.onClick.AddListener(delegate{ OnBackButtonClick(AboutView); });
        StatsBtn.onClick.AddListener( OnStatsButtonClick );
        PlayButton.onClick.AddListener(delegate { GameManager.Instance.GoToScene("MainGame"); GameManager.Instance.FadeMusic(MenuAudio, true); });
        DeletePlayerDataBtn.onClick.AddListener(DeletePlayerData);
        QuitGameBtn.onClick.AddListener(Application.Quit);

        // Play button animation
        LeanTween.scale(PlayButton.gameObject, new Vector3(0.54f, 0.5f, 0.5f), 1.0f).setEase(LeanTweenType.easeOutElastic).
            setDelay(3.0f).setLoopPingPong();
    }

    void DeletePlayerData()
    {
        PlayerPrefs.DeleteKey("PLAYER_HIGHSCORE_STATS");
        PlayerPrefs.DeleteKey("PLAYER_DISTANCE_STATS");
        PlayerPrefs.DeleteKey("PLAYER_DEATHS_STATS");
        PlayerPrefs.DeleteKey("PLAYER_TOTAL_POINTS_STATS");
        LoadPlayerStats();
    }

    void LoadPlayerStats()
    {
        HighScoreText.text = "Highscore: " + PlayerPrefs.GetInt("PLAYER_HIGHSCORE_STATS", 0);
        TotalDistanceText.text = "Distance: " + string.Format("{0:0.00}", PlayerPrefs.GetFloat("PLAYER_DISTANCE_STATS", 0)) + "km";
        TotalDeathsText.text = "Deaths: " + PlayerPrefs.GetInt("PLAYER_DEATHS_STATS", 0);
        PlaytimeText.text = "Playtime: " + string.Format("{0:0}", PlayerPrefs.GetFloat("PLAYER_PLAYTIME_STATS", 0)) + "min";
        TotalPointsText.text = "Total Points: " + PlayerPrefs.GetInt("PLAYER_TOTAL_POINTS_STATS", 0);
    }

    public static MenuController Instance
    {
        get
        {
            if (_instance == null)
                Debug.Log("GameManager is null");
            return _instance;
        }
    }

    void OnMainMenuButtonClick(GameObject objView)
    {
        if (GameManager.Instance._LastPressTime + GameManager.Instance._PressDelay > Time.unscaledTime)
            return;

        LeanTween.scale(LogoGO, new Vector3(0.4f, 0.4f, 0.4f), 1.0f).setEase(LeanTweenType.easeOutElastic);
        LeanTween.moveY(LogoGO.GetComponent<RectTransform>(), -30f, 1.0f).setEase(LeanTweenType.easeOutElastic);

        if (StatsView.activeSelf)
            OnStatsButtonClick();

        GameManager.Instance._LastPressTime = Time.unscaledTime;
        objView.SetActive(true);
        LeanTween.scale(objView, new Vector3(1, 1, 1), 1.0f).setEase(LeanTweenType.easeOutElastic).setDelay(0.2f);
        LeanTween.scale(MenuView, new Vector3(0, 0, 0), 1.0f).setEase(LeanTweenType.easeOutElastic).setDelay(0.1f);
        StartCoroutine(ViewActiveToggle(MenuView));
    }

    void OnBackButtonClick(GameObject objView)
    {
        if (GameManager.Instance._LastPressTime + GameManager.Instance._PressDelay > Time.unscaledTime)
            return;

        LeanTween.scale(LogoGO.GetComponent<RectTransform>(), new Vector3(0.6f, 0.6f, 0.6f), 1.0f).setEase(LeanTweenType.easeOutElastic);
        LeanTween.moveY(LogoGO.GetComponent<RectTransform>(), -63.4f, 1.0f).setEase(LeanTweenType.easeOutElastic);

        GameManager.Instance._LastPressTime = Time.unscaledTime;
        MenuView.SetActive(true);
        LeanTween.scale(MenuView, new Vector3(1, 1, 1), 1.0f).setEase(LeanTweenType.easeOutElastic).setDelay(0.2f);
        LeanTween.scale(objView, new Vector3(0, 0, 0), 1.0f).setEase(LeanTweenType.easeOutElastic).setDelay(0.1f);
        StartCoroutine(ViewActiveToggle(objView));
    }

    void OnStatsButtonClick()
    {
        if (GameManager.Instance._LastPressTime + GameManager.Instance._PressDelay *0.75 > Time.unscaledTime)
            return;
        GameManager.Instance._LastPressTime = Time.unscaledTime;

        LoadPlayerStats();
        if (!StatsView.activeSelf)
        {
            StatsView.SetActive(true);
            LeanTween.moveX(StatsView.gameObject.GetComponent<RectTransform>(), -112f, 0.5f).setEase(LeanTweenType.easeInOutSine);
        }
        else
        {
            LeanTween.moveX(StatsView.gameObject.GetComponent<RectTransform>(), 125f, 0.5f).setEase(LeanTweenType.easeInOutBack).setOnComplete(
                delegate ()
                {
                    StatsView.SetActive(false);
                });
        }
    }

    IEnumerator ViewActiveToggle(GameObject obj)
    {
        yield return new WaitForSeconds(0.2f);
        if (obj.activeSelf == true)
            obj.SetActive(false);
        else
            obj.SetActive(true);
    }

    public void OnMenuMusicToggle()
    {
        if (!MenuMusicToggle.isOn)
        {
            MenuAudio.Pause();
            PlayerPrefs.SetInt("MenuMusicToggle", 0);
        }
        else
        {
            MenuAudio.Play();
            PlayerPrefs.SetInt("MenuMusicToggle", 1);
        }
    }

    public void ChangeMenuVolume()
    {
        AudioListener.volume = VolumeSlider.value;
        PlayerPrefs.SetFloat("MainMenuVolume", VolumeSlider.value);
    }
}
