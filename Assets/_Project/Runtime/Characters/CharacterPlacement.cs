using System;
using UnityEngine;

[Serializable]
public struct CharacterPlacement
{
    public CharacterDefinition character;

    [Range(0, 1)]
    public float normalizedX;

    [Range(0, 1)]
    public float normalizedY;

    public float scale;
    public int sortingOrder;

    public CharacterPose pose;
    public CharacterExpression expression;

    public bool clickable;
}
