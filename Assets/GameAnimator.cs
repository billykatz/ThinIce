using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  The purpose of this class is to be able to play and sequence all animations that happen during gameplay 
/// </summary>
public class GameAnimator : MonoBehaviour
{
    [SerializeField] private DynamicDataDictionary _dataDictionary;
    
    public void Animate(GameObject owner, AnimationData animationData, FXView fxView)
    {
        _dataDictionary.Set(owner.name + "(Clone)-position-data", animationData);

        fxView.name = owner.name;
        FXView newFXView = Instantiate(fxView, Vector3.zero, Quaternion.identity);
        newFXView.SetUp(owner.transform);
        
        newFXView.Play();
    }
    
}
