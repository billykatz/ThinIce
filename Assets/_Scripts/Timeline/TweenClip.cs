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

        AnimationData data = positionData.GetValue(owner.name + "-position-data");
        tween.startPosition = data.StartPosition;
        tween.endPosition = data.EndPosition;
        
        
        tween.startRotation = data.StartRotation;
        tween.endRotation = data.EndRotation;

        tween.curve = curve;
        tween.shouldTweenPosition = shouldTweenPosition;
        tween.shouldTweenRotation = shouldTweenRotation;

        return playable;
    }
}
