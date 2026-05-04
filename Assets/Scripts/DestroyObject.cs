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

    float margin = 400f;

    private void Update()
    {
        if (Object != null)
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

    public void IsObjectInTopRightCorner()
    {
        if (Object == null)
            return;

        // Получаем позицию объекта на экране
        Vector3 screenPoint;

        // Если объект — UI-элемент (RectTransform)
        RectTransform rect = Object.GetComponent<RectTransform>();
        if (rect != null)
        {
            screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, rect.position);
        }
        else
        {
            // Если обычный 3D-объект
            screenPoint = Camera.main.WorldToScreenPoint(Object.transform.position);
        }

        Debug.Log(screenPoint.x >= Screen.width - margin && screenPoint.y >= Screen.height - margin);

        // Проверяем, находится ли точка в правом верхнем углу
        bool isActive = screenPoint.x >= Screen.width - margin && screenPoint.y >= Screen.height - margin;

        if (isActive)
        {
            DestroyObj();
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