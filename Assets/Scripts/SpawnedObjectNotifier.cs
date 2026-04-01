using UnityEngine;

public class SpawnedObjectNotifier : MonoBehaviour
{
    public SectionType section;

    public void Initialize(SectionType type)
    {
        section = type;
        // Устанавливаем тип секции для компонента цвета, если он есть
        var colorObj = GetComponent<ColorObjects>();
        if (colorObj != null)
            colorObj.sectionType = type;
    }

    private void OnDestroy()
    {
        if (InstanceObjects.Instance != null)
        {
            InstanceObjects.Instance.UnregisterSpawnedObject(section);
            InstanceObjects.Instance.MarkSpawned(section, false);
        }
    }
}