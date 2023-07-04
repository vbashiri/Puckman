using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRules
{
    public static bool MustPlaceBlock(int[] lastRow, int[] currentRow, int index)
    {

        // Rule: 3 neighbors are block => Become society's color :)
        if (MapUtils.MapStatus(currentRow, index, -1) == 1 &&
            MapUtils.MapStatus(currentRow, index, 1) == 1 &&
            MapUtils.MapStatus(lastRow, index) == 1)
        {
            return true;
        }

        // Todo: Check for very small maps >> out of range >> -1
        // Rule: 2 neighbors are block and is on the verge of map
        if (index >= currentRow.Length - 3 &&
            MapUtils.MapStatus(currentRow, index, -1) == 1 &&
            MapUtils.MapStatus(lastRow, index) != -1 &&
            MapUtils.MapStatus(lastRow, index,1) != -1)
        {
            return true;
        }
        
        // Rule: Don't create square (Top Left)
        if (MapUtils.MapStatus(currentRow, index, -1) +
            lastRow[index] +
            lastRow[MapUtils.CalculateIndex(index, -1)] == -3)
        {
            return true;
        }
        
        //Rule: Don't create square (Top Right)
        if (MapUtils.MapStatus(currentRow, index, 1) +
            lastRow[index] +
            lastRow[MapUtils.CalculateIndex(index, 1)] == -3)
        {
            return true;
        }
        
        return false;
    }
    
    public static bool CanPlaceBlock(int[] lastRow, int[] currentRow, int index)
    {
        //Rule: Don't confine middle cell
        if (index == 1 && MapUtils.MapStatus(currentRow, 0) == -1 && MapUtils.MapStatus(lastRow, 0) == 1)
        {
            return false;
        }

        //Rule: Don't block if there is block left side and left blocks don't have a way up (make a U turn)
        if ((MapUtils.MapStatus(currentRow, index, -3) == 1 ||
             MapUtils.MapStatus(currentRow, index, -2) == 1) &&
            ((MapUtils.MapStatus(currentRow, index, -2) == -1 &&
              lastRow[MapUtils.CalculateIndex(index, -2)] == 1) ||
             (MapUtils.MapStatus(currentRow, index, -1) == -1 &&
              lastRow[MapUtils.CalculateIndex(index, -1)] == 1)))
        {
            return false;
        }
        
        //Rule: Don't block if there is block right side and right blocks don't have a way up (make a U turn)
        if ((MapUtils.MapStatus(currentRow, index, 3) == 1 ||
             MapUtils.MapStatus(currentRow, index, 2) == 1) &&
            ((MapUtils.MapStatus(currentRow, index, 2) == -1 &&
              lastRow[MapUtils.CalculateIndex(index, 2)] == 1) || 
             (MapUtils.MapStatus(currentRow, index, 1) == -1 &&
              lastRow[MapUtils.CalculateIndex(index, 1)] == 1)))
        {
            return false;
        }
        

        return true;
    }

    public static bool CanPlaceVerticalPortal(int[] portalRow, int index)
    {
        //Rule: Don't make portals next to each other
        if (MapUtils.MapStatus(portalRow, index, -1) == -2 ||
            MapUtils.MapStatus(portalRow, index, 1) == -2)
        {
            return false;
        }
        return true;
    }

}
