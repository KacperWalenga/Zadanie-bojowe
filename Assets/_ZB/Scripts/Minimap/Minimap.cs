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

    private Player player;
    
    private void Awake()
    {
        EventManager.AddListener(Consts.Events.PLAYER_MOVED, PlayerMoved);
        EventManager.AddListener(Consts.Events.PLAYER_ROTATED, PlayerRotated);
        
        var mazeBuilder = FindAnyObjectByType<MazeBuilder>();
        player = FindAnyObjectByType<Player>();
        
        SetupCamera(mazeBuilder.Size);
    }

    private void Start()
    {
        PlayerRotated();
        PlayerMoved();
    }

    private void OnDestroy()
    {
        EventManager.RemoveListener(Consts.Events.PLAYER_MOVED, PlayerMoved);
        EventManager.RemoveListener(Consts.Events.PLAYER_ROTATED, PlayerRotated);
    }

    private void PlayerRotated()
    {
        playerIndicator.transform.rotation = player.PlayerRotation;
    }

    private void PlayerMoved()
    {
        var playerPosition = player.PlayerPosition;
        var minimapPointPosition = new Vector3(playerPosition.x, 19, playerPosition.z);

        playerIndicator.transform.position = new Vector3(playerPosition.x, 20, playerPosition.z);

        if (IsAlreadyMarked(minimapPointPosition))
            return;
        
        Instantiate(minimapPointPrefab, minimapPointPosition, Quaternion.identity, transform);
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
