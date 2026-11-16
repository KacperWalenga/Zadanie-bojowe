using UnityEngine;
using Random = UnityEngine.Random;

namespace ZB.Maze
{
    public class RandomRoomPlacer : IMazeGenerationStep
    {
        private const int MaxPlacementAttempts = 100;
        private readonly MazeRoom[] rooms;

        public RandomRoomPlacer(MazeRoom[] rooms)
        {
            this.rooms = rooms;
        }

        public void Apply(MazeData maze)
        {
            foreach (var room in rooms)
                PlaceRoom(maze, room);
        }

        private void PlaceRoom(MazeData maze, MazeRoom room)
        {
            var upperBound = maze.Size - room.Size + 1;

            for (var attempt = 0; attempt < MaxPlacementAttempts; attempt++)
            {
                var position = new Vector2Int(Random.Range(0, upperBound), Random.Range(0, upperBound));

                if (!maze.IsZoneFree(position, room.Size))
                    continue;

                maze.PlaceRoom(position, room);
                return;
            }
        }
    }
}
