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

    public int time;
    public int skillReady;

    public Image atherPanel;
    public Image usePanel;
    public SCardController sCard;
    public ActivateSkills activate;

    public float screenHeight;
    public List<float> playerScreenSizes;

    public LoggerScroll lS;

    public float workW;
    public float workH;

    private void Awake()
    {
        if (GameObject.Find("Log") != null)
        {
            lS = GameObject.Find("Log").GetComponent<LoggerScroll>();
        }

        playerScreenSizes = new List<float>() { 0f, 0f, 0f, 0f};

        if (gameObject.name == "OwnHand")
        {
            float screenHeight = (float)Screen.height;
            photonView.RPC("ReceiveScreenSize", RpcTarget.All, screenHeight, PhotonNetwork.LocalPlayer.ActorNumber - 1);
        }
    }

    IEnumerator Crawl(float count)
    {
        yield return new WaitForSeconds(count);
    }

    [PunRPC]
    void ReceiveScreenSize(float screenHeight, int playerID)
    {
        //lS.photonView.RPC("AddLog", RpcTarget.All, $"{playerID} Player screenHeight {screenHeight}");

        //Debug.LogWarning($"<size=24><color=red>{playerID} Player screenHeight {screenHeight}</color></size>");
        playerScreenSizes[playerID] = screenHeight;
        //Debug.LogWarning($"<size=24><color=red>playerScreenSizes[{playerID}]screenHeight {playerScreenSizes[playerID]}</color></size>");

        //lS.photonView.RPC("AddLog", RpcTarget.All, $"playerScreenSizes[{playerID}]screenHeight {playerScreenSizes[playerID]}");
    }

    public void SkillActivation()
    {
        //lS.photonView.RPC("AddLog", RpcTarget.All, $"SkillActivation");

        //Debug.LogWarning($"<size=18><color=purple>SkillActivation</color></size>");

        CallShadow();
        HideCountText();
        CallSCard(sCardNo);
        CallActivateOBJ();
        CallUsePanel();
        //photonView.RPC("DestroyUsePanel", RpcTarget.Others);
        //photonView.RPC("SCardSetField", RpcTarget.All);
        //photonView.RPC("SCardSetHand", RpcTarget.All);
        SCardSetThis();
        ShadowSetInfo();
        ShadowSetAct();
        atherPanel.gameObject.transform.localPosition = Vector3.zero;
        usePanel.gameObject.transform.localPosition = new Vector3(0, -1000f / 2540f * (float)Screen.height, transform.localPosition.z);
        sCard.gameObject.transform.localPosition = new Vector3(0, 300f / 2540f * (float)Screen.height, transform.localPosition.z);

        if ((float)Screen.height < 1001f)
        {
            usePanel.gameObject.transform.localPosition -= Vector3.up * 130;
            sCard.gameObject.transform.localPosition += Vector3.up * 70;
        }
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
        //lS.photonView.RPC("AddLog", RpcTarget.All, $"InfoForOther1");
        CallShadow();
        CallSCard(sCardNumber);
        CallOkPanel();
        ShadowSetInfo();
        atherPanel.GetComponent<CallSkill>().StartCountdown();

        screenHeight = playerScreenSizes[player];

        //lS.photonView.RPC("AddLog", RpcTarget.All, $"InfoForOther {player} Player screenHeight {screenHeight}");
        
        //Debug.LogWarning($"<size=24><color=red>InfoForOther {player} Player screenHeight {screenHeight}</color></size>");

        atherPanel.gameObject.transform.localPosition = Vector3.zero;
        usePanel.gameObject.transform.localPosition = new Vector3(0, -1000f / 2540f * screenHeight, transform.localPosition.z);
        sCard.gameObject.transform.localPosition = new Vector3(0, 300f / 2540f * screenHeight, transform.localPosition.z);

        if ((float)Screen.height < 1001f)
        {
            usePanel.gameObject.transform.localPosition -= Vector3.up * 130;
            sCard.gameObject.transform.localPosition += Vector3.up * 70;
        }
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
                
        usePanel = Instantiate(Resources.Load("Prefab/UsePanel") as GameObject).GetComponent<Image>();
        SCardUse panel = usePanel.GetComponent<SCardUse>();

        Destroy(usePanel.GetComponent<InterfaceAdjustment>());

        panel.gameObject.transform.SetParent(atherPanel.transform, false);

        panel.sCard = sCard;
        panel.activate = activate;
        panel.sCardNo = sCardNo;

        float _adj = 0;
        if (workW < workH)
        {
            _adj = workW;
        }
        else
        {
            _adj = workH;
        }
        usePanel.transform.localScale /= _adj;
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

        float _adj = 0;
        if (workW < workH)
        {
            _adj = workW;
        }
        else
        {
            _adj = workH;
        }
        usePanel.transform.localScale /= _adj;
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

        activate.ownHand = GetComponent<PlayerHand>();

        //Debug.LogWarning($"<size=24><color=red>activate.hand{activate.hand}</color></size>");
    }

    [PunRPC]
    public void SCardSetThis()
    {
        //Debug.LogWarning($"<size=24><color=red>SCardSetThis {this}</color></size>");

        activate.call = this;

        //Debug.LogWarning($"<size=24><color=red>activate.call {activate.call}</color></size>");
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
        Text text = GetComponentInChildren<Text>();
        text.text = time.ToString();

        while (time > 0)
        {
            yield return new WaitForSeconds(1);
            time--;
            text.text = time.ToString();
            GameManager.instance.StopCoroutine(CountDown());
        }

        Ready();
    }

    [PunRPC]
    public void Ready()
    {
        SkillDestroy();

        GameManager.instance.StartCoroutine(CountDown());
    }
}
