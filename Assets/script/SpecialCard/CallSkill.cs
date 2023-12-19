using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.EventSystems;
using System.Linq;

public class CallSkill : MonoBehaviourPunCallbacks
{
    public SCardController sCardPrefab;
    public Image shadow;
    public Image use;
    public Field field;
    public int sCardNo;

    public Text countText;
    public int time;
    public int skillReady;

    public Image atherPanel;
    public Image usePanel;
    public SCardController sCard;
    public ActivateSkills activate;

    public float screenHeight;
    public List<float> playerScreenSizes;

    public LoggerScroll lS;

    public GraphicRaycaster graphicRaycaster;

    private void Awake()
    {
        lS = GameObject.Find("Log").GetComponent<LoggerScroll>();

        playerScreenSizes = new List<float>() { 0f, 0f, 0f, 0f};

        if (gameObject.name == "OwnHand")
        {
            float screenHeight = (float)Screen.height;
            photonView.RPC("ReceiveScreenSize", RpcTarget.All, screenHeight, PhotonNetwork.LocalPlayer.ActorNumber - 1);
        }
    }

    [PunRPC]
    void ReceiveScreenSize(float screenHeight, int playerID)
    {
        lS.photonView.RPC("AddLog", RpcTarget.All, $"{playerID} Player screenHeight {screenHeight}");

        Debug.LogWarning($"<size=24><color=red>{playerID} Player screenHeight {screenHeight}</color></size>");
        playerScreenSizes[playerID] = screenHeight;
        Debug.LogWarning($"<size=24><color=red>playerScreenSizes[{playerID}]screenHeight {playerScreenSizes[playerID]}</color></size>");

        lS.photonView.RPC("AddLog", RpcTarget.All, $"playerScreenSizes[{playerID}]screenHeight {playerScreenSizes[playerID]}");
    }

    public void SkillActivation()
    {
        lS.photonView.RPC("AddLog", RpcTarget.All, $"SkillActivation");

        Debug.LogWarning($"<size=18><color=purple>SkillActivation</color></size>");

        CallShadow();
        HideCountText();
        CallSCard(sCardNo);
        CallActivateOBJ();
        CallUsePanel();
        photonView.RPC("DestroyUsePanel", RpcTarget.Others);
        SCardSetField();
        SCardSetHand();
        SCardSetThis();
        ShadowSetInfo();
        ShadowSetAct();
        atherPanel.gameObject.transform.localPosition = Vector3.zero;
        usePanel.gameObject.transform.localPosition = new Vector3(0, -1000f / 2540f * (float)Screen.height, transform.localPosition.z);
        sCard.gameObject.transform.localPosition = new Vector3(0, 300f / 2540f * (float)Screen.height, transform.localPosition.z);

        MovePanel();
    }

    public void SkillDestroy()
    {
        DestroySkillCard();
        DestroyUsePanel();
        DestroyShadow();

        if (activate != null)
        {
            PhotonNetwork.Destroy(activate.gameObject);
        }
    }

    [PunRPC]
    public void InfoForOther(int player, int sCardNumber)
    {
        lS.photonView.RPC("AddLog", RpcTarget.All, $"InfoForOther1");
        Debug.LogWarning($"<size=22><color=orange>InfoForOther1</color></size>");
        CallShadow();
        //lS.photonView.RPC("AddLog", RpcTarget.All, $"InfoForOther2");
        Debug.LogWarning($"<size=22><color=orange>InfoForOther2</color></size>");
        CallSCard(sCardNumber);
        //lS.photonView.RPC("AddLog", RpcTarget.All, $"InfoForOther3");
        Debug.LogWarning($"<size=22><color=orange>InfoForOther3</color></size>");
        //SCardSetField();
        //lS.photonView.RPC("AddLog", RpcTarget.All, $"InfoForOther4");
        Debug.LogWarning($"<size=22><color=orange>InfoForOther4</color></size>");
        CallOkPanel();
        //lS.photonView.RPC("AddLog", RpcTarget.All, $"InfoForOther5");
        Debug.LogWarning($"<size=22><color=orange>InfoForOther5</color></size>");
        ShadowSetInfo();
        //lS.photonView.RPC("AddLog", RpcTarget.All, $"InfoForOther6");
        Debug.LogWarning($"<size=22><color=orange>InfoForOther6</color></size>");
        StartCountdown();
        //lS.photonView.RPC("AddLog", RpcTarget.All, $"InfoForOther7");
        Debug.LogWarning($"<size=22><color=orange>InfoForOther7</color></size>");

        screenHeight = playerScreenSizes[player];

        lS.photonView.RPC("AddLog", RpcTarget.All, $"InfoForOther {player} Player screenHeight {screenHeight}");

        Debug.LogWarning($"<size=24><color=red>InfoForOther {player} Player screenHeight {screenHeight}</color></size>");

        atherPanel.gameObject.transform.localPosition = Vector3.zero;
        usePanel.gameObject.transform.localPosition = new Vector3(0, -1000f / 2540f * screenHeight, transform.localPosition.z);
        sCard.gameObject.transform.localPosition = new Vector3(0, 300f / 2540f * screenHeight, transform.localPosition.z);

        MovePanel();
    }

