using System;
using System.Collections.Generic;
using UnityEngine;

public class InstanceObjects : MonoBehaviour
{
    public static InstanceObjects Instance;

    private GameObject selectedObject;

    public bool isActiveFinger = true;

    public GameObject SelectedObject
    {
        get => selectedObject;
        set
        {
            if (selectedObject == value) return;
            selectedObject = value;
            OnSelectedObjectChanged?.Invoke(selectedObject);
        }
    }

    // Списки заспавненных объектов по секциям
    private List<GameObject>[] spawnedObjectsLists = new List<GameObject>[5];

    public Transform TransformObject;

    public event Action<SectionType, bool> OnSpawnStateChanged;
    public event Action<SectionType, SectionTypeColor> OnObjectColorChanged;
    public event Action<GameObject> OnSelectedObjectChanged;

    // Управление Z-слоями
    private Dictionary<SectionType, float> sectionZOffsets = new Dictionary<SectionType, float>();
    private int globalSortingOrder = 0;

    // *** НОВОЕ: счётчики сортировки для каждой секции ***
    private Dictionary<SectionType, int> sortingCounters = new Dictionary<SectionType, int>();

    private void Awake()
    {
        Instance = this;
        for (int i = 0; i < spawnedObjectsLists.Length; i++)
            spawnedObjectsLists[i] = new List<GameObject>();
    }

    /// <summary>Текущее количество объектов в секции</summary>
    public int GetCount(SectionType section) => spawnedObjectsLists[(int)section].Count;

    /// <summary>Регистрирует новый объект</summary>
    public void RegisterSpawnedObject(SectionType section, GameObject obj)
    {
        spawnedObjectsLists[(int)section].Add(obj);
        MarkSpawned(section, true);
    }

    /// <summary>Удаляет объект из списков</summary>
    public void UnregisterSpawnedObject(SectionType section, GameObject obj)
    {
        spawnedObjectsLists[(int)section].Remove(obj);
        MarkSpawned(section, spawnedObjectsLists[(int)section].Count > 0);
    }

    /// <summary>Возвращает первый объект секции (для обратной совместимости)</summary>
    public GameObject GetFirstObject(SectionType section)
    {
        var list = spawnedObjectsLists[(int)section];
        return list.Count > 0 ? list[0] : null;
    }

    /// <summary>Возвращает список объектов секции (только для чтения)</summary>
    public IReadOnlyList<GameObject> GetObjects(SectionType section) => spawnedObjectsLists[(int)section];

    /// <summary>Уничтожает указанный объект и убирает из учёта</summary>
    public void DestroyObject(GameObject obj, SectionType section)
    {
        if (obj == null) return;
        UnregisterSpawnedObject(section, obj);
        Destroy(obj);
    }

    public void MarkSpawned(SectionType section, bool spawned) =>
        OnSpawnStateChanged?.Invoke(section, spawned);

    public void NotifyColorChanged(SectionType section, SectionTypeColor color) =>
        OnObjectColorChanged?.Invoke(section, color);

    // ---------- Z-слои ----------
    public float GetNextZOffset(SectionType section)
    {
        if (!sectionZOffsets.ContainsKey(section))
            sectionZOffsets[section] = 0f;

        float currentOffset = sectionZOffsets[section];
        float nextOffset = currentOffset + 0.01f;
        if (nextOffset >= 1.0f)
            nextOffset = 0f;

        sectionZOffsets[section] = nextOffset;
        return currentOffset;
    }

    // ---------- Глобальный порядок сортировки (оставлен для обратной совместимости) ----------
    public int GetNextSortingOrder() => globalSortingOrder++;

    // *** НОВЫЙ МЕТОД: порядковый номер объекта внутри секции ***
    public int GetNextSortingIndex(SectionType section)
    {
        if (!sortingCounters.ContainsKey(section))
            sortingCounters[section] = 0;
        return sortingCounters[section]++;
    }

    public void ResetAllOffsets()
    {
        sectionZOffsets.Clear();
        globalSortingOrder = 0;
        sortingCounters.Clear();   // сбрасываем и новый счётчик
    }
}