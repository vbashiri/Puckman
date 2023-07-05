using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    [SerializeField] private Button button;
    private int GHOST_COUNT = 4;
    private Transform playground;
    private System.Action startListener;

    void Start()
    {
        gameManager = this;
        button.onClick.AddListener(GameOver);
        GameOver();
        
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
        
        
        Pacman pacman = GameObject.CreatePrimitive(PrimitiveType.Sphere).AddComponent<Pacman>().Setup(map);
        pacman.transform.parent = playground;
        
        Color[] colors = new[] {Color.magenta, Color.red, Color.cyan, new Color(0.7f, 0.2f, 0.9f)};
        for (int i = 0; i < GHOST_COUNT; i++)
        {
            Ghost ghost = GameObject.CreatePrimitive(PrimitiveType.Sphere)
                .AddComponent<Ghost>()
                .Setup(pacman.transform, map, colors[i % colors.Length]);
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

    //////////////////////////////////////////////////////
    /// <summary>
    /// Statics
    /// </summary>
    //////////////////////////////////////////////////////

    public static bool isGameStarted;
    private static Game gameManager;

    public static Game GameManager
    {
        get
        {
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<Game>();
            }

            return gameManager;
        }
    }
    public static void GameOver()
    {
        isGameStarted = false;
        GameManager.startListener = null;
        if (Mathf.Min(MapUtils.MapHorizontalSize, MapUtils.MapVerticalSize) < 5)
            return;
        GameManager.SetupGame();
    }

    public static void StartGame()
    {
        isGameStarted = true;
        GameManager.startListener?.Invoke();
    }

    public static void AddStartGameListener(System.Action action)
    {
        GameManager.startListener += action;
    }

}
