using UnityEngine;
using ZB.Maze;
using ZB.Mobs;

public class BossSpawner : MonoBehaviour
{
    [SerializeField] private MobData bossMobData;
    
    private MazeBuilder mazeBuilder;

    private void Start()
    {
        bossMobData.Spawn(Vector2Int.zero, transform);
    }
}
