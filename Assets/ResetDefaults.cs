using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetDefaults : MonoBehaviour
{
    public ProgressController _progressController;
    // Start is called before the first frame update
    void Start()
    {
        #if UNITY_EDITOR
        _progressController._haveInitiatedStarterDeck = false;
        _progressController.DebugResetStats();
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
