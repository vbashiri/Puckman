using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;


public class MapConnectivity
{
    private static int[] lastRowConnectivity;
    private static int[] connectivityStatus;
    private static int connectivityId;

    private static int[] LastRowConnectivity
    {
        get
        {
            if (lastRowConnectivity == null || lastRowConnectivity.Length != MapUtils.ColumnSize)
            {
                lastRowConnectivity = new int[MapUtils.ColumnSize];
            }

            return lastRowConnectivity;
        }
        set => Array.Copy(value, lastRowConnectivity, LastRowConnectivity.Length);
    }
    
    
    private static int[] ConnectivityStatus
    {
        get
        {
            if (connectivityStatus == null || connectivityStatus.Length != MapUtils.ColumnSize)
            {
                connectivityStatus = new int[MapUtils.ColumnSize];
            }

            return connectivityStatus;
        }
    }
    
    public static List<int> ConnectCells(int[] lastRow, int[] currentRow)
    {
        List<int> chosenBlocks = new List<int>();
        
        HashSet<int> isolatedIds = CheckConnectivity(currentRow);
        
        if (isolatedIds.Count < 0)
        {
            LastRowConnectivity = ConnectivityStatus;
            Array.Clear(connectivityStatus, 0, connectivityStatus.Length);
            return chosenBlocks;
        }
        
        for (int i = 0; i < LastRowConnectivity.Length; i++)
        {
            if (isolatedIds.Contains(lastRowConnectivity[i]))
            {
                if (MapRules.MustPlaceBlock(lastRow, currentRow, i) == false)
                {
                    ConnectivityStatus[i] = LastRowConnectivity[i];
                    isolatedIds.Remove(lastRowConnectivity[i]);
                    chosenBlocks.Add(i);
                }

            }
        }

        if (isolatedIds.Count > 0)
        {
            for (int i = 0; i < LastRowConnectivity.Length; i++)
            {
                if (isolatedIds.Contains(lastRowConnectivity[i]))
                {
                    ConnectivityStatus[i] = LastRowConnectivity[i];
                    isolatedIds.Remove(lastRowConnectivity[i]);
                    chosenBlocks.Add(i);
                }
            }
        }
        
        LastRowConnectivity = ConnectivityStatus;
        Array.Clear(connectivityStatus, 0, connectivityStatus.Length);
        return chosenBlocks;
    }

    
    private static HashSet<int> CheckConnectivity(int[] currentRow)
    {
        HashSet<int> isolatedIds = new HashSet<int>();
        for (int i = 0; i < currentRow.Length; i++)
        {
            if (currentRow[i] == -1)
            {
                SetConnectivityStatus(i);
            }
        }
        
        for (int i = 0; i < LastRowConnectivity.Length; i++)
        {
            if (LastRowConnectivity[i] != 0 &&
                ConnectivityStatus.Contains(LastRowConnectivity[i]) == false)
            {
                isolatedIds.Add(LastRowConnectivity[i]);
            }   
        }

        return isolatedIds;
    }
    
    private static void SetConnectivityStatus(int index)
    {
        //TODO: can do better
        if (ConnectivityStatus[MapUtils.CalculateIndex(index, -1)] +
            LastRowConnectivity[index] +
            ConnectivityStatus[MapUtils.CalculateIndex(index, 1)] <= 0)
        {
            connectivityId += 1;
            ConnectivityStatus[index] = connectivityId;
        }
        else if (ConnectivityStatus[MapUtils.CalculateIndex(index, -1)] > 0)
        {
            ConnectivityStatus[index] = ConnectivityStatus[MapUtils.CalculateIndex(index, -1)];
            if (LastRowConnectivity[index] > 0 && LastRowConnectivity[index] != ConnectivityStatus[index])
            {
                UpdateConnectivityStatuses(LastRowConnectivity[index], ConnectivityStatus[index]);
            }
        }

        else if (LastRowConnectivity[index] > 0)
        {
            ConnectivityStatus[index] = LastRowConnectivity[index];
        }
    }
    
    private static void UpdateConnectivityStatuses(int oldId, int newId)
    {
        for (int i = 0; i < ConnectivityStatus.Length; i++)
        {
            if (ConnectivityStatus[i] == oldId)
            {
                ConnectivityStatus[i] = newId;
            }
            
            if (LastRowConnectivity[i] == oldId)
            {
                LastRowConnectivity[i] = newId;
            }
        }
    }

}
