using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ThinIceSceneManager : MonoBehaviour
{
    public static ThinIceSceneManager Instance;
    private bool _showWorldMap = false;

    private bool _firstFrame = false;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += SceneLoaded;
    }

    private void Update()
    {
        if (!_firstFrame)
        {
            _firstFrame = true;
            LoadMainMenu();
        }
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 1 && _showWorldMap)
        {
            FindObjectOfType<MainMenuController>().DidPressStartButton();
        }

    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(1);
    }
    
    public void LoadTutorial()
    {
        SceneManager.LoadScene(3);
    }

    public void LoadWorldMap()
    {
        _showWorldMap = true;
        SceneManager.LoadScene(1);
    }
    
    public void LoadGameScene()
    {
        SceneManager.LoadScene(2);
    }
}
