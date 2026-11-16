using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ZB.Maze
{
    public class RecursiveBacktrackingMazeGenerator : IMazeGenerationStep
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
            var start = FindStart(maze);

            if (start.HasValue)
                CarveCorridors(maze, start.Value);
        }

        private Vector2Int? FindStart(MazeData maze)
        {
            Vector2Int? fallback = null;

            for (var index = 0; index < maze.Size * maze.Size; index++)
            {
                var position = new Vector2Int(index % maze.Size, index / maze.Size);

                if (maze.GetCellType(position) == WallType.ROOM)
                    continue;

                fallback ??= position;

                if (IsInteriorNode(maze, position) && maze.CanGenerateCorridor(position))
                    return position;
            }

            return fallback;
        }

        private bool IsInteriorNode(MazeData maze, Vector2Int position)
        {
            var isOdd = position.x % 2 == 1 && position.y % 2 == 1;

            return isOdd && position.x < maze.Size - 1 && position.y < maze.Size - 1;
        }

        private void CarveCorridors(MazeData maze, Vector2Int start)
        {
            var stack = new Stack<Vector2Int>();
            var moves = new List<Vector2Int>(directions.Length);

            maze.SetCellType(start, WallType.CORRIDOR);
            stack.Push(start);

            while (stack.Count > 0)
            {
                var current = stack.Peek();
                
                FindMoves(maze, current, moves);

                if (moves.Count == 0)
                {
                    stack.Pop();
                    continue;
                }

                var direction = moves[Random.Range(0, moves.Count)];
                var passage = current + direction;
                var nextNode = passage + direction;

                maze.SetCellType(passage, WallType.CORRIDOR);
                maze.SetCellType(nextNode, WallType.CORRIDOR);
                
                stack.Push(nextNode);
            }
        }

        private void FindMoves(MazeData maze, Vector2Int current, List<Vector2Int> moves)
        {
            moves.Clear();

            foreach (var direction in directions)
            {
                var passage = current + direction;
                var nextNode = passage + direction;

                if (maze.CanGenerateCorridor(passage) && maze.CanGenerateCorridor(nextNode))
                    moves.Add(direction);
            }
        }
    }
}
