using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillGroup : MonoBehaviour
{
    public List<GameObject> skills = new List<GameObject>();

    void Start()
    {
        foreach (Transform child in transform)
        {
            skills.Add(child.gameObject);
        }

        skills[0].GetComponent<SCIconController>().SelectIcon();
    }
}
