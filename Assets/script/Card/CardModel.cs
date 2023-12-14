using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class CardModel
{
    public int ID;
    public int Suit;
    public int Number;
    public int Strenge;
    public int UpsideDown;
    public bool Joker;
    public Sprite Icon;

    public CardModel(int cardID)
    {
        CardEntity cardEntity = Resources.Load<CardEntity>("Cards/Card" + cardID);
        ID = cardEntity.ID;
        Suit = cardEntity.Suit;
        Number = cardEntity.Number;
        Strenge = cardEntity.Strenge;
        UpsideDown = cardEntity.UpsideDown;
        Joker = cardEntity.Joker;
        Icon = cardEntity.Icon;
    }

}
