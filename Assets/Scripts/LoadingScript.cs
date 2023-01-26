using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Michsky.UI.ModernUIPack;

public class LoadingScript : MonoBehaviour
{
    public static LoadingScript instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject loadingScreen;
    public CanvasGroup[] screens; 
    public ProgressBar bar;

    float totalSceneProgress;
    float totalSpawnProgress;

    AsyncOperation operation; 

    void ResetTimeScale()
    {
        Time.timeScale = 1f;
    }

    public void LoadGame()
    {
        ResetTimeScale();
        loadingScreen.SetActive(true);
        operation = SceneManager.LoadSceneAsync((int)SceneIndexes.GEN_TEST);

        foreach (var screen in screens)
        {
            screen.alpha = 0;
        }

        screens[ModeDetails.currentCardOpen].alpha = 1;

        StartCoroutine(GetSceneLoadProgress());
        StartCoroutine(GetTotalProgress());
    }

    IEnumerator GetSceneLoadProgress()
    {
        while (!operation.isDone)
        {
            totalSceneProgress = (operation.progress / .9f) * 100f;

            yield return null;
        }
    }

    public IEnumerator GetTotalProgress()
    {
        float totalProgress = 0;

        while (ProceduralGeneration.instance == null || !ProceduralGeneration.instance.isDone)
        {
            if (ProceduralGeneration.instance == null)
            {
                totalSpawnProgress = 0f;
            }
            else
            {
                totalSpawnProgress = Mathf.Round(ProceduralGeneration.instance.progress * 100f);
            }

            totalProgress = Mathf.Round((totalSceneProgress + totalSpawnProgress) / 2f);

            bar.currentPercent = Mathf.RoundToInt(totalProgress);
            
            yield return null;
        }

        loadingScreen.SetActive(false);
        bar.currentPercent = 0f;
        ProceduralGeneration.instance = null;
    }

    public void LoadMenu()
    {
        ResetTimeScale();
        loadingScreen.SetActive(true);
        operation = SceneManager.LoadSceneAsync((int)SceneIndexes.MAIN_MENU);

        StartCoroutine(GetMenuLoadProgress());
    }

    IEnumerator GetMenuLoadProgress()
    {
        while (!operation.isDone)
        {
            totalSceneProgress = (operation.progress / .9f) * 100f;

            bar.currentPercent = Mathf.RoundToInt(totalSceneProgress);

            yield return null;
        }

        loadingScreen.SetActive(false);
        bar.currentPercent = 0f;
    }  
}
