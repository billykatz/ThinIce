using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance;
    
    [SerializeField] private ScriptableLevelRules[] levels;
    [SerializeField] private int _levelIndex = 1;


    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public ScriptableLevelRules CurrentLevel()
    {
        return levels[_levelIndex];
    }
    
    public int CurrentLevelIndex()
    {
        return _levelIndex;
    }

}
