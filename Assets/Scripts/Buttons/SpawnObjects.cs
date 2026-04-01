using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnObjects : MonoBehaviour
{
    [SerializeField] private SectionType section;
    [SerializeField] private GameObject prefabObject;
    [SerializeField] private int buttonId;          // уникальный id кнопки (задаётся в инспекторе)

    private Transform spawnTransform;
    private GameObject spawnedInstance;
    private Button button;

    public Image BackGroundImage;

    public float RandomMin = 0f;

    private void Awake() => button = GetComponent<Button>();

    private void Start() => spawnTransform = InstanceObjects.Instance.TransformObject;

    private void OnEnable()
    {
        if (InstanceObjects.Instance != null)
        {
            InstanceObjects.Instance.OnSpawnStateChanged += OnSpawnStateChanged;
            UpdateState(); // сразу обновляем состояние при включении
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
            UpdateState();
    }

    // Можно оставить Update для надёжности (если событие вдруг не сработает)
    private void Update() => UpdateState();

    private void UpdateState()
    {
        if (button == null || InstanceObjects.Instance == null) return;

        // Активность кнопки (разрешён ли спавн)
        button.enabled = InstanceObjects.Instance.CanSpawn(section);

        // Управление фоном
        if (BackGroundImage != null)
        {
            GameObject obj = InstanceObjects.Instance.GetSpawnedObject(section);
            int spawnedButtonId = InstanceObjects.Instance.GetButtonId(section);
            // Включаем фон только если объект существует и его id совпадает с id этой кнопки
            BackGroundImage.enabled = (obj != null && spawnedButtonId == buttonId);
        }
    }

    public void SpawnerObject()
    {
        SetAllChildImagesState();

        // Выбираем текущий раздел
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.SelectSection(section);

        // Если объект уже существует – выходим
        if (!InstanceObjects.Instance.CanSpawn(section))
            return;

        // Спавним объект
        spawnedInstance = Instantiate(prefabObject, spawnTransform.position, spawnTransform.rotation);
        spawnedInstance.transform.parent = spawnTransform;

        Vector3 localPos = spawnedInstance.transform.localPosition;
        localPos.z = Random.Range(RandomMin, RandomMin);
        spawnedInstance.transform.localPosition = localPos;

        // Регистрируем объект и передаём id кнопки
        InstanceObjects.Instance.RegisterSpawnedObject(section, spawnedInstance, buttonId);
        InstanceObjects.Instance.MarkSpawned(section, true);

        // Добавляем уведомитель об удалении
        var notifier = spawnedInstance.AddComponent<SpawnedObjectNotifier>();
        notifier.Initialize(section);
    }

    public void SetAllChildImagesState()
    {
        DisableChildImages disableChildImages = GetComponentInParent<DisableChildImages>();
        disableChildImages.DisableAllImages();

        GameObject parentObject = transform.parent.gameObject.transform.parent.gameObject;
        Image parentImage = parentObject.GetComponent<Image>();
        parentImage.enabled = true;
    }
}