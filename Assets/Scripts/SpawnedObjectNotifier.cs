using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;

public class SpawnedObjectNotifier : MonoBehaviour
{
    public SectionType section;
    private Button linkedButton;
    private LeanSelectableByFinger selectable;

    // Префаб, из которого был создан этот объект
    [HideInInspector] public GameObject sourcePrefab;

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

    /// <summary>
    /// Инициализация с указанием префаба-источника.
    /// </summary>
    public void Initialize(SectionType type, Button buttonRef, GameObject prefab)
    {
        section = type;
        linkedButton = buttonRef;
        sourcePrefab = prefab;

        var colorObj = GetComponent<ColorObjects>();
        if (colorObj != null)
            colorObj.sectionType = type;
    }

    public void HandleTouch()
    {
        if (InstanceObjects.Instance != null)
            InstanceObjects.Instance.SelectedObject = gameObject;

        if (SelectionManager.Instance != null)
            SelectionManager.Instance.SelectSection(section);

        if (linkedButton != null)
            linkedButton.onClick.Invoke();

        Debug.Log($"Выбран объект {gameObject.name}, секция {section}");
    }

    private void OnDestroy()
    {
        // Убираем из реестра InstanceObjects
        if (InstanceObjects.Instance != null)
            InstanceObjects.Instance.UnregisterSpawnedObject(section, gameObject);

        // Убираем из трекера префабов
        if (PrefabInstanceTracker.Instance != null && sourcePrefab != null)
            PrefabInstanceTracker.Instance.Unregister(sourcePrefab);
    }
}