using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorButton : MonoBehaviour
{
    public SectionTypeColor color;
    private Button button;

    public Image BackGroundImage;
    public Image RedLine;

    public Color backGroundColor;
    public Color _color;

    private void Awake() => button = GetComponent<Button>();

    private void OnEnable()
    {
        if (InstanceObjects.Instance != null)
        {
            InstanceObjects.Instance.OnSpawnStateChanged += OnSpawnStateChanged;
            InstanceObjects.Instance.OnObjectColorChanged += OnObjectColorChanged;
        }
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.OnSectionChanged += OnSectionChanged;
        UpdateState();
    }

    private void OnDisable()
    {
        if (InstanceObjects.Instance != null)
        {
            InstanceObjects.Instance.OnSpawnStateChanged -= OnSpawnStateChanged;
            InstanceObjects.Instance.OnObjectColorChanged -= OnObjectColorChanged;
        }
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.OnSectionChanged -= OnSectionChanged;
    }

    private void OnSpawnStateChanged(SectionType changedSection, bool spawned) => UpdateState();
    private void OnSectionChanged(SectionType newSection) => UpdateState();

    private void OnObjectColorChanged(SectionType changedSection, SectionTypeColor newColor)
    {
        if (changedSection == SelectionManager.Instance?.CurrentSection)
            UpdateState();
    }

    private void Update()
    {
        UpdateState();
    }

    private void UpdateState()
    {
        if (button == null || SelectionManager.Instance == null || InstanceObjects.Instance == null)
        {
            if (button != null) button.interactable = false;
            if (BackGroundImage != null) BackGroundImage.enabled = false;
            GetComponent<Image>().color = backGroundColor;

            if (RedLine != null)
            {
                RedLine.color = Color.darkRed;
            }

            return;
        }

        SectionType current = SelectionManager.Instance.CurrentSection;
        GameObject obj = InstanceObjects.Instance.GetSpawnedObject(current);
        bool objExists = obj != null;

        // Интерактивность кнопки: только если объект существует
        button.interactable = objExists;

        // Фон: включается, если объект существует и его цвет совпадает с цветом кнопки
        if (BackGroundImage != null)
        {
            if (objExists)
            {
                var colorObj = obj.GetComponent<ColorObjects>();
                if (colorObj != null && colorObj.sectionColorSprite == color)
                {
                    BackGroundImage.enabled = true;
                    GetComponent<Image>().color = _color;

                    if (RedLine != null)
                    {
                        RedLine.color = Color.red;
                    }
                }
                else
                {
                    BackGroundImage.enabled = false;
                    GetComponent<Image>().color = backGroundColor;
                }
                   
            }
            else
            {
                BackGroundImage.enabled = false;
                GetComponent<Image>().color = backGroundColor;
            }
        }
    }

    public void ApplyColor()
    {
        if (SelectionManager.Instance == null || InstanceObjects.Instance == null) return;

        SectionType current = SelectionManager.Instance.CurrentSection;
        GameObject obj = InstanceObjects.Instance.GetSpawnedObject(current);

        if (obj == null) return;

        var colorObj = obj.GetComponent<ColorObjects>();
        if (colorObj != null) colorObj.SetColor(color);
    }
}