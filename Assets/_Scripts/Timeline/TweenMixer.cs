using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Freya;

public class TweenMixerBehaviour : PlayableBehaviour
{
    bool m_FirstFrameHappened;
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Transform trackBinding = playerData as Transform;

        if (trackBinding == null)
            return;

        Vector3 defaultPosition = trackBinding.position;
        Quaternion defaultRotation = trackBinding.rotation;

        int inputCount = playable.GetInputCount();

        float positionTotalWeight = 0f;
        float rotationTotalWeight = 0f;

        Vector3 blendedPosition = trackBinding.position;
        Quaternion blendedRotation = new Quaternion(0f,0f,0f,0f);
        
        for (int i = 0; i < inputCount; i++)
        {
            ScriptPlayable<TweenBehaviour> playableInput = (ScriptPlayable<TweenBehaviour>)playable.GetInput(i);
            TweenBehaviour input = playableInput.GetBehaviour();

            float inputWeight = playable.GetInputWeight(i);

            float normalisedTime = (float)(playableInput.GetTime() / playableInput.GetDuration ());
            float tweenProgress = input.EvaluateCurrentCurve(normalisedTime);
            
            if (input.shouldTweenPosition && inputWeight > 0f)
            {
                blendedPosition = Mathfs.Lerp(input.startPosition, input.endPosition, new Vector3(tweenProgress, tweenProgress, tweenProgress)) * inputWeight;
            }
            
            if (input.shouldTweenRotation)
            {
                rotationTotalWeight += inputWeight;
            
                Quaternion desiredRotation = Quaternion.Lerp(input.startRotation, input.endRotation, tweenProgress);
                desiredRotation = NormalizeQuaternion(desiredRotation);
            
                if (Quaternion.Dot (blendedRotation, desiredRotation) < 0f)
                {
                    desiredRotation = ScaleQuaternion (desiredRotation, -1f);
                }
            
                desiredRotation = ScaleQuaternion(desiredRotation, inputWeight);
            
                blendedRotation = AddQuaternions (blendedRotation, desiredRotation);
            }

        }
        
        // blendedPosition += input.startPosition.position * (1f - positionTotalWeight);
        Quaternion weightedDefaultRotation = ScaleQuaternion (defaultRotation, 1f - rotationTotalWeight);
        blendedRotation = AddQuaternions (blendedRotation, weightedDefaultRotation);

        trackBinding.position = blendedPosition;
        trackBinding.rotation = blendedRotation;
        
        m_FirstFrameHappened = true;
    }
    
    public override void OnPlayableDestroy (Playable playable)
    {
        m_FirstFrameHappened = false;
    }
    
    static Quaternion AddQuaternions (Quaternion first, Quaternion second)
    {
        first.w += second.w;
        first.x += second.x;
        first.y += second.y;
        first.z += second.z;
        return first;
    }
    
    static Quaternion ScaleQuaternion (Quaternion rotation, float multiplier)
    {
        rotation.w *= multiplier;
        rotation.x *= multiplier;
        rotation.y *= multiplier;
        rotation.z *= multiplier;
        return rotation;
    }
    
    static float QuaternionMagnitude (Quaternion rotation)
    {
        return Mathf.Sqrt ((Quaternion.Dot (rotation, rotation)));
    }
    
    static Quaternion NormalizeQuaternion (Quaternion rotation)
    {
        float magnitude = QuaternionMagnitude (rotation);
    
        if (magnitude > 0f)
            return ScaleQuaternion (rotation, 1f / magnitude);
    
        Debug.LogWarning ("Cannot normalize a quaternion with zero magnitude.");
        return Quaternion.identity;
    }
}
