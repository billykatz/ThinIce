using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject _worldMapView;
    public void DidPressStartButton()
    {
        
        _worldMapView.transform.DOMove(Vector3.zero, 1f);
    }
    public void DidPressTutorialButton()
    {
        
    }
    
    public void DidPressBackButtonFromWorldMap()
    {
        
    }
}
