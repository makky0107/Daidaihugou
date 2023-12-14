using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "SpecialCard", menuName = "Create SEntity")]
//カードデータそのもの

public class SCardEntity : ScriptableObject
{
    public int sCardNo;
    public Sprite image;
    public string title;
    public string text;
}