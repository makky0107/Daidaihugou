using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.EventSystems;
using System.Text;
using Unity.VisualScripting;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] LoggerScroll lS;

    // カードを生成
    [SerializeField] PlayerHand ownHand;
    [SerializeField] PlayerHand threePHand;
    [SerializeField] PlayerHand twoPHand;
    [SerializeField] PlayerHand fourPHand;
    [SerializeField] Field field;

    public Text countText;

    public List<PlayerHand> hands;
    public List<Transform> allPosition;
    public List<Transform> otherHnands;
    public List<int> playerOrder;
    public List<int> otherActors;

    public List<int> onePCards;
    public List<int> twoPCards;
    public List<int> threePCards;
    public List<int> fourPCards;

    public List<List<int>> AllPCards;

    int ready;

    int onePIndex;
    int twoPIndex;
    int threePIndex;
    int fourPIndex;
    int time;
    public int deactiveCount;

    public int currentPlayerIndex;
    public int turnCount = 1;
    public int actorNumber;

    public int sCardNo;

    public static GameManager instance;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void Start()
    {
        //photonView.RPC("CrawlOnline", PhotonNetwork.LocalPlayer, 4f);

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("HandsSet", RpcTarget.All);

            photonView.RPC("IndexSet", PhotonNetwork.PlayerList[0]);

            Destribute(ownHand.transform, threePHand.transform, twoPHand.transform, fourPHand.transform);

            photonView.RPC("FieldCountSet", RpcTarget.All);

            photonView.RPC("NextPlayer", PhotonNetwork.PlayerList[0]);
            Debug.Log("start NextPlayer");
        }
    }

    [PunRPC]
    void CrawlOnline(float count)
    {
        StartCoroutine(Crawl(count));
    }

    IEnumerator SuccessionCarwl(float count)
    {
        while (true)
        {
            if (ready < PhotonNetwork.PlayerList.Length)
            {
                yield return new WaitForSeconds(count);

                Debug.LogWarning($"<size=15><color=green>ready {ready}</color></size>");
            }
            else
            {
                ready = 0;

                break;
            }
        }
    }

    [PunRPC]
    IEnumerator Crawl(float count)
    {
        yield return new WaitForSeconds(count);
    }

    [PunRPC]
    void HandsSet()
    {
        if (PlayerPrefs.HasKey("SCardKey"))
        {
            sCardNo = PlayerPrefs.GetInt("SCardKey");
        }

        ownHand.GetComponent<CallSkill>().sCardNo = sCardNo;

        playerOrder = PhotonNetwork.PlayerList.OrderBy(x => x.ActorNumber).Select(x => x.ActorNumber).ToList();

        hands = new List<PlayerHand> { ownHand, twoPHand, threePHand, fourPHand };

        AllPCards = new List<List<int>>() { onePCards, twoPCards, threePCards, fourPCards };
    }

    [PunRPC]
    void IndexSet()
    {
        if (PhotonNetwork.PlayerList.ToList().Count < 2)
        {
            twoPIndex = 1;
            twoPHand.index = twoPIndex;
        }
        if (PhotonNetwork.PlayerList.ToList().Count < 3)
        {
            threePIndex = 2;
            threePHand.index = threePIndex;
        }
        if (PhotonNetwork.PlayerList.ToList().Count < 4)
        {
            fourPIndex = 3;
            fourPHand.index = fourPIndex;
        }
        if (PhotonNetwork.PlayerList.ToList().Count > 1)
        {
            twoPIndex = 4;
            twoPHand.index = twoPIndex;
        }
        if (PhotonNetwork.PlayerList.ToList().Count > 2)
        {
            threePIndex = 4;
            threePHand.index = threePIndex;
        }
        if (PhotonNetwork.PlayerList.ToList().Count > 3)
        {
            fourPIndex = 4;
            fourPHand.index = fourPIndex;
        }

        Debug.Log($"<size=24><color=red>twoPIndex{twoPIndex}</color></size>");
        Debug.Log($"<size=24><color=red>threePIndex{threePIndex}</color></size>");
        Debug.Log($"<size=24><color=red>fourPIndex{fourPIndex}</color></size>");

        currentPlayerIndex = 4;
    }

    [PunRPC]
    void FieldCountSet()
    {
        field.turnCount = turnCount;
    }

    public IEnumerable<int> GetRandom(List<int> pack, int count)
    {
        var random = new System.Random();

        var indexList = new List<int>();
        for (int i = 0; i < pack.Count; i++)
        {
            indexList.Add(i);
        }

        for (int i = 0; i < count; i++)
        {
            int index = random.Next(0, indexList.Count);
            int value = indexList[index];
            yield return pack[value];
            indexList.RemoveAt(index);

        }
    }

    void Destribute(Transform onehand, Transform threehand, Transform twohand, Transform fourhand)
    {
        List<int> pack = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                                           11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
                                           21, 22, 23, 24, 25, 26, 27, 28, 29, 30,
                                           31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
                                           41, 42, 43, 44, 45, 46, 47, 48, 49, 50,
                                           51, 52, 53};

        photonView.RPC("AllPPositionSet", RpcTarget.All);

        int count = 0;
        foreach (var card in GetRandom(pack, 53))
        {
            if (count < 13)
            {
                photonView.RPC("CardCreateOfline", PhotonNetwork.PlayerList[0], card, 0);
                photonView.RPC("SetOwnCards", RpcTarget.All, 0, card);
            }
            else if (count < 13 * 2)
            {
                if (playerOrder.Count >= 2)
                {
                    photonView.RPC("CardCreateOfline", PhotonNetwork.PlayerList[1], card, 0);
                    photonView.RPC("SetOwnCards", RpcTarget.All, 1, card);
                }
                if (playerOrder.Count < 2)
                {
                    photonView.RPC("CardCreate", RpcTarget.All, card, 1);
                }
            }
            else if (count < 13 * 3)
            {
                if (playerOrder.Count >= 3)
                {
                    photonView.RPC("CardCreateOfline", PhotonNetwork.PlayerList[2], card, 0);
                    photonView.RPC("SetOwnCards", RpcTarget.All, 2, card);
                }
                if (playerOrder.Count < 3)
                {
                    photonView.RPC("CardCreate", RpcTarget.All, card, 2);
                }
            }
            else if (count <= 13 * 4)
            {
                if (playerOrder.Count >= 4)
                {
                    photonView.RPC("CardCreateOfline", PhotonNetwork.PlayerList[3], card, 0);
                    photonView.RPC("SetOwnCards", RpcTarget.All, 3, card);
                }
                if (playerOrder.Count < 4)
                {
                    photonView.RPC("CardCreate", RpcTarget.All, card, 3);
                }
            }
            count++;
        }

        foreach (var player in PhotonNetwork.PlayerList.ToList())
        {
            photonView.RPC("OtherhandsSet", player);
        }

        for (int i = 0; i < 4; i++)
        {
            if (i == 0)
            {
                photonView.RPC("OwnHandRessetPosition", RpcTarget.All, i);
            }
            else
            {
                photonView.RPC("OtherHandRessetPosition", RpcTarget.All, i);
            }
        }
    }

    [PunRPC]
    void AllPPositionSet()
    {
        allPosition = new List<Transform>() { ownHand.transform, twoPHand.transform, threePHand.transform, fourPHand.transform };
    }

    [PunRPC]
    void SetOwnCards(int position, int card)
    {
        AllPCards[position].Add(card);
    }

    [PunRPC]
    public void CardCreateOfline(int cardID, int i)
    {
        CardController card = Instantiate(Resources.Load("Prefab/Card")).GetComponent<CardController>();
        card.transform.SetParent(allPosition[i], false);
        card.Init(cardID);
    }
    
    [PunRPC]
    public void CardCreate(int cardID, int i)
    {
        CardController card = PhotonNetwork.Instantiate("Prefab/Card", allPosition[i].position, Quaternion.identity).GetComponent<CardController>();
        card.transform.SetParent(allPosition[i], false);
        card.photonView.RPC("Init", RpcTarget.All, cardID);
    }

    [PunRPC]
    void OtherhandsSet()
    {
        actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        otherActors = new List<int>(playerOrder);
        otherActors.Remove(actorNumber);
        otherHnands = new List<Transform>() { twoPHand.transform, threePHand.transform, fourPHand.transform };

        if (otherActors.Any())
        {
            for (int i = 0; i < otherActors.Count; i++)
            {
                photonView.RPC("SetOtherOnline", PhotonNetwork.LocalPlayer, otherActors[i], i);
            }
        }
    }

    [PunRPC]
    public void SetOtherOnline(int actorNo, int handNo)
    {
        foreach (var card in AllPCards[actorNo - 1])
        {
            CardController body = Instantiate(Resources.Load("Prefab/Card")).GetComponent<CardController>();

            body.transform.SetParent(otherHnands[handNo], false);
            body.Init(card);
        }        
    }

    [PunRPC]
    public void PlayCardsOnline(int cardID, int handNo)
    {
        CardController card = otherHnands[handNo].GetComponentsInChildren<CardController>().ToList().Find(x => x.model.ID == cardID);
        card.transform.SetParent(field.transform, false);
        field.cards.Add(card);
        card.turnCount = field.turnCount;
        card.hand.allCards.Remove(card);
    }

    [PunRPC]
    public void OwnHandRessetPosition(int handNo)
    {
        CardController[] cardsList = allPosition[handNo].GetComponentsInChildren<CardController>();
        List<CardController> cardsNumberList = cardsList.ToList();
        cardsNumberList = PutInOrder(cardsNumberList);

        for (int i = 0; i < cardsNumberList.Count; i++)
        {
            int posX = i * 60;
            int posXToCenter = cardsNumberList.Count * 28;
            cardsNumberList[i].transform.localPosition = new Vector3(posX - posXToCenter, 0);
            cardsNumberList[i].transform.SetSiblingIndex(i);
        }
    }

    [PunRPC]
    public void OtherHandRessetPosition(int handNo)
    {
        CardController[] cardsList = allPosition[handNo].GetComponentsInChildren<CardController>();
        List<CardController> cardsNumberList = cardsList.ToList();
        cardsNumberList = PutInOrder(cardsNumberList);

        for (int i = 0; i < cardsNumberList.Count; i++)
        {
            int posX = i * 20;
            int posXToCenter = cardsNumberList.Count * 9;
            cardsNumberList[i].transform.localPosition = new Vector3(posX - posXToCenter, 0);
            cardsNumberList[i].transform.localScale = Vector3.one / 3;
            cardsNumberList[i].transform.SetSiblingIndex(i);
        }
    }

    public List<CardController> PutInOrder(List<CardController> cardsNumberList)
    {
        var newCardsNumber = cardsNumberList.OrderBy(x => x.model.Suit).OrderBy(x => x.model.Strenge);

        List<CardController> newCardsNumberList = newCardsNumber.ToList();

        cardsNumberList = newCardsNumberList;

        return cardsNumberList;
    }

    public void ReturnPosition()
    {
        CardController[] cardsList = ownHand.GetComponentsInChildren<CardController>();
        List<CardController> cardsNumberList = cardsList.ToList();

        foreach (var card in cardsNumberList)
        {
            if (card.isSelected == true)
            {
                card.transform.localPosition -= Vector3.up * 50;
                card.isSelected = false;
            }
        }
    }

    [PunRPC]
    public void OnPass()
    {
        Debug.LogWarning("<size=24><color=grey>OnPass</color></size>");

        photonView.RPC("Pass", PhotonNetwork.PlayerList[0], PhotonNetwork.LocalPlayer.ActorNumber - 1);
    }

    [PunRPC]
    public void OnPassCPU()
    {
        Debug.LogWarning("<size=24><color=grey>OnPassCPU</color></size>");

        photonView.RPC("Pass", PhotonNetwork.PlayerList[0], currentPlayerIndex);
    }

    [PunRPC]
    public void Pass(int index)
    {
        Debug.LogWarning($"<size=24><color=grey>Pass index{index} currentPlayerIndex{currentPlayerIndex}</color></size>");

        if (index == currentPlayerIndex)
        {
            if (currentPlayerIndex < PhotonNetwork.PlayerList.Length)
            {
                if (currentPlayerIndex == PhotonNetwork.PlayerList[currentPlayerIndex].ActorNumber - 1 && PhotonNetwork.PlayerList[currentPlayerIndex] != null)
                {
                    photonView.RPC("RestrictionPlayerHand", PhotonNetwork.PlayerList[currentPlayerIndex], currentPlayerIndex);
                    ownHand.photonView.RPC("Pass", RpcTarget.All);
                }
            }
            else if (currentPlayerIndex == twoPIndex && playerOrder.Count < 2)
            {
                twoPHand.restriction = true;
                twoPHand.photonView.RPC("Pass", RpcTarget.All);
            }
            else if (currentPlayerIndex == threePIndex && playerOrder.Count < 3)
            {
                threePHand.restriction = true;
                threePHand.photonView.RPC("Pass", RpcTarget.All);
            }
            else if (currentPlayerIndex == fourPIndex && playerOrder.Count < 4)
            {
                fourPHand.restriction = true;
                fourPHand.photonView.RPC("Pass", RpcTarget.All);
            }

            photonView.RPC("NextPlayer", PhotonNetwork.PlayerList[0]);
            Debug.Log($"<size=24><color=yellow>Pass NextPlayer</color></size>");

            if (hands.Where(hand => hand.restriction == true && hand != ownHand).ToList().Count + deactiveCount > 2)
            {
                Debug.LogWarning($"<size=24><color=purple>hand.restrictions + deactiveCount{hands.Where(hand => hand.restriction == true && hand != ownHand).ToList().Count + deactiveCount}</color></size>");
                photonView.RPC("ResetField", PhotonNetwork.PlayerList[0]);
            }
        }  
    }

    [PunRPC]
    public void RestrictionPlayerHand(int actorNumber)
    {
        // 全てのPlayerHandを探して、指定されたactorNumberを持つものを見つける
        PlayerHand[] allHands = FindObjectsOfType<PlayerHand>();
        foreach (var hand in allHands)
        {
            if (hand.gameObject.name == "OwnHand")
            {
                Debug.LogWarning($"<size=24><color=red>hand.ownerActorNumber{hand.ownerActorNumber} actorNumber{actorNumber}</color></size>");

                if (hand.ownerActorNumber == actorNumber)
                {
                    if (!hand.restriction)
                    {
                        photonView.RPC("DactiveCountUp", RpcTarget.All);
                    }

                    hand.restriction = true;

                    Debug.LogWarning($"<size=24><color=purple>hand.restrictions + deactiveCount{hands.Where(hand => hand.restriction == true && hand != ownHand).ToList().Count + deactiveCount}</color></size>");

                    Debug.LogWarning($"<size=24><color=purple>{PhotonNetwork.LocalPlayer.ActorNumber}Player restriction {ownHand.restriction}</color></size>");

                    break;
                }
            }
        }
    }

    [PunRPC]
    void DactiveCountUp()
    {
        deactiveCount++;
    }

    [PunRPC]
    public void ResetField()
    {
        photonView.RPC("CheckMyTurn", RpcTarget.All);

        if (PhotonNetwork.PlayerList.ToList().Count < 2)
        {
            if (twoPHand.restriction)
            {
                twoPHand.isTurn = false;
            }
            else
            {
                twoPHand.isTurn = true;
            }
        }
        if (PhotonNetwork.PlayerList.ToList().Count < 3)
        {
            if (threePHand.restriction)
            {
                threePHand.isTurn = false;
            }
            else
            {
                threePHand.isTurn = true;
            }
        }
        if (PhotonNetwork.PlayerList.ToList().Count < 4)
        {
            if (fourPHand.restriction)
            {
                fourPHand.isTurn = false;
            }
            else
            {
                fourPHand.isTurn = true;
            }
        }

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            field.photonView.RPC("ResetField", PhotonNetwork.PlayerList[i]);

            photonView.RPC("FieldArrange", PhotonNetwork.PlayerList[i]);

            photonView.RPC("RestrictionCheck", PhotonNetwork.PlayerList[i]);
        }

        foreach (var hand in hands)
        {
            if (!hand.restriction && hand.gameObject.name != "OwnHand" && hand.index != 4)
            {
                Debug.LogWarning($"<size=24><color=AFDFE4>!hand.restriction name {hand} index {hand.index}</color></size>");

                currentPlayerIndex = hand.index;
                break;
            }
        }

        StartCoroutine(SuccessionCarwl(0.1f));

        foreach (var hand in hands)
        {
            if (hand.gameObject.name != "OwnHand")
            {
                hand.restriction = false;
            }
        }

        photonView.RPC("OwnHandRestrictionReset", RpcTarget.All);

        deactiveCount = 0;

        Debug.LogWarning($"<size=24><color=blue>ResetField currentPlayerIndex {currentPlayerIndex}</color></size>");

        photonView.RPC("StartTurnControl", PhotonNetwork.PlayerList[0]);
    }

    [PunRPC]
    void CheckMyTurn()
    {
        Debug.LogWarning($"<size=24><color=purple>{PhotonNetwork.LocalPlayer.ActorNumber}Player restriction {ownHand.restriction}</color></size>");
        if (ownHand.restriction)
        {
            ownHand.isTurn = false;
        }
        else
        {
            ownHand.isTurn = true;
        }
    }

    [PunRPC]
    void RestrictionCheck()
    {
        if (!ownHand.restriction)
        {
            photonView.RPC("IndexInsert", PhotonNetwork.PlayerList[0], PhotonNetwork.LocalPlayer.ActorNumber - 1);
        }

        photonView.RPC("ReadyPulass", PhotonNetwork.PlayerList[0]);

        Debug.LogWarning($"<size=24><color=green>{PhotonNetwork.LocalPlayer.ActorNumber}Player restriction {ownHand.restriction} currentPlayerIndex{currentPlayerIndex}</color></size>");
    }

    [PunRPC]
    void IndexInsert(int number)
    {
        Debug.LogWarning($"<size=18><color=grey>{PhotonNetwork.LocalPlayer.ActorNumber}Player Insert number{number}</color></size>");

        currentPlayerIndex = number;
    }

    [PunRPC]
    void ReadyPulass()
    {
        ready++;
    }

    [PunRPC]
    void FieldArrange()
    {
        field.bindInfoBool = false;

        turnCount++;
        field.turnCount = turnCount;

        if (field.elevenUD)
        {
            field.elevenUD = false;
            field.upsideDown = !field.upsideDown;
        }
    }

    [PunRPC]
    void OwnHandRestrictionReset()
    {
        ownHand.restriction = false;
    }

    [PunRPC]
    public void NextPlayer()
    {
        photonView.RPC("OtherCardsCount", RpcTarget.All);

        photonView.RPC("StopCountdownOnNetwork", RpcTarget.All);

        if (currentPlayerIndex < 3)
        {
            currentPlayerIndex++;

            photonView.RPC("StartTurnControl", PhotonNetwork.PlayerList[0]);
        }
        else
        {
            currentPlayerIndex = 0;

            photonView.RPC("StartTurnControl", PhotonNetwork.PlayerList[0]);
        }
    }

    [PunRPC]
    void OtherCardsCount()
    {
        Text twoPText = twoPHand.GetComponentInChildren<Image>().GetComponentInChildren<Image>().GetComponentInChildren<Text>();
        int twoPInt = twoPHand.transform.childCount - 1;
        twoPText.text = twoPInt.ToString();
        
        Text threePText = threePHand.GetComponentInChildren<Image>().GetComponentInChildren<Image>().GetComponentInChildren<Text>();
        int threePInt = threePHand.transform.childCount - 1;
        threePText.text = threePInt.ToString();
        
        Text fourPText = fourPHand.GetComponentInChildren<Image>().GetComponentInChildren<Image>().GetComponentInChildren<Text>();
        int fourPInt = fourPHand.transform.childCount - 1;
        fourPText.text = fourPInt.ToString();
    }

    [PunRPC]
    public void StartTurnControl()
    {
        StartCoroutine(TurnControl());
    }

    [PunRPC]
    public IEnumerator TurnControl()
    {
        Debug.Log($"<size=24><color=red>TurnChenge{currentPlayerIndex}</color></size>");

        Photon.Realtime.Player player = PhotonNetwork.LocalPlayer;
        player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "currentPlayerIndex", currentPlayerIndex } });

        GameObject twoPObj = GameObject.Find("TwoPHand");
        CPUAI twoPCPUAI = twoPObj.GetComponent<CPUAI>();
        GameObject threePObj = GameObject.Find("ThreePHand");
        CPUAI threePCPUAI = threePObj.GetComponent<CPUAI>();
        GameObject fourPObj = GameObject.Find("FourPHand");
        CPUAI fourPCPUAI = fourPObj.GetComponent<CPUAI>();

        yield return new WaitForSeconds(1f);

        Debug.Log($"<size=24><color=red>currentPlayerIndex {currentPlayerIndex}</color></size>");

        //int ActorNumber = playerOrder.Find(x => x == currentPlayerIndex + 1);

        if (playerOrder.Any(x => x == currentPlayerIndex + 1))
        {
            Debug.Log($"<size=24><color=red>discovery {currentPlayerIndex}</color></size>");

            photonView.RPC("MyTurn", PhotonNetwork.PlayerList[currentPlayerIndex]);
        }
        else if (currentPlayerIndex == twoPIndex)
        {
            field.photonView.RPC("Judge", RpcTarget.All, 1);

            photonView.RPC("PlayerUnableAct", RpcTarget.All);

            foreach (var hand in otherHnands)
            {
                hand.GetComponent<PlayerHand>().isTurn = false;
            }

            twoPHand.isTurn = true;

            if (twoPHand.restriction == true)
            {
                twoPHand.photonView.RPC("Restriction", RpcTarget.All);
            }

            Debug.Log("twoPHand.isTurn" + twoPHand.isTurn);

            twoPCPUAI.photonView.RPC("CPUTurn", PhotonNetwork.PlayerList[0]);

            if (twoPCPUAI.turnEnd)
            {
                twoPCPUAI.photonView.RPC("NotCPUTurn", PhotonNetwork.PlayerList[0]);
                photonView.RPC("OnPassCPU", PhotonNetwork.PlayerList[0]);
            }
        }
        else if (currentPlayerIndex == threePIndex)
        {
            field.photonView.RPC("Judge", RpcTarget.All, 2);

            photonView.RPC("PlayerUnableAct", RpcTarget.All);

            foreach (var hand in otherHnands)
            {
                hand.GetComponent<PlayerHand>().isTurn = false;
            }

            threePHand.isTurn = true;

            if (threePHand.restriction == true)
            {
                threePHand.photonView.RPC("Restriction", RpcTarget.All);
            }

            Debug.Log("threePHand.isTurn" + threePHand.isTurn);

            threePCPUAI.photonView.RPC("CPUTurn", PhotonNetwork.PlayerList[0]);

            if (threePCPUAI.turnEnd)
            {
                threePCPUAI.photonView.RPC("NotCPUTurn", PhotonNetwork.PlayerList[0]);
                photonView.RPC("OnPassCPU", PhotonNetwork.PlayerList[0]);
            }
        }
        else if (currentPlayerIndex == fourPIndex)
        {
            field.photonView.RPC("Judge", RpcTarget.All, 3);

            photonView.RPC("PlayerUnableAct", RpcTarget.All);

            foreach (var hand in otherHnands)
            {
                hand.GetComponent<PlayerHand>().isTurn = false;
            }

            fourPHand.isTurn = true;

            if (fourPHand.restriction == true)
            {
                fourPHand.photonView.RPC("Restriction", RpcTarget.All);
            }

            Debug.Log("fourPHand.isTurn" + fourPHand.isTurn);

            fourPCPUAI.photonView.RPC("CPUTurn", PhotonNetwork.PlayerList[0]);

            if (fourPCPUAI.turnEnd)
            {
                fourPCPUAI.photonView.RPC("NotCPUTurn", PhotonNetwork.PlayerList[0]); 
                photonView.RPC("OnPassCPU", PhotonNetwork.PlayerList[0]);
            }
        }
    }

    [PunRPC]
    public void StopCountdownOnNetwork()
    {
        StopCoroutine(CountDown());

        countText.text = 60.ToString();
    }

    [PunRPC]
    public void StopAllCoroutinesOnNetwork()
    {
        StopAllCoroutines();
    }

    [PunRPC]
    void PlayerUnableAct()
    {
        ownHand.isTurn = false;
    }

    [PunRPC]
    void StartCountdown()
    {
        lS.photonView.RPC("AddLog", RpcTarget.All, $"StartCountdown Player[{PhotonNetwork.LocalPlayer.ActorNumber - 1}]");
        StartCoroutine(CountDown());
    }

    IEnumerator CountDown()
    {
        time = 60;
        countText.text = time.ToString();

        while (time > 0)
        {
            yield return new WaitForSeconds(1);
            time--;
            countText.text = time.ToString();
        }

        photonView.RPC("OnPass", PhotonNetwork.LocalPlayer);
    }

    [PunRPC]
    void MyTurn()
    {
        lS.photonView.RPC("AddLog", RpcTarget.All, $"<color=gray>MyTurn {PhotonNetwork.LocalPlayer.ActorNumber}Player</color>");

        Debug.Log($"<size=24><color=green>MyTurn {PhotonNetwork.LocalPlayer.ActorNumber}Player</color></size>");

        int index = (int)PhotonNetwork.PlayerList[0].CustomProperties["currentPlayerIndex"];

        lS.photonView.RPC("AddLog", RpcTarget.All, $"PlayerList[{PhotonNetwork.LocalPlayer.ActorNumber - 1}] index {index}");

        if (index != PhotonNetwork.LocalPlayer.ActorNumber - 1)
        {
            lS.photonView.RPC("AddLog", RpcTarget.All, $"<color=black>MyTurn 1</color>");

            photonView.RPC("MyTurn", PhotonNetwork.PlayerList[index]);
        }
        else
        {
            lS.photonView.RPC("AddLog", RpcTarget.All, $"<color=black>MyTurn 2</color>");

            ownHand.isTurn = true;

            field.Judge(PhotonNetwork.LocalPlayer.ActorNumber - 1);

            Debug.Log($"<size=24><color=red>ownHand.isTurn {ownHand.isTurn}</color></size>");
            //lS.photonView.RPC("AddLog", RpcTarget.All, $"<color=black>MyTurn 3</color>");

            actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

            otherActors = new List<int>(playerOrder);
            otherActors.Remove(actorNumber);

            //lS.photonView.RPC("AddLog", RpcTarget.All, $"<color=black>MyTurn 4</color>");

            foreach (var other in otherActors)
            {
                foreach (var player in PhotonNetwork.PlayerList.ToList())
                {
                    if (player.ActorNumber == other)
                    {
                        photonView.RPC("PlayerUnableAct", player);
                    }
                }
            }

            //lS.photonView.RPC("AddLog", RpcTarget.All, $"<color=black>MyTurn 5</color>");

            foreach (var hand in otherHnands)
            {
                hand.GetComponent<PlayerHand>().isTurn = false;
            }

            //lS.photonView.RPC("AddLog", RpcTarget.All, $"<color=black>MyTurn 6</color>");

            photonView.RPC("NotMyTurn", RpcTarget.Others);

            //lS.photonView.RPC("AddLog", RpcTarget.All, $"<color=black>MyTurn 7</color>");

            if (ownHand.restriction == true)
            {
                //lS.photonView.RPC("AddLog", RpcTarget.All, $"<color=black>MyTurn 8</color>");

                ownHand.photonView.RPC("Restriction", PhotonNetwork.PlayerList[currentPlayerIndex]);
            }
            if (ownHand.allCards.Count == 0)
            {
                //lS.photonView.RPC("AddLog", RpcTarget.All, $"<color=black>MyTurn 9</color>");

                photonView.RPC("OnPass", PhotonNetwork.PlayerList[0]);
            }
            else
            {
                //lS.photonView.RPC("AddLog", RpcTarget.All, $"<color=black>MyTurn 10</color>");

                Debug.Log("ownHand.isTurn" + ownHand.isTurn);

                photonView.RPC("StopAllCoroutinesOnNetwork", RpcTarget.All);

                StartCountdown();
            }
        }  
    }

    [PunRPC]
    void NotMyTurn()
    {
        ownHand.isTurn = false;
    }
}