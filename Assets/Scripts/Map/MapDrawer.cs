using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDrawer : MonoBehaviour
{
    private GameObject cube;
    private GameObject dot;

    private void Awake()
    {
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.parent = transform;
        var newMaterial = new Material(Shader.Find("Standard"));
        newMaterial.color = Color.blue;
        cube.GetComponent<MeshRenderer>().material = newMaterial;
        cube.SetActive(false);
        
        dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dot.transform.parent = transform;
        newMaterial = new Material(Shader.Find("Standard"));
        newMaterial.color = Color.white;
        dot.GetComponent<MeshRenderer>().material = newMaterial;
        dot.transform.localScale = 0.25f * Vector3.one;
        dot.layer = LayerMask.NameToLayer("Dot");
        dot.GetComponent<Collider>().isTrigger = true;
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
                    PlaceDot(i,
                        j,
                        MapUtils.GetRowStatus(row, i, -1) == -1,
                        j > 0 ? MapUtils.GetRowStatus(map[j - 1], i) == -1 : false);
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

    private void PlaceDot(int hIndex, int vIndex, bool hasLeftDot, bool hasTopDot)
    {
        
        var offset = 0f;
        if (MapUtils.IsMapEvenWidth)
        {
            offset = 0.5f;
        }

        Instantiate(dot,
            new Vector3(hIndex * 1 + offset, 0, vIndex  * - 1),
            Quaternion.identity,
            transform).SetActive(true);

        if (hasTopDot)
        {
            Instantiate(dot,
                new Vector3(hIndex * 1 + offset, 0, vIndex  * - 1 + 0.5f),
                Quaternion.identity,
                transform).SetActive(true);
        }

        if (hasLeftDot)
        {
            Instantiate(dot,
                new Vector3(hIndex * 1 + offset - 0.5f, 0, vIndex  * - 1),
                Quaternion.identity,
                transform).SetActive(true);
        }

        if (hIndex == 0 && MapUtils.IsMapEvenWidth == false)
        {
            return;
        }
        
        if (hasTopDot)
        {
            Instantiate(dot,
                new Vector3(hIndex * -1 - offset, 0, vIndex  * - 1 + 0.5f),
                Quaternion.identity,
                transform).SetActive(true);
        }
        
        if (hasLeftDot)
        {
            Instantiate(dot,
                new Vector3(hIndex * -1 - offset + 0.5f, 0, vIndex  * - 1),
                Quaternion.identity,
                transform).SetActive(true);
        }
        
        Instantiate(dot,
            new Vector3(hIndex * -1 - offset, 0, vIndex  * - 1),
            Quaternion.identity,
            transform).SetActive(true);
    }

    private void PlaceVerticalPortal(int hIndex)
    {
        Debug.Log("Vertical Portals");
    }
}
