using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private static FinishLine _instance;
    [SerializeField] private AudioSource FinishSFX;
    private bool _finishThreshold = false;
    private bool _fadeThreshold = false;
    private float DistanceFromPlayer;
    private bool collided = false;



    public static FinishLine Instance
    {
        get
        {
            if (_instance == null)
                Debug.Log("FinishLine is null.");
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Update()
    {
        if (gameObject.transform.position.z - GameLogic.Instance.PlayerBall.transform.position.z < 70f && !_fadeThreshold)
        {
            _fadeThreshold = true;
            GameManager.Instance.FadeGameMusicTo(0.4f);
        }

        if (gameObject.transform.position.z - GameLogic.Instance.PlayerBall.transform.position.z < 30f)
            PlayFinishSFX();

        DistanceFromPlayer = gameObject.transform.position.z - GameLogic.Instance.PlayerBall.transform.position.z;
    }

    public float GetPlayerDistanceFromFinish()
    {
        return DistanceFromPlayer;
    }

    void PlayFinishSFX()
    {
        if (!_finishThreshold)
        {
            FinishSFX.Play();
        }
        _finishThreshold = true;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        { 
            if(!collided) 
            {
                GameLogic.Instance.OnLevelFinish();
                var timeString = GameLogic.Instance.GetLevelFinishTimeInString();
                LeaderboardManager.Instance.SendLeaderboard(timeString);
                collided = true;            
            }
        }
    }

}
