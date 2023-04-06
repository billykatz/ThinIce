using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using DG.Tweening;

public class WorldMapController : MonoBehaviour
{
    [FormerlySerializedAs("levelLocations")] [SerializeField] private Transform[] _levelLocations;
    [SerializeField] private GameObject _hero;

    [SerializeField] private ThinIceCanvasButton _playButton;
    [SerializeField] private DeckModificationController _deckModificationController;

    [SerializeField] private CurrentLevelReference _currentLevelReference;
    [SerializeField] private ProgressController _progressController;
    [SerializeField] private GameObject _thanksForPlayingView;
    
    private ScriptableLevelRules _currentLevel;

    public bool IsShowingDeckModification;

    private void Start()
    {
        _currentLevel = _currentLevelReference.LevelRules;
        _playButton.TextField.text = _currentLevel.WorldMapLevelTitle;
    }

    public void PlayButtonPressed()
    {
        if (_currentLevel.isLevel)
        {
            if (_currentLevel.LevelNumber == 4)
            {
                _thanksForPlayingView.SetActive(true);
                return;
            }
            _progressController.DidStartLevel();
            ThinIceSceneManager.Instance.LoadGameScene();
        }
        else if (!IsShowingDeckModification)
        {
            IsShowingDeckModification = true;
            _deckModificationController.Configure(_currentLevel);
        }
    }

    public void DidAppearOnScreen()
    {
        int currentLevelIdx = _currentLevel.LevelNumber;
        int previousLevelIndex = Mathf.Max(0, currentLevelIdx-1);
        
        // set the hero to the level before
        _hero.transform.position = _levelLocations[previousLevelIndex].position;
        _hero.transform.DOMove(_levelLocations[currentLevelIdx].position, 1.5f);
        
        // set the play text
        _playButton.TextField.text = _currentLevel.WorldMapLevelTitle;
    }

    /// <summary>
    ///  Called after completing a in map level, like a shop
    /// </summary>
    public void DidCompleteLevel()
    {
        IsShowingDeckModification = false;
        _currentLevel = _currentLevelReference.LevelRules;
        DidAppearOnScreen();
    }
}
