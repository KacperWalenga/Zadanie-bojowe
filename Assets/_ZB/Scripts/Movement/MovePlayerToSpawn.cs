using UnityEngine;
using ZB.Maze;

namespace ZB
{
    public class MovePlayerToSpawn : MonoBehaviour
    {
        private Movement movement;
        private MazeBuilder mazeBuilder;
        
        private void Awake()
        {
            EventManager.AddListener(Consts.Events.MAZE_GENERATED, Teleport);

            movement = FindAnyObjectByType<Movement>();
            mazeBuilder = FindAnyObjectByType<MazeBuilder>();
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener(Consts.Events.MAZE_GENERATED, Teleport);
        }

        private void Teleport()
        {
            var position = movement.WorldToGrid(transform.position);
            
            movement.Initialize(mazeBuilder.Maze, position);
        }
    }
}