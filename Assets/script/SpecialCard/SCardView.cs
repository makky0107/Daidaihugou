using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SCardView : MonoBehaviour
{
    public Image image;
    public Text title;
    public Text text;

    public void Show(SCardModel model)
    {
        image.sprite = model.image;
        title.text = model.title;
        text.text = model.text;
    }
}
