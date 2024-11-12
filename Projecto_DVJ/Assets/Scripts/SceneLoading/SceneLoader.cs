using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader>
{
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    [SerializeField] private string sceneName;

    void Awake()
    {
        // Evitar que el objeto se destruya al cambiar de escena
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene()
    {
        StartCoroutine(Helper.Fade(fadeCanvasGroup, 1, fadeDuration));
        StartCoroutine(LoadSceneCoroutine());
    }

    private IEnumerator LoadSceneCoroutine()
    {
        yield return new WaitForSeconds(fadeDuration);
        //StartCoroutine(ProcessLevelLoading(sceneName));
        SceneManager.LoadScene(sceneName);
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        while(scene.isLoaded == false)
        {
            yield return null;
        }
        StartCoroutine(Helper.Fade(fadeCanvasGroup, 0, fadeDuration));
    }

    private IEnumerator ProcessLevelLoading(string request)
    {
        if (request != null)
        {
            AsyncOperation loadSceneProcess = SceneManager.LoadSceneAsync(request, LoadSceneMode.Additive);

            // Level is being loaded, it could take some seconds (or not). Waiting until is fully loaded
            while (!loadSceneProcess.isDone)
            {
                yield return null;
            }

            // Once the level is ready, activate it!
            FadeIn(request);

            // Descargar la escena anterior (ahora que ya tenemos una nueva activa)
            var currentLoadedLevel = SceneManager.GetActiveScene();
            if (currentLoadedLevel.name != request)  // Evitar descargar la misma escena
            {
                SceneManager.UnloadSceneAsync(currentLoadedLevel);
            }
        }
    }

    private void FadeIn(string name)
    {
       bool loaded = false;
       Scene scene = SceneManager.GetSceneByName(sceneName);
       /*
       while(loaded == false)
       {
            if(scene.isLoaded)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(name));
                loaded = true;
            }
       }
       */

        StartCoroutine(Helper.Fade(fadeCanvasGroup, 0, 1f));
    }
    
    
}
