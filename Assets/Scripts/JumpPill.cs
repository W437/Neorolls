using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPill : MonoBehaviour
{
    private AudioSource objAudio;
    private Animator objAnimator;
    private CapsuleCollider objCollider;

    private void Start()
    {
        objAnimator = gameObject.GetComponent<Animator>();
        objAudio = gameObject.GetComponentInChildren<AudioSource>();
        objCollider = gameObject.GetComponent<CapsuleCollider>();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            objAnimator.SetBool("jumpPillRemove", true);
            objAudio.Play();
            GameLogic.Instance.JumpEnabled = true;
            objCollider.isTrigger = false;
            objCollider.enabled = false;
            Destroy(gameObject, 3f); // to allow for SFX to fully play
        }
    }

}
