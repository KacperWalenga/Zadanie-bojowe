using UnityEngine;

namespace ZB.Maze
{
    [CreateAssetMenu(fileName = "Room", menuName = "ZB/Room")]
    public class MazeRoom : ScriptableObject
    {
        public int Size = 9;
        public GameObject Prefab;
        public Vector2Int[] Doors;
    }
}