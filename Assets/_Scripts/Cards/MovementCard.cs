using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovementCard : BaseCard
{
    [SerializeField] List<Movement> movement;

    public void SetFullCardHighlight(bool onOff) {
        GameObject child = transform.Find("FullCardHighlight").gameObject;
        child.SetActive(onOff);
    }
}