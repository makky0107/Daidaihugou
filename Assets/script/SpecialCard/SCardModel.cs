using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]

public class SCardModel
{
    public int sCardNo;
    public Sprite image;
    public string title;
    public string text;

    public SCardModel(int sCardID)
    {
        SCardEntity SCardEntity = Resources.Load<SCardEntity>("SpecialCards/SCard" + sCardID);
        sCardNo = SCardEntity.sCardNo;
        image = SCardEntity.image;
        title = SCardEntity.title;
        text = SCardEntity.text;
    }
}
