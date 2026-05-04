using UnityEngine;
using TMPro;

public class ColorAvailabilityText : MonoBehaviour
{
    [Header("Цвета текста")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = Color.gray;

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
            InstanceObjects.Instance.OnSpawnStateChanged += OnSpawnStateChanged;
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.OnSectionChanged += OnSectionChanged;

        UpdateTextColor();
    }

    private void OnDestroy()
    {
        if (InstanceObjects.Instance != null)
            InstanceObjects.Instance.OnSpawnStateChanged -= OnSpawnStateChanged;
        if (SelectionManager.Instance != null)
            SelectionManager.Instance.OnSectionChanged -= OnSectionChanged;
    }

    private void OnSpawnStateChanged(SectionType section, bool spawned) => UpdateTextColor();
    private void OnSectionChanged(SectionType newSection) => UpdateTextColor();

    private void UpdateTextColor()
    {
        bool isObjectExists = false;

        if (InstanceObjects.Instance != null && SelectionManager.Instance != null)
        {
            int count = InstanceObjects.Instance.GetCount(SelectionManager.Instance.CurrentSection);
            isObjectExists = count > 0;
        }

        if (textMesh != null)
            textMesh.color = isObjectExists ? activeColor : inactiveColor;
    }
}