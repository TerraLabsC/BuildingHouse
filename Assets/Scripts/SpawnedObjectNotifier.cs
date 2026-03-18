using UnityEngine;

public class SpawnedObjectNotifier : MonoBehaviour
{
    private SectionType section;

    public void Initialize(SectionType type) => section = type;

    private void OnDestroy()
    {
        if (InstanceObjects.Instance != null)
        {
            InstanceObjects.Instance.UnregisterSpawnedObject(section);
            InstanceObjects.Instance.MarkSpawned(section, false);
        }
    }
}
