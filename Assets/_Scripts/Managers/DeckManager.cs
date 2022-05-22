using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    private void Awake() {
        Instance = this;
    }
}
