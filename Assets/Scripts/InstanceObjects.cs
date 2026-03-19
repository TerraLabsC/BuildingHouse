using System;
using UnityEngine;
using UnityEngine.UIElements;

public enum SectionType
{
    House,
    Roof,
    Windows,
    Doors,
    Trees
}

public class InstanceObjects : MonoBehaviour
{
    public static InstanceObjects Instance;

    private bool[] spawnedFlags = new bool[5];
    private GameObject[] spawnedObjects = new GameObject[5];

    public Transform TransformObject;

    public event Action<SectionType, bool> OnSpawnStateChanged;

    private void Awake() => Instance = this;

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

    public void RegisterSpawnedObject(SectionType section, GameObject obj)
    {
        spawnedObjects[(int)section] = obj;

        switch (section)
        {
            case SectionType.House: House = obj; break;
            case SectionType.Roof: Roof = obj; break;
            case SectionType.Windows: Windows = obj;break;
            case SectionType.Doors: Doors = obj; break;
            case SectionType.Trees: Trees = obj; break;
            default: break;
        }
     }

    public void UnregisterSpawnedObject(SectionType section)
    {
        spawnedObjects[(int)section] = null;

        switch (section)
        {
            case SectionType.House: House = null; break;
            case SectionType.Roof: Roof = null; break;
            case SectionType.Windows: Windows = null; break;
            case SectionType.Doors: Doors = null; break;
            case SectionType.Trees: Trees = null; break;
            default: break;
        }
    }

    public GameObject GetSpawnedObject(SectionType section)
    {
        return spawnedObjects[(int)section];
    }
}