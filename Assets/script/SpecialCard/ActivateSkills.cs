using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Linq;

public class ActivateSkills : MonoBehaviourPunCallbacks
{
    public Field field;

    public PlayerHand ownHand;
    public PlayerHand twoPHand;
    public PlayerHand threePHand;
    public PlayerHand fourPHand;
    List<PlayerHand> others;

    public CallSkill call;

    public bool indexMatch;

    public int index;
    public int searchCount;

    public Text text;

    public LoggerScroll lS;

    List<CardController> cards;
    List<CardController> distCards;

    private void Start()
    {
        if (GameObject.Find("Log") != null)
        {
            lS = GameObject.Find("Log").GetComponent<LoggerScroll>();
        }
    }

    public bool IndexJudge(int currentIndex)
    {
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.ContainsKey("currentPlayerIndex"))
            {
                index = (int)player.CustomProperties["currentPlayerIndex"];
            }
        }

        return index == currentIndex;
    }

    public void CannotInfo()
    {
        text = Instantiate(Resources.Load("Prefab/InfoText")).GetComponent<Text>();

        //lS.photonView.RPC("AddLog", RpcTarget.All, $"{PhotonNetwork.LocalPlayer.ActorNumber - 1}player text {text.gameObject.name}");

        text.transform.SetParent(GameObject.Find("Canvas").transform, false);
        text.transform.localPosition = Vector3.zero;
        text.text = "ç°ÇÕégópÇ≈Ç´Ç‹ÇπÇÒ";

        StartCoroutine(FadeOut(text));
    }

    IEnumerator FadeOut(Text text)
    {
        yield return new WaitForSeconds(1);

        for (float t = 0.01f; t < 1f; t += Time.deltaTime)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Lerp(1, 0, t / 1f));
            yield return null;
        }

        Destroy(text.gameObject);
    }

    [PunRPC]
    public void Judge(int sCardNo)
    {
        indexMatch = false;

        if (sCardNo == 0)
        {
            if (lS != null)
            {
                lS.photonView.RPC("AddLog", RpcTarget.All, $"{PhotonNetwork.LocalPlayer.ActorNumber - 1}player");
            }

            if (IndexJudge(PhotonNetwork.LocalPlayer.ActorNumber - 1))
            {
                photonView.RPC("ADeclarationOfWar", RpcTarget.All);

                SkillButtonDestroy();

                indexMatch = true;
            }
            else
            {
                CannotInfo();
            }
        }

        if (sCardNo == 1)
        {
            if (IndexJudge(PhotonNetwork.LocalPlayer.ActorNumber - 1))
            {
                GraveRobbing();

                SkillButtonDestroy();

                indexMatch = true;
            }
            else
            {
                CannotInfo();
            }
        }

        if (sCardNo == 2)
        {
            if (IndexJudge(PhotonNetwork.LocalPlayer.ActorNumber - 1))
            {
                Siirecus();

                SkillButtonDestroy();

                indexMatch = true;
            }
            else
            {
                CannotInfo();
            }
        }

        if (sCardNo == 3)
        {
            BadEnd();

            SkillButtonDestroy();

            indexMatch = true;
        }

        if (sCardNo == 4)
        {


            SkillButtonDestroy();

            indexMatch = true;
        }

        if (indexMatch)
        {
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (i != PhotonNetwork.LocalPlayer.ActorNumber - 1)
                {
                    call.photonView.RPC("InfoForOther", PhotonNetwork.PlayerList[i], i, sCardNo);
                }
            }
        }
    }

    public void GetField()
    {
        field = GameObject.Find("Field").GetComponent<Field>();
    }

    public void GetOwnHand()
    {
        ownHand = GameObject.Find("OwnHand").GetComponent<PlayerHand>();
    }

    public void GetTwoPHand()
    {
        twoPHand = GameObject.Find("TwoPHand").GetComponent<PlayerHand>();
    }

    public void GetThreePHand()
    {
        threePHand = GameObject.Find("ThreePHand").GetComponent<PlayerHand>();
    }

    public void GetForPHand()
    {
        fourPHand = GameObject.Find("FourPHand").GetComponent<PlayerHand>();
    }

    public void SkillButtonDestroy()
    {
        Destroy(GameObject.Find("SkillButton"));
    }

    public void CardOderOwnHand(PlayerHand hand)
    {
        var miniList = hand.allCards.Where(x => x.model.Joker == true).ToList();
        foreach (var joker in miniList)
        {
            joker.model.Strenge = 14;
        }
        miniList = null;

        hand.allCards = hand.allCards.OrderBy(x => x.model.Suit).OrderBy(x => x.model.Strenge).ToList();

        for (int i = 0; i < hand.allCards.Count; i++)
        {
            int posX = i * 60;
            int posXToCenter = hand.allCards.Count * 28;
            hand.allCards[i].transform.localPosition = new Vector3(posX - posXToCenter, 0);
            hand.allCards[i].transform.SetSiblingIndex(i);
            hand.allCards[i].GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
    }

    public void CardOrderOtherHand(PlayerHand hand)
    {
        var miniList = hand.allCards.Where(x => x.model.Joker == true).ToList();
        foreach (var joker in miniList)
        {
            joker.model.Strenge = 14;
        }
        miniList = null;

        hand.allCards = hand.allCards.OrderBy(x => x.model.Suit).OrderBy(x => x.model.Strenge).ToList();

        for (int i = 0; i < hand.allCards.Count; i++)
        {
            int posX = i * 20;
            int posXToCenter = hand.allCards.Count * 9;
            hand.allCards[i].transform.localPosition = new Vector3(posX - posXToCenter, 0);
            hand.allCards[i].transform.localPosition = new Vector3(1f, 1f, 1f);
            hand.allCards[i].transform.localScale = Vector3.one / 3;
            hand.allCards[i].transform.SetSiblingIndex(i);
        }
    }

    public void CardOrderField(Field field)
    {
        field.cards = field.cards.OrderBy(x => x.model.Suit).OrderBy(x => x.model.Strenge).ToList();

        for (int i = 0; i < field.cards.Count; i++)
        {
            int posX;
            int posXToCenter;
            if (field.cards.Count > 1)
            {
                posX = i * 75;
                posXToCenter = field.cards.Count * 40;
            }
            else
            {
                posX = i * 0;
                posXToCenter = field.cards.Count * 0;
            }
            field.cards[i].transform.localPosition = new Vector3(posX - posXToCenter, 0);
            field.cards[i].transform.SetSiblingIndex(i);
            field.cards[i].transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
            field.cards[i].isSelected = false;
            field.cards[i].BackSideNotActive();
        }
    }

    [PunRPC]
    public void ADeclarationOfWar()
    {
        GetField();

        field.upsideDown = !field.upsideDown;

        field.Judge(PhotonNetwork.LocalPlayer.ActorNumber - 1);
    }

    [PunRPC]
    public void GraveRobbing()
    {
        GetField();
        GetOwnHand();

        ownHand.allCards.AddRange(field.cards);
        for (int i = 0; i < field.cards.Count; i++)
        {
            field.cards[i].transform.SetParent(ownHand.transform, false);
            field.cards[i].transform.localScale = new Vector3(1f, 1f, 1f);
            field.cards[i].hand = ownHand;
        }
        field.cards.Clear();
        CardOderOwnHand(ownHand);

        photonView.RPC("GraveRobbingForOther", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber);

        photonView.RPC("WeitingCardsBack", RpcTarget.All);

        field.Judge(PhotonNetwork.LocalPlayer.ActorNumber - 1);
    }

    [PunRPC]
    public void GraveRobbingForOther(int player)
    {
        GetField();

        int index = GameManager.instance.otherActors.IndexOf(player);

        GameManager.instance.otherHnands[index].GetComponent<PlayerHand>().allCards.AddRange(field.cards);
        for (int i = 0; i < field.cards.Count; i++)
        {
            field.cards[i].transform.SetParent(GameManager.instance.otherHnands[index], false);
        }
        field.cards.Clear();

        CardOrderOtherHand(GameManager.instance.otherHnands[index].GetComponent<PlayerHand>());
    }

    [PunRPC]
    public void WeitingCardsBack()
    {
        foreach (var card in field.waitingCards)
        {
            CardController body = Instantiate(Resources.Load("Prefab/Card")).GetComponent<CardController>();

            body.transform.SetParent(field.transform, false);
            body.Init(card.model.ID);

            field.cards.Remove(card);
            field.cards.Add(body);
        }
        field.waitingCards.Clear();

        CardOrderField(field);
    }

    public void Siirecus()
    {
        GetOwnHand();

        for (int i = 0; i < 2; i++)
        {
            CardController card = Instantiate(Resources.Load("Prefab/Card")).GetComponent<CardController>();

            card.transform.SetParent(ownHand.transform);
            card.Init(53);

            float workW = (float)Screen.width / 1170f;
            float workH = (float)Screen.height / 2540f;

            float _adj = 0;
            if (workW < workH)
            {
                _adj = workW;
            }
            else
            {
                _adj = workH;
            }
            card.transform.localScale *= _adj;

            ownHand.allCards.Add(card);
        }
        CardOderOwnHand(ownHand);

        //lS.photonView.RPC("AddLog", RpcTarget.All, $"<color=red>ownHand.allCards {ownHand.allCards.Count}");

        photonView.RPC("SiirecusForOther", RpcTarget.Others, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    public void SiirecusForOther(int player)
    {
        int index = GameManager.instance.otherActors.IndexOf(player);

        PlayerHand crrentPlayer = GameManager.instance.otherHnands[index].GetComponent<PlayerHand>();

        for (int i = 0; i < 2; i++)
        {
            CardController card = Instantiate(Resources.Load("Prefab/Card")).GetComponent<CardController>();

            card.transform.SetParent(crrentPlayer.transform, false);
            card.Init(53);

            crrentPlayer.allCards.Add(card);
        }

        CardOrderOtherHand(crrentPlayer);
    }

    public void BadEnd()
    {
        photonView.RPC("TurnChengeMine", PhotonNetwork.PlayerList[0], PhotonNetwork.LocalPlayer.ActorNumber - 1);
    }

    [PunRPC]
    public void TurnChengeMine(int index)
    {
        GameManager.instance.currentPlayerIndex = index;

        GameManager.instance.StartTurnControl();
    }

    public void Sincerity()
    {
        GetOwnHand();
        GetTwoPHand();
        GetThreePHand();
        GetForPHand();

        GameObject.Find("SelectOne").SetActive(true);
        GameObject.Find("SelectTwo").SetActive(true);
        GameObject.Find("SelectThree").SetActive(true);

        others = new List<PlayerHand>() { twoPHand, threePHand, fourPHand };

        searchCount = 0;

        ApexStrengeSearch(ownHand);
    }

    public void ApexStrengeSearch(PlayerHand hand)
    {
        if (searchCount >= 3 && hand.allCards.Count > 1)
        {
            CardController maxCard = hand.allCards.Find(x => x.model.Strenge == hand.allCards.Max(x => x.model.Strenge));
            photonView.RPC("CardPulass", RpcTarget.All, maxCard.model.ID);
            hand.allCards.Remove(maxCard);

            searchCount++;

            ApexStrengeSearch(hand);
        }
        
    }

    [PunRPC]
    public void CardPulass(int no)
    {
        CardController card = Instantiate(Resources.Load("Prefab/Card")).GetComponent<CardController>();

        card.Init(no);

        float workW = (float)Screen.width / 1170f;
        float workH = (float)Screen.height / 2540f;

        float _adj = 0;
        if (workW < workH)
        {
            _adj = workW;
        }
        else
        {
            _adj = workH;
        }
        card.transform.localScale *= _adj;

        distCards.Add(card);
    }

    [PunRPC]
    public void SincerityDistribute(int hand)
    {
        others[hand].allCards.AddRange(cards);
    }

    [PunRPC]
    public void SincerityDistributeForOther(int ownerHand, int distHand)
    {
        int ownIndex = GameManager.instance.otherActors.IndexOf(ownerHand);
        PlayerHand crrentPlayer = GameManager.instance.otherHnands[ownIndex].GetComponent<PlayerHand>();

        for (int i = 0; i < distCards.Count; i++)
        {
            crrentPlayer.allCards.Remove(crrentPlayer.allCards.Find(x => x.model.ID == distCards[i].model.ID));
        }

        if (distHand == PhotonNetwork.LocalPlayer.ActorNumber - 1)
        {
            GetOwnHand();
            ownHand.allCards.AddRange(distCards);
        }
        else
        {
            int index = GameManager.instance.otherActors.IndexOf(distHand);
            PlayerHand distributePlayer = GameManager.instance.otherHnands[index].GetComponent<PlayerHand>();
            distributePlayer.allCards.AddRange(distCards); 
        }
    }
}
