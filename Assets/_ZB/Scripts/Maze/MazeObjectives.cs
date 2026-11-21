using UnityEngine;

namespace ZB.Maze
{
    [RequireComponent(typeof(MazeBuilder))]
    public class MazeObjectives : MonoBehaviour
    {
        [SerializeField] private GameObject exitDoorPrefab;

        private MazeBuilder builder;
        private bool fatherFightRequested;

        private void Awake()
        {
            builder = GetComponent<MazeBuilder>();
            
            EventManager.AddListener(Consts.Events.Maze.GENERATED, MazeGenerated);
            EventManager.AddListener<Vector2Int>(Consts.Events.Player.MOVED, PlayerMoved);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener(Consts.Events.Maze.GENERATED, MazeGenerated);
            EventManager.RemoveListener<Vector2Int>(Consts.Events.Player.MOVED, PlayerMoved);
        }

        private void MazeGenerated()
        {
            var maze = builder.Maze;

            if (maze.GetCellType(maze.StartPosition) == WallType.EXIT_DOOR)
                Spawn(exitDoorPrefab, maze.StartPosition);
        }

        private GameObject Spawn(GameObject prefab, Vector2Int position)
        {
            var root = builder.FloorRoot;
            return Instantiate(prefab, builder.GridToWorld(position), root.rotation, root);
        }

        private void PlayerMoved(Vector2Int position)
        {
            var cellType = builder.Maze.GetCellType(position);

            if (cellType == WallType.EXIT_DOOR)
                ReachExitDoor();

            if (cellType == WallType.FLOOR_TRANSITION)
            {
                if (builder.IsFirstRealFloor)
                    EventManager.Trigger(Consts.Events.Objectives.BEER_COLLECTED);
                    
                builder.LoadNextFloor(position);
            }
        }

        private void ReachExitDoor()
        {
            if (fatherFightRequested)
                return;

            fatherFightRequested = true;
            
            EventManager.Trigger(Consts.Events.Objectives.FATHER_FIGHT_REQUESTED);
        }
    }
}
