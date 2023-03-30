using System.Collections;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class PlayerBall : MonoBehaviour
{
    void OnCollisionEnter(Collision col)
    {
        if (col.collider.CompareTag("obstacle"))
        {
            GameLogic.Instance.PlayerBall.GetComponentInChildren<TrailRenderer>().time = 0;
            GameLogic.Instance.PausePlayerMovement();  
            Debug.Log("Collision: " + col);
            //GameLogic.Instance.RestartFromCheckpoint();
            //GameLogic.Instance.PlayerFaults++;
            GameLogic.Instance.OnGameOver();
            var timeString = GameLogic.Instance.GetPlayerLevelTimeFinishInString();
            Debug.Log("coll time: " + timeString);
            LeaderboardManager.Instance.SendLeaderboard(timeString);
        }
    }
}


