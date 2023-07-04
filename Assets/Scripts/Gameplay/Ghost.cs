using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Ghost : MonoBehaviour
{
    private const float HEURISTIC_FACTOR = 0.2f;
    private Transform packman;
    private Character character;
    private bool isMovingHorizontally;
    private Character.MoveDirection currentDirection;
    
    public Ghost Setup(Transform packman, List<int[]> map, Color color)
    {
        gameObject.layer = LayerMask.NameToLayer("Ghost");
        gameObject.GetComponent<Collider>().isTrigger = true;
        
        var newMaterial = new Material(Shader.Find("Standard"));
        newMaterial.color = color;
        gameObject.GetComponent<MeshRenderer>().material = newMaterial;
        this.packman = packman;
        float offset = MapUtils.IsMapEvenWidth ? 0.5f : 0f;
        character = gameObject.AddComponent<Character>()
            .Setup(map, new Vector3(0f + offset, 0f, -map.Count / 2f));
        isMovingHorizontally = currentDirection == Character.MoveDirection.left ||
                               currentDirection == Character.MoveDirection.right;
        Navigate();
        return this;
    }
    
    private void Navigate()
    {
        isMovingHorizontally = currentDirection == Character.MoveDirection.left ||
                               currentDirection == Character.MoveDirection.right;

        var packmanPosition = packman.position;
        var ghostPosition = transform.position;
        var verticalHeuristic = packmanPosition.z > ghostPosition.z ? HEURISTIC_FACTOR : -HEURISTIC_FACTOR;
        var horizontalHeuristic = packmanPosition.x > ghostPosition.x ? HEURISTIC_FACTOR : -HEURISTIC_FACTOR;
        
        if (isMovingHorizontally && character.CanMove(Character.MoveDirection.up))
        {
            if (character.CanMove(Character.MoveDirection.down) == false &&
                character.CanMove(currentDirection) == false)
            {
                currentDirection = Character.MoveDirection.up;
                character.Move(Character.MoveDirection.up, Navigate);
                return;
            }
            if (Random.value + verticalHeuristic > 0.5f)
            {
                currentDirection = Character.MoveDirection.up;
                character.Move(Character.MoveDirection.up, Navigate);
                return;
            }
        }

        if (isMovingHorizontally && character.CanMove(Character.MoveDirection.down))
        {
            if (character.CanMove(currentDirection) == false)
            {
                currentDirection = Character.MoveDirection.down;
                character.Move(Character.MoveDirection.down, Navigate);
                return;
            }
            if (Random.value - verticalHeuristic > 0.5f)
            {
                currentDirection = Character.MoveDirection.down;
                character.Move(Character.MoveDirection.down, Navigate);
                return;
            }
        }
        
        if (isMovingHorizontally == false && character.CanMove(Character.MoveDirection.right))
        {
            if (character.CanMove(Character.MoveDirection.left) == false &&
                character.CanMove(currentDirection) == false)
            {
                currentDirection = Character.MoveDirection.right;
                character.Move(Character.MoveDirection.right, Navigate);
                return;
            }
            if (Random.value + horizontalHeuristic > 0.5f)
            {
                currentDirection = Character.MoveDirection.right;
                character.Move(Character.MoveDirection.right, Navigate);
                return;
            }
        }
        
        if (isMovingHorizontally == false && character.CanMove(Character.MoveDirection.left))
        {
            if (character.CanMove(currentDirection) == false)
            {
                currentDirection = Character.MoveDirection.left;
                character.Move(Character.MoveDirection.left, Navigate);
                return;
            }
            if (Random.value - horizontalHeuristic > 0.5f)
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
