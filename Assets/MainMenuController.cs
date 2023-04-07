using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject _worldMapView;
    [SerializeField] private WorldMapController _worldMapController;
    private Vector3 _originalWorlMapPosition;
    [SerializeField] private ProgressController _progressController;

    private void Awake()
    {
        _originalWorlMapPosition = _worldMapView.transform.position;
    }

    public void DidPressStartButton()
    {
        
        _worldMapView.transform.DOMove(Vector3.zero, 1f).OnComplete(() =>
        {
            _worldMapController.DidAppearOnScreen();
        });

        Vector3 mainMenuOffScreenPosition = transform.position;
        mainMenuOffScreenPosition.y -= _originalWorlMapPosition.y;
        transform.DOMove(mainMenuOffScreenPosition, 1f);
    }
    public void DidPressTutorialButton()
    {
        _progressController.DidStartTutorial();
        ThinIceSceneManager.Instance.LoadTutorial();
    }
    
    public void DidPressBackButtonFromWorldMap()
    {
        _worldMapView.transform.DOMove(_originalWorlMapPosition, 1f);
        transform.DOMove(Vector3.zero, 1f);
    }
}
