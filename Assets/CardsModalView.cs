
using TMPro;
using UnityEngine;

public class CardsModalView : MonoBehaviour
{

    [SerializeField] private TMP_Text _titleTextField;

    
    void SetTitle(string text)
    {
        _titleTextField.text = text;
    }

    public void DidSelectConfirm()
    {
        
    }
}
