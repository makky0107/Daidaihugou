using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayingCards : MonoBehaviourPunCallbacks
{
    public List<CardController> cards = new List<CardController>();

    public bool getPlace;

    public Transform ownHand;
    public PlayerHand hand;
    public Field field;

    RectTransform canvasRect;

    private void Start()
    {
        canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>();

        getPlace = false;
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            var mousePos = Input.mousePosition;
            var magnification = canvasRect.sizeDelta.x / Screen.width;
            mousePos.x = mousePos.x * magnification - canvasRect.sizeDelta.x / 2;
            mousePos.y = mousePos.y * magnification - canvasRect.sizeDelta.y / 2;
            mousePos.z = transform.localPosition.z;
            transform.localPosition = mousePos;
        }

        if (Input.GetMouseButtonUp(0))
        {
            List<RaycastResult> results = new List<RaycastResult>();
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;

            EventSystem.current.RaycastAll(eventData, results);

            foreach (RaycastResult result in results)
            {
                field = result.gameObject.GetComponent<Field>();
                if (field != null)
                {
                    photonView.RPC("CardSet", PhotonNetwork.LocalPlayer);

                    break;
                }
            }

            if (!getPlace)
            {
                List<CardController> resetCardsList = GetComponentInChildren<CardController>().hand.allCards;

                foreach (CardController card in cards)
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

                        Image[] images = resetCardsList[i].GetComponentsInChildren<Image>();

                        resetCardsList[i].GetComponent<CanvasGroup>().blocksRaycasts = true;

                        resetCardsList[i].GetComponentInChildren<CardMovement>().isDrag = false;

                        foreach (var item in images)
                        {
                            item.raycastTarget = true;
                        }
                    }
                }

                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    [PunRPC]
    void CardSet()
    {
        Debug.LogWarning($"<size=24><color=blue>CardSet {PhotonNetwork.LocalPlayer.ActorNumber}Player</color></size>");

        field.photonView.RPC("FieldPreparation", RpcTarget.All);

        hand = cards[0].hand;

        for (int i = 0; i < cards.Count; i++)
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player.ActorNumber < PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    GameManager.instance.photonView.RPC("PlayCardsOnline", player, cards[i].model.ID, player.ActorNumber - 1);
                }
                if (player.ActorNumber > PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    GameManager.instance.photonView.RPC("PlayCardsOnline", player, cards[i].model.ID, player.ActorNumber - 2);
                }
            }

            cards[i].GetComponentInChildren<CardMovement>().isDrag = false;
            cards[i].GetComponent<CanvasGroup>().blocksRaycasts = false;
            cards[i].transform.SetParent(field.transform);
            cards[i].GetComponent<CardMovement>().originalParent = field.transform;
            field.cards.Add(cards[i]);
            cards[i].turnCount = field.turnCount;
            cards[i].hand.allCards.Remove(cards[i]);
            cards[i].hand.selectedCards.Remove(cards[i]);
            cards[i].BackSideNotActive();
        }

        List<CardController> handcards = hand.allCards;

        handcards = PutInOrder(handcards);

        for (int i = 0; i < handcards.Count; i++)
        {
            int posX = i * 60;
            int posXToCenter = hand.allCards.Count * 28;
            handcards[i].transform.localPosition = new Vector3(posX - posXToCenter, 0);
            handcards[i].transform.SetSiblingIndex(i);
        }

        photonView.RPC("FieldSetting", PhotonNetwork.LocalPlayer);

        cards.Clear();

        getPlace = true;

        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    public void FieldSetting()
    {
        Debug.LogWarning($"<size=24><color=blue>FieldSetting hand.gameObject is{hand}</color></size>");

        field.photonView.RPC("IsDone", PhotonNetwork.LocalPlayer, hand.gameObject.name);
    }

    public List<CardController> PutInOrder(List<CardController> cardsNumberList)
    {
        List<CardController> jokers = cardsNumberList.FindAll(card => card.model.Joker);

        foreach (var joker in jokers)
        {
            joker.model.Strenge = 14;
        }

        var newCardsNumber = cardsNumberList.OrderBy(x => x.model.Suit).OrderBy(x => x.model.Strenge);

        List<CardController> newCardsNumberList = newCardsNumber.ToList();

        cardsNumberList = newCardsNumberList;

        return cardsNumberList;
    }
}
