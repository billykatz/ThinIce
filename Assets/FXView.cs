using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


public class FXView : MonoBehaviour
{
    [SerializeField] private PlayableDirector _director;
    [SerializeField] private TrackAsset bindingTrack;
    public Action<FXView> DidStop;
    
    public void SetUp(Transform transformToAnimate)
    {
        // Take the transform and bind it to the track
        _director.SetGenericBinding(bindingTrack, transformToAnimate);
    }

    public void Play()
    {
        _director.Play();
        _director.stopped += Stopped;

    }
    
    public void Cancel()
    {
        if (_director != null)
        {
            _director.Stop();
        }
    }

    public void Stopped(PlayableDirector director)
    {
        DidStop.Invoke(this);
    }
}
