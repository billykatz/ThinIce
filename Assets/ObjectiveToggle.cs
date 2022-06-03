using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveToggle : MonoBehaviour
{
   [SerializeField] Toggle toggle;
   [SerializeField] Text objectiveText;

   [SerializeField] Text progressText;

   public void SetObjectiveText(string txt) {
       objectiveText.text = txt;
   }

   public void SetProgressText(string txt) {
       progressText.text = txt;
   }

   public void SetToggle(bool isOn) {
       toggle.isOn = isOn;
   }

   
}
