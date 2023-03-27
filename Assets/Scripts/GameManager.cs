using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    [SerializeField] private GameObject _startingSceneTransition;
    [SerializeField] private GameObject _endingSceneTransition;
    [SerializeField] public float SkyboxRotationSpeed { get; private set; } = 1.4f;

    public float _LastPressTime;
    public float _PressDelay = 0.8f;


    // Start is a Unity function that executes once, when the script is called.
    void Start()
    {
        EndSceneTransition();
    }

    // Awake is a Unity function that executes before anything else.
    void Awake()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Application.targetFrameRate = 60;
        _instance = this;
    }

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.Log("GameManager is null");
            return _instance;
        }
    }

    public void AddSkyboxSpeed(float speed)
    {
        SkyboxRotationSpeed += speed;
    }

    void StartSceneTransition()
    {
        _startingSceneTransition.SetActive(true);
        StartCoroutine("DeactivateSceneTransition", "Start");
    }


    void EndSceneTransition()
    {
        _endingSceneTransition.SetActive(true);
        StartCoroutine("DeactivateSceneTransition", "End");
    }

    IEnumerator DeactivateSceneTransition(string transitionName)
    {
        yield return new WaitForSeconds(2.0f);
        if (transitionName == "Start")
        {
            _startingSceneTransition.SetActive(false);
        }
        else if(transitionName == "End")
        {
            _endingSceneTransition.SetActive(false);
        }
    }

    public void GoToScene(string scene)
    {
        switch (scene)
        {
            case "MainGame":
                StartSceneTransition();
                StartCoroutine("InitiateScene", 1);
                break;

            case "MainMenu":
                StartSceneTransition();
                StartCoroutine("InitiateScene", 0);
                break;

            case "Restart":
                GameLogic.Instance.PausePlayerMovement();
                StartSceneTransition();
                StartCoroutine("InitiateScene", 1);
                break;

            default:
                Debug.Log("_Scene" + scene + " is null.");
                break;
        }
    }

    public void FadeMusic(AudioSource audio, bool on)
    {
        if (on)
            LeanTween.value(audio.volume, 0, 0.5f).setOnUpdate((float val) => { audio.volume = val; }).
                setOnComplete(delegate () { audio.Pause(); });
        else
        {
            audio.UnPause();
            LeanTween.value(0, PlayerPrefs.GetFloat("MAIN_VOLUME", 0.8f), 0.5f).
                setOnUpdate((float val) => { audio.volume = val; });
        }
    }

    public void FadeGameMusicTo(float to)
    {
        var _music = GameLogic.Instance.GameMusic;
        if(_music)
            LeanTween.value(_music.volume, to, .5f).setOnUpdate((float val) => { _music.volume = val; });
    }

    public void PitchMusic(AudioSource audio)
    {
        LeanTween.value(audio.pitch, 0, 2.5f).setOnUpdate((float val) => { audio.pitch = val; }).
            setOnComplete(delegate () { audio.Pause(); });
    }

    IEnumerator InitiateScene(int scene)
    {
        yield return new WaitForSeconds(0.9f);
        SceneManager.LoadScene(scene);
    }

}
