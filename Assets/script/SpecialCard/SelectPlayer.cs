using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SelectPlayer : MonoBehaviourPunCallbacks
{
    [SerializeField] int player;
    ActivateSkills activate;

    PlayerHand twoPHand;
    PlayerHand threePHand;
    PlayerHand fourPHand;

    private void Awake()
    {
        this.gameObject.SetActive(false);
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

    public void PlsyerSelect()
    {
        activate = GameObject.Find("Empty(Clone)").GetComponent<ActivateSkills>();
        activate.SincerityDistribute(player);

        activate.photonView.RPC("SincerityDistributePreparation", RpcTarget.Others, player);

        GetTwoPHand();
        GetThreePHand();
        GetForPHand();

        twoPHand.transform.Find("SelectOne").gameObject.SetActive(false);
        threePHand.transform.Find("SelectTwo").gameObject.SetActive(false);
        fourPHand.transform.Find("SelectThree").gameObject.SetActive(false);
    }
    
    public void JudgePlayer(int player)
    {
        if (PhotonNetwork.PlayerList[player] != null)
        {
            if (PhotonNetwork.PlayerList[player].ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                activate.photonView.RPC("SincerityDistributeForYou", PhotonNetwork.PlayerList[player]);
            }
            else
            {
                JudgePlayer(player + 1);
            }
        }
        else
        {
            activate.photonView.RPC("SincerityDistributeForOther", RpcTarget.Others, player);
        }
    }
}
