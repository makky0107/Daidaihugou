using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SCardIcon", menuName = "Create SCardIcon")]
//カードデータそのもの

public class SCardIconEntity : ScriptableObject
{
    public int ID;
    public Sprite Icon;
}
