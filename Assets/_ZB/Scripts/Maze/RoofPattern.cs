using UnityEngine;

namespace ZB.Maze
{
    [CreateAssetMenu(fileName = "RoofPattern", menuName = "ZB/Maze/Roof Pattern")]
    public class RoofPattern : ScriptableObject
    {
        [TextArea(3, 5)]
        public string pattern;

        public GameObject prefab;
    }
}