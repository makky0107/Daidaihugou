using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "Create CardEntity")]
//カードデータそのもの

public class CardEntity : ScriptableObject
{
    public int ID;
    public int Suit;
    public int Number;
    public int Strenge;
    public int UpsideDown;
    public bool Joker;
    public Sprite Icon;

}