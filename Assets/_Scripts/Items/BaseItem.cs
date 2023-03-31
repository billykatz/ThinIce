using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class BaseItem : MonoBehaviour
{
    public Tile OccupiedTile;
    public ItemStat Stat;
    public int Amount;
    

    [SerializeField] private PlayableDirector _playableDirector;
    private Action _animationCompletedCallback;
    public void Setup(ItemStat stat, int amount)
    {
        Stat = stat;
        Amount = amount;
    }

    private void OnEnable()
    {
        _playableDirector.stopped += DidStop;
    }

    private void OnDisable()
    {
        _playableDirector.stopped -= DidStop;
    }

    public void PlayCollectedAnimation(Action completion)
    {
        _animationCompletedCallback = completion;
        _playableDirector.Play();
    }

    void DidStop(PlayableDirector director)
    {
        _animationCompletedCallback?.Invoke();
    }
}
