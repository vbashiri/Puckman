using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/MapConfig")]
public class MapConfig : ScriptableObject
{
    public int horizontalSize;
    public int verticalSize;
    [Range(0f, 1f)]
    public float placeDotsChance;
}
