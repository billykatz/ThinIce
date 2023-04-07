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
    
    
    private Action _callback;

    public void AnimateCombat(BaseUnit attackingUnit, BaseUnit defendingUnit, Action attackHitCallback, Action actionFinishedCallback)
    {
        AnimationData data = new AnimationData();
        data.StartPosition = attackingUnit.gameObject.transform.position;
        data.EndPosition = defendingUnit.gameObject.transform.position;
        _dataDictionary.Set(attackingUnit.gameObject.name, data);
        
        attackingUnit.PlayAttackAnimation(attackHitCallback, actionFinishedCallback);
    }
    
    public void Animate(GameObject owner, AnimationData animationData, FXView fxView)
    {
        Animate(owner, animationData, fxView, () => { });
    }
    
    public void Animate(GameObject owner, AnimationData animationData, FXView fxView, Action animationCompleteCallback)
    {
        string key = "" + owner.GetInstanceID();
        _dataDictionary.Set(key, animationData);

        FXView newFXView = Instantiate(fxView, Vector3.zero, Quaternion.identity);
        newFXView.name = key;
        newFXView.SetUp(owner.transform);
        
        newFXView.DidStop += Stopped;
        animatingObjects[owner.GetInstanceID()] = newFXView;
        newFXView.Play(animationCompleteCallback);
    }

    public void Animate(FXView fxView, AnimationData animationData)
    {
        string key = "" + fxView.gameObject.GetInstanceID();
        _dataDictionary.Set(key, animationData);
        
        fxView.DidStop += Stopped;
        fxView.name = key;
        animatingObjects[fxView.gameObject.GetInstanceID()] = fxView;
        fxView.Play();
    }

    
    public void CancelAnimation(GameObject owner)
    {
        animatingObjects[owner.GetInstanceID()].Cancel();
    }

    public void Stopped(FXView view)
    {
        Debug.Log($"FXView did stop {view}");
        view.DidStop -= Stopped;
        Destroy(view.gameObject);
    }
    
}
