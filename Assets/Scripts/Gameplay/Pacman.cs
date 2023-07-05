using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pacman : MonoBehaviour
{
    private Character character;
    private bool isMovingHorizontally;
    private Character.MoveDirection currentDirection;
    
    public Pacman Setup(List<int[]> map)
    {
        gameObject.layer = LayerMask.NameToLayer("Pacman");
        var newMaterial = new Material(Shader.Find("Standard"));
        newMaterial.color = new Color(1f, 0.8f, 0.1f);
        gameObject.GetComponent<MeshRenderer>().material = newMaterial;
        gameObject.AddComponent<Rigidbody>().isKinematic = true;
        
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
        
        return this;
    }
    
    private void Update()
    {
        if (Game.isGameStarted == false &&
            (Input.GetKey("up")) ||
            Input.GetKey("down") ||
            Input.GetKey("left") ||
            Input.GetKey("right"))
        {
            Game.StartGame();
        }

        if (Game.isGameStarted == false)
        {
            return;
        }

        if (Input.GetKey("up"))
        {
            if ((character.CanMove(Character.MoveDirection.up) && character.IsCentered()) ||
                (isMovingHorizontally == false && character.IsCentered() == false))
            {
                currentDirection = Character.MoveDirection.up;
                isMovingHorizontally = false;
                character.Move(Character.MoveDirection.up);
                return;
            }
        }

        if (Input.GetKey("down"))
        {
            if ((character.CanMove(Character.MoveDirection.down) && character.IsCentered()) ||
                (isMovingHorizontally == false && character.IsCentered() == false))
            {
                currentDirection = Character.MoveDirection.down;
                isMovingHorizontally = false;
                character.Move(Character.MoveDirection.down);
                return;
            }
        }
        
        if (Input.GetKey("right"))
        {
            if ((character.CanMove(Character.MoveDirection.right) && character.IsCentered()) ||
                (isMovingHorizontally && character.IsCentered() == false))
            {
                currentDirection = Character.MoveDirection.right;
                isMovingHorizontally = true;
                character.Move(Character.MoveDirection.right);
                return;
            }
        }

        if (Input.GetKey("left"))
        {
            if ((character.CanMove(Character.MoveDirection.left) && character.IsCentered()) ||
                (isMovingHorizontally && character.IsCentered() == false))
            {
                currentDirection = Character.MoveDirection.left;
                isMovingHorizontally = true;
                character.Move(Character.MoveDirection.left);
                return;
            }
        }

        if (character.CanMove(currentDirection) || character.IsCentered() == false)
        {
            character.Move(currentDirection);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Dot"))
        {
            Destroy(other.gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ghost"))
        {
            Game.GameOver();
        }
    }
}
