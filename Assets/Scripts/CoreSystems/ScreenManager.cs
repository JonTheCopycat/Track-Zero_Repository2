using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager current = null;
    
    private float startTime;
    private AsyncOperation operation;
    //private Canvas loadingScreen;
    private string sceneName;

    private void OnStart()
    {
        startTime = Time.time;
        //Time.timeScale = 1;
        //loadingScreen = GetComponentInChildren<Canvas>();
        //loadingScreen.gameObject.SetActive(false);
        sceneName = SceneManager.GetActiveScene().name;
        current = this;
    }

    private void Awake()
    {
        OnEnable();
        Application.targetFrameRate = 60;
    }

    private void OnEnable()
    {
        sceneName = SceneManager.GetActiveScene().name;
        current = this;
    }

    private void OnDestroy()
    {
        if (current == this)
        {
            current = null;
        }
    }

    public string GetSceneName()
    {
        return sceneName;
    }

    public float GetSceneStartTime()
    {
        return startTime;
    }

    public float GetTimeSinceStart()
    {
        return Time.time - startTime;
    }

    public void GoToScene(string sceneName)
    {
        Time.timeScale = 1;
        //loadingScreen.gameObject.SetActive(true);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    public void MakeInactive(GameObject screen)
    {
        screen.SetActive(false);
    }
    
    public void MakeActive(GameObject screen)
    {
        screen.SetActive(true);
    }

    public void AnimateScreenIn(GameObject screen)
    {
        Animator anim = screen.GetComponent<Animator>();
        if (anim)
        {
            MakeActive(screen);
            anim.SetTrigger("In");
        }
        else
        {
            MakeActive(screen);
        }
    }

    public void AnimateScreenOut(GameObject screen)
    {
        Animator anim = screen.GetComponent<Animator>();
        if (anim)
        {
            anim.SetTrigger("Out");

        }
        else
        {
            MakeInactive(screen);
        }
    }

    public void Quit()
    {
        Application.Quit();

    }

    public void ToggleActive(GameObject screen)
    {
        if (screen.activeSelf)
        {
            screen.SetActive(false);
        }
        else
        {
            screen.SetActive(true);
        }
    }
}
