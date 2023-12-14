using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class CPUAI : MonoBehaviourPunCallbacks
{
    public PlayerHand hand;
    public GameObject fieldOBJ;
    public Field field;
    public static CPUAI CPUAIInstance;

    public bool turnEnd;
    public bool stairs;

    public string two = "TwoPHand";
    public string three = "ThreePHand";
    public string four = "FourPHand";

    public List<CardController> selectCards;

    public System.Random r = new System.Random();

    public void Awake()
    {
        if (photonView == null)
        {
            Debug.LogError("photonView is null!");
        }

        if (PhotonNetwork.PlayerList.ToList().Count < 2)
        {
            if(gameObject.name == two)
            {
                photonView.RPC("AwakeOnline", RpcTarget.All);
            }
        }
        if (PhotonNetwork.PlayerList.ToList().Count < 3)
        {
            if(gameObject.name == three)
            {
                photonView.RPC("AwakeOnline", RpcTarget.All);
            }
        }
        if (PhotonNetwork.PlayerList.ToList().Count < 4)
        {
            if(gameObject.name == four)
            {
                photonView.RPC("AwakeOnline", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public void AwakeOnline()
    {
        CPUAIInstance = this;
        hand = GetComponent<PlayerHand>();
        field = fieldOBJ.GetComponent<Field>();
        selectCards = new List<CardController>();
    }

    [PunRPC]
    public void CPUTurn()
    {
        selectCards = new List<CardController>();

        turnEnd = false;

        if (hand.isTurn)
        {
            List<CardController> preparation = hand.allCards.Where(card => card.canSelect && !card.isSelected).ToList();

            if (preparation.Count > 0)
            {
                Selected(preparation);

                if (hand.selectedCards.Count == hand.field.cards.Count || hand.field.cards.Count == 0)
                {
                    if (selectCards.Count == 2)
                    {
                        JokerSetNumberTwo(selectCards);
                    }

                    if (selectCards.Count >= 3)
                    {
                        var jokerCard = selectCards.Find(card => card.model.Joker);

                        if (jokerCard)
                        {
                            if (jokerCard && jokerCard.model.Strenge == 14)
                            {
                                JokerSetNumberThree(selectCards);
                            }

                            if (hand.selectedCards.Max(card => card.model.Strenge) != hand.selectedCards.Min(card => card.model.Strenge))
                            {
                                stairs = true;
                                StairsBool(hand.selectedCards, 0);

                                if (!stairs)
                                {
                                    List<CardController> resetCardsList = GetComponentInChildren<CardController>().hand.allCards;

                                    foreach (CardController card in hand.selectedCards)
                                    {
                                        card.transform.SetParent(card.GetComponent<CardMovement>().originalParent);  // 元の親オブジェクトAに戻す
                                    }

                                    resetCardsList = PutInOrder(resetCardsList);

                                    for (int i = 0; i < resetCardsList.Count; i++)
                                    {
                                        int posX = i * 60;
                                        int posXToCenter = resetCardsList.Count * 28;
                                        resetCardsList[i].transform.localPosition = new Vector3(posX - posXToCenter, 0);
                                        resetCardsList[i].transform.SetSiblingIndex(i);

                                        if (resetCardsList[i].isSelected)
                                        {
                                            resetCardsList[i].transform.localPosition += Vector3.up * 50;

                                            resetCardsList[i].GetComponent<CanvasGroup>().blocksRaycasts = true;

                                            resetCardsList[i].GetComponentInChildren<CardMovement>().isDrag = false;

                                            Image[] images = GetComponentsInChildren<Image>();

                                            foreach (var item in images)
                                            {
                                                item.raycastTarget = true;
                                            }

                                            resetCardsList[i].photonView.RPC("OnSelectedCard", PhotonNetwork.PlayerList[0]);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    photonView.RPC("FieldSetting", PhotonNetwork.PlayerList[0]);
                }
                else
                {
                    turnEnd = true;
                }
            }  
            else
            {
                turnEnd = true;
            }
        }   
    }

    public void Selected(List<CardController> preparation)
    {
        CardController card = preparation[r.Next(preparation.Count)];

        card.photonView.RPC("OnClickCard", PhotonNetwork.PlayerList[0]);

        selectCards = hand.allCards.Where(card => card.canSelect && !card.isSelected).ToList();

        /*
        foreach (var listCard in selectCards)
        {
            Debug.Log("<color=green><size=20>Number</size></color> " + listCard.model.Number + " <color=green><size=20>Suit</size></color> " + listCard.model.Suit);
        }
        */

        if (selectCards.Count > 0)
        {
            Selected(selectCards);
        }
        else
        {
            return;
        }
    }

    void JokerSetNumberTwo(List<CardController> preparation)
    {
        CardController joker = selectCards.Find(card => card.model.Joker == true);
        CardController max = selectCards.Find(card => card.model.Joker == false);

        if (joker && max)
        {
            joker.model.Strenge = max.model.Strenge;
            joker.model.UpsideDown = max.model.UpsideDown;
        }
    }

    public List<CardController> JokerSetNumberThree(List<CardController> preparation)
    {
        Debug.Log("<size=18>JokerSetNumberThree</size>");

        List<CardController> jokersSearch = selectCards.FindAll(card => card.model.Joker == true && card.isSelected);
        List<CardController> jokers = new List<CardController>(jokersSearch);
        List<CardController> listStairs = new List<CardController>();

        selectCards.RemoveAll(card => card.model.Joker);

        foreach (var card in selectCards)
        {
            Debug.Log("<color=green>After Joker Remove Numver </color> " + card.model.Number + " <color=green> Strenge </color> " + card.model.Strenge);
        }

        if (selectCards.Max(card => card.model.Strenge) == selectCards.Min(card => card.model.Strenge))
        {
            for (int i = 0; i <= jokers.Count - 1; i++)
            {
                jokers[i].model.Strenge = selectCards[i].model.Strenge;
                jokers[i].model.UpsideDown = selectCards[i].model.UpsideDown;
                selectCards.Add(jokers[i]);
            }

            jokers.Clear();
        }
        if (selectCards.Max(card => card.model.Strenge) != selectCards.Min(card => card.model.Strenge))
        {
            var orderStrairs = selectCards.OrderBy(card => card.model.Strenge);
            listStairs = orderStrairs.ToList();
            JokerSetStairsThree(jokers, listStairs, 0);
        }

        return selectCards = listStairs;
    }

    void JokerSetStairsThree(List<CardController> jokers, List<CardController> listStairs, int No)
    {
        Debug.Log("Joker<size=18>SetStairs</size>Three");

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
        if (listStairs.Count > No)
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

    public List<CardController> PutInOrder(List<CardController> cardsNumberList)
    {
        var newCardsNumber = cardsNumberList.OrderBy(x => x.model.Suit).OrderBy(x => x.model.Strenge);

        List<CardController> newCardsNumberList = newCardsNumber.ToList();

        cardsNumberList = newCardsNumberList;

        return cardsNumberList;
    }

    [PunRPC]
    public void FieldSetting()
    {
        Debug.LogWarning($"<size=24><color=green>ToField {PhotonNetwork.LocalPlayer.ActorNumber}Player</color></size>");

        foreach (var card in hand.selectedCards)
        {
            Debug.LogWarning($"<color=green>cards No{card.model.Number} Suit{card.model.Suit}</color>");
        }

        field.photonView.RPC("FieldPreparation", RpcTarget.All);

        hand.selectedCards = hand.selectedCards.OrderBy(x => x.model.Suit).OrderBy(x => x.model.Strenge).ToList();

        for (int i = hand.selectedCards.Count - 1; i >= 0; i--)
        {
            hand.selectedCards[i].transform.SetParent(fieldOBJ.transform, false);
            field.cards.Add(hand.selectedCards[i]);
            hand.selectedCards[i].turnCount = field.turnCount;

            photonView.RPC("CPUCardsSet", RpcTarget.Others, hand.selectedCards[i].model.ID, field.gameObject.name);

            hand.selectedCards[i].hand.allCards.Remove(hand.selectedCards[i]);
            hand.selectedCards[i].hand.selectedCards.Remove(hand.selectedCards[i]);
        }

        //field.photonView.RPC("ShowCard", RpcTarget.All);
        field.photonView.RPC("IsDone", PhotonNetwork.PlayerList[0], hand.gameObject.name);
    }

    [PunRPC]
    public void CPUCardsSet(int cardID, string parentname)
    {
        CardController card = hand.allCards.Find(x => x.model.ID == cardID);
        Field field = GameObject.Find(parentname).GetComponent<Field>();
        card.transform.SetParent(field.transform, false);
        field.cards.Add(card);
        card.turnCount = field.turnCount;
        card.hand.allCards.Remove(card);
    }

    [PunRPC]
    public void NotCPUTurn()
    {
        if (!hand.isTurn)
        {
            turnEnd = false;
        }
    }
}
