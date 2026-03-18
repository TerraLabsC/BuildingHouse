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
    }

    public void UnregisterSpawnedObject(SectionType section)
    {
        spawnedObjects[(int)section] = null;
    }

    public GameObject GetSpawnedObject(SectionType section)
    {
        return spawnedObjects[(int)section];
    }
}