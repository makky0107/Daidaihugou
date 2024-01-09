using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCIconController : MonoBehaviour
{
    public int iconID;

    SkillGroup parent;
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
        parent = GetComponentInParent<SkillGroup>();
    }

    public void SelectOntap()
    {
        foreach (var icon in parent.skills)
        {
            icon.GetComponent<SCIconController>().SelectCanselIcon();
        }
        SelectIcon();
    }

    public void SelectIcon()
    {
        view.SetActiveSelectPanel(true);
    }
    
    public void SelectCanselIcon()
    {
        view.SetActiveSelectPanel(false);
    }
}
