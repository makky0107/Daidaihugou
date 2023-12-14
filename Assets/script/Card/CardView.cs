using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField] Image IconImage;
    [SerializeField] GameObject selectablePanel;

    public void Show(CardModel cardModel)
    {
        IconImage.sprite = cardModel.Icon;
    }
    /*
    public void SetCannotSelectPanel()
    {
        selectablePanel.set
    }
    */
}
