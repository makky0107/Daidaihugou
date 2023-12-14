using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SCardUse : MonoBehaviourPunCallbacks
{
    public SCardController sCard;
    public ActivateSkills activate;
    public int sCardNo;

    public void SkillTrigger()
    {
        activate.photonView.RPC("Judge", PhotonNetwork.LocalPlayer, sCardNo);

        CallSkill skill = GetComponentInParent<CallSkill>();
        Destroy(skill.atherPanel.gameObject);
        Destroy(skill.sCard.gameObject);
        Destroy(skill.usePanel.gameObject);
    }
}
