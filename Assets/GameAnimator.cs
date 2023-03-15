using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  The purpose of this class is to be able to play and sequence all animations that happen during gameplay 
/// </summary>
public class GameAnimator : MonoBehaviour
{
    [SerializeField] private DynamicDataDictionary _dataDictionary;
    private Dictionary<int, FXView> animatingObjects = new Dictionary<int, FXView>();
    
    public void Animate(GameObject owner, AnimationData animationData, FXView fxView)
    {
        string key = "" + owner.GetInstanceID();
        _dataDictionary.Set(key, animationData);

        FXView newFXView = Instantiate(fxView, Vector3.zero, Quaternion.identity);
        newFXView.name = key;
        newFXView.SetUp(owner.transform);

        // FXView test;
        // if (animatingObjects.TryGetValue(owner.GetInstanceID(), out test))
        // {
        //     Debug.Log("Animator: This object is already being animated.");
        //     return;
        // }
        newFXView.DidStop += Stopped;
        animatingObjects[owner.GetInstanceID()] = newFXView;
        newFXView.Play();
    }

    public void CancelAnimation(GameObject owner)
    {
        animatingObjects[owner.GetInstanceID()].Cancel();
    }

    public void Stopped(FXView view)
    {
        view.DidStop -= Stopped;
        Destroy(view.gameObject);
    }
    
}
