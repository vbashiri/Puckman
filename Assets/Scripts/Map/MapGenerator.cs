using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject cube;
    [SerializeField] private float randomValue;
    private Transform playground;

    private int[] lastRow;
    private int[] map;
    private int[] rowHint;
    private int[] firstRow;
    private int[] verticalPortals;
    
    private int[] connectivityStatus;
    private int[] lastRowConnectivity;
    private int connectivityId;

    private int[][] presetRow = {
        new int[] {1, 1, 0,  1, 1 },
        new int[] { -2, -2, -2, -2, -1},
        new int[] { -2, 2, 2, -1},
        new int[] { -2, -2, 2, -1},
        new int[] { 2, 2, 2, -1},
        new int[] { -1, -1, -1, -1, -1}
    };
    private int presetRowIndex;

    private int debugv = 0;
    private int debugh = 0;
    void Start()
    {
        button.onClick.AddListener(SetupMap);
        SetupMap();
        
    }

    private void SetupMap()
    {
        if (playground != null)
        {
            Destroy(playground.gameObject);
        }
        InitializeVariables();

        StartCoroutine(GenerateMap());
    }

    private void InitializeVariables()
    {
        playground = Instantiate(new GameObject(), transform).GetComponent<Transform>();
        lastRow = new int[MapUtils.ColumnSize];
        map = new int[MapUtils.ColumnSize];
        rowHint = new int[MapUtils.ColumnSize];
        firstRow = new int[MapUtils.ColumnSize];
        verticalPortals = new int[MapUtils.ColumnSize];
        connectivityStatus = new int[MapUtils.ColumnSize];
        lastRowConnectivity = new int[MapUtils.ColumnSize];
        connectivityId = 0;
        presetRowIndex = Mathf.CeilToInt((MapUtils.MapVerticalSize - presetRow.Length) / 2f); 
        Array.Fill(lastRow, 1);
    }

    private IEnumerator GenerateMap()
    {
        int reachableMapSize = MapUtils.ColumnSize - 1;
        CheckForData(0);
        AddFirstRowDots();
        
        for (int j = 0; j < MapUtils.MapVerticalSize; j ++)
        {
            yield return null;

            DrawVerticalBorder(j);
            CheckForData(j);
            DrawPredeterminedCells(j);

            CheckForMustBlocks();
            

            for (int i = 0; i < reachableMapSize; i++)
            {
                GenerateCell(i);
            }


            CheckConnectivity();

            Array.Clear(rowHint, 0, rowHint.Length);
            CalculateNextRow();

            if (Mathf.Abs(j) == MapUtils.MapVerticalSize - 2)
            {
                EditOneToLastRow(j);
                CalculateLastRow();
            }

            for (int i = 0; i < map.Length; i++)
            {
                if (map[i] == 1)
                {
                    PlaceBlock(i, j);
                }
                else
                {
                    PlaceDot(i, j);
                }
            }
            

            string line = "";
            for (int i = 0; i < map.Length; i++)
            {
                line += (map[i] + 1) + " ";
            }
            Debug.Log(line);
            string nextRowLine = "";
            for (int i = 0; i < rowHint.Length; i++)
            {
                nextRowLine += (rowHint[i]) + " ";
            }
            Debug.Log(nextRowLine);
            
            Array.Copy(map, lastRow, map.Length);
            if (j == 0)
            {
                Array.Copy(map, firstRow, map.Length);
            }

            Array.Copy(connectivityStatus, lastRowConnectivity, connectivityStatus.Length);
            Array.Clear(connectivityStatus, 0, connectivityStatus.Length);
            
            Array.Clear(map, 0, map.Length);
            map[reachableMapSize] = 1;
        }
        DrawHorizontalBorder();
    }
    
    private void CheckForData(int row)
    {
        int rowIndex = row - presetRowIndex;
        if (rowIndex < presetRow.Length && rowIndex >= 0)
        {
            for (int i = 0; i < Mathf.Min(presetRow[rowIndex].Length, rowHint.Length) ; i++)
            {
                if (presetRow[rowIndex][i] == 2)
                {
                    rowHint[i] = 1; ;
                }
                else if (presetRow[rowIndex][i] == -2)
                {
                    rowHint[i] = -1;
                }
                else if (rowHint[i] == 0)
                {
                    rowHint[i] = presetRow[rowIndex][i];
                }
            }
        }
    }
    
    private void AddFirstRowDots()
    {
        int firstRowMin = rowHint.Min();
        if (firstRowMin >= 0)
        {
            rowHint[Random.Range(0, rowHint.Length)] = -1; //Add at least one dot
        }
    }
    
    private void DrawPredeterminedCells(int row)
    {
        for (int i = 0; i < MapUtils.ColumnSize - 1; i++)
        {
            debugh = i;
            if (rowHint[i] == -1)
            {
                Debug.Log("Draw Predetermind   Dot " + row  + " " + i);
                map[i] = -1;
            }

            if (rowHint[i] == 1)
            {
                Debug.Log("Draw Predetermind Block " + row  + " " + i);
                map[i] = 1;
            }
        }
    }

    private void CheckForMustBlocks()
    {
        for (int i = 0; i < MapUtils.ColumnSize - 1; i++)
        {
            debugh = i;
            if (MapUtils.MapStatus(map, i) == -1 || MapUtils.MapStatus(map, i) == 1)
            {
                continue;
            }
            if (MapRules.MustPlaceBlock(lastRow, map, i))
            {
                map[i] = 1;
            }
            
        }
    }

    private void GenerateCell(int hIndex)
    {
        if (map[hIndex] == -1 || map[hIndex] == 1)
        {
            return;
        }

        if (MapRules.MustPlaceBlock(lastRow, map, hIndex))
        {
            map[hIndex] = 1;
            return;
        }

        if (MapRules.CanPlaceBlock(lastRow, map, hIndex))
        {
            if (Random.value > randomValue)
            {
                map[hIndex] = 1;
                return;
            }
        }
                
        map[hIndex] = -1;
    }
    
    private void CheckConnectivity()
    {
        for (int i = 0; i < map.Length; i++)
        {
            if (map[i] == -1)
            {
                SetConnectivityStatus(i);
            }
        }

        HashSet<int> isolatedCells = new HashSet<int>();
        for (int i = 0; i < lastRowConnectivity.Length; i++)
        {
            if (lastRowConnectivity[i] != 0 &&
                connectivityStatus.Contains(lastRowConnectivity[i]) == false)
            {
                isolatedCells.Add(lastRowConnectivity[i]);
            }   
        }

        if (isolatedCells.Count > 0)
        {
            ConnectCells(isolatedCells);
        }
        
    }

    private void ConnectCells(HashSet<int> isolatedIds)
    {
        for (int i = 0; i < lastRowConnectivity.Length; i++)
        {
            if (isolatedIds.Contains(lastRowConnectivity[i]))
            {
                if (MapRules.MustPlaceBlock(lastRow, map, i) == false)
                {
                    map[i] = -1;
                    if (Random.value > randomValue)
                    {
                        isolatedIds.Remove(lastRowConnectivity[i]);
                    }
                }
            }
        }
    }

    private void SetConnectivityStatus(int index)
    {
        //TODO: can do better
        if (connectivityStatus[MapUtils.CalculateIndex(index, -1)] +
            lastRowConnectivity[index] +
            connectivityStatus[MapUtils.CalculateIndex(index, 1)] <= 0)
        {
            connectivityId += 1;
            connectivityStatus[index] = connectivityId;
        }
        else if (connectivityStatus[MapUtils.CalculateIndex(index, -1)] > 0)
        {
            connectivityStatus[index] = connectivityStatus[MapUtils.CalculateIndex(index, -1)];
            if (lastRowConnectivity[index] > 0 && lastRowConnectivity[index] != connectivityStatus[index])
            {
                UpdateConnectivityStatuses(lastRowConnectivity[index], connectivityStatus[index]);
            }
        }

        else if (lastRowConnectivity[index] > 0)
        {
            connectivityStatus[index] = lastRowConnectivity[index];
        }
    }

    private void UpdateConnectivityStatuses(int oldId, int newId)
    {
        for (int i = 0; i < connectivityStatus.Length; i++)
        {
            if (connectivityStatus[i] == oldId)
            {
                connectivityStatus[i] = newId;
            }
            
            if (lastRowConnectivity[i] == oldId)
            {
                lastRowConnectivity[i] = newId;
            }
        }
    }


    private void CalculateNextRow()
    {
        bool hasWay = false;
        List<int> currentRowDots = new List<int>();
        for (int index = 0; index < map.Length; index++)
        {
            Debug.Log("v h " + debugv + " " + index + " ### " + "check for next row hint");
            if (MapUtils.MapStatus(map, index) == 1)
            {
                Debug.Log("it is block");
                continue;
            }
            currentRowDots.Add(index);
            if (MapUtils.MapStatus(map, index, -1) + MapUtils.MapStatus(map, index, 1) + lastRow[index] < 0)
            {
                Debug.Log("it has more way to go: " + (MapUtils.MapStatus(map, index, -1) + MapUtils.MapStatus(map, index, 1) + lastRow[index] ));
                continue;
            }


            Debug.Log("v h " + debugv + " " + index + " ### " +  " ---> right or left is a wall");
            rowHint[index] = -1;
            hasWay = true;
        }

        if (currentRowDots.Count == 0)
        {
            return;
        }

        if (hasWay == false)
        {
            var randomIndex = Random.Range(0, currentRowDots.Count);
            rowHint[currentRowDots[randomIndex]] = -1;
        }

    }

    private void EditOneToLastRow(int row)
    {
        for (int index = map.Length - 2; index >= 0; index--)
        {
            if (MapUtils.MapStatus(map, index) == 1)
            {
                continue;
            }
            if (lastRow[index] != -1)
            {
                map[index] = 1;
            }

            if (lastRow[index] == -1)
            {
                break;
            }
        }
        //todo: SOOO KASSSSSSIIIIFFF
        for (int index = map.Length - 2; index >= 0; index--)
        {
            Debug.Log(" Be Man Mavad Bedahid:: " + MapUtils.MapStatus(map, index, -1) + "  " + MapUtils.MapStatus(map, index, 1) + "  %% " +
                      MapUtils.MapStatus(map, index, -2));
            Debug.Log(index + "  -->> " + (MapUtils.CalculateIndex(index, -1)) + " :: " + MapUtils.MapStatus(map, index, -1));
            if (MapUtils.MapStatus(map, index) == 1)
            {
                continue;
            }
            
            if (MapUtils.MapStatus(map, index, -1) == -1 &&
                MapUtils.MapStatus(map, index, 1) == 1 &&
                MapUtils.MapStatus(map, index, -2) == 1)
            {
                Debug.Log("%%%%%%%  double: " + index);
                if (lastRow[index] != -1)
                {
                    map[index] = 1;
                }
                else
                {
                    map[MapUtils.CalculateIndex(index, -1)] = 1;
                }
            }
        }
    }
    
    private void CalculateLastRow()
    {
        int disjointDots = 0; //todo: remove?
        for (int i = 0; i < map.Length; i++)
        {   
            // Debug.Log("i -->> " + MapUtils.MapStatus(map, i) + " ::: " + MapUtils.MapStatus(map, i, -1));
            if (MapUtils.MapStatus(map, i) == -1 && MapUtils.MapStatus(map, i, 1) == 1)
                disjointDots++;
        }

        if (disjointDots <= 1 && MapUtils.MapStatus(map, 0) == -1)
        {
            List<int> firstRowPortalCandidates = new List<int>();
            for (int i = 0; i < firstRow.Length; i++)
            {
                if (firstRow[i] == -1 )
                {
                    firstRowPortalCandidates.Add(i);
                }
            }
            int portalIndex = firstRowPortalCandidates[Random.Range(0, firstRowPortalCandidates.Count)];
            Array.Fill(rowHint, 1);
            PlaceVerticalPortal(portalIndex);
            for (int i = portalIndex; i >= 0; i--)
            {
                rowHint[i] = -1;
                if (MapUtils.MapStatus(map, i) == -1)
                {
                    return;
                }
            }
        }

        bool firstDots = false;
        bool secondDots = false;
        for (int index = map.Length - 2; index >= 0; index--)
        {
            if (MapUtils.MapStatus(map, index) == 1 && (firstDots == false || rowHint[MapUtils.CalculateIndex(index, 1)] == 1))
            {
                Debug.Log(index + "   A");
                rowHint[index] = 1;
                continue;
            }

            if (MapUtils.MapStatus(map, index) == 1 && (firstDots))
            {
                Debug.Log(index + "   B");
                rowHint[index] = -1;
                if (firstRow[index] == -1 && Random.value > randomValue)
                {
                    PlaceVerticalPortal(index);
                }
                continue;
            }

            if (MapUtils.MapStatus(map, index) == -1 && MapUtils.MapStatus(map, index, -1) == -1 && firstDots == false)
            {
                Debug.Log(index + "   C");
                rowHint[index] = 1;
                continue;
            }
            
            if (MapUtils.MapStatus(map, index) == -1 && MapUtils.MapStatus(map, index, -1) == -1 && firstDots)
            {
                Debug.Log(index + "   D");
                rowHint[index] = -1;
                if (firstRow[index] == -1 && Random.value > randomValue)
                {
                    PlaceVerticalPortal(index);
                }
                firstDots = false;
                secondDots = true;
                continue;
            }
            
            if (MapUtils.MapStatus(map, index) == -1 && MapUtils.MapStatus(map, index, -1) == 1 && secondDots == false)
            {
                Debug.Log(index + "   E");
                firstDots = true;
                rowHint[index] = -1;
                if (firstRow[index] == -1 && Random.value > randomValue)
                {
                    PlaceVerticalPortal(index);
                }
                continue;
            }
            
            if (MapUtils.MapStatus(map, index) == -1 && MapUtils.MapStatus(map, index, -1) == 1 && secondDots)
            {
                Debug.Log(index + "   F:   rowHint[index]: " + rowHint[index]);
                if (Random.value > randomValue && rowHint[index] != -1)
                {
                    Debug.Log("FF");
                    firstDots = false;
                    secondDots = false;
                    rowHint[index] = 1;
                    continue;
                }
                Debug.Log("Not FF");
                firstDots = true;
                secondDots = false;
                rowHint[index] = -1;
                if (firstRow[index] == -1 && Random.value > randomValue)
                {
                    PlaceVerticalPortal(index);
                }
            }
            
        }

    }
    
    
    
    private void PlaceBlock(int hIndex, int vIndex)
    {
        var offset = 0f;
        if (MapUtils.IsMapEvenWidth)
        {
            offset = 0.5f;
        }
        Debug.Log("v h " + debugv + " " + debugh + " ### " + "Place Block");
        connectivityStatus[hIndex] = 0;
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
        
        Debug.Log("v h " + debugv + " " + debugh + " ### " + "Place Dot");
    }

    private void PlaceVerticalPortal(int hIndex)
    {
        Debug.Log("v h " + debugv + " " + debugh + " ### " + "Place Portal");
        if (verticalPortals[MapUtils.CalculateIndex(hIndex, 1)] == -1 ||
            verticalPortals[MapUtils.CalculateIndex(hIndex, -1)] == -1)
        {
            return;
        }
        verticalPortals[hIndex] = -1;
    }

    private void DrawHorizontalBorder()
    {
        for (int i = 0; i < MapUtils.ColumnSize; i++)
        {
            if (verticalPortals[i] == -1)
            {
                //TODO: actually place Portal
                continue;
            }
            PlaceBlock(i, -1);
            PlaceBlock(i, MapUtils.MapVerticalSize);
        }
    }

    private void DrawVerticalBorder(int row)
    {
        PlaceBlock(MapUtils.ColumnSize - 1, row);
        map[MapUtils.ColumnSize - 1] = 1;
    }

}
