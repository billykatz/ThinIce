using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class TweenClip : PlayableAsset
{
    
    public bool shouldTweenPosition;
    public bool shouldTweenRotation;

    public AnimationCurve curve;

    public DynamicDataDictionary positionData;
    
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        ScriptPlayable<TweenBehaviour> playable = ScriptPlayable<TweenBehaviour>.Create(graph);
        TweenBehaviour tween = playable.GetBehaviour();
        
        tween.startPosition = positionData.GetValue(owner.name + "-start-position").position;
        tween.endPosition = positionData.GetValue(owner.name + "-end-position").position;

        tween.curve = curve;
        tween.shouldTweenPosition = shouldTweenPosition;
        tween.shouldTweenRotation = shouldTweenRotation;

        return playable;
    }
}
