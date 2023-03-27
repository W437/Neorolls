using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pills : MonoBehaviour
{
    private Animator objAnimator;
    private AudioSource objAudio;
    private CapsuleCollider objCollider;

    void Start()
    {
        objAnimator = gameObject.GetComponent<Animator>();
        objAudio = gameObject.GetComponentInChildren<AudioSource>();
        objCollider = gameObject.GetComponent<CapsuleCollider>();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            objAnimator.SetBool("pillGone", true);
            objAudio.Play();
            GameLogic.Instance.PlayerAddPoints(1);
            objCollider.isTrigger = false;
            objCollider.enabled = false;
            Destroy(gameObject, 5f); // 3s to fully play the SFX
        }
    }
}