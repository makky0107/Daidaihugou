using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToOniline : MonoBehaviourPunCallbacks
{
    public void ToOnlineScene()
    {
        // ���[������ޏo����
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("OnlineScene-F91");
    }
}
