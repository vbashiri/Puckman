using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Packman : MonoBehaviour
{
    private Character character;
    private bool isMovingHorizontally;
    private Character.MoveDirection currentDirection;
    
    public Packman Setup(List<int[]> map)
    {
        gameObject.layer = LayerMask.NameToLayer("Packman");
        int hIndex = 0;
        for (int i = 0; i < map[0].Length; i++)
        {
            if (MapUtils.GetRowStatus(map[map.Count / 4 * 3], i) == -1)
            {
                hIndex = i;
                break;
            }
        }

        float offset = MapUtils.IsMapEvenWidth ? 0.5f : 0f;
        character = gameObject.AddComponent<Character>()
            .Setup(map, new Vector3(hIndex + offset, 0f, -(map.Count / 4 * 3)));
        isMovingHorizontally = currentDirection == Character.MoveDirection.left ||
                               currentDirection == Character.MoveDirection.right;
        Debug.LogError("AAAADSAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        Navigate();
        return this;
    }
    
    private void Navigate()
    {

        isMovingHorizontally = currentDirection == Character.MoveDirection.left ||
                               currentDirection == Character.MoveDirection.right;
        Debug.Log("Can't Move " + currentDirection);
        Debug.Log(character.CanMove(Character.MoveDirection.up) + " " +
                  character.CanMove(Character.MoveDirection.down) + " " +
                  character.CanMove(Character.MoveDirection.right) + " " +
                  character.CanMove(Character.MoveDirection.left)+ " ");
        

        if (isMovingHorizontally && character.CanMove(Character.MoveDirection.up))
        {
            Debug.Log("UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU");
            if (character.CanMove(Character.MoveDirection.down) == false &&
                character.CanMove(currentDirection) == false)
            {
                currentDirection = Character.MoveDirection.up;
                character.Move(Character.MoveDirection.up, Navigate);
                return;
            }
            if (Random.value > 0.5f)
            {
                currentDirection = Character.MoveDirection.up;
                character.Move(Character.MoveDirection.up, Navigate);
                return;
            }
        }

        if (isMovingHorizontally && character.CanMove(Character.MoveDirection.down))
        {
            Debug.Log("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            if (character.CanMove(currentDirection) == false)
            {
                currentDirection = Character.MoveDirection.down;
                character.Move(Character.MoveDirection.down, Navigate);
                return;
            }
            if (Random.value > 0.5f)
            {
                currentDirection = Character.MoveDirection.down;
                character.Move(Character.MoveDirection.down, Navigate);
                return;
            }
        }
        if (isMovingHorizontally == false && character.CanMove(Character.MoveDirection.right))
        {
            Debug.Log("RRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR");
            if (character.CanMove(Character.MoveDirection.left) == false &&
                character.CanMove(currentDirection) == false)
            {
                currentDirection = Character.MoveDirection.right;
                character.Move(Character.MoveDirection.right, Navigate);
                return;
            }
            if (Random.value > 0.5f)
            {
                currentDirection = Character.MoveDirection.right;
                character.Move(Character.MoveDirection.right, Navigate);
                return;
            }
        }
        
        if (isMovingHorizontally == false && character.CanMove(Character.MoveDirection.left))
        {
            Debug.Log("LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL");
            if (character.CanMove(currentDirection) == false)
            {
                currentDirection = Character.MoveDirection.left;
                character.Move(Character.MoveDirection.left, Navigate);
                return;
            }
            if (Random.value > 0.5f)
            {

                currentDirection = Character.MoveDirection.left;
                character.Move(Character.MoveDirection.left, Navigate);
                return;
            }
        }

        if (character.CanMove(currentDirection))
        {
            character.Move(currentDirection, Navigate);
        }
        else
        {
            if (isMovingHorizontally)
            {
                currentDirection = currentDirection == Character.MoveDirection.right ?
                    Character.MoveDirection.left :
                    Character.MoveDirection.right;
                character.Move(currentDirection, Navigate);
            }
            else
            {
                currentDirection = currentDirection == Character.MoveDirection.up ?
                    Character.MoveDirection.down :
                    Character.MoveDirection.up;
                character.Move(currentDirection, Navigate);
            }
        }
    }
}
