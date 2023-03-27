using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowDown : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            GameLogic.Instance.PlayerAddSpeed(-5f);
            Destroy(gameObject, 3f);
        }
    }
}
