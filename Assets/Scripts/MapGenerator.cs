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
    [SerializeField] private int horizontalSize;
    [SerializeField] private int verticalSize;
    [SerializeField] private float randomValue;
    private Transform playground;
    private int columnSize;
    private bool isEvenWidth;

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
        playground = Instantiate(new GameObject(), transform).GetComponent<Transform>();
        columnSize = Mathf.CeilToInt((horizontalSize + 2f) / 2f);
        isEvenWidth = horizontalSize % 2 == 0;
        lastRow = new int[columnSize];
        map = new int[columnSize];
        rowHint = new int[columnSize];
        firstRow = new int[columnSize];
        verticalPortals = new int[columnSize];
        connectivityStatus = new int[columnSize];
        lastRowConnectivity = new int[columnSize];
        connectivityId = 0;

        presetRowIndex = Mathf.CeilToInt((verticalSize - presetRow.Length) / 2f); 
        
        Array.Fill(lastRow, 1);
        StartCoroutine(GenerateMap());
    }

    private IEnumerator GenerateMap()
    {
        int reachableMapSize = columnSize - 1;
        CheckForData(0);
        int firstRowMin = rowHint.Min();
        if (firstRowMin >= 0)
        {
            rowHint[Random.Range(0, rowHint.Length)] = -1; //Add at least one dot
        }
        for (int j = 0; j < verticalSize; j ++)
        {
            yield return new WaitForSeconds(0.2f);
            debugv = j;
            DrawVerticalBorder(j);
            CheckForData(j);
            DrawPredeterminedCells(j);

            Debug.Log("****************************************************");
            for (int i = 0; i < reachableMapSize; i++)
            {
                debugh = i;
                if (MapStatus(i) == -1 || MapStatus(i) == 1)
                {
                    continue;
                }
                if (MustPlaceBlock(i))
                {
                    map[i] = 1;
                }
            
            }
            
            Debug.Log("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
            for (int i = 0; i < reachableMapSize; i++)
            {
                debugh = i;
                Debug.Log("v h " + debugv + " " + debugh + " ### " + " here we go");
                if (map[i] == -1 || map[i] == 1)
                {
                    Debug.Log("v h " + debugv + " " + debugh + " ### " + " already decided moves on");
                    continue;
                }

                // if (i == 1)
                // {
                //     if (map[0] == -1 && j == 0)
                //     {
                //         PlaceDot(i, j);
                //         PlaceDot(-i, j);
                //         map[i] = -1;
                //         continue;
                //     }
                // }

                if (MustPlaceBlock(i))
                {
                    map[i] = 1;
                    continue;
                }

                if (CanPlaceBlock(i))
                {
                    if (Random.value > randomValue)
                    {
                        map[i] = 1;
                        continue;
                    }
                    // else
                    // {
                    //     PlaceDot(i,j);
                    //     PlaceDot(-i,j);
                    //     map[i] = -1;
                    //     continue;
                    // }
                }
                
                map[i] = -1;
            }
            
            Array.Clear(rowHint, 0, rowHint.Length);

            if (CheckConnectivity() == false)
            {
                string lastRowConnectivityLine = "";
                string currentRowConnectivityLine = "";
                for (int i = 0; i < connectivityStatus.Length; i++)
                {
                    lastRowConnectivityLine += (lastRowConnectivity[i]) + " ";
                    currentRowConnectivityLine += (connectivityStatus[i]) + " ";
                }
            
                Debug.Log(lastRowConnectivityLine);
                Debug.Log(currentRowConnectivityLine);
                yield return new WaitForSeconds(5f);
            }
            
            CalculateNextRow();
            if (Mathf.Abs(j) == verticalSize - 2)
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

    private bool MustPlaceBlock(int index)
    {
        // if both side is blocked
        bool right = MapStatus(index, 1) == 1;
        bool left = MapStatus(index, -1) == 1;
        if (MapStatus(index, -1) == 1 && MapStatus(index, 1) == 1 && lastRow[index] == 1)
        {
            Debug.Log("v h " + debugv + " " + debugh + " ### " + " must block true left and right and top");
            return true;
        }

        // Check for very small maps >> out of range >> -1
        if (index >= map.Length - 3 && MapStatus(index, -1) == 1 &&
            lastRow[index] != -1 && lastRow[CalculateIndex(index,1)] != -1)
        {
            Debug.Log("v h " + debugv + " " + debugh + " ### " + " must block true edge of map");
            return true;
        }
        
        if (MapStatus(index, -1) + lastRow[index] + lastRow[CalculateIndex(index, -1)] == -3)
        {
            Debug.Log("v h " + debugv + " " + debugh + " ### " + " must block top left square");
            Debug.Log(MapStatus(index, -1) + " " + lastRow[index] + " " + lastRow[CalculateIndex(index, -1)]);
            return true;
        }
        if (MapStatus(index, 1) + lastRow[index] + lastRow[CalculateIndex(index, 1)] == -3)
        {
            Debug.Log("v h " + debugv + " " + debugh + " ### " + " must block true top right square");
            Debug.Log(MapStatus(index, 1) + " " + lastRow[index] + " " + lastRow[CalculateIndex(index, 1)]);
            return true;
        }
        
        Debug.Log("v h " + debugv + " " + debugh + " ### " + "must block false");
        return false;
    }

    private bool CanPlaceBlock(int index)
    {
        Debug.Log("v h " + debugv + " " + debugh + " ### " + "can be blocked");
        //Todo: if i + 2 > out of range

        if (index == 1 && MapStatus(0) == -1 && lastRow[0] == 1)
        {
            Debug.Log("v h " + debugv + " " + debugh + " ### " + "can block: false (middle condition)");
            return false;
        }

        if ((MapStatus(index, -3) == 1 || MapStatus(index, -2) == 1) &&
            ((MapStatus(index, -2) == -1 && lastRow[CalculateIndex(index, -2)] == 1) ||
             (MapStatus(index, -1) == -1 && lastRow[CalculateIndex(index, -1)] == 1)))
        {
            Debug.Log("v h " + debugv + " " + debugh + " ### " + "can block: false (left condition)");
            return false;
        }
        if ((MapStatus(index, 3) == 1 || MapStatus(index, 2) == 1) &&
            ((MapStatus(index, 2) == -1 && lastRow[CalculateIndex(index, 2)] == 1) || 
             (MapStatus(index, 1) == -1 && lastRow[CalculateIndex(index, 1)] == 1)))
        {
            Debug.Log("v h " + debugv + " " + debugh + " ### " + "can block: false (right condition)");
            return false;
        }
        
        Debug.Log("v h " + debugv + " " + debugh + " ### " + "can block: true");

        return true;
    }

    private int MapStatus(int index, int distance = 0)
    {
        return map[CalculateIndex(index, distance)];
    }

    private int CalculateIndex(int index, int distance)
    {
        if (distance <= 0)
        {
            if (isEvenWidth && index + distance < 0)
            {
                return Mathf.Abs(index + distance + 1);
            }
            return Mathf.Abs(index + distance);
        }

        if (index + distance < map.Length)
        {
            return index + distance;
        }

        return map.Length - 1;
    }

    private void CalculateNextRow()
    {
        bool hasWay = false;
        List<int> currentRowDots = new List<int>();
        for (int index = 0; index < map.Length; index++)
        {
            Debug.Log("v h " + debugv + " " + index + " ### " + "check for next row hint");
            if (MapStatus(index) == 1)
            {
                Debug.Log("it is block");
                continue;
            }
            currentRowDots.Add(index);
            if (MapStatus(index, -1) + MapStatus(index, 1) + lastRow[index] < 0)
            {
                Debug.Log("it has more way to go: " + (MapStatus(index, -1) + MapStatus(index, 1) + lastRow[index] ));
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
            if (MapStatus(index) == 1)
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
            Debug.Log(" Be Man Mavad Bedahid:: " + MapStatus(index, -1) + "  " + MapStatus(index, 1) + "  %% " +
                      MapStatus(index, -2));
            Debug.Log(index + "  -->> " + (CalculateIndex(index, -1)) + " :: " + MapStatus(index, -1));
            if (MapStatus(index) == 1)
            {
                continue;
            }
            
            if (MapStatus(index, -1) == -1 &&
                MapStatus(index, 1) == 1 &&
                MapStatus(index, -2) == 1)
            {
                Debug.Log("%%%%%%%  double: " + index);
                if (lastRow[index] != -1)
                {
                    map[index] = 1;
                }
                else
                {
                    map[CalculateIndex(index, -1)] = 1;
                }
            }
        }
    }
    
    private void CalculateLastRow()
    {
        int disjointDots = 0; //todo: remove?
        for (int i = 0; i < map.Length; i++)
        {   
            // Debug.Log("i -->> " + MapStatus(i) + " ::: " + MapStatus(i, -1));
            if (MapStatus(i) == -1 && MapStatus(i, 1) == 1)
                disjointDots++;
        }

        if (disjointDots <= 1 && MapStatus(0) == -1)
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
                if (MapStatus(i) == -1)
                {
                    return;
                }
            }
        }

        bool firstDots = false;
        bool secondDots = false;
        for (int index = map.Length - 2; index >= 0; index--)
        {
            if (MapStatus(index) == 1 && (firstDots == false || rowHint[CalculateIndex(index, 1)] == 1))
            {
                Debug.Log(index + "   A");
                rowHint[index] = 1;
                continue;
            }

            if (MapStatus(index) == 1 && (firstDots))
            {
                Debug.Log(index + "   B");
                rowHint[index] = -1;
                if (firstRow[index] == -1 && Random.value > randomValue)
                {
                    PlaceVerticalPortal(index);
                }
                continue;
            }

            if (MapStatus(index) == -1 && MapStatus(index, -1) == -1 && firstDots == false)
            {
                Debug.Log(index + "   C");
                rowHint[index] = 1;
                continue;
            }
            
            if (MapStatus(index) == -1 && MapStatus(index, -1) == -1 && firstDots)
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
            
            if (MapStatus(index) == -1 && MapStatus(index, -1) == 1 && secondDots == false)
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
            
            if (MapStatus(index) == -1 && MapStatus(index, -1) == 1 && secondDots)
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
        if (isEvenWidth)
        {
            offset = 0.5f;
        }
        Debug.Log("v h " + debugv + " " + debugh + " ### " + "Place Block");
        connectivityStatus[hIndex] = 0;
        Instantiate(cube,
            new Vector3(hIndex * 1 + offset, 0, vIndex  * - 1 - 3),
            Quaternion.identity,
            playground);

        if (hIndex == 0 && isEvenWidth == false)
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

    private bool CheckConnectivity()
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
            return false;
        }

        return true;
    }

    private void ConnectCells(HashSet<int> isolatedIds)
    {
        for (int i = 0; i < lastRowConnectivity.Length; i++)
        {
            if (isolatedIds.Contains(lastRowConnectivity[i]))
            {
                if (MustPlaceBlock(i) == false)
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
        if (connectivityStatus[CalculateIndex(index, -1)] +
            lastRowConnectivity[index] +
            connectivityStatus[CalculateIndex(index, 1)] <= 0)
        {
            connectivityId += 1;
            connectivityStatus[index] = connectivityId;
        }
        else if (connectivityStatus[CalculateIndex(index, -1)] > 0)
        {
            connectivityStatus[index] = connectivityStatus[CalculateIndex(index, -1)];
            if (lastRowConnectivity[index] > 0 && lastRowConnectivity[index] != connectivityStatus[index])
            {
                UpdateConnectivityStatus(lastRowConnectivity[index], connectivityStatus[index]);
            }
        }

        else if (lastRowConnectivity[index] > 0)
        {
            connectivityStatus[index] = lastRowConnectivity[index];
        }
    }

    private void UpdateConnectivityStatus(int oldId, int newId)
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
    
    private void PlaceVerticalPortal(int hIndex)
    {
        Debug.Log("v h " + debugv + " " + debugh + " ### " + "Place Portal");
        if (verticalPortals[CalculateIndex(hIndex, 1)] == -1 ||
            verticalPortals[CalculateIndex(hIndex, -1)] == -1)
        {
            return;
        }
        verticalPortals[hIndex] = -1;
    }

    private void DrawHorizontalBorder()
    {
        for (int i = 0; i < columnSize; i++)
        {
            if (verticalPortals[i] == -1)
            {
                //TODO: actually place Portal
                continue;
            }
            PlaceBlock(i, -1);
            PlaceBlock(i, verticalSize);
        }
    }

    private void DrawVerticalBorder(int row)
    {
        PlaceBlock(columnSize - 1, row);
        map[columnSize - 1] = 1;
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

    private void DrawPredeterminedCells(int row)
    {
        for (int i = 0; i < columnSize - 1; i++)
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

}
