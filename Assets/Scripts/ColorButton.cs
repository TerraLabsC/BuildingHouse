using UnityEngine;
using UnityEngine.UI;

public class ColorButton : MonoBehaviour
{
    public SectionTypeColor color;
    private Button button;

    private void Awake() => button = GetComponent<Button>();

    private void OnEnable()
    {
        if (InstanceObjects.Instance != null)
            InstanceObjects.Instance.OnSpawnStateChanged += OnSpawnStateChanged;
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.OnSectionChanged += OnSectionChanged;
        UpdateInteractable();
    }

    private void OnDisable()
    {
        if (InstanceObjects.Instance != null)
            InstanceObjects.Instance.OnSpawnStateChanged -= OnSpawnStateChanged;
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.OnSectionChanged -= OnSectionChanged;
    }

    private void OnSpawnStateChanged(SectionType changedSection, bool spawned) => UpdateInteractable();
    private void OnSectionChanged(SectionType newSection) => UpdateInteractable();

    private void Update()
    {
        UpdateInteractable();
    }

    private void UpdateInteractable()
    {
        // Проверяем все необходимые компоненты
        if (button == null || SelectionManager.Instance == null || InstanceObjects.Instance == null)
        {
            // Если чего-то нет, кнопка неактивна
            if (button != null) button.interactable = false;
            return;
        }

        SectionType current = SelectionManager.Instance.CurrentSection;
        GameObject obj = InstanceObjects.Instance.GetSpawnedObject(current);
        button.interactable = obj != null;
    }

    public void ApplyColor()
    {
        if (SelectionManager.Instance == null || InstanceObjects.Instance == null) return;

        SectionType current = SelectionManager.Instance.CurrentSection;
        GameObject obj = InstanceObjects.Instance.GetSpawnedObject(current);
        Debug.Log($"ApplyColor: currentSection={current}, obj={obj?.name}");

        if (obj == null) return;

        var colorObj = obj.GetComponent<ColorObjects>();
        if (colorObj != null) colorObj.SetColor(color);
    }
}