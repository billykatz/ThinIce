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
    [SerializeField] private ProgressManager _progressManager;
    [SerializeField] private DeckModificationController _deckModificationController;
    
    private ScriptableLevelRules _currentLevel;

    private void Start()
    {
        _currentLevel = _progressManager.CurrentLevel();
        _playButton.TextField.text = _currentLevel.WorldMapLevelTitle;
    }

    public void PlayButtonPressed()
    {
        if (_currentLevel.isLevel)
        {
            ThinIceSceneManager.Instance.LoadGameScene();
        }
        else
        {
            _deckModificationController.Configure(_currentLevel);
        }
    }

    public void DidAppearOnScreen()
    {
        int currentLevelIdx = _progressManager.CurrentLevelIndex();
        int previousLevelIndex = Mathf.Max(0, currentLevelIdx-1);
        // set the hero to the level before
        _hero.transform.position = _levelLocations[previousLevelIndex].position;
        _hero.transform.DOMove(_levelLocations[currentLevelIdx].position, 1.5f);
    }
}
