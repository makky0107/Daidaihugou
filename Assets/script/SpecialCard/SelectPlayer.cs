using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SelectPlayer : MonoBehaviourPunCallbacks
{
    [SerializeField] int player;
    ActivateSkills activate;

    private void Awake()
    {
        this.gameObject.SetActive(false);
    }

    public void PlsyerSelect()
    {
        activate = GameObject.Find("Empty(Clone)").GetComponent<ActivateSkills>();
        activate.SincerityDistribute(player);

        activate.photonView.RPC("SincerityDistributePreparation", PhotonNetwork.LocalPlayer, player);

        GameObject.Find("TwoPhand").GetComponentInChildren<SelectPlayer>().gameObject.SetActive(false);
        GameObject.Find("ThreePhand").GetComponentInChildren<SelectPlayer>().gameObject.SetActive(false);
        GameObject.Find("FourPhand").GetComponentInChildren<SelectPlayer>().gameObject.SetActive(false);
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
