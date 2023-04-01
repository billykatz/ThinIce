using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class WorldMapController : MonoBehaviour
{
    [FormerlySerializedAs("levelLocations")] [SerializeField] private Transform[] _levelLocations;
    [SerializeField] private GameObject _hero;

    [SerializeField] private ThinIceCanvasButton _playButton;


    public void PlayButtonPressed()
    {
        
    }

    public void DidSelectBackButton()
    {
        
    }
}
