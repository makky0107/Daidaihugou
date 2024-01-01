using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Unity.VisualScripting;

public class ActivateSkills : MonoBehaviourPunCallbacks
{
    public Field field;

    public PlayerHand ownHand;


    public CallSkill call;

    public bool indexMatch;

    public int index;

    public Text text;

    public LoggerScroll lS;

    private void Start()
    {
        lS = GameObject.Find("Log").GetComponent<LoggerScroll>();
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

        lS.photonView.RPC("AddLog", RpcTarget.All, $"{PhotonNetwork.LocalPlayer.ActorNumber - 1}player text {text.gameObject.name}");

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
            lS.photonView.RPC("AddLog", RpcTarget.All, $"{PhotonNetwork.LocalPlayer.ActorNumber - 1}player");

            if (IndexJudge(PhotonNetwork.LocalPlayer.ActorNumber - 1))
            {
                lS.photonView.RPC("AddLog", RpcTarget.All, $"{PhotonNetwork.LocalPlayer.ActorNumber - 1}player index {index}");

                photonView.RPC("ADeclarationOfWar", RpcTarget.All);

                indexMatch = true;
            }
            else
            {
                CannotInfo();
            }
        }

        if (indexMatch)
        {
            //lS.photonView.RPC("AddLog", RpcTarget.All, $"call {call}");
            //Debug.LogWarning($"<size=24><color=red>call {call}</color></size>");

            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                //lS.photonView.RPC("AddLog", RpcTarget.All, $"i = {i}");
                //Debug.LogWarning($"<size=22><color=orange>i = {i}</color></size>");

                if (i != PhotonNetwork.LocalPlayer.ActorNumber - 1)
                {
                    //lS.photonView.RPC("AddLog", RpcTarget.All, $"Judge Info Act");
                    //Debug.LogWarning($"<size=22><color=orange>Judge Info Act</color></size>");

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

    [PunRPC]
    public void ADeclarationOfWar()
    {
        GetField();

        lS.photonView.RPC("AddLog", RpcTarget.All, $"{PhotonNetwork.LocalPlayer.ActorNumber}player field {field}");

        field.upsideDown = !field.upsideDown;

        field.Judge(PhotonNetwork.LocalPlayer.ActorNumber - 1);

        lS.photonView.RPC("AddLog", RpcTarget.All, $"{PhotonNetwork.LocalPlayer.ActorNumber}player upsideDown {field.upsideDown}");
    }
}
