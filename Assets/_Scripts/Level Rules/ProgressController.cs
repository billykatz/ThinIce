using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "New Progress Controller", menuName = "Progress Controller")]
public class ProgressController : ScriptableObject
{
    [SerializeField] private CurrentLevelReference _currentLevelReference;
    [SerializeField] private ScriptableLevelRules _levelRulesBeforeTutorial;

    [SerializeField] private ScriptableLevelRules[] levels;
    [SerializeField] private ScriptableLevelRules TutorialLevel;
    [SerializeField] private int _levelIndex;
    public bool CompletedTutorial;
    public bool JustForTesting;

    public int playerMaxAttack;
    public int playerMaxArmor;
    public int playerMaxHealth;
    public int playerStartAttack;
    public int playerStartArmor;

    public bool _haveInitiatedStarterDeck;

    private void OnValidate()
    {
        _currentLevelReference.LevelRules = levels[_levelIndex];
    }

    private void Awake()
    {
        _currentLevelReference.LevelRules = levels[_levelIndex];
    }
    public void DebugResetStats()
    {
        playerMaxAttack = 4;
        playerMaxArmor = 4;
        playerMaxHealth = 4;
        playerStartArmor = 1;
        playerStartAttack = 1;
    }
    

    public void WillGenerateGrid()
    {
        PlayerManager playerManager = FindObjectOfType<PlayerManager>();
        playerManager.SetPlayerMaxStats(playerMaxAttack, playerMaxArmor, playerMaxHealth);
        playerManager.SetPlayerStartingStats(playerStartAttack, playerStartArmor);
    }
    
    public void UpdatePlayerMaxStats(int newMax)
    {
        playerMaxAttack = newMax;
        playerMaxArmor = newMax;
        playerMaxHealth = newMax;
    }
    
    public void UpdatePlayersStartStats(int newStart)
    {
        playerStartArmor = newStart;
        playerStartAttack = newStart;

    }

    
    public ScriptableLevelRules CurrentLevel()
    {
        return levels[_levelIndex];
    }
    
    public int CurrentLevelIndex()
    {
        return _levelIndex;
    }

    public void DidStartLevel()
    {
        DeckManager.Instance.IsTutorial = false;
        if (!_haveInitiatedStarterDeck)
        {
            _haveInitiatedStarterDeck = true;
            DeckManager.Instance.ShouldCreateStarterDeck = true;
        }
        else
        {
            DeckManager.Instance.ShouldCreateStarterDeck = false;
        }
    }

    public void DidCompleteTutorial()
    {
        CompletedTutorial = true;
        _currentLevelReference.LevelRules = _levelRulesBeforeTutorial;
    }

    public void DidStartTutorial()
    {
        _levelRulesBeforeTutorial = _currentLevelReference.LevelRules;
        DeckManager.Instance.IsTutorial = true;
        _currentLevelReference.LevelRules = TutorialLevel;
    }

    public void DidCompleteLevel()
    {
        _levelIndex++;
        _currentLevelReference.LevelRules = levels[_levelIndex];
        DeckManager.Instance.ShuffleEverything();
    }

}
