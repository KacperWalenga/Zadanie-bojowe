using System.Collections.Generic;
using UnityEngine;

namespace ZB.Maze
{
    public class BfsRoomEntranceConnector : IMazeGenerationStep
    {
        private static readonly Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        public void Apply(MazeData maze)
        {
            if (!maze.HasCorridors())
                return;

            foreach (var placedRoom in maze.Rooms)
                foreach (var door in placedRoom.Room.Doors)
                    ConnectEntrance(maze, placedRoom.Position + door);
        }

        private void ConnectEntrance(MazeData maze, Vector2Int entrance)
        {
            if (!CanTraverse(maze, entrance) || maze.GetCellType(entrance) == WallType.CORRIDOR)
                return;

            var queue = new Queue<Vector2Int>();
            var cameFrom = new Dictionary<Vector2Int, Vector2Int> { [entrance] = entrance };

            queue.Enqueue(entrance);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (maze.GetCellType(current) == WallType.CORRIDOR)
                {
                    MarkPath(maze, entrance, current, cameFrom);
                    return;
                }

                EnqueueNeighbours(maze, current, queue, cameFrom);
            }
        }

        private void EnqueueNeighbours(MazeData maze, Vector2Int current, Queue<Vector2Int> queue, Dictionary<Vector2Int, Vector2Int> cameFrom)
        {
            foreach (var direction in directions)
            {
                var next = current + direction;

                if (!CanTraverse(maze, next) || !cameFrom.TryAdd(next, current))
                    continue;

                queue.Enqueue(next);
            }
        }

        private bool CanTraverse(MazeData maze, Vector2Int position)
        {
            return maze.IsInsideGrid(position) && maze.GetCellType(position) != WallType.ROOM;
        }

        private void MarkPath(MazeData maze, Vector2Int entrance, Vector2Int current, Dictionary<Vector2Int, Vector2Int> cameFrom)
        {
            while (current != entrance)
            {
                current = cameFrom[current];
                maze.SetCellType(current, WallType.CORRIDOR);
            }
        }
    }
}
