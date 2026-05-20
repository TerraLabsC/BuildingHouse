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

    [Header("Цвета для пяти кнопок")]
    [SerializeField] private SectionTypeColor[] colorsFive;
    [Header("Цвета для шести кнопок")]
    [SerializeField] private SectionTypeColor[] colorsSix;

    public SectionType section;

    public bool isActive = false;

    public GameObject FingerLessonMain;

    public GameObject FingerLessonObjectMain;

    private void Start()
    {
        if (isActive)
        {
            DestroyChild();
            SpawnerButtons();
            ColorButtonSix();
        }
    }

    public void SpawnerButtons()
    {
        SelectionManager.Instance.SelectSection(section);

        DestroyChild();

        foreach (GameObject obj in spawnerObjects)
        {
           var buttons = Instantiate(obj, transform.position, Quaternion.identity, content.transform);
           buttons.GetComponentInChildren<SpawnObjects>().@event = gameObject.GetComponent<Button>();
           buttons.GetComponentInChildren<SpawnObjects>().FingerLesson = FingerLessonMain;
           buttons.GetComponentInChildren<SpawnObjects>().FingerLessonObject = FingerLessonObjectMain;
        }

        ForceUpdateCanvas();
    }

    private void ForceUpdateCanvas()
    {
        //LayoutRebuilder.ForceRebuildLayoutImmediate(filterContent.GetComponent<RectTransform>());
    }

    public void ColorButtonFive()
    {
        for (int i = 0; i < spawnerObjectsColorButtonsFive.Count && i < colorsFive.Length; i++)
        {
            GameObject newBtn = Instantiate(spawnerObjectsColorButtonsFive[i], contentColorButton.transform);
            ColorButton cb = newBtn.GetComponent<ColorButton>();
            if (cb != null) cb.color = colorsFive[i];
        }
    }

    public void ColorButtonSix()
    {
        for (int i = 0; i < spawnerObjectsColorButtonsSix.Count && i < colorsSix.Length; i++)
        {
            GameObject newBtn = Instantiate(spawnerObjectsColorButtonsSix[i], contentColorButton.transform);
            ColorButton cb = newBtn.GetComponent<ColorButton>();
            if (cb != null) cb.color = colorsSix[i];
        }
    }

    public void DestroyChild()
    {
        for (int i = content.transform.childCount - 1; i >= 0; i--)
            Destroy(content.transform.GetChild(i).gameObject);

        for (int i = contentColorButton.transform.childCount - 1; i >= 0; i--)
            Destroy(contentColorButton.transform.GetChild(i).gameObject);
    }
}
