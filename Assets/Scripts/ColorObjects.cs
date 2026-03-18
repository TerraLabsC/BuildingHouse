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
    [SerializeField] private SectionTypeColor sectionColorSprite;
}
