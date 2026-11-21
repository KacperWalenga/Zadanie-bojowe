using System.Collections.Generic;
using UnityEngine;
using ZB;
using ZB.Maze;

public class Minimap : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private GameObject minimapPointPrefab;
    [SerializeField] private LayerMask minimapPointLayer; 
    [SerializeField] private LayerMask MazePointLayer;
    [SerializeField] private GameObject playerIndicator;

    private readonly Dictionary<int, (GameObject Root, HashSet<Vector2Int> Positions)> floors = new();

    private MazeData maze;
    private Player player;
    private (GameObject Root, HashSet<Vector2Int> Positions) currentFloor;
    
    private void Awake()
    {
        EventManager.AddListener<Vector2Int>(Consts.Events.Player.MOVED, PlayerMoved);
        EventManager.AddListener(Consts.Events.Player.ROTATED, PlayerRotated);
        EventManager.AddListener<int>(Consts.Events.Maze.FLOOR_CHANGED, SetupMaze);
    }

    private void SetupMaze(int floor)
    {
        var mazeBuilder = FindAnyObjectByType<MazeBuilder>();
        
        maze = mazeBuilder.Maze;
        player = FindAnyObjectByType<Player>();
        
        if (currentFloor.Root != null)
            currentFloor.Root.SetActive(false);

        if (!floors.TryGetValue(floor, out currentFloor))
        {
            var points = new GameObject($"Floor {floor}");
            
            points.transform.SetParent(transform, false);
            currentFloor = (points, new HashSet<Vector2Int>());
            floors.Add(floor, currentFloor);
        }

        currentFloor.Root.SetActive(true);
        SetupCamera(mazeBuilder.Size);
        
        PlayerRotated();
        PlayerMoved(mazeBuilder.ArrivalPosition);
    }
    
    private void OnDestroy()
    {
        EventManager.RemoveListener<Vector2Int>(Consts.Events.Player.MOVED, PlayerMoved);
        EventManager.RemoveListener(Consts.Events.Player.ROTATED, PlayerRotated);
        EventManager.RemoveListener<int>(Consts.Events.Maze.FLOOR_CHANGED, SetupMaze);
    }

    private void PlayerRotated()
    {
        playerIndicator.transform.rotation = player.PlayerRotation;
    }

    private void PlayerMoved(Vector2Int position)
    {
        var playerPosition = player.PlayerPosition;
        var minimapPointPosition = new Vector3(playerPosition.x, 19, playerPosition.z);

        playerIndicator.transform.position = new Vector3(playerPosition.x, 20, playerPosition.z);

        if (!currentFloor.Positions.Add(position))
            return;
        
        var point = Instantiate(minimapPointPrefab, minimapPointPosition, Quaternion.identity, currentFloor.Root.transform);
        var renderer = point.GetComponent<Renderer>();
        var cellType = maze.GetCellType(position);

        if (cellType == WallType.EXIT_DOOR)
            renderer.material.color = Color.cyan;
        else if (cellType == WallType.BEER_CRATE)
            renderer.material.color = new Color(0.75f, 0.35f, 1f);
        else if (position == maze.NextFloorPosition)
            renderer.material.color = new Color(1f, 0.75f, 0f);
        else if (maze.IsDeadEnd(position))
            renderer.material.color = Color.red;
        else if (maze.IsIntersection(position))
            renderer.material.color = Color.white;
    }

    private void SetupCamera(int mapSize)
    {
        camera.orthographicSize = mapSize / 2f;
        
        SetCameraPosition(mapSize);
    }
    
    private void SetCameraPosition(int mapSize)
    {
        var halfMapSize = mapSize / 2f;
        
        camera.transform.position = new Vector3(halfMapSize, 21, halfMapSize);
    }

}
