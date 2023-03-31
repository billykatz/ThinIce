using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


public class FXView : MonoBehaviour
{
    [SerializeField] private PlayableDirector _director;
    [SerializeField] private TrackAsset bindingTrack;
    [SerializeField] private TextMeshPro _textfield;
    public Action<FXView> DidStop;
    
    private Action _animationComplete;
    
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
    
    public void Play(Action animationCompleteCallback)
    {
        _director.Play();
        _director.stopped += Stopped;
        _animationComplete = animationCompleteCallback;

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
        _animationComplete?.Invoke();
    }

    public void SetText(string text)
    {
        if (_textfield != null)
        {
            _textfield.text = text;
        }
    }
}
