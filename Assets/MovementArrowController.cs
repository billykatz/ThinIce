using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementArrowController : MonoBehaviour
{

    [SerializeField] ThinIceButton leftArrow;
    [SerializeField] ThinIceButton rightArrow;
    [SerializeField] ThinIceButton upArrow;
    [SerializeField] ThinIceButton downArrow;


    public delegate void ArrowTapped(GridMovement movement);
    public event ArrowTapped OnArrowTapped;

    public void SetArrows(List<GridMovement> movements) {
        leftArrow.gameObject.SetActive(false);
        rightArrow.gameObject.SetActive(false);
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);


        foreach (GridMovement movement in movements) {
            switch (movement) {
                case GridMovement.Left:
                    leftArrow.gameObject.SetActive(true);
                    leftArrow.ResetButton();
                    leftArrow.OnClicked += LeftArrowClicked;
                    break;
                case GridMovement.Right:
                    rightArrow.gameObject.SetActive(true);
                    rightArrow.ResetButton();
                    rightArrow.OnClicked += RightArrowClicked;
                    break;
                case GridMovement.Up:
                    upArrow.gameObject.SetActive(true);
                    upArrow.ResetButton();
                    upArrow.OnClicked += UpArrowClicked;
                    break;
                case GridMovement.Down:
                    downArrow.gameObject.SetActive(true);
                    downArrow.ResetButton();
                    downArrow.OnClicked += DownArrowClicked;
                    break;
            }
        }
    }

    private void OnDisable() {
        leftArrow.OnClicked -= LeftArrowClicked;
        rightArrow.OnClicked -= RightArrowClicked;
        upArrow.OnClicked -= UpArrowClicked;
        downArrow.OnClicked -= DownArrowClicked;
    }

    private void LeftArrowClicked() {
        if (OnArrowTapped != null) {
            OnArrowTapped(GridMovement.Left);
        }
    }

    private void RightArrowClicked() {
        if (OnArrowTapped != null) {
            OnArrowTapped(GridMovement.Right);
        }
    }

    private void UpArrowClicked() {
        if (OnArrowTapped != null) {
            OnArrowTapped(GridMovement.Up);
        }
    }

    private void DownArrowClicked() {
        if (OnArrowTapped != null) {
            OnArrowTapped(GridMovement.Down);
        }
    }
    
}