    [PunRPC]
    public void CallShadow()
    {
        Debug.LogWarning($"<size=18><color=purple>CallShadow</color></size>");

        atherPanel = Instantiate(Resources.Load("Prefab/SkillAtherPanel") as GameObject).GetComponent<Image>();

        atherPanel.gameObject.transform.SetParent(GameObject.Find("Canvas").transform, false);
    }

    [PunRPC]
    public void HideCountText()
    {
        //Debug.LogWarning($"<size=18><color=purple>HideCountText</color></size>");

        atherPanel.GetComponentInChildren<Text>().enabled = false;
    }

    [PunRPC]
    public void ShadowSetInfo()
    {
        Debug.LogWarning($"<size=18><color=purple>ShadowSetInfo</color></size>");

        CallSkill ather = atherPanel.GetComponent<CallSkill>();
        ather.sCard = sCard;
        ather.atherPanel = atherPanel;
        ather.usePanel = usePanel;
    }

    public void ShadowSetAct()
    {
        atherPanel.GetComponent<CallSkill>().activate = activate;
    }

    [PunRPC]
    public void CallUsePanel()
    {
        //Debug.LogWarning($"<size=18><color=purple>CallUsePanel</color></size>");

        usePanel = PhotonNetwork.Instantiate("Prefab/UsePanel", transform.position, Quaternion.identity).GetComponent<Image>();
        SCardUse panel = usePanel.GetComponent<SCardUse>();

        Destroy(usePanel.GetComponent<InterfaceAdjustment>());

        panel.gameObject.transform.SetParent(atherPanel.transform, false);

        panel.sCard = sCard;
        panel.activate = activate;
        panel.sCardNo = sCardNo;
    }

    [PunRPC]
    void CallOkPanel()
    {
        usePanel = Instantiate(Resources.Load("Prefab/OkPanel") as GameObject).GetComponent<Image>();
        CallSkill use = usePanel.GetComponent<CallSkill>();
        use.sCard = sCard;
        use.atherPanel = atherPanel;
        use.usePanel = usePanel;

        Destroy(usePanel.GetComponent<InterfaceAdjustment>());

        usePanel.gameObject.transform.SetParent(atherPanel.transform, false);
    }

    [PunRPC]
    public void CallSCard(int No)
    {
        //Debug.LogWarning($"<size=18><color=purple>CallSCard</color></size>");

        sCard = Instantiate(Resources.Load("Prefab/SpecialCard") as GameObject).GetComponent<SCardController>();
        sCard.Init(No);
        sCard.gameObject.transform.SetParent(GameObject.Find("Canvas").transform, false);
        //sCard.gameObject.SetActive(false);
    }

    [PunRPC]
    public void CallActivateOBJ()
    {
        activate = PhotonNetwork.Instantiate("Prefab/Empty", transform.position, Quaternion.identity).GetComponent<ActivateSkills>();
    }

    [PunRPC]
    public void SCardSetField()
    {
        //Debug.LogWarning($"<size=18><color=purple>SCardSetField</color></size>");

        activate.field = field;
    }

