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

    public float workW;
    public float workH;

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

    IEnumerator Crawl(float count)
    {
        yield return new WaitForSeconds(count);
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
        lS.photonView.RPC("AddLog", RpcTarget.All, $"InfoForOther1");
        CallShadow();
        CallSCard(sCardNumber);
        CallOkPanel();
        ShadowSetInfo();
        StartCountdown();

        screenHeight = playerScreenSizes[player];

        lS.photonView.RPC("AddLog", RpcTarget.All, $"InfoForOther {player} Player screenHeight {screenHeight}");

        Debug.LogWarning($"<size=24><color=red>InfoForOther {player} Player screenHeight {screenHeight}</color></size>");

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

        Vector3 worldPosition = targetPanel.position;

        RectTransform canvasRectTransform = GameObject.Find("Canvas").GetComponent<RectTransform>();

        Vector3 localPosition = canvasRectTransform.InverseTransformPoint(worldPosition);

        GameObject nullObject = Instantiate(Resources.Load("Prefab/UsePanel") as GameObject);

        nullObject.transform.SetParent(GameObject.Find("Canvas").transform);

        nullObject.GetComponent<RectTransform>().anchoredPosition = localPosition;

        pointerEventData.position = nullObject.transform.position;

        if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
        {
            Destroy(nullObject);
        }

        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerEventData, results);

        lS.photonView.RPC("AddLog", RpcTarget.All, $"results.count {results.Count}");

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
