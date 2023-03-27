using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Checkpoint : MonoBehaviour
{
    private static Checkpoint _instance;
    public float musicTime;
    public Vector3 playerPosition;
    public Vector3 playerVelocity;
    public float playerMaxSpeed;
    [SerializeField] List<GameObject> checkPoints;

    public static Checkpoint Instance
    {
        get
        {
            if (_instance == null)
                Debug.Log("Checkpoint is null.");
            return _instance;
        }
    }

    private void Start()
    {
        _instance = this;
    }

    // ability to start level on checkpoint after finishing.
    // requires to purchase the ability from the shop.
    // shop system w different balls, trails, styles, etc.
    // make it dark far away
    // so cant see more than 100m for example
    // for level removing.

    // When player collides with checkpoint, save all game data from that point.
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("checkpoint"))
        {
            musicTime = GameLogic.Instance.GameMusic.time;
            playerPosition = GameLogic.Instance.PlayerBall.transform.position;
            playerVelocity = GameLogic.Instance.PlayerBallRB.velocity;
            playerMaxSpeed = GameLogic.Instance.PlayerBallRB.maxAngularVelocity;
            Destroy(col.gameObject, 0.5f);
        }
    }
}

