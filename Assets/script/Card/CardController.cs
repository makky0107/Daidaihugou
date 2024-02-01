using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

public class CardController : MonoBehaviourPunCallbacks
{
    CardView view;
    public PlayerHand hand;
    public CardModel model;
    public GameObject shadow;
    public GameObject theBackSide;
    public CardMovement cardMovement;

    public int turnCount;
    public int stairsCount;

    public bool isSelected;
    public bool canSelect;
    public bool stairs;
    public bool sameCard;

    public string myHand = "OwnHand";
    public string myField = "OwnField";

    List<CardController> searchedCards;
    List<CardController> holdJCards = new List<CardController>();

    private void Awake()
    {
        view = GetComponent<CardView>();
        canSelect = true;
        ShadowUpdate();
    }

    [PunRPC]
    public void Init(int cardID)
    {
        model = new CardModel(cardID);
        view.Show(model);
        hand = GetComponentInParent<PlayerHand>();
        if (hand != null)
        {
            hand.allCards.Add(this);
        }
        TheBackSide();
    }

    [PunRPC]
    public void OnSelectedCard()
    {
        if (isSelected == false)
        {
            transform.localPosition += Vector3.up * 50;
            isSelected = true;
            if (hand != null)
            {
                hand.selectedCards.Add(this);
            }
        }
        else
        {
            transform.localPosition -= Vector3.up * 50;
            isSelected = false;
            sameCard = false;
            stairs = false;

            if (model.Joker)
            {
                JokerNumberRiset();
            }
            if (this == hand.theFirst)
            {
                hand.theFirst = null;
            }

            hand.selectedCards.Remove(this);
        }
    }



    [PunRPC]
    public void OnClickCard()
    {
        Debug.Log("<color=yellow><size=18>Click model.Number</size></color>" + model.Number);

        if (canSelect && hand.isTurn)
        {
            if (model.Joker)
            {
                holdJCards.Clear();
                holdJCards = hand.allCards.Where(x => x.canSelect == true).ToList();
            }

            hand.stairsList.Clear();
            stairsCount = 0;

            OnSelectedCard();

            /*
            foreach (var card in hand.allCards.Where(card => card.canSelect == true).ToList())
            {
                Debug.Log($"<color=purple><size=18>CanSelectCard 1 No {card.model.Number}</size></color>" + $"<color=purple><size=18> Suit {card.model.Suit}</size></color>");
            }
            */

            foreach (var card in hand.allCards)
            {
                if (!card.isSelected)
                {
                    card.canSelect = false;
                }
            }

            int holdJoker = hand.selectedCards.Count - hand.selectedCards.Where(card => card.model.Joker && card.isSelected).ToList().Count;

            if (hand.fieldCount > 0 && hand.fieldCount <= hand.selectedCards.Count)
            {
                foreach (var card in hand.allCards)
                {
                    if (!card.isSelected)
                    {
                        card.canSelect = false;
                    }
                }
            }
            else if (hand.fieldCount > 0 && hand.fieldCount > hand.selectedCards.Count)
            {
                CardController isSelectedCard = hand.canSelectCards.Find(card => card.isSelected == true && !card.model.Joker);

                if (isSelectedCard)
                {
                    if (hand.theFirst == null && !isSelectedCard.model.Joker)
                    {
                        hand.theFirst = isSelectedCard;

                        Debug.Log($"<color=grey><size=20>TheFirst {hand.theFirst.model.Number} {hand.theFirst.model.Suit}</size></color>");
                    }
                }


                if (!model.Joker)
                {
                    if (holdJoker >= 1)
                    {
                        if (!hand.sameCancel)
                        {
                            SameCardsP(hand.theFirst);
                        }
                        if (!hand.stairCancel)
                        {
                            StairsP(hand.theFirst);
                        }

                        foreach (var card in hand.allCards)
                        {
                            card.sameCard = false;
                            card.stairs = false;
                        }
                    }

                    if (holdJoker >= 2)
                    {
                        IsSelectedStatus();

                        if (sameCard)
                        {
                            StairsCancelETC();
                        }
                        if (stairs)
                        {
                            SameCardCancel();
                        }
                    }
                }

                if (hand.selectedCards.Count >= 3)
                {
                    var jokerCard = hand.selectedCards.Find(card => card.model.Joker);

                    if (jokerCard)
                    {
                        JokerSetNumberThree();
                        Debug.Log("<color=purple><size=20>JOKER</size></color> " + jokerCard.model.Strenge + " " + jokerCard.model.UpsideDown);
                    }
                }

                if (model.Joker)
                {
                    foreach (var card in holdJCards)
                    {
                        card.canSelect = true;
                    }
                }

                if (holdJoker == 0)
                {
                    Debug.Log("<color=yellor>hand.canSelectCards.Count</color>" + hand.canSelectCards.Count);
                    ResetHandP();
                }
            }
            else if (hand.fieldCount == 0)
            {
                CardController isSelectedCard = hand.allCards.Find(card => card.isSelected == true);

                if (isSelectedCard)
                {
                    if (hand.theFirst == null && !isSelectedCard.model.Joker)
                    {
                        hand.theFirst = isSelectedCard;

                        Debug.Log("<color=grey><size=20>TheFirst </size></grey>" + hand.theFirst.model.Number + " " + hand.theFirst.model.Suit);
                    }
                }

                if (!model.Joker)
                {
                    if (holdJoker >= 1)
                    {
                        SameCards(hand.theFirst);
                        Stairs(hand.theFirst);

                        foreach (var card in hand.allCards)
                        {
                            card.sameCard = false;
                            card.stairs = false;
                        }
                    }

                    if (holdJoker >= 2)
                    {
                        IsSelectedStatus();

                        if (sameCard)
                        {
                            StairsCancelETC();
                        }
                        if (stairs)
                        {
                            SameCardCancel();
                        }
                    }
                }

                if (hand.selectedCards.Count >= 3)
                {
                    var jokerCard = hand.selectedCards.Find(card => card.model.Joker);

                    if (jokerCard)
                    {
                        JokerSetNumberThree();
                    }
                }

                if (model.Joker)
                {
                    foreach (var card in holdJCards)
                    {
                        card.canSelect = true;
                    }
                }

                if (holdJoker == 0)
                {
                    ResetHand();
                }
            }

            /*
            foreach (var card in hand.allCards.Where(card => card.canSelect == true).ToList())
            {
                Debug.Log("<color=purple><size=18>CanSelectCard 2 No </size></color>" + card.model.Number + "<color=purple><size=18> Suit </size></color>" + card.model.Suit);
            }
            */

            CardUpdate();
        }
    }

