using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class GameLogic : MonoBehaviour
{
    private static GameLogic _instance;

    // Player
    [Header("")]
    [Header("=> Player")]
    [Header("")]
    [SerializeField] public GameObject PlayerBall;
    [SerializeField] public Rigidbody PlayerBallRB;
    [SerializeField] private float MovePower;
    [SerializeField] private float MaxVelocity;
    [SerializeField] private float JumpPower;
    [SerializeField] private float _distToGround = 2f;
    [SerializeField] public bool JumpEnabled { get; set; } = false;
    private float _lastClickTime;
    private float _catchTime = 0.25f; // Double tap for jump catch time
    private Vector3 _prevMousePosition;
    private Vector3 _prevPlayerVelocity;
    private bool _ghostMode = false;
    private bool _cheatMode = false;

    // Game HUD
    [Header("")]
    [Header("=> Game HUD")]
    [Header("")]
    [SerializeField] private GameObject[] UIElements;
    [SerializeField] private GameObject[] PausedPanel;
    [SerializeField] private GameObject PausedPanelParent;
    [SerializeField] private Button PauseBtn;
    [SerializeField] private Button[] PausedBtns;
    [SerializeField] public TMPro.TextMeshProUGUI T_Timer;
    [SerializeField] private TMPro.TextMeshProUGUI T_Pills;
    [SerializeField] private TMPro.TextMeshProUGUI T_Speed;
    [SerializeField] private TMPro.TextMeshProUGUI T_Faults;

    // Game over elements
    [Header("")]
    [Header("=> Game Over Elements")]
    [Header("")]
    [SerializeField] private GameObject GameOverPanelParent;
    [SerializeField] private GameObject GameOverPanel;
    [SerializeField] private GameObject GameOverPanelStatsHolder;
    [SerializeField] private GameObject[] GameOverStars;
    [SerializeField] private GameObject GameOverHighScoreTrophy;
    [SerializeField] private GameObject GameOverButtonsParent;
    [SerializeField] private Button[] GameOverButtons;
    [SerializeField] private GameObject GameOverLogo;
    [SerializeField] private GameObject GameOverHighScore;
    [SerializeField] private GameObject GameOverScore;
    [SerializeField] private GameObject GameOverSpeed;
    [SerializeField] private GameObject GameOverTime;
    [SerializeField] private TMPro.TextMeshProUGUI T_GameOverHighscore;
    [SerializeField] private TMPro.TextMeshProUGUI T_GameOverScore;
    [SerializeField] private TMPro.TextMeshProUGUI T_GameOverSpeed;
    [SerializeField] private TMPro.TextMeshProUGUI T_GameOverTime;

    // Game finish elements
    [Header("")]
    [Header("=> Game Finish Elements")]
    [Header("")]
    [SerializeField] private GameObject WinPanelParent;
    [SerializeField] private GameObject WinPanel;
    [SerializeField] private GameObject WinPanelStatsHolder;
    [SerializeField] private GameObject[] WinStars;
    [SerializeField] private GameObject WinHighScoreTrophy;
    [SerializeField] private GameObject WinButtonsParent;
    [SerializeField] private Button[] WinButtons;
    [SerializeField] private GameObject WinLogo;
    [SerializeField] private GameObject WinHighScore;
    [SerializeField] private GameObject WinScore;
    [SerializeField] private GameObject WinSpeed;
    [SerializeField] private GameObject WinTime;
    [SerializeField] private TMPro.TextMeshProUGUI T_WinHighScore;
    [SerializeField] private TMPro.TextMeshProUGUI T_WinScore;
    [SerializeField] private TMPro.TextMeshProUGUI T_WinSpeed;
    [SerializeField] private TMPro.TextMeshProUGUI T_WinTime;

    // Misc
    [Header("")]
    [Header("=> Music & Others")]
    [Header("")]
    [SerializeField] public AudioSource GameMusic;
    [SerializeField] private Light PlatformFloorLight;
    [SerializeField] public TMPro.TextMeshProUGUI T_LeaderboardEntry;
    [SerializeField] public GameObject LeaderboardPanel;

    // Player stats
    public int PlayerPillsStats { get; set; }
    public int PlayerHighscoreStats { get; set; }
    public float PlayerDistanceStats { get; set; }
    public float PlayerPlaytimeStats { get; set; }
    public int PlayerDeathStats { get; set; }
    public int PlayerFaults;

    // Game states
    public bool GameIsPaused { get; set; } = false;
    public bool GameIsOver { get; set; } = false;
    public bool GameIsFinished { get; set; } = false;

    // Timer
    private int TimerMS;
    private float TimerSeconds;
    private int TimerMinutes;
    private float _statsMinutes;
    private float _timer;
    private bool _timerOn = true;


    // make it a checkpoints game
    // make importance for time
    // fault adds seconds.
    // checkpoints waste time
    // make leaderboard for faults, time, and coins earned
    // just like redlynx
    // make achievements
    // harder levels
    // checkpoints have Music time data, fades it in nicely, player resumes
    // faults UI element
    // all in nice leaderboard
    // online leaderboard w all players countries etc.
    // make it feel alive
    // the 3D cube dash
    // but with much better graphics and levels
    // get mtasa mappers to do level design


    public static GameLogic Instance
    {
        get
        {
            if (_instance == null) 
                Debug.Log("GameManager is null");
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {

        LeaderboardManager.Instance.SendLeaderboard("01:42:33");
        // Setting initial UI elements positions before tweening

        // Game HUD
        UIElements[0].transform.localPosition = new Vector3(366f, UIElements[0].transform.localPosition.y, UIElements[0].transform.localPosition.z);
        UIElements[1].transform.localPosition = new Vector3(361.8f, UIElements[1].transform.localPosition.y, UIElements[1].transform.localPosition.z);
        UIElements[2].transform.localPosition = new Vector3(364.5f, UIElements[2].transform.localPosition.y, UIElements[2].transform.localPosition.z);
        PauseBtn.transform.position = new Vector3(-106.75f, PauseBtn.transform.position.y, PauseBtn.transform.position.z);

        // Pause panel
        PausedPanel[0].transform.localPosition = new Vector3(908.3f, PausedPanel[0].transform.localPosition.y, PausedPanel[0].transform.localPosition.z);
        PausedPanel[1].transform.localScale = new Vector3(0,0,0);
        PausedPanel[2].transform.localScale = new Vector3(0, 0, 0);


        // Game over panel
        GameOverLogo.transform.localScale = new Vector3(0, 0, 0);
        GameOverPanelStatsHolder.transform.localScale = new Vector3(0, 0, 0);
        GameOverButtonsParent.transform.localScale = new Vector3(0, 0, 0);
        GameOverHighScore.transform.localScale = new Vector3(0, 0, 0);
        GameOverScore.transform.localScale = new Vector3(0, 0, 0);
        GameOverSpeed.transform.localScale = new Vector3(0, 0, 0);
        GameOverTime.transform.localScale = new Vector3(0, 0, 0);

        // Game win panel
        WinLogo.transform.localScale = new Vector3(0, 0, 0);
        WinPanelStatsHolder.transform.localScale = new Vector3(0, 0, 0);
        WinButtonsParent.transform.localScale = new Vector3(0, 0, 0);
        WinHighScore.transform.localScale = new Vector3(0, 0, 0);
        WinScore.transform.localScale = new Vector3(0, 0, 0);
        WinSpeed.transform.localScale = new Vector3(0, 0, 0);
        WinTime.transform.localScale = new Vector3(0, 0, 0);


        // Fade In Music
        GameMusic.volume = 0;
        GameManager.Instance.FadeMusic(GameMusic, false);

        // To do -----
        // Show how to play animation
        // Quick 3s show controls (mobile / keyboard)

        PlayerBallRB.maxAngularVelocity = MaxVelocity;

        // Load saved player stats
        PlayerHighscoreStats = PlayerPrefs.GetInt("PLAYER_HIGHSCORE_STATS", 0);
        PlayerDistanceStats = PlayerPrefs.GetFloat("PLAYER_DISTANCE_STATS", 0f);
        PlayerPlaytimeStats = PlayerPrefs.GetFloat("PLAYER_PLAYTIME_STATS", 0); // mins

        // Animate game UI elements
        LeanTween.moveX(UIElements[0].GetComponent<RectTransform>(), 252f, 0.5f).setEase(LeanTweenType.easeInOutBack).setDelay(.5f);
        LeanTween.moveX(UIElements[1].GetComponent<RectTransform>(), 275.71f, 0.5f).setEase(LeanTweenType.easeInOutBack).setDelay(0.6f);
        LeanTween.moveX(UIElements[2].GetComponent<RectTransform>(), 286.77f, 0.5f).setEase(LeanTweenType.easeInOutBack).setDelay(0.7f);
        LeanTween.moveX(PauseBtn.GetComponent<RectTransform>(), -17.7f, 0.5f).setEase(LeanTweenType.easeInOutBack).setDelay(0.8f);

        // Pause game button listener
        PauseBtn.onClick.AddListener(delegate { ToggleGamePause(false); });

        // Paused panel button listeners
        PausedBtns[0].onClick.AddListener(delegate { ToggleGamePause(false); });
        PausedBtns[1].onClick.AddListener(BackToMenu);
        PausedBtns[2].onClick.AddListener(RestartGame);

        // Game over button listeners
        GameOverButtons[1].onClick.AddListener(BackToMenu);
        GameOverButtons[2].onClick.AddListener(RestartGame);

        // Game finished button listeners
        WinButtons[1].onClick.AddListener(BackToMenu);
        WinButtons[2].onClick.AddListener(RestartGame);
    }

    void FixedUpdate()
    {
        // Move player ball forward continously
        MovePlayer(Vector3.forward, false);
    }

    void Update()
    {

        // Player Input Handling
        if (Input.GetKeyDown(KeyCode.W) && JumpEnabled && IsGrounded())
            MovePlayer(Vector3.forward, true);

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            MovePlayer(Vector3.right, false);

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            MovePlayer(Vector3.left, false);

        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleGamePause(false);

        if (Input.GetKeyDown(KeyCode.R))
            RestartGame();

        if (Input.GetKeyUp(KeyCode.G))
            ToggleGhostMode();

        if (Input.GetKeyUp(KeyCode.T))
            _cheatMode = true;

        if (Input.GetKey(KeyCode.LeftShift) && _cheatMode)
        {
            MovePlayer(Vector3.forward, false);
            PlayerBallRB.maxAngularVelocity += 1;
        }

        if (Input.GetKey(KeyCode.DownArrow) && _cheatMode)
        {
            MovePlayer(Vector3.forward, false);
            PlayerBallRB.maxAngularVelocity -= 6;
        }

        // Mobile Input Handling
        if (Input.GetMouseButtonDown(0) && Input.mousePosition.y < Screen.height * 0.75 && JumpEnabled)
        {
            // Checks if double tap was in the same position (<=20p)
            if (Time.time - _lastClickTime < _catchTime
                && Math.Abs(Input.mousePosition.x - _prevMousePosition.x) <= 20
                && Math.Abs(Input.mousePosition.y - _prevMousePosition.y) <= 20)
            { 
                MovePlayer(Vector3.forward, true);
                Debug.Log("Jump");
            }
            _lastClickTime = Time.time;
            _prevMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            if(Input.mousePosition.x > Screen.width / 2 && Input.mousePosition.y < Screen.height * 0.75 ) // Touch boundaries (to be able to click pause btn)
                MovePlayer(Vector3.right, false);
            else if(Input.mousePosition.x < Screen.width / 2 && Input.mousePosition.y < Screen.height * 0.75)
                MovePlayer(Vector3.left, false);
        }

        // Timer
        if (_timerOn)
        {
            _timer += Time.deltaTime;
            TimerMS = (int)(_timer * 1000f) % 1000;
            TimerSeconds = (int)(_timer % 60);
            TimerMinutes = (int)(_timer / 60);
            _statsMinutes = _timer / 60;
        }
        T_Timer.text = string.Format("{0:00}:{1:00}", TimerMinutes, TimerSeconds);

        // Speed
        T_Speed.text = "" + (float)Math.Round(PlayerBallRB.velocity.z);
        if (GameIsPaused)
        {
            T_Speed.text = "" + (float)Math.Round(_prevPlayerVelocity.z);
        }

        T_Faults.text = "" + PlayerFaults;

        // Other checks
        OnPlayerFall();
    }


    void BackToMenu()
    {
        if (GameManager.Instance._LastPressTime + GameManager.Instance._PressDelay > Time.unscaledTime)
            return;
        GameManager.Instance._LastPressTime = Time.unscaledTime;
        GameManager.Instance.GoToScene("MainMenu");
    }

    // Ghostmode removes box colliders from all obstacles. (for testing)
    void ToggleGhostMode()
    {
        if (!_ghostMode)
        {
            var _obstacles = GameObject.FindGameObjectsWithTag("obstacle");
            for (int i = 0; i < _obstacles.Length-1; i++)
            {
                var _objCol = _obstacles[i].GetComponent<BoxCollider>();
                if (_objCol)
                    _objCol.enabled = false;
            }
            Debug.Log("===> Ghostmode Enabled");
        }
        _ghostMode = true;
    }

    public void PlayerAddPoints(int points)
    {
        PlayerPillsStats += points;
        T_Pills.text = "" + PlayerPillsStats;
    }

    public void PlayerAddSpeed(float speed)
    {
        PlayerBallRB.maxAngularVelocity += speed;
        //AddLightIntensity(PlatformFloorLight, 0.0001f);
    }

    // Check if player is grounded (for jumping)
    bool IsGrounded()
    {
        if (Physics.Raycast(PlayerBallRB.position, Vector3.down, _distToGround))
            return true;
        else
            return false;
    }

    public void MovePlayer(Vector3 moveDirection, bool jump)
    {
        if (jump)
            PlayerBallRB.AddForce(Vector3.up * JumpPower, ForceMode.Impulse);
        PlayerBallRB.AddTorque(new Vector3(moveDirection.z, 0, -moveDirection.x) * MovePower);
    }

    void ToggleGamePause(bool gameOver)
    {
        // Button spam guard
        if (GameManager.Instance._LastPressTime + GameManager.Instance._PressDelay > Time.unscaledTime)
            return;
        GameManager.Instance._LastPressTime = Time.unscaledTime;

        if (!GameIsPaused)
        {
            PausePlayerMovement();
            _timerOn = false;

            if (gameOver && !GameIsFinished)
                GameManager.Instance.PitchMusic(GameMusic);
            else if (GameIsFinished)
                GameManager.Instance.FadeMusic(GameMusic, true);
            else
            {
                GameMusic.volume = 0;
                GameMusic.Pause();
            }

            GameIsPaused = true;

            if (!gameOver)
            {
                // Show paused panel
                PausedPanelParent.SetActive(true);
                LeanTween.moveX(PausedPanel[0].GetComponent<RectTransform>(), 0f, 0.5f).setEase(LeanTweenType.easeInOutExpo);
                LeanTween.scale(PausedPanel[1].GetComponent<RectTransform>(), new Vector3(1, 1, 1), 0.5f).setEase(LeanTweenType.easeInOutBack).setDelay(0.1f);
                LeanTween.scale(PausedPanel[2].GetComponent<RectTransform>(), new Vector3(1, 1, 1), 0.5f).setEase(LeanTweenType.easeInOutExpo).setDelay(0.1f);
                ToggleGameHUD();
            }
        }

        else
        {
            // Remove paused panel
            LeanTween.moveX(PausedPanel[0].GetComponent<RectTransform>(), 908.3f, 0.5f).setEase(LeanTweenType.easeInOutExpo).setDelay(0.2f);
            LeanTween.scale(PausedPanel[1].GetComponent<RectTransform>(), new Vector3(0, 0, 0), 0.5f).setEase(LeanTweenType.easeInOutExpo);
            LeanTween.scale(PausedPanel[2].GetComponent<RectTransform>(), new Vector3(0, 0, 0), 0.5f).setEase(LeanTweenType.easeInOutExpo).
                setDelay(0.1f).
                setOnComplete(
                delegate ()
                {
                    PausedPanelParent.SetActive(false);
                    PlayerBallRB.velocity = _prevPlayerVelocity;
                    PlayerBallRB.isKinematic = false;
                    _timerOn = true;
                    GameManager.Instance.FadeMusic(GameMusic, false);
                    GameIsPaused = false;
                    ToggleGameHUD();
                });
        }
    }


    public void PausePlayerMovement()
    {
        _prevPlayerVelocity = PlayerBallRB.velocity;
        PlayerBallRB.isKinematic = true;
    }


    void ToggleGameHUD()
    {
        if (GameIsPaused || GameIsOver)
        {
            LeanTween.moveX(UIElements[0].GetComponent<RectTransform>(), 366f, 0.5f).setEase(LeanTweenType.easeInOutBack);
            LeanTween.moveX(UIElements[1].GetComponent<RectTransform>(), 361.8f, 0.5f).setEase(LeanTweenType.easeInOutBack);
            LeanTween.moveX(UIElements[2].GetComponent<RectTransform>(), 364.5f, 0.5f).setEase(LeanTweenType.easeInOutBack);
            LeanTween.moveX(PauseBtn.GetComponent<RectTransform>(), -106.75f, 0.5f).setEase(LeanTweenType.easeInOutBack);
        }
        else
        {
            LeanTween.moveX(UIElements[0].GetComponent<RectTransform>(), 252f, 0.5f).setEase(LeanTweenType.easeInOutBack);
            LeanTween.moveX(UIElements[1].GetComponent<RectTransform>(), 275.71f, 0.5f).setEase(LeanTweenType.easeInOutBack);
            LeanTween.moveX(UIElements[2].GetComponent<RectTransform>(), 286.77f, 0.5f).setEase(LeanTweenType.easeInOutBack);
            LeanTween.moveX(PauseBtn.GetComponent<RectTransform>(), -17.7f, 0.5f).setEase(LeanTweenType.easeInOutBack);
        }
    }

    public string GetPlayerLevelTimeFinishInString()
    {
        return string.Format("{0:00}:{1:00}:{2:00}", TimerMinutes, TimerSeconds, TimerMS);
    }


    public void OnGameOver()
    {
        if (!GameIsOver)
        {
            // Show leaderboard data

            LeaderboardPanel.SetActive(true);
            LeaderboardManager.Instance.UpdateLeaderboardUI();


            GameIsOver = true;
            GameManager.Instance.PitchMusic(GameMusic);
            PlayerDistanceStats += PlayerBallRB.position.z / 2000f; // to km?

            // Save player data
            PlayerPrefs.SetFloat("PLAYER_DISTANCE_STATS", PlayerDistanceStats);
            PlayerPrefs.SetInt("PLAYER_DEATHS_STATS", PlayerPrefs.GetInt("PLAYER_DEATHS_STATS", 0) + 1);
            PlayerPrefs.SetInt("PLAYER_TOTAL_POINTS_STATS", PlayerPrefs.GetInt("PLAYER_TOTAL_POINTS_STATS", 0) + PlayerPillsStats);
            PlayerPrefs.SetFloat("PLAYER_PLAYTIME_STATS", PlayerPlaytimeStats + _statsMinutes);

            ToggleGameHUD();
            ToggleGamePause(true);
            GameOverPanelParent.SetActive(true);

            T_GameOverHighscore.text = "Highscore: " + PlayerPrefs.GetInt("PLAYER_HIGHSCORE_STATS");
            T_GameOverScore.text = "Pills: " + PlayerPillsStats;
            T_GameOverSpeed.text = "Speed: " + PlayerBallRB.maxAngularVelocity;
            T_GameOverTime.text = "" + string.Format("{0:00}:{1:00}:{2:00}", TimerMinutes, TimerSeconds, TimerMS);
            
            // Animate game over elements
            LeanTween.alpha(GameOverPanel.GetComponent<RectTransform>(), 0.784f, 0.5f).setEase(LeanTweenType.easeInOutExpo);
            LeanTween.scale(GameOverLogo.GetComponent<RectTransform>(), new Vector3(0.7f, 0.7f, 0.7f), 1f).setEase(LeanTweenType.easeOutElastic).setDelay(0.1f);
            LeanTween.scale(GameOverPanelStatsHolder.GetComponent<RectTransform>(), new Vector3(1, 1, 1), 0.2f).setEase(LeanTweenType.animationCurve).setDelay(0.2f);
            LeanTween.scale(GameOverButtonsParent.GetComponent<RectTransform>(), new Vector3(1, 1, 1), 1f).setEase(LeanTweenType.easeOutElastic).setDelay(2.0f);
            LeanTween.scale(GameOverHighScore.GetComponent<RectTransform>(), new Vector3(1, 1, 1), 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(0.4f);
            LeanTween.scale(GameOverScore.GetComponent<RectTransform>(), new Vector3(1, 1, 1), 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(0.6f);
            LeanTween.scale(GameOverSpeed.GetComponent<RectTransform>(), new Vector3(1, 1, 1), 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(0.8f);
            LeanTween.scale(GameOverTime.GetComponent<RectTransform>(), new Vector3(1, 1, 1), 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(1f);

            // Check for new hiscore & save data
            if (PlayerPillsStats > PlayerPrefs.GetInt("PLAYER_HIGHSCORE_STATS"))
            {
                // Tween highscore value
                LeanTween.value(GameOverHighScore, PlayerPrefs.GetInt("PLAYER_HIGHSCORE_STATS"), PlayerPillsStats, 1f).
                    setDelay(0.4f).
                    setOnUpdate((float val) => { T_GameOverHighscore.text = "Highscore: " + (int)val; }).
                    setOnComplete(
                    delegate()
                    {
                        GameOverHighScore.GetComponent<Animation>().Play(); // Pop highscore
                        GameOverHighScoreTrophy.GetComponent<Animation>().Play(); // Animate trophy
                    });
                PlayerPrefs.SetInt("PLAYER_HIGHSCORE_STATS", PlayerPillsStats);
            }


            // Gameplay stars 
            int _playerStars = 1;
            if (PlayerPillsStats >= 49 ) { _playerStars = 2; }
            if (PlayerPillsStats >= 99) { _playerStars = 3; }

            // Show stars
            for (int i = 0; i <= _playerStars-1; i++)
            {
                switch (i)
                {
                    case 0:
                        LeanTween.scale(GameOverStars[i].GetComponent<RectTransform>(), new Vector3(0.4f, 0.4f, 0.4f), 1.5f).setEase(LeanTweenType.easeOutElastic).setDelay(1.5f);
                        LeanTween.rotateZ(GameOverStars[i], 15f, 2f).setEase(LeanTweenType.easeOutElastic);
                        break;
                    case 1:
                        LeanTween.scale(GameOverStars[i].GetComponent<RectTransform>(), new Vector3(0.45f, 0.45f, 0.45f), 1.5f).setEase(LeanTweenType.easeOutElastic).setDelay(1.7f);
                        break;
                    case 2:
                        LeanTween.scale(GameOverStars[i].GetComponent<RectTransform>(), new Vector3(0.4f, 0.4f, 0.4f), 1.5f).setEase(LeanTweenType.easeOutElastic).setDelay(1.9f);
                        LeanTween.rotateZ(GameOverStars[i], -15f, 0.5f).setEase(LeanTweenType.easeOutElastic);
                        break;
                    default:
                        break;
                }
            }

        }
    }


    public void OnLevelFinish()
    {
        if (!GameIsFinished && !GameIsOver)
        {
            GameIsOver = true;
            GameIsFinished = true;
            //GameManager.Instance.PitchMusic(GameMusic);
            PlayerDistanceStats += PlayerBallRB.position.z / 2000f; // to km?

            // Save player data
            PlayerPrefs.SetFloat("PLAYER_DISTANCE_STATS", PlayerDistanceStats);
            PlayerPrefs.SetInt("PLAYER_TOTAL_POINTS_STATS", PlayerPrefs.GetInt("PLAYER_TOTAL_POINTS_STATS", 0) + PlayerPillsStats);
            PlayerPrefs.SetFloat("PLAYER_PLAYTIME_STATS", PlayerPlaytimeStats + _statsMinutes);

            ToggleGameHUD();
            ToggleGamePause(true);
            WinPanelParent.SetActive(true);

            T_WinHighScore.text = "Highscore: " + PlayerPrefs.GetInt("PLAYER_HIGHSCORE_STATS");
            T_WinScore.text = "Pills: " + PlayerPillsStats;
            T_WinSpeed.text = "Speed: " + PlayerBallRB.maxAngularVelocity;
            T_WinTime.text = "" + string.Format("{0:00}:{1:00}:{2:00}", TimerMinutes, TimerSeconds, TimerMS);

            // Animate game over elements
            LeanTween.alpha(WinPanel.GetComponent<RectTransform>(), 0.784f, 0.5f).setEase(LeanTweenType.easeInOutExpo);
            LeanTween.scale(WinLogo.GetComponent<RectTransform>(), new Vector3(0.7f, 0.7f, 0.7f), 1f).setEase(LeanTweenType.easeOutElastic).setDelay(0.1f);
            LeanTween.scale(WinPanelStatsHolder.GetComponent<RectTransform>(), new Vector3(1, 1, 1), 0.2f).setEase(LeanTweenType.animationCurve).setDelay(0.2f);
            LeanTween.scale(WinButtonsParent.GetComponent<RectTransform>(), new Vector3(1, 1, 1), 1f).setEase(LeanTweenType.easeOutElastic).setDelay(2.0f);
            LeanTween.scale(WinHighScore.GetComponent<RectTransform>(), new Vector3(1, 1, 1), 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(0.4f);
            LeanTween.scale(WinScore.GetComponent<RectTransform>(), new Vector3(1, 1, 1), 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(0.6f);
            LeanTween.scale(WinSpeed.GetComponent<RectTransform>(), new Vector3(1, 1, 1), 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(0.8f);
            LeanTween.scale(WinTime.GetComponent<RectTransform>(), new Vector3(1, 1, 1), 0.5f).setEase(LeanTweenType.easeOutExpo).setDelay(1f);

            // Check for new hiscore & save data
            if (PlayerPillsStats > PlayerPrefs.GetInt("PLAYER_HIGHSCORE_STATS"))
            {
                // Tween highscore value
                LeanTween.value(WinHighScore, PlayerPrefs.GetInt("PLAYER_HIGHSCORE_STATS"), PlayerPillsStats, 1f).
                    setDelay(0.4f).
                    setOnUpdate((float val) => { T_WinHighScore.text = "Highscore: " + (int)val; }).
                    setOnComplete(
                    delegate ()
                    {
                        WinHighScore.GetComponent<Animation>().Play(); // Pop highscore once
                        WinHighScoreTrophy.GetComponent<Animation>().Play(); // Animate trophy
                    }); 
                PlayerPrefs.SetInt("PLAYER_HIGHSCORE_STATS", PlayerPillsStats);
            }


            // Gameplay stars 
            int _playerStars = 1;
            if (PlayerPillsStats >= 49) { _playerStars = 2; }
            if (PlayerPillsStats >= 99) { _playerStars = 3; }

            // Show stars
            for (int i = 0; i <= _playerStars - 1; i++)
            {
                switch (i)
                {
                    case 0:
                        LeanTween.scale(WinStars[i].GetComponent<RectTransform>(), new Vector3(0.4f, 0.4f, 0.4f), 1.5f).setEase(LeanTweenType.easeOutElastic).setDelay(1.5f);
                        LeanTween.rotateZ(WinStars[i], 15f, 2f).setEase(LeanTweenType.easeOutElastic);
                        break;

                    case 1:
                        LeanTween.scale(WinStars[i].GetComponent<RectTransform>(), new Vector3(0.45f, 0.45f, 0.45f), 1.5f).setEase(LeanTweenType.easeOutElastic).setDelay(1.7f);
                        break;

                    case 2:
                        LeanTween.scale(WinStars[i].GetComponent<RectTransform>(), new Vector3(0.4f, 0.4f, 0.4f), 1.5f).setEase(LeanTweenType.easeOutElastic).setDelay(1.9f);
                        LeanTween.rotateZ(WinStars[i], -15f, 0.5f).setEase(LeanTweenType.easeOutElastic);
                        break;

                    default:
                        break;
                }
            }
        }
    }


    void OnPlayerFall()
    {
        if (!GameIsOver)
        {
            if(PlayerBallRB.position.y < -95)
            {
                PlayerCamera.Instance.FollowTargetZ = false;
                PlayerCamera.Instance.FollowTargetY = false;
            }
            if (PlayerBallRB.position.y < -130)
            {
                OnGameOver();
            }
            if (PlayerBallRB.position.y < -5)
            {
                GameManager.Instance.AddSkyboxSpeed(-PlayerBallRB.position.y * 0.0017999f); // DizzZy
                GameMusic.pitch = 1 + (Math.Abs(PlayerBallRB.position.y) / 48.2f); // Rolling into enternity sound
            }
        }
    }


    void AddLightIntensity(Light light, float intensity)
    {
        LeanTween.value(light.intensity, light.intensity+intensity, 0.5f).setOnUpdate((float val) => { light.intensity = light.intensity + intensity; });
    }


    void RestartGame()
    {
        if (GameManager.Instance._LastPressTime + GameManager.Instance._PressDelay > Time.unscaledTime)
            return;

        GameManager.Instance._LastPressTime = Time.unscaledTime;
        GameManager.Instance.FadeMusic(GameMusic, true);
        GameManager.Instance.GoToScene("Restart");
    }


    public void RestartFromCheckpoint()
    {
        PlayerBallRB.velocity = Checkpoint.Instance.playerVelocity;
        PlayerBallRB.maxAngularVelocity = Checkpoint.Instance.playerMaxSpeed;
        PlayerBall.transform.position = Checkpoint.Instance.playerPosition;
        GameMusic.time = Checkpoint.Instance.musicTime;
        PlayerBallRB.isKinematic = false;
        GameMusic.volume = 0;
        GameManager.Instance.FadeMusic(GameMusic, false);
        LeanTween.value(0, 0.1f, 1f).setOnUpdate((float val) => { GameLogic.Instance.PlayerBall.GetComponentInChildren<TrailRenderer>().time = val; });
    }

}