using System;
using UnityEngine;
using UnityEngine.UIElements;

public class InstanceObjects : MonoBehaviour
{
    public static InstanceObjects Instance;

    private bool[] spawnedFlags = new bool[5];          // флаги: заспавнен ли объект
    private GameObject[] spawnedObjects = new GameObject[5]; // ссылки на объекты
    private int[] spawnedButtonIds = new int[5];        // id кнопок, создавших объекты

    public Transform TransformObject;

    public event Action<SectionType, bool> OnSpawnStateChanged;

    private void Awake()
    {
        Instance = this;
        // Инициализируем id как -1 (означает отсутствие)
        for (int i = 0; i < spawnedButtonIds.Length; i++)
            spawnedButtonIds[i] = -1;
    }

    // Публичные ссылки на объекты (можно оставить для удобства)
    public GameObject House;
    public GameObject Roof;
    public GameObject Windows;
    public GameObject Doors;
    public GameObject Trees;

    public bool CanSpawn(SectionType section) => !spawnedFlags[(int)section];

    public void MarkSpawned(SectionType section, bool spawned)
    {
        if (spawnedFlags[(int)section] == spawned) return;
        spawnedFlags[(int)section] = spawned;
        OnSpawnStateChanged?.Invoke(section, spawned);
    }

    public void RegisterSpawnedObject(SectionType section, GameObject obj, int buttonId)
    {
        spawnedObjects[(int)section] = obj;
        spawnedButtonIds[(int)section] = buttonId;

        switch (section)
        {
            case SectionType.House: House = obj; break;
            case SectionType.Roof: Roof = obj; break;
            case SectionType.Windows: Windows = obj; break;
            case SectionType.Doors: Doors = obj; break;
            case SectionType.Trees: Trees = obj; break;
        }
    }

    public void UnregisterSpawnedObject(SectionType section)
    {
        spawnedObjects[(int)section] = null;
        spawnedButtonIds[(int)section] = -1;

        switch (section)
        {
            case SectionType.House: House = null; break;
            case SectionType.Roof: Roof = null; break;
            case SectionType.Windows: Windows = null; break;
            case SectionType.Doors: Doors = null; break;
            case SectionType.Trees: Trees = null; break;
        }
    }

    public GameObject GetSpawnedObject(SectionType section) => spawnedObjects[(int)section];
    public int GetButtonId(SectionType section) => spawnedButtonIds[(int)section];

    public event Action<SectionType, SectionTypeColor> OnObjectColorChanged;

    public void NotifyColorChanged(SectionType section, SectionTypeColor color)
    {
        OnObjectColorChanged?.Invoke(section, color);
    }
}