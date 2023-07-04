using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDrawer : MonoBehaviour
{
    private GameObject cube;

    private void Awake()
    {
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.SetActive(false);
    }

    public void DrawMap(List<int[]> map)
    {

        for (int j = 0; j < map.Count; j++)
        {
            var row = map[j];
            string debugLine = "";
            for (int i = 0; i < row.Length; i++)
            {
                debugLine += row[i] + " ";
                if (row[i] == 1)
                {
                    PlaceBlock(i, j);
                }
                else if (row[i] == -1)
                {
                    PlaceDot(i, j);
                }
                else if (row[i] == -2)
                {
                    if (j == 0)
                    {
                        PlaceVerticalPortal(i);
                    }
                }
                else
                {
                    Debug.LogError("Invalid map cell id: " + row[i] + " at row: " + j + " column: " + i);
                }
            }
            Debug.Log(debugLine);
        }
    }
    
    private void PlaceBlock(int hIndex, int vIndex)
    {
        var offset = 0f;
        if (MapUtils.IsMapEvenWidth)
        {
            offset = 0.5f;
        }
        
        Instantiate(cube,
            new Vector3(hIndex * 1 + offset, 0, vIndex  * - 1),
            Quaternion.identity,
            transform).SetActive(true);

        if (hIndex == 0 && MapUtils.IsMapEvenWidth == false)
        {
            return;
        }
        
        Instantiate(cube,
            new Vector3(hIndex * -1 - offset, 0, vIndex  * - 1),
            Quaternion.identity,
            transform).SetActive(true);
        
    }

    private void PlaceDot(int hIndex, int vIndex)
    {
        
        Debug.Log("v h " + vIndex + " " + hIndex + " ### " + "Place Dot");
    }

    private void PlaceVerticalPortal(int hIndex)
    {
        Debug.Log("Vertical Portals");
    }
}
