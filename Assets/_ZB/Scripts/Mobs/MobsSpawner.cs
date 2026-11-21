using System.Collections.Generic;
using UnityEngine;
using ZB.Maze;

namespace ZB.Mobs
{
    public class MobsSpawner : MonoBehaviour
    {
        [SerializeField] private int mobsCount;
        [SerializeField] private MobData[] mobDatas;

        bool beerColected = false;

        private void Awake()
        {
            EventManager.AddListener(Consts.Events.Maze.GENERATED, MazeGenerated);
            EventManager.AddListener(Consts.Events.Objectives.BEER_COLLECTED, BeerColected);
        }

        private void BeerColected()
        {
            beerColected = true;
            EventManager.AddListener(Consts.Events.Objectives.BEER_COLLECTED, BeerColected);
        }

        private void OnDestroy()
        {
            EventManager.AddListener(Consts.Events.Objectives.BEER_COLLECTED, BeerColected);
            EventManager.RemoveListener(Consts.Events.Maze.GENERATED, MazeGenerated);
        }

        private void MazeGenerated()
        {
            if (!beerColected)
                return;
                
            var mazeBuilder = FindAnyObjectByType<MazeBuilder>();
            var positions = GetSpawnPositions(mazeBuilder.Maze, mazeBuilder.ArrivalPosition);

            for (var i = 0; i < mobsCount && positions.Count > 0; i++)
            {
                var index = Random.Range(0, positions.Count);
                var mob = mobDatas[Random.Range(0, mobDatas.Length)];
                
                mob.Spawn(positions[index], mazeBuilder.FloorRoot);
                positions.RemoveAt(index);
            }
        }

        private List<Vector2Int> GetSpawnPositions(MazeData maze, Vector2Int arrival)
        {
            var positions = new List<Vector2Int>();

            for (var index = 0; index < maze.Size * maze.Size; index++)
            {
                var position = maze.GetCellPosition(index);

                if (position != arrival && maze.GetCellType(position) == WallType.CORRIDOR)
                    positions.Add(position);
            }

            return positions;
        }
    }
}
