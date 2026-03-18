using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonsActivateBuilder : MonoBehaviour
{
    [SerializeField] private List<GameObject> spawnerObjects;

    [SerializeField] private GameObject content;

    [SerializeField] private ContentSizeFitter filterContent;

    [Header("Спавним кнопки цвета")]
    [SerializeField] private List<GameObject> spawnerObjectsColorButtonsFive;
    [SerializeField] private List<GameObject> spawnerObjectsColorButtonsSix;
    [SerializeField] private GameObject contentColorButton;

    public void SpawnerButtons()
    {
        DestroyChild();

        foreach (GameObject obj in spawnerObjects)
        {
            Instantiate(obj, transform.position, Quaternion.identity, content.transform);
        }

        ForceUpdateCanvas();
    }

    private void ForceUpdateCanvas()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(filterContent.GetComponent<RectTransform>());
    }

    public void DestroyChild()
    {
        for (int i = content.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(content.transform.GetChild(i).gameObject);
        }

        for (int i = contentColorButton.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(contentColorButton.transform.GetChild(i).gameObject);
        }
    }

    public void ColorButtonFive()
    {
        foreach (GameObject obj in spawnerObjectsColorButtonsFive)
        {
            Instantiate(obj, transform.position, Quaternion.identity, contentColorButton.transform);
        }
    }

    public void ColorButtonSix()
    {
        foreach (GameObject obj in spawnerObjectsColorButtonsSix)
        {
            Instantiate(obj, transform.position, Quaternion.identity, contentColorButton.transform);
        }
    }
}
