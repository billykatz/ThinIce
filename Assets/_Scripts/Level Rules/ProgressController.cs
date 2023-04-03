using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "New Progress Controller", menuName = "Progress Controller")]
public class ProgressController : ScriptableObject
{
    [SerializeField] private CurrentLevelReference _currentLevelReference;
    [SerializeField] private ScriptableLevelRules[] levels;
    [SerializeField] private int _levelIndex;

    private void Awake()
    {
        _currentLevelReference.LevelRules = levels[_levelIndex];
    }
    
    public ScriptableLevelRules CurrentLevel()
    {
        return levels[_levelIndex];
    }
    
    public int CurrentLevelIndex()
    {
        return _levelIndex;
    }

    public void DidCompleteLevel()
    {
        _levelIndex++;
        _currentLevelReference.LevelRules = levels[_levelIndex];
    }

}
