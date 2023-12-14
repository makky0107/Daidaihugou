using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OwnHandController : MonoBehaviour
{
    public GameObject skill;
    public GameObject pass;

    public GameObject first;
    public GameObject second;
    public GameObject third;
    public GameObject fourth;

    public List<GameObject> rank;

    private void Awake()
    {
        pass.SetActive(true);
        skill.SetActive(true);

        rank = new List<GameObject> { first, second, third, fourth};

        foreach (var image in rank)
        {
            image.SetActive(false);
        }
    }

}
