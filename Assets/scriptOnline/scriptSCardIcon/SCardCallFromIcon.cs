using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SCardCallFromIcon : MonoBehaviour
{
    public Image atherPanel;
    public Image usePanel;
    public SCardController sCard;
    public SCIconController SCIconCont;

    public void SkillActivation()
    {
        CallShadow();
        HideCountText();
        CallSCard(SCIconCont.iconID);
        CallOkPanel();
        ShadowSetInfo();

        atherPanel.gameObject.transform.localPosition = Vector3.zero;
        usePanel.gameObject.transform.localPosition = new Vector3(0, -1000f / 2540f * (float)Screen.height, transform.localPosition.z);
        sCard.gameObject.transform.localPosition = new Vector3(0, 300f / 2540f * (float)Screen.height, transform.localPosition.z);

        if ((float)Screen.height < 1001f)
        {
            usePanel.gameObject.transform.localPosition -= Vector3.up * 130;
            sCard.gameObject.transform.localPosition += Vector3.up * 70;
        }
    }

    public void SkillSelct()
    {
        PlayStart.instance.sCardNo = SCIconCont.iconID;
        //Debug.Log($"PlayStart.instance.sCardNo {PlayStart.instance.sCardNo}");
        SCIconCont.SelectOntap();
    }

    public void SkillDestroy()
    {
        DestroyShadow();
        DestroyUsePanel();
        DestroySkillCard();
    }

    public void CallShadow()
    {
        //Debug.LogWarning($"<size=18><color=purple>CallShadow</color></size>");

        atherPanel = Instantiate(Resources.Load("Prefab/SkillAtherPanel") as GameObject).GetComponent<Image>();

        atherPanel.gameObject.transform.SetParent(GameObject.Find("Canvas").transform, false);
    }

    public void HideCountText()
    {
        //Debug.LogWarning($"<size=18><color=purple>HideCountText</color></size>");

        atherPanel.GetComponentInChildren<Text>().enabled = false;
    }

    public void CallSCard(int No)
    {
        //Debug.LogWarning($"<size=18><color=purple>CallSCard</color></size>");

        sCard = Instantiate(Resources.Load("Prefab/SpecialCard") as GameObject).GetComponent<SCardController>();
        sCard.Init(No);
        sCard.gameObject.transform.SetParent(GameObject.Find("Canvas").transform, false);
        //sCard.gameObject.SetActive(false);
    }



    void CallOkPanel()
    {
        usePanel = Instantiate(Resources.Load("Prefab/OkPanel") as GameObject).GetComponent<Image>();
        CallSkill use = usePanel.GetComponent<CallSkill>();
        use.sCard = sCard;
        use.atherPanel = atherPanel;
        use.usePanel = usePanel;

        Debug.Log($"usePanel {usePanel.gameObject}");

        Destroy(usePanel.GetComponent<InterfaceAdjustment>());

        usePanel.gameObject.transform.SetParent(atherPanel.transform, false);

        float workW = (float)Screen.width / 1170f;
        float workH = (float)Screen.height / 2540f;

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

    public void ShadowSetInfo()
    {
        //Debug.LogWarning($"<size=18><color=purple>ShadowSetInfo</color></size>");

        CallSkill ather = atherPanel.GetComponent<CallSkill>();
        ather.sCard = sCard;
        ather.atherPanel = atherPanel;
        ather.usePanel = usePanel;
    }

    public void DestroyShadow()
    {
        Destroy(atherPanel.gameObject);
    }

    public void DestroyUsePanel()
    {
        //Debug.LogWarning($"<size=18><color=purple>DestroyUsePanel usePanel{usePanel.gameObject}</color></size>");

        Destroy(usePanel.gameObject);
    }

    public void DestroySkillCard()
    {
        //Debug.LogWarning($"<size=18><color=purple>DestroySkillCard sCard{sCard.gameObject}</color></size>");

        Destroy(sCard.gameObject);
    }
}
