using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    private void Awake() {
        Instance = this;
    }
}
