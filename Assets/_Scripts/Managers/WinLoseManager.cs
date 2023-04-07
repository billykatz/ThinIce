using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinLoseManager : MonoBehaviour
{
    public static WinLoseManager Instance;

    [SerializeField] private GameObject _levelOverParent;

    [SerializeField] private GameObject _youWinElements;
    [SerializeField] private GameObject _youLoseElements;

    [SerializeField] private ProgressController _progressManager;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        _levelOverParent.SetActive(false);
    }

    private void OnDestroy() {
    }

    public void GameWin() {
        
        HandManager.Instance.DiscardHand();
        _levelOverParent.SetActive(true);
        _youWinElements.SetActive(true);
        _youLoseElements.SetActive(false);
    }

    public void GameLose() {
        HandManager.Instance.DiscardHand();
        _levelOverParent.SetActive(true);
        _youWinElements.SetActive(false);
        _youLoseElements.SetActive(true);
    }
    
    public void DidPressContinueButton() {
        if (DeckManager.Instance.IsTutorial)
        {
            
            _progressManager.DidCompleteTutorial();
        }
        else
        {
            // save the data to the player
            _progressManager.DidCompleteLevel();
        }

        // move to the next scene
        ThinIceSceneManager.Instance.LoadWorldMap();
    }

    public void DidPressRetryButton() {
        // move to the next scene
        ThinIceSceneManager.Instance.LoadGameScene();
        
    }
    public void DidPressMainMenuButton() {
        ThinIceSceneManager.Instance.LoadMainMenu();
    }



}
