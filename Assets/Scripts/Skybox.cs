using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skybox : MonoBehaviour
{
    void FixedUpdate()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * GameManager.Instance.SkyboxRotationSpeed);
    }
}
