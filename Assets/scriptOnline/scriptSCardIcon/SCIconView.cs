using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SCIconView : MonoBehaviour
{
    [SerializeField] Image IconImage;
    [SerializeField] GameObject selectPanel;

    public void Show(SCIconModel iconModel)
    {
        IconImage.sprite = iconModel.Icon;
    }

    public void SetActiveSelectPanel(bool flag)
    {
        selectPanel.SetActive(flag);
    }
}
