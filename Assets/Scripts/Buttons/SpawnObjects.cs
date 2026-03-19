using UnityEngine;
using UnityEngine.UI;

public class SpawnObjects : MonoBehaviour
{
    [SerializeField] private SectionType section;
    [SerializeField] private GameObject prefabObject;
    private Transform spawnTransform;

    private GameObject spawnedInstance;
    private Button button;

    private void Awake() => button = GetComponent<Button>();

    private void Start() => spawnTransform = InstanceObjects.Instance.TransformObject;

    private void OnEnable()
    {
        if (InstanceObjects.Instance != null)
        {
            InstanceObjects.Instance.OnSpawnStateChanged += OnSpawnStateChanged;

            UpdateInteractable();
        }
    }

    private void OnDisable()
    {
        if (InstanceObjects.Instance != null)
            InstanceObjects.Instance.OnSpawnStateChanged -= OnSpawnStateChanged;
    }

    private void OnSpawnStateChanged(SectionType changedSection, bool spawned)
    {
        if (changedSection == section)
        {
            UpdateInteractable();
        }
    }

    private void Update()
    {
        UpdateInteractable();
    }

    private void UpdateInteractable()
    {
        if (button != null && InstanceObjects.Instance != null)
        {
            button.interactable = InstanceObjects.Instance.CanSpawn(section);
        }
    }

    public void SpawnerObject()
    {
        // Всегда выбираем текущий раздел
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.SelectSection(section);

        // Если объект уже существует — ничего не делаем, просто вышли
        if (!InstanceObjects.Instance.CanSpawn(section))
            return;

        // Спавним новый объект
        spawnedInstance = Instantiate(prefabObject, spawnTransform.position, spawnTransform.rotation);
        spawnedInstance.transform.parent = spawnTransform;

        InstanceObjects.Instance.RegisterSpawnedObject(section, spawnedInstance);
        InstanceObjects.Instance.MarkSpawned(section, true);

        var notifier = spawnedInstance.AddComponent<SpawnedObjectNotifier>();
        notifier.Initialize(section);
    }
}
