using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ZB.Maze
{
    public enum WallType
    {
        AIR = 0,
        CORRIDOR = 1,
        FLOOR_TRANSITION = 2,
        EXIT_DOOR = 3,
    }

    public class MazeData
    {
        private readonly WallType[] cells;

        public int Size { get; private set; }
        public Vector2Int StartPosition => Vector2Int.one;
        public Vector2Int? PreviousFloorPosition { get; private set; }
        public Vector2Int? NextFloorPosition { get; private set; }
        public Vector2Int? ExitDoorPosition { get; private set; }

        public MazeData(int size = 51)
        {
            Size = size;
            cells = new WallType[size * size];
        }

        public void SetSpecialCells(bool hasPreviousFloor, bool hasNextFloor)
        {
            var farthest = GetFarthestCell(StartPosition);

            if (hasNextFloor)
                NextFloorPosition = farthest;
            else
                ExitDoorPosition = farthest;

            if (hasPreviousFloor)
                PreviousFloorPosition = StartPosition;

            SetCellType(StartPosition, hasPreviousFloor ? WallType.FLOOR_TRANSITION : WallType.CORRIDOR);
            SetCellType(farthest, hasNextFloor ? WallType.FLOOR_TRANSITION : WallType.EXIT_DOOR);
        }
        
        public WallType GetCellType(Vector2Int position)
        {
            if (!IsInsideGrid(position))
                return WallType.AIR;

            var index = GetCellIndex(position);
            
            return cells[index];
        }

        public int GetCellIndex(Vector2Int position)
        {
            return position.y * Size + position.x;
        }

        public Vector2Int GetCellPosition(int index)
        {
            return new Vector2Int(index % Size, index / Size);
        }

        public bool IsInsideGrid(Vector2Int position)
        {
            return position.x >= 0 && position.y >= 0 && position.x < Size && position.y < Size;
        }

        public bool IsCellFree(Vector2Int position)
        {
            return GetCellType(position) == WallType.AIR;
        }

        public bool CanGenerateCorridor(Vector2Int position)
        {
            return IsInsideGrid(position) && IsCellFree(position);
        }

        public void SetCellType(Vector2Int position, WallType type)
        {
            if (!IsInsideGrid(position))
                return;

            var index = GetCellIndex(position);
            cells[index] = type;
        }

        public bool IsIntersection(Vector2Int position)
        {
            return CountNeighbors(position) >= 3;
        }

        public bool IsDeadEnd(Vector2Int position)
        {
            return CountNeighbors(position) == 1;
        }

        private int CountNeighbors(Vector2Int position)
        {
            var neighbors = 0;

            if (!IsCellFree(Vector2Int.up + position))
                neighbors += 1;

            if (!IsCellFree(Vector2Int.down + position))
                neighbors += 1;

            if (!IsCellFree(Vector2Int.left + position))
                neighbors += 1;

            if (!IsCellFree(Vector2Int.right + position))
                neighbors += 1;

            return neighbors;
        }

        private Vector2Int GetFarthestCell(Vector2Int start)
        {
            var directions = new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            var visited = new HashSet<Vector2Int> { start };
            var queue = new Queue<Vector2Int>();
            var farthest = start;

            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                farthest = queue.Dequeue();

                foreach (var direction in directions)
                {
                    var position = farthest + direction;

                    if (IsCellFree(position) || !visited.Add(position))
                        continue;

                    queue.Enqueue(position);
                }
            }
            
            return farthest;
        }
    }
}
