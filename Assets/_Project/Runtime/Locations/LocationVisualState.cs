using System;
using UnityEngine;

[Serializable]
public struct LocationVisualState
{
    public string stateId;

    public Sprite background;
    public Sprite[] midgroundProps;
    public Sprite[] foregroundProps;

    public GameObject[] optionalEffects;
}
