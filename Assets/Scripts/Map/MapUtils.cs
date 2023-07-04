using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapUtils
{
    private static MapConfig mapData;

    public static MapConfig MapData
    {
        get
        {
            if (mapData == null)
            {
                mapData = Resources.Load<MapConfig>("Configs/MapConfig");
            }

            return mapData;
        }
        
    }

    public static int MapVerticalSize => MapData.verticalSize;

    public static int MapHorizontalSize => MapData.horizontalSize;

    public static int ColumnSize => Mathf.CeilToInt((MapHorizontalSize + 2f) / 2f);
    
    public static bool IsMapEvenWidth => MapHorizontalSize % 2 == 0;
    
    

    public static int CalculateIndex(int index, int distance)
    {
        if (distance <= 0)
        {
            if (IsMapEvenWidth && index + distance < 0)
            {
                return Mathf.Abs(index + distance + 1);
            }
            return Mathf.Abs(index + distance);
        }

        if (index + distance < ColumnSize)
        {
            return index + distance;
        }

        return ColumnSize - 1;
    }

    public static int MapStatus(int[] map, int index, int distance = 0)
    {
        return map[CalculateIndex(index, distance)];
    }

}
