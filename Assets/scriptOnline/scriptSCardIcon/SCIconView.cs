using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SCIconView : MonoBehaviour
{
    [SerializeField] Image IconImage;

    public void Show(SCIconModel iconModel)
    {
        IconImage.sprite = iconModel.Icon;
    }
}
