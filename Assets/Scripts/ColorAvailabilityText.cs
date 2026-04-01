using UnityEngine;
using TMPro;

public class ColorAvailabilityText : MonoBehaviour
{
    [Header("Цвета текста")]
    [SerializeField] private Color activeColor = Color.white;   // когда кнопки цветов активны
    [SerializeField] private Color inactiveColor = Color.gray; // когда кнопки неактивны

    private TextMeshProUGUI textMesh;

    private void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        if (textMesh == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on this object.");
            return;
        }

        // Подписываемся на события
        if (InstanceObjects.Instance != null)
        {
            InstanceObjects.Instance.OnSpawnStateChanged += OnSpawnStateChanged;
        }
        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.OnSectionChanged += OnSectionChanged;
        }

        // Устанавливаем начальный цвет
        UpdateTextColor();
    }

    private void OnDestroy()
    {
        // Отписываемся, чтобы избежать утечек
        if (InstanceObjects.Instance != null)
        {
            InstanceObjects.Instance.OnSpawnStateChanged -= OnSpawnStateChanged;
        }
        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.OnSectionChanged -= OnSectionChanged;
        }
    }

    private void OnSpawnStateChanged(SectionType section, bool spawned)
    {
        // При спавне или удалении объекта обновляем цвет
        UpdateTextColor();
    }

    private void OnSectionChanged(SectionType newSection)
    {
        // При смене выбранной секции обновляем цвет
        UpdateTextColor();
    }

    private void UpdateTextColor()
    {
        // Проверяем, существует ли объект текущей секции
        bool isObjectExists = (InstanceObjects.Instance != null &&
                               SelectionManager.Instance != null &&
                               InstanceObjects.Instance.GetSpawnedObject(SelectionManager.Instance.CurrentSection) != null);

        textMesh.color = isObjectExists ? activeColor : inactiveColor;
    }
}