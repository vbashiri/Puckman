using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDrawer : MonoBehaviour
{
    [SerializeField] private GameObject cube;
    private Transform playground;

    public void DrawMap(List<int[]> map)
    {
        if (playground != null)
        {
            Destroy(playground.gameObject);
        }
        playground = Instantiate(new GameObject(), transform).GetComponent<Transform>();

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
            new Vector3(hIndex * 1 + offset, 0, vIndex  * - 1 - 3),
            Quaternion.identity,
            playground);

        if (hIndex == 0 && MapUtils.IsMapEvenWidth == false)
        {
            return;
        }
        
        Instantiate(cube,
            new Vector3(hIndex * -1 - offset, 0, vIndex  * - 1 - 3),
            Quaternion.identity,
            playground);
        
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
