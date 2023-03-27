using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    // === Code found online === //

    private static PlayerCamera _instance;

    [Tooltip("Target to be followed")]
    public Transform Target;
    [Tooltip("Mimic the Target's changes in x coordinate")]
    public bool FollowTargetX = true;
    [Tooltip("Mimic the Target's changes in y coordinate")]
    public bool FollowTargetY = true;
    [Tooltip("Mimic the Target's changes in z coordinate")]
    public bool FollowTargetZ = true;


    private void Awake()
    {
        _instance = this;
    }

    public static PlayerCamera Instance
    {
        get
        {
            if (_instance == null)
                Debug.Log("PlayerCamera is null");
            return _instance;
        }
    }

    void Update()
    {
        if (Target)
        {
            Vector3 targetMovement = GetTargetMovement();
            UpdatePosition(targetMovement);
        }
    }


    private Vector3 oldTargetPosition;

    private Vector3 GetTargetMovement()
    {
        if (oldTargetPosition == Vector3.zero)
        {
            oldTargetPosition = Target.transform.position;
        }
        Vector3 newTargetPosition = Target.transform.position;
        Vector3 targetMovement = newTargetPosition - oldTargetPosition;
        oldTargetPosition = new Vector3(newTargetPosition.x, newTargetPosition.y, newTargetPosition.z);
        return targetMovement;
    }


    private void UpdatePosition(Vector3 targetMovement)
    {
        float xPosition = transform.position.x;
        float yPosition = transform.position.y;
        float zPosition = transform.position.z;
        if (FollowTargetX)
        {
            xPosition += targetMovement.x;
        }
        if (FollowTargetY)
        {
            yPosition += targetMovement.y;
        }
        if (FollowTargetZ)
        {
            zPosition += targetMovement.z;
        }
        Vector3 updatedPosition = new Vector3(xPosition, yPosition, zPosition);
        transform.position = updatedPosition;
    }
}

