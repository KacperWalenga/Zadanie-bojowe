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
                
                maze.SetCellType(pos, WallType.CORRIDOR);
            }
        }
    }
}
