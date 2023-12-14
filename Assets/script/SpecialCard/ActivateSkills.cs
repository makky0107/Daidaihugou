using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ActivateSkills : MonoBehaviourPunCallbacks
{
    public Field field;
    public PlayerHand hand;
    public CallSkill call;

    public LoggerScroll lS;

    private void Start()
    {
        lS = GameObject.Find("Log").GetComponent<LoggerScroll>();
    }

    [PunRPC]
    public void Judge(int sCardNo)
    {
        if (sCardNo == 0)
        {
            photonView.RPC("ADeclarationOfWar", RpcTarget.All);
        }

        lS.photonView.RPC("AddLog", RpcTarget.All, $"call {call}");

        Debug.LogWarning($"<size=24><color=red>call {call}</color></size>");

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            lS.photonView.RPC("AddLog", RpcTarget.All, $"i = {i}");

            Debug.LogWarning($"<size=22><color=orange>i = {i}</color></size>");

            if (i != PhotonNetwork.LocalPlayer.ActorNumber - 1)
            {
                lS.photonView.RPC("AddLog", RpcTarget.All, $"Judge Info Act");

                Debug.LogWarning($"<size=22><color=orange>Judge Info Act</color></size>");

                call.photonView.RPC("InfoForOther", PhotonNetwork.PlayerList[i], sCardNo);
            }
        }
    }

    [PunRPC]
    public void ADeclarationOfWar()
    {
        field.upsideDown = !field.upsideDown;
    }
}
