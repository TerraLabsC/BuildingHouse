using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Отслеживает количество живых экземпляров для каждого префаба.
/// </summary>
public class PrefabInstanceTracker : MonoBehaviour
{
    public static PrefabInstanceTracker Instance { get; private set; }

    // Словарь: префаб → количество живых экземпляров
    private Dictionary<GameObject, int> prefabCounts = new Dictionary<GameObject, int>();

    // Событие: изменился счётчик конкретного префаба (префаб, новое количество)
    [HideInInspector]
    public UnityEvent<GameObject, int> OnCountChanged = new UnityEvent<GameObject, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);   // если нужно сохранять трекер между сценами
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Вызывать при создании нового экземпляра префаба.
    /// </summary>
    public void Register(GameObject prefab)
    {
        if (prefab == null) return;

        if (!prefabCounts.ContainsKey(prefab))
            prefabCounts[prefab] = 0;

        prefabCounts[prefab]++;
        OnCountChanged.Invoke(prefab, prefabCounts[prefab]);
    }

    /// <summary>
    /// Вызывать при уничтожении экземпляра префаба.
    /// </summary>
    public void Unregister(GameObject prefab)
    {
        if (prefab == null) return;
        if (!prefabCounts.ContainsKey(prefab)) return;

        prefabCounts[prefab]--;
        if (prefabCounts[prefab] < 0) prefabCounts[prefab] = 0;

        OnCountChanged.Invoke(prefab, prefabCounts[prefab]);
    }

    /// <summary>
    /// Получить текущее количество живых экземпляров префаба.
    /// </summary>
    public int GetCount(GameObject prefab)
    {
        if (prefab == null) return 0;
        prefabCounts.TryGetValue(prefab, out int count);
        return count;
    }
}