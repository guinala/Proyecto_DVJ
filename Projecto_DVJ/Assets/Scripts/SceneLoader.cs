using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
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
        SceneManager.LoadScene(sceneName);
    }

    

    private IEnumerator ProcessLevelLoading(string request)
    {
        if (request != null)
        {
            var currentLoadedLevel = SceneManager.GetActiveScene();
            SceneManager.UnloadSceneAsync(currentLoadedLevel);

            AsyncOperation loadSceneProcess = SceneManager.LoadSceneAsync(request, LoadSceneMode.Additive);

            // Level is being loaded, it could take some seconds (or not). Waiting until is fully loaded
            while (!loadSceneProcess.isDone)
            {
                yield return null;
            }

            // Once the level is ready, activate it!
            ActivateLevel(request);
        }
    }

    private void ActivateLevel(string name)
    {
        // Set active
        var loadedLevel = SceneManager.GetSceneByName(name);
        SceneManager.SetActiveScene(loadedLevel);

        StartCoroutine(Helper.Fade(fadeCanvasGroup, 0, 1f));
    }
    
}
