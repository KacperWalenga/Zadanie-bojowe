using System.Collections.Generic;
using UnityEngine;
using ZB.Maze;
using Random = System.Random;

namespace ZB.Mobs
{
    public class MobsSpawner : MonoBehaviour
    {
        [SerializeField] private int mobsCount;
        [SerializeField] private MobData[] mobDatas;
        
        private MazeData mazeData;
        private List<Vector2Int> occupiedPositions = new();

        private void Awake()
        {
            EventManager.AddListener(Consts.Events.MAZE_GENERATED, MazeGenerated);
        }

        private void MazeGenerated()
        {
            var mazeBuilder = FindAnyObjectByType<MazeBuilder>();
            mazeData = mazeBuilder.Maze;
            
            SpawnMobs();
        }

        private void SpawnMobs()
        {
            for (var i = 0; i < mobsCount; i++)
            {
                var mob = GetRandomMob();
                var position = GetRandomCorridorPosition();

                if (occupiedPositions.Contains(position))
                {
                    i--;
                    continue;
                }
                
                mob.Spawn(position);
                occupiedPositions.Add(position);
            }
        }

        private Vector2Int GetRandomCorridorPosition()
        {
            var randomCorridorIndex = mazeData.GetRandomCorridorIndex();
            return mazeData.GetCellPosition(randomCorridorIndex);
        }
        
        private MobData GetRandomMob()
        {
            var randomIndex = new Random().Next(mobDatas.Length);

            return mobDatas[randomIndex];
        }
    }
}