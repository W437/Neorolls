using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerCountryLocation : MonoBehaviour
{

    public static string PlayerCountryCode = "";
    void Start()
    {
        StartCoroutine("DetectCountry");
    }

    [System.Obsolete]
    IEnumerator DetectCountry()
    {
        UnityWebRequest request = UnityWebRequest.Get("https://extreme-ip-lookup.com/json");
        request.chunkedTransfer = false;
        yield return request.Send();
        Debug.Log("Locating");

        if (request.isNetworkError)
        {
            Debug.Log("error : " + request.error);
        }
        else
        {
            if (request.isDone)
            {
                Country res = JsonUtility.FromJson<Country>(request.downloadHandler.text);
                PlayerCountryCode = res.countryCode;
            }
        }
    }

    public static string GetPlayerCountryCode()
    {
        return PlayerCountryCode;
    }
}