    [PunRPC]
    public void SCardSetHand()
    {
        //Debug.LogWarning($"<size=18><color=purple>SCardSetHand</color></size>");

        activate.hand = GetComponent<PlayerHand>();

        //Debug.LogWarning($"<size=24><color=red>activate.hand{activate.hand}</color></size>");
    }

    [PunRPC]
    public void SCardSetThis()
    {
        //Debug.LogWarning($"<size=24><color=red>SCardSetThis {this}</color></size>");

        activate.call = this;

        //Debug.LogWarning($"<size=24><color=red>activate.call {activate.call}</color></size>");
    }

    public void MovePanel()
    {
        lS.photonView.RPC("AddLog", RpcTarget.All, $"MovePanel1");

        lS.photonView.RPC("AddLog", RpcTarget.All, $"usePanel is {usePanel}");

        lS.photonView.RPC("AddLog", RpcTarget.All, $"MovePanel Is {IsOver(usePanel.GetComponent<RectTransform>())}");

        lS.photonView.RPC("AddLog", RpcTarget.All, $"MovePanel2");

        if (IsOver(usePanel.GetComponent<RectTransform>()))
        {
            lS.photonView.RPC("AddLog", RpcTarget.All, $"MovePanel3");

            usePanel.transform.localPosition -= Vector3.up * 20;
            MovePanel();
        }
        else
        {
            lS.photonView.RPC("AddLog", RpcTarget.All, $"MovePanel4");

            return;
        }
    }

    [PunRPC]
    public bool IsOver(RectTransform targetPanel)
    {
        lS.photonView.RPC("AddLog", RpcTarget.All, $"IsOver1");

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);

        lS.photonView.RPC("AddLog", RpcTarget.All, $"IsOver2");

        pointerEventData.position = targetPanel.position;

        lS.photonView.RPC("AddLog", RpcTarget.All, $"IsOver3");

        List<RaycastResult> results = new List<RaycastResult>();

        lS.photonView.RPC("AddLog", RpcTarget.All, $"IsOver4");

        graphicRaycaster.Raycast(pointerEventData, results);

        lS.photonView.RPC("AddLog", RpcTarget.All, $"results.count {results.Count}");

        lS.photonView.RPC("AddLog", RpcTarget.All, $"IsOver5");

        foreach (RaycastResult result in results)
        {
            lS.photonView.RPC("AddLog", RpcTarget.All, $"result {result.gameObject.name}");

            SCardController cont = result.gameObject.GetComponent<SCardController>();

            if (cont != null)
            {
                lS.photonView.RPC("AddLog", RpcTarget.All, $"IsOver6");

                return true;
            }
        }
        
        return false;
    }

    [PunRPC]
    public void DestroyShadow()
    {
        Destroy(atherPanel.gameObject);
    }

    [PunRPC]
    public void DestroyUsePanel()
    {
        //Debug.LogWarning($"<size=18><color=purple>DestroyUsePanel usePanel{usePanel.gameObject}</color></size>");

        Destroy(usePanel.gameObject);
    }

    [PunRPC]
    public void DestroySkillCard()
    {
        //Debug.LogWarning($"<size=18><color=purple>DestroySkillCard sCard{sCard.gameObject}</color></size>");

        Destroy(sCard.gameObject);
    }

    [PunRPC]
    void StartCountdown()
    {
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

        photonView.RPC("Ready", PhotonNetwork.LocalPlayer);
    }

    [PunRPC]
    public void Ready()
    {
        photonView.RPC("SkillReadyPlus", RpcTarget.All);

        photonView.RPC("SkillDestroy", PhotonNetwork.LocalPlayer);

        StartCoroutine(SuccessionCarwl(0.5f));
    }

    IEnumerator SuccessionCarwl(float count)
    {
        while (true)
        {
            if (skillReady < PhotonNetwork.PlayerList.Length - 1)
            {
                GameManager.instance.StopCoroutine(CountDown());

                yield return new WaitForSeconds(count);
            }
            else
            {
                GameManager.instance.StartCoroutine(CountDown());

                photonView.RPC("SikllReadyReset", RpcTarget.All);

                break;
            }
        }
    }

    [PunRPC]
    void SkillReadyPlus()
    {
        skillReady++;
    }

    [PunRPC]
    void SikllReadyReset()
    {
        skillReady = 0;
    }
}
