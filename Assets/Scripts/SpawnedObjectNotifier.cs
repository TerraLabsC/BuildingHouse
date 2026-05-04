using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;

public class SpawnedObjectNotifier : MonoBehaviour
{
    public SectionType section;
    private Button linkedButton;
    private LeanSelectableByFinger selectable;

    private void Start()
    {
        selectable = GetComponent<LeanSelectableByFinger>();
        if (selectable != null)
        {
            selectable.OnSelected.AddListener(_ => HandleTouch());
        }
        else
        {
            Debug.LogWarning("LeanSelectableByFinger не найден на объекте!");
        }
    }

    public void Initialize(SectionType type, Button buttonRef)
    {
        section = type;
        linkedButton = buttonRef;

        // Устанавливаем тип секции в ColorObjects
        var colorObj = GetComponent<ColorObjects>();
        if (colorObj != null)
            colorObj.sectionType = type;
    }

    public void HandleTouch()
    {
        // 1. Устанавливаем этот объект как выделенный
        if (InstanceObjects.Instance != null)
            InstanceObjects.Instance.SelectedObject = gameObject;

        // 2. Переключаем текущую секцию в SelectionManager
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.SelectSection(section);

        // 3. Вызываем событие кнопки (например, для анимации UI)
        if (linkedButton != null)
            linkedButton.onClick.Invoke();

        Debug.Log($"Выбран объект {gameObject.name}, секция {section}");
    }

    private void OnDestroy()
    {
        if (InstanceObjects.Instance != null)
            InstanceObjects.Instance.UnregisterSpawnedObject(section, gameObject);
    }
}