    [PunRPC]
    public void CardUpdate()
    {
        foreach (CardController card in hand.allCards)
        {
            card.ShadowUpdate();
            card.cardMovement.isDrag = false;
            if (card.canSelect == false && card.isSelected == true)
            {
                card.photonView.RPC("OnSelectedCard", PhotonNetwork.LocalPlayer);
            }
        }
    }

    [PunRPC]
    public void ShadowUpdate()
    {
        if (canSelect && shadow.activeSelf)
        {
            shadow.SetActive(false);
        }
        else if (!canSelect && !shadow.activeSelf)
        {
            shadow.SetActive(true);
        }

        //Debug.LogWarning($"<size=24><color=yellow>ShadowUpdate{shadow.activeSelf}</color></size>");
    }

    [PunRPC]
    public void TheBackSide()
    {
        string parentName = transform.parent.name;
        if (parentName == myHand || parentName == myField)
        {
            BackSideNotActive();
        }
    }

    [PunRPC]
    public void BackSideNotActive()
    {
        theBackSide.SetActive(false);
    }
    
    public void BackSideActive()
    {
        theBackSide.SetActive(true);
    }

    [PunRPC]
    void Stairs(CardController isSelectedCard)
    {
        Debug.Log("<size=20>StairsSeach</size>");

        searchedCards = new List<CardController>(hand.allCards);
        hand.stairsList.Add(isSelectedCard);

        StairsSearch(hand.stairsList, searchedCards, isSelectedCard, 1);
        StairsSearch(hand.stairsList, searchedCards, isSelectedCard, -1);

        if (hand.stairsList.Count >= 3)
        {
            foreach (CardController card in hand.stairsList)
            {
                //Debug.Log("<color=brown><size=18>StairsCard No </size></color>" + card.model.Number + "<color=brown><size=18> Suit </size></color>" + card.model.Suit);

                card.canSelect = true;
            }
        }
    }

