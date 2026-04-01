using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum SectionTypeColor
{
    Perpl,
    Blue,
    Orange,
    Green,
    Red,
    White
}

public class ColorObjects : MonoBehaviour
{
    public Sprite SpriteObjectPirpl;
    public Sprite SpriteObjectBlue;
    public Sprite SpriteObjectOrange;
    public Sprite SpriteObjectGreen;
    public Sprite SpriteObjectRed;
    public Sprite SpriteObjectWhite;

    [SerializeField] private SectionType section;
    public SectionTypeColor sectionColorSprite;
    public SectionType sectionType; // добавлено: тип секции, к которой принадлежит объект

    private SpriteRenderer spriteRenderer;

    public event Action<SectionType, SectionTypeColor> OnColorChanged;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetColor(SectionTypeColor color)
    {
        sectionColorSprite = color;
        if (spriteRenderer == null) return;

        switch (color)
        {
            case SectionTypeColor.Perpl: spriteRenderer.sprite = SpriteObjectPirpl; break;
            case SectionTypeColor.Blue: spriteRenderer.sprite = SpriteObjectBlue; break;
            case SectionTypeColor.Orange: spriteRenderer.sprite = SpriteObjectOrange; break;
            case SectionTypeColor.Green: spriteRenderer.sprite = SpriteObjectGreen; break;
            case SectionTypeColor.Red: spriteRenderer.sprite = SpriteObjectRed; break;
            case SectionTypeColor.White: spriteRenderer.sprite = SpriteObjectWhite; break;
        }

        // Уведомляем об изменении цвета
        OnColorChanged?.Invoke(sectionType, color);
        if (InstanceObjects.Instance != null)
            InstanceObjects.Instance.NotifyColorChanged(sectionType, color);
    }
}