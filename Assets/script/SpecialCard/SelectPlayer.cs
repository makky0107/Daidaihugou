using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectPlayer : MonoBehaviour
{
    [SerializeField] int player;

    private void Awake()
    {
        this.gameObject.SetActive(false);
    }

    public void PlsyerSelect()
    {
        ActivateSkills activate = GameObject.Find("empty").GetComponent<ActivateSkills>();
        activate.SincerityDistribute(player);

        activate.photonView.RPC("SincerityDistributeForOther", Photon.Pun.PhotonNetwork.LocalPlayer, player);

        GameObject.Find("SelectOne").SetActive(true);
        GameObject.Find("SelectTwo").SetActive(true);
        GameObject.Find("SelectThree").SetActive(true);
    }
    
}
