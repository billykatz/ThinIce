using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

public class MermaidCupEnemy : BaseEnemy
{
    public override bool ShouldAttack(Tile currentTile, Tile playerTile)
    {
        return AttackVectors.Contains(playerTile.coord - currentTile.coord);
    }

    public override List<Vector2> AttackTiles(Tile currentTile)
    {

        List<Vector2> attackTiles = new List<Vector2>();
        for (int i = 0; i < AttackVectors.Length; i++)
        {
            Vector2 attackCoord = new Vector2(currentTile.coord.x + AttackVectors[i].x,
                currentTile.coord.y + AttackVectors[i].y);
            attackTiles.Add(attackCoord);
        }
        
        return attackTiles;

    }

    public override List<Vector2> WantToMoveTo(Tile currentTile, Tile playerTile)
    {
        // get Attack tiles from the player tile to know where we want to be.
        List<Vector2> idealAttackTiles = AttackTiles(playerTile);  
        int width = _level.LevelRules.Width;
        int minHeight = GridManager.Instance.BottomMostRowIndex;
        int maxHeight = _level.LevelRules.CurrentNumberRows;
        return idealAttackTiles.Where(coord => coord.IsInBounds(width, minHeight, maxHeight)).OrderBy(coord => Vector2.Distance(currentTile.coord, coord)).ToList();


    }
}
