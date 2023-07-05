using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MapGenerator
{
    private List<int[]> map;

    private int[] lastRow;
    private int[] currentRow;
    private int[] rowHint;
    private int[] firstRow;
    private int[] verticalPortals;
    
    private int[] connectivityStatus;
    private int[] lastRowConnectivity;
    private int connectivityId;

    private int[][] presetRow = {
        new int[] {1,0, 0,  1, 1 },
        new int[] { -2, -2, -2, -2},
        new int[] { -2, 2, 2, -1},
        new int[] { -2, -2, 2, -1},
        new int[] { 2, 2, 2, -1},
        new int[] { -1, -1, -1, -1}
    };
    private int presetRowIndex;

    // private int debugv = 0;
    // private int debugh = 0;


    public List<int[]> SetupMap()
    {

        InitializeVariables();

        return GenerateMap();
    }

    private void InitializeVariables()
    {
        map = new List<int[]>();
        lastRow = new int[MapUtils.ColumnSize];
        currentRow = new int[MapUtils.ColumnSize];
        rowHint = new int[MapUtils.ColumnSize];
        firstRow = new int[MapUtils.ColumnSize];
        verticalPortals = new int[MapUtils.ColumnSize];
        for (int i = 0; i < verticalPortals.Length; i++)
        {
            verticalPortals[i] = 1;
        }
        connectivityStatus = new int[MapUtils.ColumnSize];
        lastRowConnectivity = new int[MapUtils.ColumnSize];
        connectivityId = 0;
        presetRowIndex = Mathf.CeilToInt((MapUtils.MapVerticalSize - presetRow.Length) / 2f);
        for (int i = 0; i < lastRow.Length; i++)
        {
            lastRow[i] = 1;
        }
    }

    private List<int[]> GenerateMap()
    {
        int reachableMapSize = MapUtils.ColumnSize - 1;
        CheckForData(0);
        AddFirstRowDots();
        
        for (int j = 0; j < MapUtils.MapVerticalSize; j ++)
        {
            currentRow[reachableMapSize] = 1; // can place portals
            CheckForData(j);
            DrawPredeterminedCells(j);

            CheckForMustBlocks();
            

            for (int i = 0; i < reachableMapSize; i++)
            {
                GenerateCell(i);
            }

            AddDotsToFullBlock();
            
            if (Mathf.Abs(j) == MapUtils.MapVerticalSize - 2)
            {
                EditOneToLastRow(j);
            }
            
            CheckConnectivity();

            Array.Clear(rowHint, 0, rowHint.Length);
            CalculateNextRow();

            if (Mathf.Abs(j) == MapUtils.MapVerticalSize - 2)
            {
                CalculateLastRow();
            }

            ResetArrays(j);
        }

        map.Insert(0, verticalPortals);
        map.Add(verticalPortals);
        return map;
    }
    
    private void CheckForData(int row)
    {
        if (Mathf.Min(MapUtils.MapHorizontalSize, MapUtils.MapVerticalSize) < 7)
        {
            return;
        }
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
        if (rowHint.Min() >= 0)
        {
            rowHint[Random.Range(0, rowHint.Length - 1)] = -1; //Add at least one dot
        }
    }
    
    private void AddDotsToFullBlock()
    {
        if (currentRow.Min() < 0)
        {
            return;
        }

        List<int> canBeDots = new List<int>();
        for (int i = MapUtils.IsMapEvenWidth ? 1 : 0; i < currentRow.Length; i++)
        {
            if (MapUtils.GetRowStatus(lastRow, i) == -1)
            {
                canBeDots.Add(i);
            }
        }
        
        currentRow[canBeDots[Random.Range(0, canBeDots.Count)]] = -1; //Add at least one dot
    }

    
    private void DrawPredeterminedCells(int row)
    {
        for (int i = 0; i < MapUtils.ColumnSize - 1; i++)
        {
            if (rowHint[i] == -1)
            {
                currentRow[i] = -1;
            }

            if (rowHint[i] == 1)
            {
                currentRow[i] = 1;
            }
        }
    }

    private void CheckForMustBlocks()
    {
        for (int i = 0; i < MapUtils.ColumnSize - 1; i++)
        {
            if (MapUtils.GetRowStatus(currentRow, i) == -1 || MapUtils.GetRowStatus(currentRow, i) == 1)
            {
                continue;
            }
            if (MapRules.MustPlaceBlock(lastRow, currentRow, i))
            {
                currentRow[i] = 1;
            }
            
        }
    }

    private void GenerateCell(int hIndex)
    {
        if (currentRow[hIndex] == -1 || currentRow[hIndex] == 1)
        {
            return;
        }

        if (MapRules.MustPlaceBlock(lastRow, currentRow, hIndex))
        {
            currentRow[hIndex] = 1;
            return;
        }

        if (MapRules.CanPlaceBlock(lastRow, currentRow, hIndex))
        {
            if (Random.value > MapUtils.DotChanceValue)
            {
                currentRow[hIndex] = 1;
                return;
            }
        }
                
        currentRow[hIndex] = -1;
    }
    
    private void CheckConnectivity()
    {
        for (int i = 0; i < currentRow.Length; i++)
        {
            if (currentRow[i] == -1)
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
        bool isConnected = false;
        List<int> candidateBlocks = new List<int>();
        for (int i = 0; i < lastRowConnectivity.Length; i++)
        {
            if (isolatedIds.Contains(lastRowConnectivity[i]))
            {
                if (MapRules.MustPlaceBlock(lastRow, currentRow, i) == false)
                {
                    currentRow[i] = -1;
                    connectivityStatus[i] = lastRowConnectivity[i];
                    isConnected = true;
                    if (Random.value > MapUtils.DotChanceValue)
                    {
                        isolatedIds.Remove(lastRowConnectivity[i]);
                    }
                }
                else
                {
                    candidateBlocks.Add(i);
                }
            }
        }

        if (isConnected == false)
        {
            var randomBlock = candidateBlocks[Random.Range(0, candidateBlocks.Count)];
            connectivityStatus[randomBlock] = lastRowConnectivity[randomBlock];
            currentRow[randomBlock] = -1;
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
        for (int index = 0; index < currentRow.Length; index++)
        {
            if (MapUtils.GetRowStatus(currentRow, index) == 1)
            {
                continue;
            }
            currentRowDots.Add(index);
            if (MapUtils.GetRowStatus(currentRow, index, -1) + MapUtils.GetRowStatus(currentRow, index, 1) + lastRow[index] < 0)
            {
               continue;
            }
            
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
        for (int index = currentRow.Length - 2; index >= 0; index--)
        {
            if (MapUtils.GetRowStatus(currentRow, index) == 1)
            {
                continue;
            }
            if (lastRow[index] != -1)
            {
                currentRow[index] = 1;
            }

            if (lastRow[index] == -1)
            {
                break;
            }
        }
        //todo: SOOO KASSSSSSIIIIFFF
        for (int index = currentRow.Length - 2; index >= 0; index--)
        {

            if (MapUtils.GetRowStatus(currentRow, index) == 1)
            {
                continue;
            }
            
            if (MapUtils.GetRowStatus(currentRow, index, -1) == -1 &&
                MapUtils.GetRowStatus(currentRow, index, 1) == 1 &&
                MapUtils.GetRowStatus(currentRow, index, -2) == 1)
            {
                if (lastRow[index] != -1)
                {
                    currentRow[index] = 1;
                }
                else
                {
                    currentRow[MapUtils.CalculateIndex(index, -1)] = 1;
                }
            }
        }
    }
    
    private void CalculateLastRow()
    {
        int disjointDots = 0; //todo: remove?
        for (int i = 0; i < currentRow.Length; i++)
        {
            if (MapUtils.GetRowStatus(currentRow, i) == -1 && MapUtils.GetRowStatus(currentRow, i, 1) == 1)
                disjointDots++;
        }

        if (disjointDots <= 1 && MapUtils.GetRowStatus(currentRow, 0) == -1)
        {
            List<int> firstRowPortalCandidates = new List<int>();
            for (int i = 0; i < firstRow.Length; i++)
            {
                if (firstRow[i] == -1 )
                {
                    firstRowPortalCandidates.Add(i);
                }
            }

            if (firstRowPortalCandidates.Count <= 0)
            {
                return;
            }
            int portalIndex = firstRowPortalCandidates[Random.Range(0, firstRowPortalCandidates.Count)];
            for (int i = 0; i < rowHint.Length; i++)
            {
                rowHint[i] = 1;
            }
            AddVerticalPortals(portalIndex);
            for (int i = portalIndex; i >= 0; i--)
            {
                rowHint[i] = -1;
                if (MapUtils.GetRowStatus(currentRow, i) == -1)
                {
                    return;
                }
            }
        }

        bool firstDots = false;
        bool secondDots = false;
        for (int index = currentRow.Length - 2; index >= 0; index--)
        {
            if (MapUtils.GetRowStatus(currentRow, index) == 1 && (firstDots == false || rowHint[MapUtils.CalculateIndex(index, 1)] == 1))
            {
                rowHint[index] = 1;
                continue;
            }

            if (MapUtils.GetRowStatus(currentRow, index) == 1 && (firstDots))
            {
                rowHint[index] = -1;
                if (firstRow[index] == -1 && Random.value > MapUtils.DotChanceValue)
                {
                    AddVerticalPortals(index);
                }
                continue;
            }

            if (MapUtils.GetRowStatus(currentRow, index) == -1 && MapUtils.GetRowStatus(currentRow, index, -1) == -1 && firstDots == false)
            {
                rowHint[index] = 1;
                continue;
            }
            
            if (MapUtils.GetRowStatus(currentRow, index) == -1 && MapUtils.GetRowStatus(currentRow, index, -1) == -1 && firstDots)
            {
                rowHint[index] = -1;
                if (firstRow[index] == -1 && Random.value > MapUtils.DotChanceValue)
                {
                    AddVerticalPortals(index);
                }
                firstDots = false;
                secondDots = true;
                continue;
            }
            
            if (MapUtils.GetRowStatus(currentRow, index) == -1 && MapUtils.GetRowStatus(currentRow, index, -1) == 1 && secondDots == false)
            {
                firstDots = true;
                rowHint[index] = -1;
                if (firstRow[index] == -1 && Random.value > MapUtils.DotChanceValue)
                {
                    AddVerticalPortals(index);
                }
                continue;
            }
            
            if (MapUtils.GetRowStatus(currentRow, index) == -1 && MapUtils.GetRowStatus(currentRow, index, -1) == 1 && secondDots)
            {
                if (Random.value > MapUtils.DotChanceValue && rowHint[index] != -1)
                {
                    firstDots = false;
                    secondDots = false;
                    rowHint[index] = 1;
                    continue;
                }
                firstDots = true;
                secondDots = false;
                rowHint[index] = -1;
                if (firstRow[index] == -1 && Random.value > MapUtils.DotChanceValue)
                {
                    AddVerticalPortals(index);
                }
            }
            
        }
    }

    private void ResetArrays(int row)
    {
        map.Add(currentRow.Clone() as int[]);
        Array.Copy(currentRow, lastRow, currentRow.Length);
        if (row == 0)
        {
            Array.Copy(currentRow, firstRow, currentRow.Length);
        }
        Array.Copy(connectivityStatus, lastRowConnectivity, connectivityStatus.Length);
        Array.Clear(connectivityStatus, 0, connectivityStatus.Length);
        Array.Clear(currentRow, 0, currentRow.Length);
    }

    private void AddVerticalPortals(int hIndex)
    {
        if (MapRules.CanPlaceVerticalPortal(verticalPortals, hIndex))
        {
            verticalPortals[hIndex] = -2;
        }
    }
    
}
