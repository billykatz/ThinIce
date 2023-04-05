using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;
    
    [SerializeField] GameObject _tutorialView;
    [SerializeField] TMP_Text _tutorialTextView;
    [SerializeField] TMP_Text _tutorialNameView;
    [SerializeField] Image _tutorialSpeakerImage;

    [SerializeField] GameObject _tutorialDetailParent;
    [SerializeField] Image _tutorialDetailImage;
    [SerializeField] TMP_Text _tutorialDetailText;
    
    [SerializeField] TutorialStep[] _tutorialSteps;

    private int _tutorialStepIndex = 0;
    private int _innerStepCount = 0;
    private bool _tutorialComplete = false;

    [SerializeField] private InputActionReference _pointClicked; 
    [SerializeField] private InputActionReference _tutorialPointerClicked;
    [SerializeField] private ProgressController _progressController;

    private void Awake()
    {
        Instance = this;
        _tutorialPointerClicked.action.Enable();
    }

    private void OnDestroy()
    {
        _tutorialPointerClicked.action.Disable();
    }

    private void OnEnable()
    {
        _tutorialPointerClicked.action.performed += DidReceivePointerClick;
    }

    private void OnDisable()
    {
        _tutorialPointerClicked.action.performed -= DidReceivePointerClick;
    }

    private void DidReceivePointerClick(InputAction.CallbackContext ctx)
    {
        if (_tutorialView.activeSelf)
        {
            if (ctx.performed)
            {
                if (_innerStepCount < _tutorialSteps.Length)
                {
                    // tutorial has more inner steps, so lets show it
                    _innerStepCount++;
                    ShowTutorial(_tutorialSteps[_tutorialStepIndex]);
                }else if (_innerStepCount == _tutorialSteps.Length) 
                {
                    // tutorial step is over
                    HideTutorial();
                    _innerStepCount = 0;
                    _tutorialStepIndex++;
                    
                    if (_tutorialStepIndex == _tutorialSteps.Length)
                    {
                        // tutorial is over
                        _tutorialComplete = true;
                        _progressController.DidCompleteTutorial();
                    }
                }
                
            }
        }
    }

    public void EndGameState(GameState currentGameState)
    {
    }

    public void ChangeState(GameState newState)
    {
        if (_tutorialComplete)
            return;
        
        if (_tutorialSteps[_tutorialStepIndex].StateToTrigger == newState)
        {
            _innerStepCount = 0;
            ShowTutorial(_tutorialSteps[_tutorialStepIndex]);
        }
    }

    private void HideTutorial()
    {
        // let other things receives clicks now
        _pointClicked.action.Enable();
        
        _tutorialView.SetActive(false);
        _tutorialDetailParent.SetActive(false);
        
        _tutorialSpeakerImage.sprite = null;
        _tutorialTextView.text = "";
        _tutorialNameView.text = "";
        
        _tutorialDetailImage.sprite = null;
        _tutorialDetailText.text = "";
    }

    private void ShowTutorial(TutorialStep step)
    {
        // let other things receives clicks now
        _pointClicked.action.Disable();
        
        _tutorialView.SetActive(true);
        _tutorialDetailParent.SetActive(false);

        // set up the main tutorial
        _tutorialSpeakerImage.sprite = step.SpeakerImage[_innerStepCount];
        _tutorialTextView.text = step.Texts[_innerStepCount];
        _tutorialNameView.text = step.SpeakerName[_innerStepCount];
        
        // set up the detail view
        if (step.DetailImage[_innerStepCount] != null)
        {
            _tutorialDetailImage.sprite = step.DetailImage[_innerStepCount];
        }

        if (step.DetailText[_innerStepCount] != "")
        {
            _tutorialDetailParent.SetActive(true);
            _tutorialDetailText.text = step.DetailText[_innerStepCount];
        }

    }
    
}

