using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    [SerializeField] private Button button;
    private int GHOST_COUNT = 4;

    void Start()
    {
        button.onClick.AddListener(SetupGame);
        SetupGame();
        
    }

    private void SetupGame()
    {
        Transform playground = Instantiate(new GameObject()).transform;
        List<int[]> map = SetupMap(playground);
        
        Packman packman = GameObject.CreatePrimitive(PrimitiveType.Sphere).AddComponent<Packman>().Setup(map);
        packman.transform.parent = transform;
        
        for (int i = 0; i < GHOST_COUNT; i++)
        {
            Ghost ghost = GameObject.CreatePrimitive(PrimitiveType.Sphere)
                .AddComponent<Ghost>()
                .Setup(packman.transform, map);
            ghost.transform.parent = playground;
        }
    }

    private List<int[]> SetupMap(Transform playground)
    {
        MapGenerator mapGenerator = new MapGenerator();
        MapDrawer mapDrawer = Instantiate(new GameObject(), playground).AddComponent<MapDrawer>();
        List<int[]> map = mapGenerator.SetupMap();
        mapDrawer.DrawMap(map);
        return map;
    }

}
