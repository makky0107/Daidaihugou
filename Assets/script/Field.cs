using System;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Field : MonoBehaviourPunCallbacks
{
    public static CardController card;

    public List<CardController> cards;
    public List<CardController> waitingCards;

    public GameObject shadow;

    public PlayerHand own;
    public PlayerHand two;
    public PlayerHand three;
    public PlayerHand four;
    public PlayerHand currentHand;

    List<PlayerHand> hands;

    public bool upsideDown;
    public bool insert;
    public bool elevenUD;
    public bool bind;
    public bool common;
    public bool bindInfoBool = false;
    public bool same;
    public bool stairs;

    public int turnCount;
    public float fadeInTime;

    public Text text;
    public Text irregularity;
    public Text bindText;

    public GameManager gameManager;

    public int rank = 0;

    [SerializeField] LoggerScroll lS;

    private void Awake()
    {
        shadow.SetActive(false);
        hands = new List<PlayerHand> { own, two, three, four };
        cards = new List<CardController>();
        upsideDown = false;
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        irregularity.color = new Color(text.color.r, text.color.g, text.color.b, 0);
    }

    [PunRPC]
    public void Judge(int index)
    {
        lS.photonView.RPC("AddLog", RpcTarget.All, $"juge index {index}");

        Debug.Log($"<color=black><size=20>Judge {PhotonNetwork.LocalPlayer.ActorNumber} Player </size></color>");

        if (upsideDown)
        {
            Debug.Log("<color=red>upsideDown</color>");
        }

        lS.photonView.RPC("AddLog", RpcTarget.All, $"juge 1");

        common = true;

        if (index == PhotonNetwork.LocalPlayer.ActorNumber -1)
        {
            currentHand = GameObject.Find("OwnHand").GetComponent<PlayerHand>();
        }
        else
        {
            currentHand = hands[gameManager.currentPlayerIndex];
        }

        lS.photonView.RPC("AddLog", RpcTarget.All, $"juge 2");

        if (cards.Any())
        {
            foreach (var card in cards)
            {
                card.shadow.SetActive(false);
            }
        }

        lS.photonView.RPC("AddLog", RpcTarget.All, $"juge 3");

        if (cards.Count == 2)
        {
            JokerSetNumberTwo(cards);
        }
        if (cards.Count > 2)
        {
            JokerSetNumberThree();
        }

        lS.photonView.RPC("AddLog", RpcTarget.All, $"juge 4");


        foreach (var card in cards)
        {
            Debug.Log($"<color=blue><size=20>PlayingCard.Number {card.model.Number} Strenge {card.model.Strenge} Suit {card.model.Suit}</size></color>");
        }

        currentHand.sameCancel = false;
        currentHand.stairCancel = false;

        currentHand.theFirst = null;

        currentHand.canSelectCards.Clear();
        currentHand.stairsList.Clear();

        currentHand.fieldCount = cards.Count;

        lS.photonView.RPC("AddLog", RpcTarget.All, $"juge 5");

        int count = 0;

        currentHand.allCards.RemoveAll(card => card.gameObject == null);

        lS.photonView.RPC("AddLog", RpcTarget.All, $"cardsCount{currentHand.allCards.Count}");

        foreach (var card in currentHand.allCards)
        {
            count++;

            lS.photonView.RPC("AddLog", RpcTarget.All, $"<color=orange>card {count} No{card.model.Number}</color>");

            lS.photonView.RPC("AddLog", RpcTarget.All, $"juge 6");

            card.canSelect = false;

            lS.photonView.RPC("AddLog", RpcTarget.All, $"juge 7");

            card.ShadowUpdate();

            card.GetComponent<CardMovement>().isDrag = false;

            lS.photonView.RPC("AddLog", RpcTarget.All, $"juge 8");

            if (card.model.Joker)
            {
                lS.photonView.RPC("AddLog", RpcTarget.All, $"juge 9");

                card.model.Strenge = card.model.UpsideDown = 14;
            }
        }

        lS.photonView.RPC("AddLog", RpcTarget.All, $"juge 10");

        currentHand.stairsList.Clear();
        currentHand.canSelectCards.Clear();

        lS.photonView.RPC("AddLog", RpcTarget.All, $"juge 11");

        if (cards.Any())
        {
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                photonView.RPC("UpsideDownControl", PhotonNetwork.PlayerList[i]);
            }

            if (cards.Count > 0 && cards.Count == waitingCards.Count)
            {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    photonView.RPC("BindControl", PhotonNetwork.PlayerList[i]);
                }

                if (bind)
                {
                    if (!bindInfoBool)
                    {
                        StartCoroutine(BindInfo());
                    }
                    bindInfoBool = true;

                    SetCardB();

                    common = false;
                }
            }

            if (IsAllJoker(cards))
            {
                List<CardController> sThree = currentHand.allCards.Where(card => card.model.Suit == 0 && card.model.Number == 3).ToList();

                if (sThree.Count == cards.Count)
                {
                    foreach (var card in sThree)
                    {
                        card.canSelect = true;
                        card.ShadowUpdate();
                    }

                    common = false;
                }
            }

            if (cards[0].model.Number == 8 && IsAllElementsSame(cards))
            {
                foreach (var card in currentHand.allCards)
                {
                    card.canSelect = false;
                    card.ShadowUpdate();
                }

                common = false;
            }

            if (cards.Count > 1)
            {
                if (cards.Max(card => card.model.Strenge) == cards.Min(card => card.model.Strenge))
                {
                    currentHand.stairCancel = true;
                }
                else
                {
                    currentHand.sameCancel = true;
                }
            }
        }

        lS.photonView.RPC("AddLog", RpcTarget.All, $"juge 12");

        if (common)
        {
            SetCard();
        }

        lS.photonView.RPC("AddLog", RpcTarget.All, $"juge 13");

        if (currentHand.restriction)
        {
            foreach (var card in currentHand.allCards)
            {
                card.canSelect = false;
                card.ShadowUpdate();
            }
        }
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
        if (cards.Any(x => x.model.Joker))
        {
            List<CardController> jokersSearch = cards.FindAll(card => card.model.Joker == true);
            List<CardController> jokers = new List<CardController>(jokersSearch);
            List<CardController> listStairs = new List<CardController>();

            cards.RemoveAll(card => card.model.Joker);

            if (cards.Max(card => card.model.Strenge) == cards.Min(card => card.model.Strenge))
            {
                for (int i = 0; i < jokers.Count; i++)
                {
                    jokers[i].model.Strenge = cards[i].model.Strenge;
                    jokers[i].model.UpsideDown = cards[i].model.UpsideDown;
                    cards.Add(jokers[i]);
                }

                jokers.Clear();
            }
            if (cards.Max(card => card.model.Strenge) != cards.Min(card => card.model.Strenge))
            {
                var orderStrairs = cards.OrderBy(card => card.model.Strenge);
                listStairs = orderStrairs.ToList();
                JokerSetStairsThree(jokers, listStairs, 0);

                cards = listStairs;
            }
        }
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

    [PunRPC]
    void UpsideDownControl()
    {
        if (insert)
        {
            //Debug.Log($"<color=red>{PhotonNetwork.LocalPlayer.ActorNumber}Player UpsideDownControl</color>");

            if (cards.Count >= 4)
            {
                //Debug.Log($"<color=red>{PhotonNetwork.LocalPlayer.ActorNumber}Player cards.Count >= 4</color>");

                upsideDown = !upsideDown;
            }

            if (cards[0].model.Strenge == 9 && IsAllElementsSame(cards))
            {
                //Debug.Log($"<color=red>{PhotonNetwork.LocalPlayer.ActorNumber}Player cards[0].model.Strenge == 9</color>");

                upsideDown = !upsideDown;
                elevenUD = true;
            }

            insert = false;
        }

        //Debug.Log($"<color=red>{PhotonNetwork.LocalPlayer.ActorNumber}Player upsideDown {upsideDown}</color>");
    }

    [PunRPC]
    void BindControl()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].model.Suit == waitingCards[i].model.Suit && cards[i].turnCount == waitingCards[i].turnCount)
            {
                bind = true;
            }
            else
            {
                bind = false;
                break;
            }
        }
    }

    [PunRPC]
    public static bool IsAllElementsSame(List<CardController> list)
    {
        if (list == null || list.Count == 0)
        {
            throw new ArgumentException("リストは空またはnullであってはなりません。");
        }

        return list.All(x => x.model.Strenge == list[0].model.Strenge);
    }

    [PunRPC]
    public static bool IsAllJoker(List<CardController> list)
    {
        if (list == null || list.Count == 0)
        {
            throw new ArgumentException("リストは空またはnullであってはなりません。");
        }

        return list.All(x => x.model.Joker);
    }

    [PunRPC]
    public void ResetField()
    {
        waitingCards.Clear();
        waitingCards.AddRange(cards);

        cards.Clear();

        foreach (var card in waitingCards)
        {
            PhotonView view = card.gameObject.GetComponent<PhotonView>();

            if (view != null && view.ViewID != 0)
            {
                PhotonNetwork.Destroy(card.gameObject);
            }
            else
            {
                Destroy(card.gameObject);
            }
        }
    }

    [PunRPC]
    public void SetCard()
    {
        if (!upsideDown)
        {
            if (cards.Count == 0)
            {
                Debug.Log($"<color=orange><size=20>Judge cards count {cards.Count} </size></color>");

                currentHand.HandUpdateZero();
                Debug.Log("HandUpdateZero");
            }
            else if (cards.Count == 1)
            {
                currentHand.HandUpdateOne(cards[0]);
                Debug.Log("HandUpdateOne");
            }
            else if (cards.Count == 2)
            {
                currentHand.HandUpdateTwo(cards[0]);
                Debug.Log("HandUpdateTwo");
            }
            else
            {
                if (cards.Max(card => card.model.Strenge) == cards.Min(card => card.model.Strenge))
                {
                    currentHand.SameThreeOrMore(cards[0], cards.Count);
                    Debug.Log("SameThreeOrMore");
                }
                else if (cards.Max(card => card.model.Strenge) != cards.Min(card => card.model.Strenge))
                {
                    currentHand.StairsTreeOrMore(cards[0], cards.Count);
                    Debug.Log("StairsTreeOrMore");
                }
            }
        }
        else
        {
            if (cards.Count == 0)
            {
                currentHand.HandUpdateZeroUD();
                Debug.Log("HandUpdateZeroUD");
            }
            else if (cards.Count == 1)
            {
                currentHand.HandUpdateOneUD(cards[0]);
                Debug.Log("HandUpdateOneUD");
            }
            else if (cards.Count == 2)
            {
                currentHand.HandUpdateTwoUD(cards[0]);
                Debug.Log("HandUpdateTwoUD");
            }
            else
            {
                if (cards.Max(card => card.model.Strenge) == cards.Min(card => card.model.Strenge))
                {
                    currentHand.SameThreeOrMoreUD(cards[0], cards.Count);
                    Debug.Log("SameThreeOrMoreUD");
                }
                else if (cards.Max(card => card.model.Strenge) != cards.Min(card => card.model.Strenge))
                {
                    currentHand.StairsTreeOrMoreUD(cards[0], cards.Count);
                    Debug.Log("StairsTreeOrMoreUD");
                }
            }
        }
    }

    [PunRPC]
    public void SetCardB()
    {
        if (!upsideDown)
        {
            if (cards.Count == 1)
            {
                currentHand.HandUpdateOneB(cards[0]);
                Debug.Log("HandUpdateOneB");
            }
            else if (cards.Count == 2)
            {
                currentHand.HandUpdatePlurlB(cards);
                Debug.Log("HandUpdatePlurlB");
            }
            else
            {
                if (cards.Max(card => card.model.Strenge) == cards.Min(card => card.model.Strenge))
                {
                    currentHand.HandUpdatePlurlB(cards);
                    Debug.Log("HandUpdatePlurlB");
                }
                else if (cards.Max(card => card.model.Strenge) != cards.Min(card => card.model.Strenge))
                {
                    currentHand.StairsTreeOrMoreB(cards[0], cards.Count);
                    Debug.Log("StairsTreeOrMoreB");
                }
            }
        }
        else
        {
            if (cards.Count == 1)
            {
                currentHand.HandUpdateOneBUD(cards[0]);
                Debug.Log("HandUpdateOneBUD");
            }
            else if (cards.Count == 2)
            {
                currentHand.HandUpdatePlurlBUD(cards);
                Debug.Log("HandUpdatePlurlBUD");
            }
            else
            {
                if (cards.Max(card => card.model.Strenge) == cards.Min(card => card.model.Strenge))
                {
                    currentHand.HandUpdatePlurlBUD(cards);
                    Debug.Log("HandUpdatePlurlBUD");
                }
                else if (cards.Max(card => card.model.Strenge) != cards.Min(card => card.model.Strenge))
                {
                    currentHand.StairsTreeOrMoreBUD(cards[0], cards.Count);
                    Debug.Log("StairsTreeOrMoreBUD");
                }
            }
        }
    }

    public IEnumerator BindInfo()
    {
        shadow.SetActive(true);

        irregularity.enabled = false;
        text.enabled = false;
        bindText.enabled = true;

        GameManager.instance.StopAllCoroutines();
        StartCoroutine(FadeIn(bindText));
        yield return new WaitForSeconds(2.5f); // Wait for 2.5 seconds
        shadow.SetActive(false);
    }

    [PunRPC]
    void FieldPreparation()
    {
        if (cards.Count > 0)
        {
            waitingCards.Clear();
            waitingCards.AddRange(cards);

            cards.Clear();

            foreach (var card in waitingCards)
            {
                Destroy(card.gameObject);
            }
        }
    }

    [PunRPC]
    public void IsDone(string handName)
    {
        PlayerHand hand = GameObject.Find(handName).GetComponent<PlayerHand>();

        //Debug.LogWarning($"<color=orange><size=20>IsDoneOnline</size></color>");

        photonView.RPC("FieldLineUp", RpcTarget.All);

        currentHand = hands[gameManager.currentPlayerIndex];

        OwnHandController own = currentHand.GetComponent<OwnHandController>();

        if (hand.allCards.Count == 0)
        {
            if (!upsideDown)
            {
                if (AllTheSame(13) || AllTheSame(6) || AlltheSameJ())
                {
                    hand.rank = 3;

                    photonView.RPC("Irregularity", RpcTarget.All);

                    if (own)
                    {
                        own.pass.SetActive(false);
                        own.skill.SetActive(false);
                        own.rank[hand.rank].SetActive(true);

                        Destroy(GameManager.instance.countText);
                    }

                    GameManager.instance.photonView.RPC("NextPlayer", PhotonNetwork.PlayerList[0]);
                }
                else
                {
                    Debug.Log("<color=orange><size=20>Done</size></orange>");

                    photonView.RPC("Text", RpcTarget.All);

                    hand.rank = rank;

                    if (own)
                    {
                        own.pass.SetActive(false);
                        if (own.skill != null)
                        {
                            own.skill.SetActive(false);
                        }
                        own.rank[hand.rank].SetActive(true);
                        Destroy(GameManager.instance.countText);
                    }

                    photonView.RPC("RankDown", RpcTarget.All);

                    GameManager.instance.photonView.RPC("NextPlayer", PhotonNetwork.PlayerList[0]);
                }
            }
            if (upsideDown)
            {
                if (AllTheSame(1) || AllTheSame(6) || AlltheSameJ())
                {
                    hand.rank = 3;

                    photonView.RPC("Irregularity", RpcTarget.All);


                    if (own)
                    {
                        own.pass.SetActive(false);
                        own.skill.SetActive(false);
                        own.rank[hand.rank].SetActive(true);
                        Destroy(GameManager.instance.countText);
                    }

                    GameManager.instance.photonView.RPC("NextPlayer", PhotonNetwork.PlayerList[0]);
                }
                else
                {
                    Debug.Log("<color=orange><size=20>Done</size></orange>");

                    shadow.SetActive(true);

                    photonView.RPC("Text", RpcTarget.All);

                    hand.rank = rank;

                    if (own)
                    {
                        own.pass.SetActive(false);
                        own.skill.SetActive(false);
                        own.rank[hand.rank].SetActive(true);
                        Destroy(GameManager.instance.countText);
                    }

                    photonView.RPC("RankDown", RpcTarget.All);

                    GameManager.instance.photonView.RPC("NextPlayer", PhotonNetwork.PlayerList[0]);
                }
            }
        }
        else
        {
            GameManager.instance.photonView.RPC("NextPlayer", PhotonNetwork.PlayerList[0]);
        }
    }

    [PunRPC]
    void FieldLineUp()
    {
        Debug.LogWarning($"<size=24><color=green>FieldLineUp {PhotonNetwork.LocalPlayer.ActorNumber}Player</color></size>");

        foreach (var card in cards)
        {
            Debug.LogWarning($"<color=green>cards No{card.model.Number} Strenge{card.model.Strenge} Suit{card.model.Suit}</color>");
        }

        cards = cards.OrderBy(x => x.model.Suit).OrderBy(x => x.model.Strenge).ToList();

        for (int i = 0; i < cards.Count; i++)
        {
            int posX;
            int posXToCenter;
            if (cards.Count > 1)
            {
                posX = i * 75;
                posXToCenter = cards.Count * 40;
            }
            else
            {
                posX = i * 0;
                posXToCenter = cards.Count * 0;
            }
            cards[i].transform.localPosition = new Vector3(posX - posXToCenter, 0);
            cards[i].transform.SetSiblingIndex(i);
            cards[i].transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
            cards[i].isSelected = false;
            cards[i].BackSideNotActive();
        }

        insert = true;
    }

    public bool AllTheSame(int targetNumber)
    {
        return cards.All(x => x.model.Strenge == targetNumber);
    }

    public bool AlltheSameJ()
    {
        return cards.All(x => x.model.Joker == true);
    }

    IEnumerator FadeIn(Text text)
    {
        for (float t = 0.01f; t < fadeInTime; t += Time.deltaTime)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Lerp(0, 1, t / fadeInTime));
            yield return null;
        }
    }

    [PunRPC]
    void RankDown()
    {
        rank++;
    }

    [PunRPC]
    void Irregularity()
    {
        StartCoroutine(IrregularityOnline());
    }

    IEnumerator IrregularityOnline()
    {
        shadow.SetActive(true);

        text.enabled = false;
        bindText.enabled = false;
        irregularity.enabled = true;

        GameManager.instance.StopAllCoroutines();
        StartCoroutine(FadeIn(irregularity));
        yield return new WaitForSeconds(2.5f); // Wait for 2.5 seconds
        shadow.SetActive(false);
    }

    [PunRPC]
    void Text()
    {
        StartCoroutine(TextOnline());
    }

    IEnumerator TextOnline()
    {
        shadow.SetActive(true);

        irregularity.enabled = false;
        bindText.enabled = false;
        text.enabled = true;

        GameManager.instance.StopAllCoroutines();
        StartCoroutine(FadeIn(text));
        yield return new WaitForSeconds(2.5f); // Wait for 2.5 seconds
        shadow.SetActive(false);
    }
}