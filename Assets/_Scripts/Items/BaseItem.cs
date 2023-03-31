using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseItem : MonoBehaviour
{
    private ItemStat _stat;
    private int _amount;

    public void Setup(ItemStat stat, int amount)
    {
        _stat = stat;
        _amount = amount;
    }
}
