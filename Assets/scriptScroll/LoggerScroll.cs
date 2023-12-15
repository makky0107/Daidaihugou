using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LoggerScroll : MonoBehaviourPunCallbacks
{
    public Text log;
    public Vector3 delta;

    void Start()
    {
        log.text = "log start";
        delta = this.gameObject.transform.localPosition;
        //delta = new Vector3(0, -146, 0);
        //this.gameObject.transform.localPosition = delta;
    }

    [PunRPC]
    public void AddLog(string logText)
    {
        //Debug.LogWarning($"<size=24><color=blue>AddLog</color></size>");
        //delta.y += 40;
        //this.gameObject.transform.localPosition = delta;
        log.text += "\n" + logText;
    }
}
