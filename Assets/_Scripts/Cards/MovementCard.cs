using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovementCard : BaseCard
{
    public List<Movement> movement;
    public int movementIndex = 0;

    public void SetFullCardHighlight(bool onOff) {
        GameObject child = transform.Find("FullCardHighlight").gameObject;
        child.SetActive(onOff);
    }

    public List<GridMovement> GetGridMovement(int movementIndex) {
        List<GridMovement> gridMovements = new List<GridMovement>();
        switch (movement[movementIndex].movementType) {
            case MovementType.Up:
                gridMovements.Add(GridMovement.Up);
                break;
            case MovementType.Down:
                gridMovements.Add(GridMovement.Down);
                break;
            case MovementType.Left:
                gridMovements.Add(GridMovement.Left);
                break;
            case MovementType.Right:
                gridMovements.Add(GridMovement.Right);
                break;
            case MovementType.UpLeft:
                break;
            case MovementType.UpRight:
                break;
            case MovementType.DownRight:
                break;
            case MovementType.DownLeft:
                break;
            case MovementType.LeftOrRight:
                gridMovements.Add(GridMovement.Left);
                gridMovements.Add(GridMovement.Right);
                break;
            case MovementType.UpOrDown:
                gridMovements.Add(GridMovement.Up);
                gridMovements.Add(GridMovement.Down);
                break;
            case MovementType.Any:
                break;
            case MovementType.Teleport:
                break;
            case MovementType.None:
                gridMovements.Add(GridMovement.None);
                break;
        }
        return gridMovements;
    }

    public bool CanMoveForGridMovement(GridMovement gridMovement, int movementIndex) {
        switch (movement[movementIndex].movementType) {
            case MovementType.Up:
                return gridMovement == GridMovement.Up;
            case MovementType.Down:
                return gridMovement == GridMovement.Down;
            case MovementType.Left:
                return gridMovement == GridMovement.Left;
            case MovementType.Right:
                return gridMovement == GridMovement.Right;
            case MovementType.UpLeft:
                return false;
            case MovementType.UpRight:
                return false;
            case MovementType.DownRight:
                return false;
            case MovementType.DownLeft:
                return false;
            case MovementType.LeftOrRight:
                return gridMovement == GridMovement.Left || gridMovement == GridMovement.Right;
            case MovementType.UpOrDown:
                return gridMovement == GridMovement.Up || gridMovement == GridMovement.Down;
            case MovementType.Any:
                return true;
            case MovementType.Teleport:
                return false;
            case MovementType.None:
                return true;
        }
        return false;
    }

    public string MovementTutorialText(int movementIndex) {
        switch (movement[movementIndex].movementType) {
            case MovementType.Up:
                return "Moving 1 tile up";
            case MovementType.Down:
                return "Moving 1 tile down";
            case MovementType.Left:
                return "Moving 1 tile left";
            case MovementType.Right:
                return "Moving 1 tile Right";
            case MovementType.UpLeft:
                return "Moving 1 tile Up and Left";
            case MovementType.UpRight:
                return "Moving 1 tile Up and Right";
            case MovementType.DownRight:
                return "Moving 1 tile Down and Right";
            case MovementType.DownLeft:
                return "Moving 1 tile Down and Left";
            case MovementType.LeftOrRight:
                return "Use arrow keys to move 1 tile Left or Right";
            case MovementType.UpOrDown:
                return "Moving 1 tile Up or Down";
            case MovementType.Any:
                return "Moving 1 tile in any direction";
            case MovementType.Teleport:
                return "Moving to any tile";
            case MovementType.None:
                return "Staying in place";
        }

        return "Error";
    } 
}

[System.Serializable]
public class Movement {
    public MovementType movementType;
    public MovementChoice movementChoice;
}

[System.Serializable]
public enum MovementChoice {
    NoChoice = 0,
    Choice = 1
}

[System.Serializable]
public enum MovementType {
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
    UpLeft = 4, 
    UpRight = 5, 
    DownRight = 6, 
    DownLeft = 7,
    LeftOrRight = 8,
    UpOrDown = 9,
    Any = 10,
    Teleport = 11,
    None = 12
}