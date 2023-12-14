using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System;
using Photon.Pun;
using Photon.Realtime;

public class CardMovement : MonoBehaviourPunCallbacks
{
    public CardController cardController;
    public PlayerHand hand;
    public GameObject putPanel;
    public Transform originalParent;
    public List<CardController> resetCardsList;


    public bool isDrag = false;
    public bool stairs;
    public bool same;

    [PunRPC]
    public void OnPointerDown()
    {
        originalParent = transform.parent;
    }

    [PunRPC]
    public void OnBeginDrag()
    {
        if (GetComponent<CardController>().hand.selectedCards.Count > 0 && GetComponent<CardController>().isSelected == true)
        {
            if (GetComponent<CardController>().hand.selectedCards.Count == GetComponent<CardController>().hand.field.cards.Count || GetComponent<CardController>().hand.field.cards.Count == 0)
            {
                hand = GetComponent<CardController>().hand;

                List<CardController> cards = PutInOrder(hand.selectedCards);

                stairs = true;
                same = true;

                if (cards.Count > 2 && hand.selectedCards.Max(card => card.model.Strenge) != hand.selectedCards.Min(card => card.model.Strenge))
                {
                    StairsBool(cards, 0);
                }

                if (cards.Count == 2)
                {
                    JokerSetNumberTwo(cards);
                    SameBool();
                }

                if (stairs && same)
                {
                    if (isDrag)
                    {
                        return;
                    }

                    isDrag = true;

                    Canvas canvas = GetComponentInParent<Canvas>();

                    Image[] images = GetComponentsInChildren<Image>();


                    foreach (var item in images)
                    {
                        item.raycastTarget = false;
                    }

                    GameObject putCardsPalletOBJ = PhotonNetwork.Instantiate("Prefab/PutPanel", transform.position, Quaternion.identity);
                    putCardsPalletOBJ.transform.SetParent(canvas.transform, false);

                    PlayingCards putCardsPallet = putCardsPalletOBJ.GetComponent<PlayingCards>();

                    for (int i = 0; i < hand.selectedCards.Count; i++)
                    {
                        putCardsPallet.cards.Add(hand.selectedCards[i]);

                        hand.selectedCards[i].transform.SetParent(putCardsPalletOBJ.transform);
                    }

                    if (putCardsPallet.cards.Count >= 3)
                    {
                        var jokerCard = hand.selectedCards.Find(card => card.model.Joker);

                        if (jokerCard && jokerCard.model.Strenge == 14)
                        {
                            JokerSetNumberThree();
                        }
                    }

                    resetCardsList = PutInOrder(putCardsPallet.cards);

                    for (int i = 0; i < resetCardsList.Count; i++)
                    {
                        int posX = i * 60;
                        int posXToCenter = resetCardsList.Count * 28;
                        resetCardsList[i].transform.localPosition = new Vector3(posX - posXToCenter, 0);
                        resetCardsList[i].transform.SetSiblingIndex(i);
                        resetCardsList[i].GetComponent<CanvasGroup>().blocksRaycasts = false;

                        var cardCollider = resetCardsList[i].GetComponent<Collider>();
                        if (cardCollider != null)
                        {
                            cardCollider.enabled = false;
                        }
                    }
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
    }

    public List<CardController> PutInOrder(List<CardController> cardsNumberList)
    {
        var newCardsNumber = cardsNumberList.OrderBy(x => x.model.Suit).OrderBy(x => x.model.Strenge);

        List<CardController> newCardsNumberList = newCardsNumber.ToList();

        cardsNumberList = newCardsNumberList;

        return cardsNumberList;
    }

    void JokerSetNumberTwo(List<CardController> cards)
    {
        CardController joker = cards.Find(card => card.model.Joker == true);
        CardController max = cards.Find(card => card.model.Joker == false);

        if (joker && max)
        {
            joker.model.Strenge = max.model.Strenge;
            joker.model.UpsideDown = max.model.UpsideDown;
        }
    }

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
            }

            jokers.Clear();
        }
        if (hand.selectedCards.Max(card => card.model.Strenge) != hand.selectedCards.Min(card => card.model.Strenge))
        {
            var orderStrairs = hand.selectedCards.OrderBy(card => card.model.Strenge);
            listStairs = orderStrairs.ToList();
            JokerSetStairsThree(jokers, listStairs, 0);
        }

        hand.selectedCards = listStairs;
    }

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

    void StairsBool(List<CardController> listStairs, int No)
    {
        if (listStairs.Count > No + 1)
        {
            if (listStairs[No].model.Strenge + 1 == listStairs[No + 1].model.Strenge)
            {
                StairsBool(listStairs, No + 1);
            }
            else
            {
                stairs = false;
                return;
            }
        }
        else
        {
            stairs = true;
            return;
        }
    }

    void SameBool()
    {
        if (hand.selectedCards.Max(card => card.model.Strenge) != hand.selectedCards.Min(card => card.model.Strenge))
        {
            same = false;
        }
    }

    public void OnPointerUp()
    {
        if (!isDrag)
        {
            GetComponent<CardController>().OnClickCard();
        }
    }
}
