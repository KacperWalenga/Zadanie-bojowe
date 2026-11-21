using UnityEngine;

namespace ZB.Maze
{
    public class SimpleMazeGenerator : IMazeGenerationStep
    {
        public void Apply(MazeData maze)
        {
            CarveCorridors(maze, maze.StartPosition, 5);
        }

        private void CarveCorridors(MazeData maze, Vector2Int start, int length)
        {
            maze.SetCellType(start, WallType.CORRIDOR);

            for (var i = 0; i < length; i++)
            {
                var pos = start + (i * Vector2Int.up);

                if (i == length / 2)
                    maze.SetCellType(pos, WallType.BEER_CRATE);
                else
                    maze.SetCellType(pos, WallType.CORRIDOR);
            }
        }
    }
}
