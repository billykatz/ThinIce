using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceTile : Tile
{
    [SerializeField] private Color _colorOne, _colorTwo;
    


    public override void Init(int x, int y)
    {
        var computedColor = (x + y) % 2 == 1 ? _colorOne : _colorTwo;
        _spriteRenderer.color = computedColor;
    }
}
