using System;
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

    private MazeData maze;
    private Player player;
    
    private void Awake()
    {
        EventManager.AddListener<Vector2Int>(Consts.Events.PLAYER_MOVED, PlayerMoved);
        EventManager.AddListener(Consts.Events.PLAYER_ROTATED, PlayerRotated);
        EventManager.AddListener(Consts.Events.MAZE_GENERATED, SetupMaze);
    }

    private void SetupMaze()
    {
        var mazeBuilder = FindAnyObjectByType<MazeBuilder>();
        
        maze = mazeBuilder.Maze;
        player = FindAnyObjectByType<Player>();
        
        SetupCamera(mazeBuilder.Size);
        
        PlayerRotated();
        PlayerMoved(new Vector2Int((int)player.PlayerPosition.x, (int)player.PlayerPosition.z));
    }
    
    private void OnDestroy()
    {
        EventManager.RemoveListener<Vector2Int>(Consts.Events.PLAYER_MOVED, PlayerMoved);
        EventManager.RemoveListener(Consts.Events.PLAYER_ROTATED, PlayerRotated);
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

        if (IsAlreadyMarked(minimapPointPosition))
            return;
        
        var point = Instantiate(minimapPointPrefab, minimapPointPosition, Quaternion.identity, transform);

        if (maze.IsIntersection(position))
            point.GetComponent<Renderer>().material.color = Color.white;
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

    private bool IsAlreadyMarked(Vector3 position) => 
        Physics.CheckBox(position, new Vector3(0.4f, 0.4f, 0.4f));
}
