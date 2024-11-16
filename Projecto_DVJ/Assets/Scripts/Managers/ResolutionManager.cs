using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ResolutionManager : MonoBehaviour
{
    bool fullScreen;
    public int resIndex;
    public Vector2[] resolution;
    [SerializeField] GameObject resObj;
    
    [Header("FPS Management")]
    [SerializeField] GameObject FPSObj;
    [SerializeField] bool calculateFPS;
    [SerializeField] bool displayFPS;
    [SerializeField] TextMeshProUGUI FPSText;
    [SerializeField] TextMeshProUGUI resText;
    int lastFrameIndex;
    float[] frameDeltaTimeArray = new float [50];

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
            resIndex = GameManager.Instance.resIndex;
        fullScreen = true;

        ChangeResolution();
    }

    public void ScreenSize(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            fullScreen = !fullScreen;
            ChangeResolution();
        }
    }

    public void Resolution(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            resIndex++;

            if (resIndex >= resolution.Length)
                resIndex = 0;

            GameManager.Instance.resIndex = resIndex;

            ChangeResolution();
            resObj.GetComponent<Animator>().Play("Appear");
        }
    }

    private void Update()
    {
        resText.text = this.resolution[resIndex].x.ToString() + " x " + this.resolution[resIndex].y.ToString();
        if (displayFPS)
        {
            FPSObj.SetActive(true);
            calculateFPS = true;
        }
        else
            FPSObj.SetActive(false);
        if (calculateFPS && displayFPS)
            CalculateFrameRate();
    }
    
    public void OnShowHideFPS(InputAction.CallbackContext context)
    {
        if (context.started)
            displayFPS = !displayFPS;
    }

    void ChangeResolution()
    {
        Screen.SetResolution((int)resolution[resIndex].x, (int)resolution[resIndex].y, fullScreen);
    }
    
    void CalculateFrameRate()
    {
        frameDeltaTimeArray[lastFrameIndex] = Time.unscaledDeltaTime;
        lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;
        FPSText.text = Mathf.RoundToInt(CalculateFPS()).ToString() + " FPS";
    }
    
    float CalculateFPS()
    {
        float total = 0f;
        foreach (float deltaTime in frameDeltaTimeArray)
        {
            total += deltaTime;
        }
        return frameDeltaTimeArray.Length / total;
    }
    
}
