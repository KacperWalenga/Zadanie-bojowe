using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ZB.Maze
{
    public class MazeBuilder : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private int size = 51;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private int roomPadding = 1;

        [Header("Rooms")]
        [SerializeField] private MazeRoom[] rooms;

        [Header("Prefabs")]
        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private RoofPattern[] roofs;
        [SerializeField] private GameObject torchPrefab;

        [Header("Generation")]
        [SerializeField] private Transform root;

        private MazeData maze;
        public MazeData Maze => maze;
        
        private void Start()
        {
            var steps = new IMazeGenerationStep[]
            {
                new RandomRoomPlacer(rooms),
                new RecursiveBacktrackingMazeGenerator(),
                new BfsRoomEntranceConnector()
            };

            maze = new MazeData(size, roomPadding);

            foreach (var step in steps)
                step.Apply(maze);

            BuildMaze();
            
            EventManager.Trigger(Consts.Events.MAZE_GENERATED);
        }
        
        private void BuildMaze()
        {
            foreach (var placedRoom in maze.Rooms)
                BuildRoom(placedRoom);

            for (var y = 0; y < maze.Size; y++)
            {
                for (var x = 0; x < maze.Size; x++)
                {
                    var position = new Vector2Int(x, y);

                    if (maze.GetCellType(position) != WallType.CORRIDOR)
                        continue;

                    BuildCorridorCell(position);
                }
            }
        }

        private void BuildRoom(PlacedRoom placedRoom)
        {
            var room = placedRoom.Room;
            var position = new Vector3(placedRoom.Position.x * cellSize, 0f, placedRoom.Position.y * cellSize);  
            
            Instantiate(room.Prefab, position, Quaternion.identity, root);
        }

        private void BuildCorridorCell(Vector2Int position)
        {
            var worldPosition = GridToWorld(position);

            Instantiate(floorPrefab, worldPosition, Quaternion.identity, root);
            
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
            
            Instantiate(wallPrefab, GetWallPosition(position, direction), GetWallRotation(direction), root);
            
            if (Random.Range(0f, 1f) <= 0.3f)
                Instantiate(torchPrefab, GetWallPosition(position, direction), GetWallRotation(direction), root);
        }
        
        private Vector3 GetWallPosition(Vector2Int cellPosition, Vector2Int direction)
        {
            var position = GridToWorld(cellPosition);
            var wallOffset = cellSize / 2f;
            
            return position + new Vector3(direction.x * wallOffset, 0f, direction.y * wallOffset);
        }

        private Quaternion GetWallRotation(Vector2Int outward)
        {
            var inward = new Vector3(-outward.x, 0f, -outward.y);
            return Quaternion.LookRotation(inward);
        }

        private Vector3 GridToWorld(Vector2Int position)
        {
            return new Vector3((position.x + 0.5f) * cellSize, 0f, (position.y + 0.5f) * cellSize);
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
            foreach (var pattern in roofs)
            {
                if (!MatchesPattern(position, pattern.pattern, out var rotation))
                    continue;

                Instantiate(
                    pattern.prefab,
                    GridToWorld(position),
                    rotation,
                    root
                );

                return;
            }
        }
    }
}