    [PunRPC]
    public void StairsSearch(List<CardController> cardList, List<CardController> searchedCards, CardController targetCard, int direction)
    {
        var list = searchedCards.FindAll(card => card.model.Suit == targetCard.model.Suit);
        CardController serchCard = list.Find(card => card.model.Strenge == targetCard.model.Strenge + direction);
        CardController seachJoker = searchedCards.Find(card => card.model.Joker == true);
        CardController afterJoker;

        if (serchCard)
        {
            cardList.Add(serchCard);
            searchedCards.Remove(serchCard);

            if (direction == 2 || direction == -2)
            {
                direction /= 2;
            }

            StairsSearch(cardList, searchedCards, serchCard, direction);
        }
        else if(seachJoker)
        {
            if (direction == 1)
            {
                afterJoker = cardList.Find(card => card.model.Strenge == (cardList.Max(card => card.model.Strenge)));
            }
            else
            {
                afterJoker = cardList.Find(card => card.model.Strenge == (cardList.Min(card => card.model.Strenge)));
            }

            cardList.Add(seachJoker);
            searchedCards.Remove(seachJoker);

            StairsSearch(cardList, searchedCards, afterJoker, direction + direction);
        }
        else
        {
            return;
        }
    }

    [PunRPC]
    public void SameCards(CardController selectCard)
    {
        foreach (CardController card in hand.allCards)
        {
            if (card.model.Strenge == selectCard.model.Strenge)
            {
                //Debug.Log("<color=brown><size=18>SameCard No </size></color>" + card.model.Number + "<color=brown><size=18> Suit </size></color>" + card.model.Suit);

                card.canSelect = true;
            }
            if (card.model.Strenge != selectCard.model.Strenge)
            {
                card.canSelect = false;
            }
            if (card.model.Joker)
            {
                //Debug.Log("<color=brown><size=18>SameCard No </size></color>" + card.model.Number + "<color=brown><size=18> Suit </size></color>" + card.model.Suit);

                card.canSelect = true;
            }
        }
    }

    [PunRPC]
    void JokerSetNumberThree()
    {
        List<CardController> jokersSearch = hand.selectedCards.FindAll(card => card.model.Joker == true && card.isSelected);
        List<CardController> jokers = new List<CardController>(jokersSearch);
        List<CardController> listStairs = new List<CardController>();

        hand.selectedCards.RemoveAll(card => card.model.Joker);

        if (hand.selectedCards.Max(card => card.model.Strenge) == hand.selectedCards.Min(card => card.model.Strenge))
        {
            for (int i = 0; i <= jokers.Count - 1; i++)
            {
                jokers[i].model.Strenge = hand.selectedCards[i].model.Strenge;
                jokers[i].model.UpsideDown = hand.selectedCards[i].model.UpsideDown;
                hand.selectedCards.Add(jokers[i]);
                jokers.Remove(jokers[i]);
            }
        }
        if (hand.selectedCards.Max(card => card.model.Strenge) != hand.selectedCards.Min(card => card.model.Strenge))
        {
            var orderStrairs = hand.selectedCards.OrderBy(card => card.model.Strenge);
            listStairs = orderStrairs.ToList();
            JokerSetStairsThree(jokers, listStairs, 0);
            hand.selectedCards = listStairs;
        }
    }

    [PunRPC]
    void JokerSetStairsThree(List<CardController> jokers, List<CardController> listStairs, int No)
    {
        if (jokers.Count > 0)
        {
            CardController joker = jokers.Find(card => card.model.Joker);

            if (listStairs.Find(card => card.model.Strenge == listStairs[No].model.Strenge + 1) != null)
            {
                JokerSetStairsThree(jokers, listStairs, No + 1);
            }
            else
            {
                joker.model.Strenge = listStairs[No].model.Strenge + 1;
                joker.model.UpsideDown = listStairs[No].model.UpsideDown - 1;

                listStairs.Add(joker);
                jokers.Remove(joker);

                JokerSetStairsThree(jokers, listStairs, No + 1);
            }  
        }
        else
        {
            return;
        }
    }

    [PunRPC]
    void JokerNumberRiset()
    {
        foreach (CardController card in hand.allCards)
        {
            if (card.model.Joker)
            {
                card.model.Strenge = card.model.UpsideDown = 14;
            }
        }
    }

    [PunRPC]
    void IsSelectedStatus()
    {
        List<CardController> holdJoker = new List<CardController>(hand.selectedCards.Where(x => x.model.Joker == false));

        if (holdJoker[0].model.Strenge == holdJoker[1].model.Strenge)
        {
            sameCard = true;
        }
        else
        {
            stairs = true;

        }
    }

