// SectionType.cs
using System;
using UnityEngine;

public enum SectionType
{
    House,
    Roof,
    Windows,
    Doors,
    Trees
}


public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance;
    public SectionType CurrentSection { get; private set; }
    public event Action<SectionType> OnSectionChanged;

    private void Awake() => Instance = this;

    public void SelectSection(SectionType section)
    {
        if (CurrentSection == section) return;
        CurrentSection = section;
        Debug.Log($"Selected section: {section}");
        OnSectionChanged?.Invoke(section);
    }
}