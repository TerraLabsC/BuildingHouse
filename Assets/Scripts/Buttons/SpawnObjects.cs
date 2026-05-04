using Lean.Common;
using Lean.Touch;
using UnityEngine;
using UnityEngine.UI;

public class SpawnObjects : MonoBehaviour
{
    [SerializeField] private SectionType section;
    [SerializeField] private GameObject prefabObject;

    private Transform spawnTransform;
    private Button button;

    public Image BackGroundImage;
    public float RandomMin = 0f;

    public Button @event; // кнопка, чей onClick.Invoke() вызывается при выделении объекта

    public GameObject FingerLesson;
    public GameObject FingerLessonObject;

    private void Awake() => button = GetComponent<Button>();

    private void Start()
    {
        spawnTransform = InstanceObjects.Instance.TransformObject;
        if (button != null) button.interactable = true;
    }

    public void SpawnerObject()
    {
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.SelectSection(section);

        SpawnNewObject(spawnTransform.position, spawnTransform.rotation, Vector3.one);
    }

    public void FingerLessonObjectOff()
    {
        if (FingerLesson != null)
        {
            FingerLesson.SetActive(false);
        }
    }

    private void SpawnNewObject(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        DeactivateAll();

        FingerLessonObjectOff();

        GameObject spawnedInstance = Instantiate(prefabObject, position, rotation);
        spawnedInstance.transform.parent = InstanceObjects.Instance.TransformObject;
        spawnedInstance.transform.localScale = scale;
        SelectableObject(spawnedInstance);

        var instanceObj = InstanceObjects.Instance;

        // Расчёт Z (оставлен без изменений)
        float zOffset = instanceObj.GetNextZOffset(section);
        float baseZ = GetBaseZForSection(section);
        float finalZ = baseZ - zOffset;

        Vector3 localPos = spawnedInstance.transform.localPosition;
        localPos.z = finalZ;
        spawnedInstance.transform.localPosition = localPos;

        if (FingerLessonObject != null && instanceObj.isActiveFinger)
        {
            GameObject finger = Instantiate(FingerLessonObject, position, FingerLessonObject.transform.rotation);
            finger.transform.parent = spawnedInstance.transform;
            instanceObj.isActiveFinger = false;
            finger.transform.position = new Vector3(finger.transform.position.x + 2f, 0, spawnedInstance.transform.position.z - 0.01f);
            spawnedInstance.GetComponent<DestroyObj>().FingerLesson = finger;
        }

        // Назначение sorting order по диапазонам
        var spriteRenderer = spawnedInstance.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            int baseOrder = GetBaseSortingOrder(section);
            int index = instanceObj.GetNextSortingIndex(section);
            spriteRenderer.sortingOrder = baseOrder + index;
        }

        instanceObj.RegisterSpawnedObject(section, spawnedInstance);

        var notifier = spawnedInstance.AddComponent<SpawnedObjectNotifier>();
        notifier.Initialize(section, @event);

        instanceObj.SelectedObject = spawnedInstance;
    }

    private float GetBaseZForSection(SectionType section)
    {
        switch (section)
        {
            case SectionType.House: return -6f;
            case SectionType.Roof: return -7f;
            case SectionType.Windows: return -8f;
            case SectionType.Doors: return -9f;
            case SectionType.Trees: return -10f;
            default: return 0f;
        }
    }

    private int GetBaseSortingOrder(SectionType section)
    {
        switch (section)
        {
            case SectionType.House: return 0;
            case SectionType.Roof: return 101;
            case SectionType.Windows: return 201;
            case SectionType.Doors: return 301;
            case SectionType.Trees: return 401;
            default: return 0;
        }
    }

    public void DeactivateAll()
    {
        DragObject3D[] allDragObjects = FindObjectsOfType<DragObject3D>();
        foreach (DragObject3D dragObj in allDragObjects)
        {
            // Сначала снимаем выделение, если объект выделен
            var selectable = dragObj.GetComponent<LeanSelectableByFinger>();
            if (selectable != null && selectable.IsSelected)
            {
                selectable.Deselect();
            }

            // Затем отключаем DragObject3D
            dragObj.IsActive = false;
        }
    }

    public void SelectableObject(GameObject targetDragObject)
    {
        var selectable = targetDragObject.GetComponent<LeanSelectableByFinger>();
        if (selectable == null)
            return;

        // Находим селектор (он должен быть на каком-то объекте в сцене)
        var selector = FindObjectOfType<LeanSelectByFinger>();
        if (selector == null)
        {
            Debug.LogError("Нет активного LeanSelectByFinger в сцене!");
            return;
        }

        // Создаём фейковый палец, если нет реальных касаний
        LeanFinger finger = LeanTouch.Fingers.Count > 0
                            ? LeanTouch.Fingers[0]
                            : new LeanFinger();

        // В зависимости от версии метод может называться по-разному.
        // Пробуйте этот вариант (наиболее распространён):
        selector.Select(selectable, finger);
    }
}