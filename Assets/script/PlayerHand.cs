using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerHand : MonoBehaviourPunCallbacks
{
    public Field field;

    public CardController theFirst = null;

    public bool isTurn;
    public bool restriction;
    public bool sameCancel;
    public bool stairCancel;
    public bool ownHand;

    public List<CardController> allCards = new List<CardController>();
    public List<CardController> selectedCards = new List<CardController>();
    public List<CardController> stairsList = new List<CardController>();
    public List<CardController> canSelectCards = new List<CardController>();

    public int index;
    public int fieldCount;
    public int rank;
    public int ownerActorNumber;

    public string myHand = "OwnHand";

    List<CardController> searchedCards;

    private void Start()
    {
        if (this.gameObject.name == myHand)
        {
            ownHand = true;
            ownerActorNumber = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        }
    }

    [PunRPC]
    public void Pass()
    {
        foreach (var card in allCards)
        {
            if (card.isSelected)
            {
                card.photonView.RPC("OnSelectedCard", PhotonNetwork.LocalPlayer);
            }
        }
    }

    [PunRPC]
    public void ActivateHand()
    {
        restriction = false;
    }

    [PunRPC]
    public void Restriction()
    {
        foreach (var card in allCards)
        {
            card.canSelect = false;
            card.ShadowUpdate();
        }
    }

    [PunRPC]
    public void HandUpdateZero()
    {
        foreach (var card in allCards)
        {
            card.canSelect = true;
            card.ShadowUpdate();
        }
    }

    [PunRPC]
    public void HandUpdateOne(CardController cardController)
    {
        foreach (var card in allCards)
        {
            if (card.model.Strenge > cardController.model.Strenge)
            {
                card.canSelect = true;
                canSelectCards.Add(card);
            }

            card.ShadowUpdate();

        }
    }

    [PunRPC]
    public void HandUpdateTwo(CardController cardController)
    {
        foreach (var card in allCards)
        {
            if (card.model.Strenge > cardController.model.Strenge)
            {
                SeachCardsTwo(card);
            }
        }
        FinalCheck();
    }

    [PunRPC]
    public void SeachCardsTwo(CardController cardController)
    {
        List<CardController> matchedCards = allCards.Where(card => card.model.Strenge == cardController.model.Strenge).ToList();
        var jokers = allCards.Where(card => card.model.Joker == true).ToList();

        if (matchedCards.Count + jokers.Count >= 2)
        {
            foreach (var card in matchedCards)
            {
                card.canSelect = true;
                canSelectCards.Add(card);
            }
            foreach (var joker in jokers)
            {
                joker.canSelect = true;
                canSelectCards.Add(joker);

            }
        }
    }

    [PunRPC]
    public void SameThreeOrMore(CardController cardController, int count)
    {
        foreach (var card in allCards)
        {
            if (card.model.Strenge > cardController.model.Strenge)
            {
                SamesSeach(card, count);
            }
        }
        FinalCheck();
    }

    [PunRPC]
    public void SamesSeach(CardController cardController, int count)
    {
        var matchedCards = allCards.Where(card => card.model.Strenge == cardController.model.Strenge).ToList();
        var jokers = allCards.Where(card => card.model.Joker == true).ToList();

        if (matchedCards.Count + jokers.Count >= count)
        {
            foreach (var card in matchedCards)
            {
                card.canSelect = true;
                canSelectCards.Add(card);
            }
            foreach (var joker in jokers)
            {
                joker.canSelect = true;
                canSelectCards.Add(joker);
            }
        }
        foreach (var card in allCards)
        {
            card.ShadowUpdate();
        }
    }

    [PunRPC]
    public void StairsTreeOrMore(CardController cardController, int count)
    {
        foreach (var card in allCards)
        {
            if (card.model.Strenge > cardController.model.Strenge)
            {
                Stairs(card, count);
            }
        }
        FinalCheck();
    }

    [PunRPC]
    public void Stairs(CardController cardController, int count)
    {
        searchedCards = new List<CardController>(allCards);
        stairsList.Clear();
        canSelectCards.Clear();
        stairsList.Add(cardController);

        StairsSearch(stairsList, searchedCards, cardController, 1);

        if (stairsList.Count >= count)
        {
            foreach (CardController card in stairsList)
            {
                card.canSelect = true;
                canSelectCards.Add(card);
            }
        }
        foreach (var card in allCards)
        {
            card.ShadowUpdate();
        }
    }

    [PunRPC]
    public void StairsSearch(List<CardController> cardList, List<CardController> searchedCards, CardController targetCard, int direction)
    {
        var list = allCards.FindAll(card => card.model.Suit == targetCard.model.Suit);
        CardController serchCard = list.Find(card => card.model.Strenge == targetCard.model.Strenge + direction);
        CardController seachJoker = searchedCards.Find(card => card.model.Joker == true);
        CardController afterJoker;

        if (serchCard)
        {
            cardList.Add(serchCard);
            searchedCards.Remove(serchCard);
            StairsSearch(cardList, searchedCards, serchCard, direction);
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

            StairsSearch(cardList, searchedCards, afterJoker, direction + direction);
        }
        else
        {
            return;
        }
    }

    [PunRPC]
    public void HandUpdateZeroUD()
    {
        foreach (var card in allCards)
        {
            card.canSelect = true;
            card.ShadowUpdate();
        }
    }

    [PunRPC]
    public void HandUpdateOneUD(CardController cardController)
    {
        foreach (var card in allCards)
        {
            if (card.model.UpsideDown > cardController.model.UpsideDown)
            {
                card.canSelect = true;
                canSelectCards.Add(card);
            }

            card.ShadowUpdate();

        }
    }

    [PunRPC]
    public void HandUpdateTwoUD(CardController cardController)
    {
        foreach (var card in allCards)
        {
            if (card.model.UpsideDown > cardController.model.UpsideDown)
            {
                SeachCardsTwoUD(card);
            }
        }
        FinalCheck();
    }

    [PunRPC]
    public void SeachCardsTwoUD(CardController cardController)
    {
        List<CardController> matchedCards = allCards.Where(card => card.model.UpsideDown == cardController.model.UpsideDown).ToList();
        var jokers = allCards.Where(card => card.model.Joker == true).ToList();

        if (matchedCards.Count + jokers.Count >= 2)
        {
            foreach (var card in matchedCards)
            {
                card.canSelect = true;
                canSelectCards.Add(card);
            }
            foreach (var joker in jokers)
            {
                joker.canSelect = true;
                canSelectCards.Add(joker);
            }
        }
    }

    [PunRPC]
    public void SameThreeOrMoreUD(CardController cardController, int count)
    {
        foreach (var card in allCards)
        {
            if (card.model.UpsideDown > cardController.model.UpsideDown)
            {
                SamesSeachUD(card, count);
            }
        }
        FinalCheck();
    }

    [PunRPC]
    public void SamesSeachUD(CardController cardController, int count)
    {
        var matchedCards = allCards.Where(card => card.model.UpsideDown == cardController.model.UpsideDown).ToList();
        var jokers = allCards.Where(card => card.model.Joker == true).ToList();

        if (matchedCards.Count + jokers.Count >= count)
        {
            foreach (var card in matchedCards)
            {
                card.canSelect = true;
                canSelectCards.Add(card);
            }
            foreach (var joker in jokers)
            {
                joker.canSelect = true;
                canSelectCards.Add(joker);
            }
        }
    }

    [PunRPC]
    public void StairsTreeOrMoreUD(CardController cardController, int count)
    {
        foreach (var card in allCards)
        {
            if (card.model.UpsideDown > cardController.model.UpsideDown)
            {
                StairsUD(card, count);
            }
        }
        FinalCheck();
    }

    [PunRPC]
    public void StairsUD(CardController cardController, int count)
    {
        searchedCards = new List<CardController>(allCards);
        stairsList.Add(cardController);

        StairsSearchUD(stairsList, searchedCards, cardController, 1);

        if (stairsList.Count >= count)
        {
            foreach (CardController card in stairsList)
            {
                card.canSelect = true;
                canSelectCards.Add(card);
            }
        }
    }

    [PunRPC]
    public void StairsSearchUD(List<CardController> cardList, List<CardController> searchedCards, CardController targetCard, int direction)
    {
        var list = allCards.FindAll(card => card.model.Suit == targetCard.model.Suit);
        CardController serchCard = list.Find(card => card.model.UpsideDown == targetCard.model.UpsideDown + direction);
        CardController seachJoker = searchedCards.Find(card => card.model.Joker == true);
        CardController afterJoker;

        if (serchCard)
        {
            cardList.Add(serchCard);
            searchedCards.Remove(serchCard);
            StairsSearchUD(cardList, searchedCards, serchCard, direction);
        }
        else if (seachJoker)
        {
            if (direction == 1)
            {
                afterJoker = cardList.Find(card => card.model.UpsideDown == (cardList.Max(card => card.model.UpsideDown)));
            }
            else
            {
                afterJoker = cardList.Find(card => card.model.UpsideDown == (cardList.Min(card => card.model.UpsideDown)));
            }

            cardList.Add(seachJoker);
            searchedCards.Remove(seachJoker);

            StairsSearchUD(cardList, searchedCards, afterJoker, direction + direction);
        }
        else
        {
            return;
        }
    }

    [PunRPC]
    public void HandUpdateOneB(CardController cardController)
    {
        foreach (var card in allCards)
        {
            if (card.model.Strenge > cardController.model.Strenge && card.model.Suit == cardController.model.Suit)
            {
                card.canSelect = true;
                canSelectCards.Add(card);
            }

            card.ShadowUpdate();

        }
    }

    [PunRPC]
    public void HandUpdatePlurlB(List<CardController> cards)
    {
        List<List<CardController>> listOfLists = new List<List<CardController>>();

        for (int i = 0; i < cards.Count; i++)
        {
            List<CardController> cardsList = new List<CardController>();
            SeachCardsPlurlB(cards[i], cardsList);
            listOfLists.Add(cardsList);
        }

        IEnumerable<int> commonNumbers = listOfLists[0].Select(card => card.model.Strenge);

        for (int i = 1; i < listOfLists.Count; i++)
        {
            commonNumbers = commonNumbers.Intersect(listOfLists[i].Select(card => card.model.Strenge));
        }

        List<CardController> canSelectCards = allCards.Where(card => commonNumbers.Contains(card.model.Strenge)).ToList();

        FinalCheck();
    }

    [PunRPC]
    public void SeachCardsPlurlB(CardController cardController, List<CardController> listCards)
    {
        foreach (var card in allCards)
        {
            if (card.model.Strenge > cardController.model.Strenge && card.model.Suit == cardController.model.Suit)
            {
                card.canSelect = true;
                listCards.Add(card);
            }
            else if (card.model.Joker)
            {
                card.canSelect = true;
                listCards.Add(card);
            }
        }
    }

    [PunRPC]
    public void HandUpdateOneBUD(CardController cardController)
    {
        foreach (var card in allCards)
        {
            if (card.model.UpsideDown > cardController.model.UpsideDown && card.model.Suit == cardController.model.Suit)
            {
                card.canSelect = true;
                canSelectCards.Add(card);
            }

            card.ShadowUpdate();

        }
    }

    [PunRPC]
    public void HandUpdatePlurlBUD(List<CardController> cards)
    {
        List<List<CardController>> listOfLists = new List<List<CardController>>();

        for (int i = 0; i < cards.Count; i++)
        {
            List<CardController> cardsList = new List<CardController>();
            SeachCardsPlurlBUD(cards[i], cardsList);
            listOfLists.Add(cardsList);
        }

        IEnumerable<int> commonNumbers = listOfLists[0].Select(card => card.model.UpsideDown);

        for (int i = 1; i < listOfLists.Count; i++)
        {
            commonNumbers = commonNumbers.Intersect(listOfLists[i].Select(card => card.model.UpsideDown));
        }

        List<CardController> canSelectCards = allCards.Where(card => commonNumbers.Contains(card.model.UpsideDown)).ToList();

        FinalCheck();
    }

    [PunRPC]
    public void SeachCardsPlurlBUD(CardController cardController, List<CardController> listCards)
    {
        foreach (var card in allCards)
        {
            if (card.model.UpsideDown > cardController.model.UpsideDown && card.model.Suit == cardController.model.Suit)
            {
                card.canSelect = true;
                listCards.Add(card);
            }
            else if (card.model.Joker)
            {
                card.canSelect = true;
                listCards.Add(card);
            }
        }
    }

    [PunRPC]
    public void StairsTreeOrMoreB(CardController cardController, int count)
    {
        foreach (var card in allCards)
        {
            if (card.model.Strenge > cardController.model.Strenge && card.model.Suit == cardController.model.Suit)
            {
                Stairs(card, count);
            }
        }
        FinalCheck();
    }

    [PunRPC]
    public void StairsTreeOrMoreBUD(CardController cardController, int count)
    {
        foreach (var card in allCards)
        {
            if (card.model.UpsideDown > cardController.model.UpsideDown && card.model.Suit == cardController.model.Suit)
            {
                StairsUD(card, count);
            }
        }
        FinalCheck();
    }

    [PunRPC]
    public void FinalCheck()
    {
        canSelectCards = canSelectCards.Intersect(allCards).ToList();

        /*
        foreach (var card in canSelectCards)
        {
            Debug.Log("<color=orange><size=20>Check</size></color> " + card.model.Number);
        }
        */

        if (canSelectCards.Any() && canSelectCards.Count < field.cards.Count)
        {
            Debug.Log("<color=orange><size=20>Check Out</size></color>");

            foreach (var card in canSelectCards)
            {
                card.canSelect = false;
            }

            canSelectCards.Clear();
        }
        if (canSelectCards.Any())
        {
            Debug.Log("<color=orange><size=20>Check In</size></color>");

            foreach (var card in canSelectCards)
            {
                card.ShadowUpdate();
            }
        }
    }
}
