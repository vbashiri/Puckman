using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private const float SPEED = 3f;

    public enum MoveDirection
    {
        up,
        down,
        left,
        right
    }

    private float offset;
    private List<int[]> map;
    private Coroutine moveRoutine;


    public Character Setup(List<int[]> map, Vector3 initialPosition)
    {
        offset = MapUtils.IsMapEvenWidth ? 0.5f : 0f;
        transform.position = initialPosition;
        this.map = map;
        return this;
    }

    public void Move(MoveDirection moveDirection)
    {
        switch (moveDirection)
        {
            case MoveDirection.up:
                transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), 0,
                    transform.position.z + SPEED * Time.deltaTime);
                break;
            case MoveDirection.down:
                transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), 0,
                    transform.position.z - SPEED * Time.deltaTime);
                break;
            case MoveDirection.right:
                transform.position =  new Vector3(transform.position.x + SPEED * Time.deltaTime, 0,
                    Mathf.RoundToInt(transform.position.z));
                break;
            case MoveDirection.left:
                transform.position =  new Vector3(transform.position.x - SPEED * Time.deltaTime, 0,
                    Mathf.RoundToInt(transform.position.z));
                break;
        }
    }
    
    public void Move(MoveDirection moveDirection, System.Action callBack)
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }
        moveRoutine = StartCoroutine(MoveRoutine(moveDirection, callBack));
    }

    private IEnumerator MoveRoutine(MoveDirection moveDirection, System.Action callBack)
    {
        yield return null;
        Vector2Int initialPosition = GetCurrentLocation();
        Vector2Int currentPosition = initialPosition;
        while (initialPosition.x == currentPosition.x && initialPosition.y == currentPosition.y)
        {
            Move(moveDirection);
            if (IsCentered())
                currentPosition = GetCurrentLocation();
            yield return null;
        }
        callBack?.Invoke();
    }

    private Vector2Int GetCurrentLocation()
    {
        return new Vector2Int( Mathf.RoundToInt(Mathf.Abs(transform.position.x) - offset),
            Mathf.RoundToInt(Mathf.Abs(transform.position.z)));
    }

    public bool IsCentered()
    {
        var currentLocation = GetCurrentLocation();
        if (Mathf.Abs(Mathf.Abs(transform.position.x) - currentLocation.x) > 0.1f ||
            Mathf.Abs(-transform.position.z - currentLocation.y) > 0.1f)
        {
            return false;
        }

        return true;
    }

    public bool CanMove(MoveDirection moveDirection)
    {
        var currentLocation = new Vector2Int( Mathf.RoundToInt(Mathf.Abs(transform.position.x) - offset),
            Mathf.RoundToInt(Mathf.Abs(transform.position.z)));

        int mirrorFactor = Mathf.RoundToInt(transform.position.x - offset) < 0 ? -1 : 1;
        currentLocation.x = currentLocation.x * mirrorFactor;

        switch (moveDirection)
        {
            case MoveDirection.up:
                if (currentLocation.y - 1 < 0)
                {
                    return false;
                }
                if (map[currentLocation.y - 1][mirrorFactor * currentLocation.x] > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            case MoveDirection.down:
                if (currentLocation.y + 1 >= map.Count)
                {
                    return false;
                }
                if (map[currentLocation.y + 1][mirrorFactor * currentLocation.x] > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            case MoveDirection.left:
                if (map[currentLocation.y][MapUtils.CalculateIndex(mirrorFactor * currentLocation.x, -mirrorFactor)] > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            case MoveDirection.right:
                if (map[currentLocation.y][MapUtils.CalculateIndex(mirrorFactor * currentLocation.x, mirrorFactor)] > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
        }

        return false;
    }
    
}