    [PunRPC]
    void SameCardCancel()
    {
        Debug.Log("<color=blue>sameCancel</color>");

        foreach (CardController listcard in hand.selectedCards)
        {
            foreach (CardController card in hand.allCards)
            {
                if (card.model.Strenge == listcard.model.Strenge && card.model.Suit != listcard.model.Suit)
                {
                    card.canSelect = false;
                }
            }
        }

    }

    [PunRPC]
    void StairsCancelETC()
    {
        if (hand.stairsList.Any())
        {
            Debug.Log("<color=blue>StairsCancelETC</color>");
            Debug.Log("<color=blue>StairsCancel Set No</color>" + hand.selectedCards[0].model.Number);

            foreach (CardController card in hand.stairsList)
            {
                if (!card.isSelected && !card.model.Joker)
                {
                    card.canSelect = false;
                }
            }
        }

        hand.stairsList = new List<CardController>();

        foreach (var card in hand.allCards)
        {
            if (hand.selectedCards.Any() && hand.selectedCards[0].model.Number != card.model.Number && !card.model.Joker)
            {
                card.canSelect = false;
            }
            if (hand.selectedCards[0].model.Number == card.model.Number)
            {
                card.canSelect = true;
            }
        }
    }

    [PunRPC]
    void ResetHand()
    {
        foreach (CardController card in hand.allCards)
        {
            if (card.canSelect == false)
            {
                card.canSelect = true;
            }
        }
    }

    [PunRPC]
    void StairsP(CardController isSelectedCard)
    {
        searchedCards = new List<CardController>(hand.canSelectCards);
        hand.stairsList.Add(isSelectedCard);

        StairsSearchP(hand.stairsList, searchedCards, isSelectedCard, 1);
        StairsSearchP(hand.stairsList, searchedCards, isSelectedCard, -1);

        if (hand.stairsList.Count >= hand.fieldCount)
        {
            foreach (CardController card in hand.stairsList)
            {
                Debug.Log("<color=brown><size=18>StairsCard No </size></color>" + card.model.Number + "<color=brown><size=18> Suit </size></color>" + card.model.Suit);

                card.canSelect = true;
            }
        }
    }

    [PunRPC]
    public void StairsSearchP(List<CardController> cardList, List<CardController> searchedCards, CardController targetCard, int direction)
    {
        if (stairsCount < hand.fieldCount)
        {
            var list = searchedCards.FindAll(card => card.model.Suit == targetCard.model.Suit);
            CardController serchCard = list.Find(card => card.model.Strenge == targetCard.model.Strenge + direction);
            CardController seachJoker = searchedCards.Find(card => card.model.Joker == true);
            CardController afterJoker;

            if (serchCard)
            {
                cardList.Add(serchCard);
                searchedCards.Remove(serchCard);

                stairsCount++;

                if (direction == 2 || direction == -2)
                {
                    direction /= 2;
                }

                StairsSearchP(cardList, searchedCards, serchCard, direction);
            }
            else if (seachJoker)
            {
                if (direction == 1)
                {
                    afterJoker = cardList.Find(card => card.model.Strenge == (cardList.Max(card => card.model.Strenge)));
                }
                else
                {
                    afterJoker = cardList.Find(card => card.model.Strenge == (cardList.Min(card => card.model.Strenge)));
                }

                cardList.Add(seachJoker);
                searchedCards.Remove(seachJoker);

                stairsCount++;

                StairsSearchP(cardList, searchedCards, afterJoker, direction + direction);
            }
            else
            {
                return;
            }
        }
        else
        {
            return;
        }

    }

    [PunRPC]
    public void SameCardsP(CardController selectCard)
    {
        foreach (CardController card in hand.canSelectCards)
        {
            if (card.model.Strenge == selectCard.model.Strenge || card.model.Joker)
            {
                Debug.Log("<color=brown><size=18>SameCard No </size></color>" + card.model.Number + "<color=brown><size=18> Suit </size></color>" + card.model.Suit);
                card.canSelect = true;
            }
            else
            {
                card.canSelect = false;
            }

        }
    }

    [PunRPC]
    void ResetHandP()
    {
        foreach (CardController card in hand.allCards)
        {
            card.canSelect = false;
        }

        foreach (CardController card in hand.canSelectCards)
        {
            if (card.canSelect == false)
            {
                card.canSelect = true;
            }
        }
    }
}
