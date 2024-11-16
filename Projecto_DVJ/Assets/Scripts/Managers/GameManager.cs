using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int resIndex;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        //QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;

        if (Instance != null && Instance != this)
            Destroy(this);
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }
}