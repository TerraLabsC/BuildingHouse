using UnityEngine;
using UnityEngine.UI;

public class ColorButton : MonoBehaviour
{
    public SectionTypeColor color;
    private Button button;

    public bool IsActiveColor { get; private set; }

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (InstanceObjects.Instance != null)
        {
            InstanceObjects.Instance.OnObjectColorChanged += OnObjectColorChanged;
            InstanceObjects.Instance.OnSelectedObjectChanged += OnSelectedObjectChanged;
        }
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.OnSectionChanged += OnSectionChanged;

        UpdateState();
    }

    private void OnDisable()
    {
        if (InstanceObjects.Instance != null)
        {
            InstanceObjects.Instance.OnObjectColorChanged -= OnObjectColorChanged;
            InstanceObjects.Instance.OnSelectedObjectChanged -= OnSelectedObjectChanged;
        }
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.OnSectionChanged -= OnSectionChanged;
    }

    private void OnObjectColorChanged(SectionType changedSection, SectionTypeColor newColor)
    {
        UpdateState();
    }

    private void OnSectionChanged(SectionType newSection)
    {
        UpdateState();
    }

    private void OnSelectedObjectChanged(GameObject newSelected)
    {
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
            if (button != null)
                button.interactable = false;

            IsActiveColor = false;
            return;
        }

        GameObject targetObject = GetTargetObject();
        bool objExists = targetObject != null;

        button.interactable = objExists;

        if (objExists)
        {
            var colorObj = targetObject.GetComponent<ColorObjects>();
            IsActiveColor = (colorObj != null && colorObj.sectionColorSprite == color);
        }
        else
        {
            IsActiveColor = false;
        }
    }

    private GameObject GetTargetObject()
    {
        var selected = InstanceObjects.Instance.SelectedObject;
        if (selected == null)
            return null;

        var notifier = selected.GetComponent<SpawnedObjectNotifier>();
        if (notifier != null && notifier.section == SelectionManager.Instance.CurrentSection)
            return selected;

        return null;
    }

    public void ApplyColor()
    {
        GameObject obj = GetTargetObject();
        if (obj == null)
        {
            Debug.LogWarning("[ColorButton] Нет подходящего выделенного объекта для смены цвета");
            return;
        }

        var colorObj = obj.GetComponent<ColorObjects>();
        if (colorObj != null)
            colorObj.SetColor(color);
        else
            Debug.LogWarning($"[ColorButton] На объекте {obj.name} нет компонента ColorObjects");
    }
}