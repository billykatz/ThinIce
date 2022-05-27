using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThinIceToggle : MonoBehaviour
{
    public Image toggleImage;
    public Sprite toggleOn;
    public Sprite toggleOff;

    public void SetToggleOn(bool onOff) {
        if (onOff) {
            toggleImage.sprite = toggleOn;
        } else {
            toggleImage.sprite = toggleOff;
        }
    }
}
