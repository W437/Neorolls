using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    private Animator ObjAnimator;
    private AudioSource ObjAudio;
    private CapsuleCollider ObjCollider;

    void Start()
    {
        ObjAnimator = gameObject.GetComponent<Animator>();
        ObjAudio = gameObject.GetComponentInChildren<AudioSource>();
        ObjCollider = gameObject.GetComponent<CapsuleCollider>();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            ObjAnimator.SetBool("RemovePill", true);
            ObjAudio.Play();
            GameLogic.Instance.PlayerAddSpeed(5f);
            ObjCollider.isTrigger = false;
            ObjCollider.enabled = false;
            Destroy(gameObject, 3f);
        }
    }

}
