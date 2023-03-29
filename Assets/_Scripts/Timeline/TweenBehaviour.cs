using UnityEngine;
using UnityEngine.Playables;

public class TweenBehaviour : PlayableBehaviour
{
    public Vector3 startPosition;
    public Vector3 endPosition;
    
    public Quaternion startRotation;
    public Quaternion endRotation;

    public bool shouldTweenPosition;
    public bool shouldTweenRotation;

    public AnimationCurve curve;

    public float EvaluateCurrentCurve (float time)
    {
        return curve.Evaluate(time);
    }
    
    bool IsCustomCurveNormalised ()
    {
        if (!Mathf.Approximately (curve[0].time, 0f))
            return false;
        
        if (!Mathf.Approximately (curve[0].value, 0f))
            return false;
        
        if (!Mathf.Approximately (curve[curve.length - 1].time, 1f))
            return false;
        
        return Mathf.Approximately (curve[curve.length - 1].value, 1f);
    }
}

