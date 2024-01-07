using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCIconController : MonoBehaviour
{
    public int iconID;

    SCIconView view;
    public SCIconModel model;

    public GameObject shadow;


    private void Awake()
    {
        view = GetComponent<SCIconView>();
    }

    private void Start()
    {
        model = new SCIconModel(iconID);
        view.Show(model);
        view.SetActiveSelectPanel(false);
    }
}
