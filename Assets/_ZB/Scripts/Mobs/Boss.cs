using UnityEngine;
using ZB;
using ZB.Maze;
using ZB.Mobs;

public class Boss : MonoBehaviour
{
    [SerializeField] private MobData bossMobData;

    private Mob boss;
    private MazeBuilder mazeBuilder;

    private void Start()
    {
        boss = bossMobData.Spawn(Vector2Int.zero, transform);
        
        boss.MobKilled += BossKilled;
    }

    private void OnDestroy()
    {
        boss.MobKilled -= BossKilled;
    }

    private void BossKilled()
    {
        EventManager.Trigger(Consts.Events.Game.WON);
    }
}
