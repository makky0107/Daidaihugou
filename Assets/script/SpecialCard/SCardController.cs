using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SCardController : MonoBehaviourPunCallbacks
{
    public SCardView view;
    public SCardModel model;

    private void Awake()
    {
        view = GetComponent<SCardView>();
    }

    public void Init(int sCardNo)
    {
        model = new SCardModel(sCardNo);
        view.Show(model);
    }
}
