using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonBGM : MonoBehaviour
{
    // singleton enforcer
    private void Awake()
    {
        if (FindObjectsOfType<SingletonBGM>().Length > 1)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

}
