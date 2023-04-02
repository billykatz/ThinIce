using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ThinIceSceneManager : MonoBehaviour
{
    public static ThinIceSceneManager Instance;
    private bool _showWorldMap = false;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += SceneLoaded;
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0 && _showWorldMap)
        {
            FindObjectOfType<MainMenuController>().DidPressStartButton();
        }
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void LoadWorldMap()
    {
        _showWorldMap = true;
        SceneManager.LoadScene(0);
    }
    
    public void LoadGameScene()
    {
        SceneManager.LoadScene(1);
    }
}
