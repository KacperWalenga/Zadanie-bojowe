using UnityEngine;

namespace ZB.Maze
{
    [RequireComponent(typeof(MazeBuilder))]
    public class MazeObjectives : MonoBehaviour
    {
        [SerializeField] private GameObject exitDoorPrefab;
        [SerializeField] private GameObject beerCratePrefab;

        private MazeBuilder builder;
        private GameObject beerCrate;
        private bool beerCollected;
        private bool fatherFightRequested;

        private void Awake()
        {
            builder = GetComponent<MazeBuilder>();
            
            EventManager.AddListener(Consts.Events.MAZE_GENERATED, MazeGenerated);
            EventManager.AddListener<Vector2Int>(Consts.Events.PLAYER_MOVED, PlayerMoved);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener(Consts.Events.MAZE_GENERATED, MazeGenerated);
            EventManager.RemoveListener<Vector2Int>(Consts.Events.PLAYER_MOVED, PlayerMoved);
        }

        private void MazeGenerated()
        {
            var maze = builder.Maze;

            if (maze.GetCellType(maze.StartPosition) == WallType.EXIT_DOOR)
                Spawn(exitDoorPrefab, maze.StartPosition);

            if (maze.BeerCratePosition is Vector2Int position)
                beerCrate = Spawn(beerCratePrefab, position);
        }

        private GameObject Spawn(GameObject prefab, Vector2Int position)
        {
            var root = builder.FloorRoot;
            return Instantiate(prefab, builder.GridToWorld(position), root.rotation, root);
        }

        private void PlayerMoved(Vector2Int position)
        {
            var cellType = builder.Maze.GetCellType(position);

            if (cellType == WallType.BEER_CRATE)
                CollectBeer();
            else if (cellType == WallType.EXIT_DOOR)
                ReachExitDoor();
        }

        private void CollectBeer()
        {
            if (beerCollected)
                return;

            beerCollected = true;
            beerCrate.SetActive(false);
            
            EventManager.Trigger(Consts.Events.BEER_COLLECTED);
        }

        private void ReachExitDoor()
        {
            if (!beerCollected)
            {
                EventManager.Trigger(Consts.Events.EXIT_DOOR_LOCKED);
                return;
            }

            if (fatherFightRequested)
                return;

            fatherFightRequested = true;
            
            EventManager.Trigger(Consts.Events.FATHER_FIGHT_REQUESTED);
        }
    }
}
