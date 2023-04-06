using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

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
    [SerializeField] RawImage _tutorialDetailVideoParent;
    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private RenderTexture _renderTexture;
    
    [SerializeField] TutorialStep[] _tutorialSteps;
    [SerializeField] private CurrentLevelReference _level;

    private int _tutorialStepIndex = 0;
    private int _innerStepCount = 0;
    private bool _tutorialComplete = false;
    

    [SerializeField] private InputActionReference _pointClicked; 
    [SerializeField] private InputActionReference _tutorialPointerClicked;
    [SerializeField] private ProgressController _progressController;

    private float _clickSpamSafetyThreshold = 0.5f;
    private float _lastClickTime = 0;
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
            if (Time.time - _lastClickTime < _clickSpamSafetyThreshold)
            {
                return;
            }
            _lastClickTime = Time.time;
            
            if (ctx.performed)
            {
                if (_tutorialStepIndex < _tutorialSteps.Length && _innerStepCount < _tutorialSteps[_tutorialStepIndex].Texts.Length -1)
                {
                    // tutorial has more inner steps, so lets show it
                    _innerStepCount++;
                    ShowTutorial(_tutorialSteps[_tutorialStepIndex]);
                }
                else if (_tutorialStepIndex < _tutorialSteps.Length && _innerStepCount == _tutorialSteps[_tutorialStepIndex].Texts.Length-1) 
                {
                    // tutorial step is over
                    HideTutorial();
                    _innerStepCount = 0;
                    _tutorialStepIndex++;
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

        if (_tutorialStepIndex >= _tutorialSteps.Length)
        {
            // tutorial is over
            _tutorialComplete = true;
            return;
        }

        TutorialStep step = _tutorialSteps[_tutorialStepIndex];
        if (step.StateToTrigger == newState)
        {
            _innerStepCount = 0;
            // trigger on enemy spawn
            if (step.TriggerOnEnemySpawn && GridManager.Instance.GetEnemyTiles().Count > 0)
            {
                ShowTutorial(step);
                return; 
            }
            
            // trigger on goal spawn
            if (step.TriggerOnGoalSpawn && _level.LevelRules.Rows.Length <= _level.LevelRules.CurrentNumberRows)
            {
                ShowTutorial(step);
                return; 
            }

            // trigger on hazard
            if (step.TriggerOnHazardSpawn && GridManager.Instance.GetHazardTiles().Count > 0)
            {
                ShowTutorial(step);
                return; 
            }
            
            // trigger on item spawn
            if (step.TriggerOnItemSpawn && GridManager.Instance.GetItemTiles().Count > 0)
            {
                ShowTutorial(step);
                return; 
            }

            // trigger on just the state check
            if (step.NoTriggers)
            { 
                ShowTutorial(step);
                return;
            }
        }
    }

    public void EnemyDidDie()
    {
        if (_tutorialComplete)
            return;

        TutorialStep step = _tutorialSteps[_tutorialStepIndex];
        if (step.TriggerOnEnemyDead)
        {
            ShowTutorial(step);
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
        
        // tutorial detail text
        _tutorialDetailImage.sprite = null;
        _tutorialDetailText.text = "";
        
        // tutorial detail video
        _tutorialDetailVideoParent.texture = null;
        _tutorialDetailVideoParent.gameObject.SetActive(false);
        _videoPlayer.clip = null;
        _videoPlayer.targetTexture = null;
        _videoPlayer.Stop();
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
        if (step.DetailImage.Length > _innerStepCount && step.DetailImage[_innerStepCount] != null)
        {
            _tutorialDetailImage.sprite = step.DetailImage[_innerStepCount];
        }

        if (step.DetailText.Length > _innerStepCount && step.DetailText[_innerStepCount] != "")
        {
            _tutorialDetailParent.SetActive(true);
            _tutorialDetailText.text = step.DetailText[_innerStepCount];
        }
        
        if (step.PlayVideo)
        {
            _tutorialDetailParent.SetActive(true);
            _tutorialDetailVideoParent.gameObject.SetActive(true);
            
            // // give the image the render texture
            _tutorialDetailVideoParent.texture = _renderTexture;
            
            //set up the video player with the clip and render texture
            // _videoPlayer.targetTexture = step.DetailRenderTexture[_innerStepCount];
            // _videoPlayer.clip = step.DetailVideoClip[_innerStepCount];
            _videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath,"drag-a-card.mp4");
            _videoPlayer.targetTexture = _renderTexture;
            _videoPlayer.Play();
        }

    }
    
}

