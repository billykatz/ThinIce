using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

public class MermaidCupEnemy : BaseEnemy
{
    public override bool ShouldAttack(Tile currentTile, Tile playerTile)
    {
        List<Vector2> attackTiles = AttackTiles(currentTile);
        return attackTiles.Contains(playerTile.coord);
    }

    public override List<Vector2> AttackTiles(Tile currentTile) {
        Vector2 currentCoord = new Vector2((float)currentTile.x, (float)currentTile.y);

        Vector2 attackCoord1 = new Vector2((float)currentTile.x-2, (float)currentTile.y);
        Vector2 attackCoord2 = new Vector2((float)currentTile.x+2, (float)currentTile.y);
        Vector2 attackCoord3 = new Vector2((float)currentTile.x, (float)currentTile.y-2);
        Vector2 attackCoord4 = new Vector2((float)currentTile.x, (float)currentTile.y+2);

        List<Vector2> attackTiles = new List<Vector2>();
        attackTiles.Add(attackCoord1);
        attackTiles.Add(attackCoord2);
        attackTiles.Add(attackCoord3);
        attackTiles.Add(attackCoord4);

        return attackTiles;

    }

    public override List<Vector2> WantToMoveTo(Tile currentTile, Tile playerTile)
    {
        // get attack tiles from the player tile to know where we want to be.
        List<Vector2> idealAttackTiles = AttackTiles(playerTile);  
        int width = _levelRules.Width;
        int minHeight = GridManager.Instance.BottomMostRowIndex;
        int maxHeight = _levelRules.CurrentNumberRows;
        return idealAttackTiles.Where(coord => coord.IsInBounds(width, minHeight, maxHeight)).OrderBy(coord => Vector2.Distance(currentTile.coord, coord)).ToList();


    }
}
