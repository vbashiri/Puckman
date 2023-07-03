using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
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
    private int rowSize;
    
    private int[] lastRow;
    private int[] map;
    private int[] rowHint;
    private int[] firstRow;

    private int[][] presetRow = {
        new int[] {1, 1, 1,  0, 1 },
        new int[] { -1, -1, -1, -1, -1},
        new int[] { -1, 1, 1, -1},
        new int[] { -1, -1, 1, -1},
        new int[] { 1, 1, 1, -1},
        new int[] { -1, -1, -1, -1, -1}
    };

    private int[] firstTopRow = new[] {-1, 1, 1};
    private int[] firstBottomRow = new[] {1, 1, 1};

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
        columnSize = Mathf.FloorToInt(horizontalSize / 2);
        rowSize = Mathf.CeilToInt(verticalSize / 2);
        lastRow = new int[columnSize];
        map = new int[columnSize];
        rowHint = new int[columnSize];
        firstRow = new int[columnSize];
        
        
        DrawHorizontalBorder();
        Array.Fill(lastRow, 1);
        GenerateMap(-rowSize + 1, 1, columnSize - 1);
    }

    private void GenerateMap(int firstRowIndex, int step, int reachableMapSize)
    {
        for (int j = firstRowIndex; j < rowSize; j += step)
        {
            debugv = j;
            DrawVerticalBorder(j);
            CheckForData(j);
            DrawPredeterminedCells(j);

            Debug.Log("****************************************************");
            for (int i = 0; i < reachableMapSize; i++)
            {
                debugh = i;
                if (MustPlaceBlock(i))
                {
                    PlaceBlock(i, j);
                    if (i > 0)
                    {
                        PlaceBlock(-i, j);
                    }
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
                    PlaceBlock(i, j);
                    PlaceBlock(-i, j);
                    map[i] = 1;
                    continue;
                }

                if (CanPlaceBlock(i))
                {
                    if (Random.value > randomValue)
                    {
                        PlaceBlock(i, j);
                        PlaceBlock(-i, j);
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
                


                PlaceDot(i, j);
                map[i] = -1;
                PlaceDot(-i, j);
                //TODO: Other rows first dot
            }

            Array.Clear(rowHint, 0, rowHint.Length);

            CalculateNextRow();
            if (Mathf.Abs(j) == rowSize - 2)
            {
                EditOneToLastRow(j);
                CalculateLastRow();
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
            if (j == firstRowIndex)
            {
                Array.Copy(map, firstRow, map.Length);
            }
            Array.Clear(map, 0, map.Length);
            map[reachableMapSize] = 1;
        }
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
                PlaceBlock(index, row);
                PlaceBlock(-index, row);
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
                    PlaceBlock(index, row);
                    PlaceBlock(-index, row);
                    map[index] = 1;
                }
                else
                {
                    PlaceBlock(CalculateIndex(index, -1), row);
                    PlaceBlock(-CalculateIndex(index, -1), row);
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
        
        Debug.Log("==================>> disjointDots:: " + disjointDots);
        
        if (disjointDots <= 1)
        {
            for (int i = 0; i < map.Length; i++)
            {
                if (MapStatus(i) == 1)
                {
                    rowHint[i] = -1;
                    continue;
                }

                rowHint[i] = -1;
                for (int j = i+1; j < map.Length; j++)
                {
                    rowHint[j] = 1;
                }
                return;
            }
        }

        int jointedDots = 0; //todo: remove

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
                firstDots = false;
                secondDots = true;
                continue;
            }
            
            if (MapStatus(index) == -1 && MapStatus(index, -1) == 1 && secondDots == false)
            {
                Debug.Log(index + "   E");
                firstDots = true;
                rowHint[index] = -1;
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
            }
            
        }

    }
    
    
    
    private void PlaceBlock(int hIndex, int vIndex)
    {
        Debug.Log("v h " + debugv + " " + debugh + " ### " + "Place Block");
        Instantiate(cube,
            new Vector3(hIndex * 1, 0, vIndex  * - 1 - 3),
            Quaternion.identity,
            playground);
    }

    private void PlaceDot(int hIndex, int vIndex)
    {
        Debug.Log("v h " + debugv + " " + debugh + " ### " + "Place Dot");
    }

    private void DrawHorizontalBorder()
    {
        for (int i = 0; i < columnSize; i++)
        {
            Debug.Log("first row indeed");
            PlaceBlock(i, -rowSize);
            PlaceBlock(-i, -rowSize);
            PlaceBlock(i, rowSize);
            PlaceBlock(-i, rowSize);
        }
    }

    private void DrawVerticalBorder(int row)
    {
        PlaceBlock(columnSize - 1, row);
        PlaceBlock(-columnSize + 1, row);
        map[columnSize - 1] = 1;
    }

    private void CheckForData(int row)
    {
        row += 2; //Todo: You can do much better
        if (row < presetRow.Length && row >= 0)
        {
            Debug.LogError((row - 2) + "  ----------------------> ");
            Debug.LogError((row - 2) + "  ----------------------> ");
            Debug.LogError((row - 2) + "  ----------------------> ");
            Debug.LogError((row - 2) + "  ----------------------> ");
            Debug.LogWarning("--------------------------------------------------------------");
            Array.Copy(presetRow[row], rowHint, presetRow[row].Length);
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
                PlaceDot(i, row);
                PlaceDot(-i, row);
                map[i] = -1;
            }

            if (rowHint[i] == 1)
            {
                Debug.Log("Draw Predetermind Block " + row  + " " + i);
                PlaceBlock(i, row);
                PlaceBlock(-i, row);
                map[i] = 1;
            }
        }
    }

}
