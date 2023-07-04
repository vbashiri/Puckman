using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    [SerializeField] private Button button;
    private int GHOST_COUNT = 1;
    private Transform playground;

    void Start()
    {
        game = this;
        button.onClick.AddListener(SetupGame);
        SetupGame();
        
    }

    private void SetupGame()
    {
        if (playground != null)
        {
            Destroy(playground.gameObject);
        }
        playground = new GameObject().transform;
        playground.parent = transform;
        playground.gameObject.name = "Playground";
        
        List<int[]> map = SetupMap(playground);
        
        Packman packman = GameObject.CreatePrimitive(PrimitiveType.Sphere).AddComponent<Packman>().Setup(map);
        packman.transform.parent = playground;
        
        Color[] colors = new[] {Color.magenta, Color.red, Color.cyan, new Color(0.7f, 0.2f, 0.9f)};
        for (int i = 0; i < GHOST_COUNT; i++)
        {
            Ghost ghost = GameObject.CreatePrimitive(PrimitiveType.Sphere)
                .AddComponent<Ghost>()
                .Setup(packman.transform, map, colors[i % colors.Length]);
            ghost.transform.parent = playground;
        }
    }

    private List<int[]> SetupMap(Transform playground)
    {
        MapGenerator mapGenerator = new MapGenerator();
        MapDrawer mapDrawer = new GameObject().AddComponent<MapDrawer>();
        mapDrawer.transform.parent = playground;
        List<int[]> map = mapGenerator.SetupMap();
        mapDrawer.DrawMap(map);
        return map;
    }


    private static Game game;
    public static void GameOver()
    {
        if (game == null)
        {
            game = FindObjectOfType<Game>();
        }
        game.SetupGame();
    }

}
