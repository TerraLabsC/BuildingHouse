using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DestroyObject : MonoBehaviour
{
    public GameObject Object;

    public GameObject content;

    public Image colorButtonOne;
    public Image colorButtonTwo;

    public Button All;
    public Image colorButtonAllOne;
    public Image colorButtonAllTwo;

    public Color colorHighlihts;
    public Color colorOff;

    private void Update()
    {
        if(Object != null)
        {
            colorButtonOne.color = colorHighlihts;
            colorButtonTwo.color = colorHighlihts;
        }
        else
        {
            colorButtonOne.color = colorOff;
            colorButtonTwo.color = colorOff;
        }

        if (content.transform.childCount > 0)
        {
            All.enabled = true;
            colorButtonAllOne.color = colorHighlihts;
            colorButtonAllTwo.color = colorHighlihts;
        }
        else
        {
            All.enabled = false;
            colorButtonAllOne.color = colorOff;
            colorButtonAllTwo.color = colorOff;
        }
    }

    public void DestroyObj()
    {
        if (Object != null)
        {
            Destroy(Object);
        }
    }

    public void DestroyAllChildren()
    {
        if (content == null)
        {
            return;
        }

        List<GameObject> children = new List<GameObject>();

        foreach (Transform child in content.transform)
        {
            children.Add(child.gameObject);
        }

        foreach (GameObject child in children)
        {
            Destroy(child);
        }
    }
}
