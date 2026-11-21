using UnityEngine;
using Random = UnityEngine.Random;

namespace ZB.Maze
{
    public class MazeBuilder : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField, Min(5)] private int size = 51;
        [SerializeField] private float cellSize = 1f;

        [Header("Floors")]
        [SerializeField] private MazeFloor[] floors;
        [SerializeField] private GameObject transitionFloorPrefab;

        [Header("Generation")]
        [SerializeField] private Transform root;

        private MazeData[] mazes;
        private GameObject[] floorObjects;
        private MazeData maze;
        private Movement movement;
        private int currentFloor = -1;
        private int? pendingFloor;

        public MazeData Maze => maze;
        public int Size => size;
        public Transform FloorRoot => floorObjects[currentFloor].transform;
        public Vector2Int ArrivalPosition { get; private set; }

        private MazeFloor Floor => floors[currentFloor];

        private void Awake()
        {
            EventManager.AddListener<Vector2Int>(Consts.Events.Player.MOVED, CheckFloorTransition);
        }

        private void Start()
        {
            movement = FindAnyObjectByType<Movement>();
            mazes = new MazeData[floors.Length];
            floorObjects = new GameObject[floors.Length];

            LoadFloor(0);
        }

        private void LateUpdate()
        {
            if (!pendingFloor.HasValue)
                return;

            var nextFloor = pendingFloor.Value;
            pendingFloor = null;
            
            LoadFloor(nextFloor);
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<Vector2Int>(Consts.Events.Player.MOVED, CheckFloorTransition);
        }

        private void CheckFloorTransition(Vector2Int position)
        {
            if (position == maze.NextFloorPosition)
                pendingFloor = currentFloor + 1;
            else if (position == maze.PreviousFloorPosition)
                pendingFloor = currentFloor - 1;
        }

        private void LoadFloor(int floor)
        {
            var previousFloor = currentFloor;

            if (previousFloor >= 0)
                floorObjects[previousFloor].SetActive(false);

            currentFloor = floor;
            var isNew = mazes[floor] == null;

            if (isNew)
                GenerateFloor(floor);

            maze = mazes[floor];
            floorObjects[floor].SetActive(true);
            ArrivalPosition = GetArrivalPosition(previousFloor);
            movement.Initialize(maze, ArrivalPosition);

            if (isNew)
                EventManager.Trigger(Consts.Events.Maze.GENERATED);

            EventManager.Trigger(Consts.Events.Maze.FLOOR_CHANGED, floor + 1);
        }

        private void GenerateFloor(int floor)
        {
            maze = new MazeData(size);
            new RecursiveBacktrackingMazeGenerator().Apply(maze);
            maze.SetSpecialCells(floor > 0, floor < floors.Length - 1);

            floorObjects[floor] = new GameObject($"Floor {floor + 1}");
            FloorRoot.SetParent(root, false);
            mazes[floor] = maze;
            
            BuildMaze();
        }

        private Vector2Int GetArrivalPosition(int previousFloor)
        {
            if (previousFloor < 0)
                return maze.StartPosition;

            return previousFloor < currentFloor ? maze.PreviousFloorPosition.Value : maze.NextFloorPosition.Value;
        }

        private void BuildMaze()
        {
            for (var y = 0; y < maze.Size; y++)
            {
                for (var x = 0; x < maze.Size; x++)
                {
                    var position = new Vector2Int(x, y);

                    if (maze.GetCellType(position) == WallType.AIR)
                        continue;

                    BuildCorridorCell(position);
                }
            }
        }

        private void BuildCorridorCell(Vector2Int position)
        {
            var worldPosition = GridToWorld(position);

            var cellType = maze.GetCellType(position);
            var floorPrefab = cellType == WallType.FLOOR_TRANSITION ? transitionFloorPrefab : Floor.floorPrefab;
            Instantiate(floorPrefab, worldPosition, FloorRoot.rotation, FloorRoot);

            
            BuildWall(position, Vector2Int.up);
            BuildWall(position, Vector2Int.right);
            BuildWall(position, Vector2Int.down);
            BuildWall(position, Vector2Int.left);

            BuildRoof(position);
        }

        private void BuildWall(Vector2Int position, Vector2Int direction)
        {
            var neighbour = position + direction;

            if (maze.GetCellType(neighbour) != WallType.AIR)
                return;
            
            var worldPosition = GetWallPosition(position, direction);
            var rotation = GetWallRotation(direction);
            
            Instantiate(Floor.wallPrefab, worldPosition, rotation, FloorRoot);

            if (Random.Range(0f, 1f) <= Floor.torchSpawnRate)
            {
                var torch = Instantiate(Floor.torchPrefab, worldPosition, rotation, FloorRoot);

                foreach (var light in torch.GetComponentsInChildren<Light>())
                    light.color = Floor.torchColor;
            }
        }

        private Vector3 GetWallPosition(Vector2Int cellPosition, Vector2Int direction)
        {
            var position = GridToWorld(cellPosition);
            var wallOffset = cellSize / 2f;
            
            var offset = new Vector3(direction.x * wallOffset, 0f, direction.y * wallOffset);
            
            return position + FloorRoot.TransformVector(offset);
        }

        private Quaternion GetWallRotation(Vector2Int outward)
        {
            var inward = new Vector3(-outward.x, 0f, -outward.y);
            
            return FloorRoot.rotation * Quaternion.LookRotation(inward);
        }

        internal Vector3 GridToWorld(Vector2Int position)
        {
            var local = new Vector3((position.x + 0.5f) * cellSize, 0f, (position.y + 0.5f) * cellSize);
            
            return FloorRoot.TransformPoint(local);
        }
        
        private bool MatchesPattern(Vector2Int position, string pattern, out Quaternion rotation)
        {
            var rows = pattern.Split('\n');

            for (var i = 0; i < 4; i++)
            {
                if (MatchesRotation(position, rows, i))
                {
                    rotation = Quaternion.Euler(0f, -i * 90f, 0f);
                    return true;
                }
            }

            rotation = Quaternion.identity;
            
            return false;
        }

        private bool MatchesRotation(Vector2Int position, string[] rows, int rotation)
        {
            for (var y = 0; y < 3; y++)
            {
                for (var x = 0; x < 3; x++)
                {
                    var symbol = rows[y][x];

                    if (symbol == 'X' || symbol == '?')
                        continue;

                    var offset = new Vector2Int(x - 1, 1 - y);

                    for (var i = 0; i < rotation; i++)
                        offset = new Vector2Int(-offset.y, offset.x);

                    var isAir = maze.GetCellType(position + offset) == WallType.AIR;

                    if (symbol == '#' && !isAir || symbol == '.' && isAir)
                        return false;
                }
            }

            return true;
        }

        private void BuildRoof(Vector2Int position)
        {
            foreach (var pattern in Floor.roofs)
            {
                if (!MatchesPattern(position, pattern.pattern, out var rotation))
                    continue;

                Instantiate(pattern.prefab, GridToWorld(position), FloorRoot.rotation * rotation, FloorRoot);

                return;
            }
        }
    }
}
