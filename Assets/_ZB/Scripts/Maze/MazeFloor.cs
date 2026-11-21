using System;
using UnityEngine;

namespace ZB.Maze
{
    [Serializable]
    public enum MazeAlgoritm
    {
        RB,
        Line
    }

    [Serializable]
    public class MazeFloor
    {
        public GameObject floorPrefab;
        public GameObject wallPrefab;
        public RoofPattern[] roofs;
        public GameObject torchPrefab;
        public float torchSpawnRate = 0.3f;
        public Color torchColor = new(1f, 0.6544963f, 0f);
        public MazeAlgoritm algoritm = MazeAlgoritm.RB;
    }
}
