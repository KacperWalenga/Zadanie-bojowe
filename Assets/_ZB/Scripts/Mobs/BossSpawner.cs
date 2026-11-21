using UnityEngine;
using ZB;
using ZB.Maze;
using ZB.Mobs;

public class BossSpawner : MonoBehaviour
{
    [SerializeField] private MobData bossMobData;
    
    private MazeBuilder mazeBuilder;

    private void Awake()
    {
        EventManager.AddListener(Consts.Events.Maze.GENERATED, SpawnBoss);
    }
    
    private void SpawnBoss()
    {
        var position = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.z));
        bossMobData.Spawn(position, transform);
    }
}
