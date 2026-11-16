
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZB.Maze.Editor
{
    public class RoomGeneratorWindow : EditorWindow
    {
        private const float CellSize = 1f;

        private const string FloorPath = "Assets/_ZB/Prefabs/Floor.prefab";
        private const string WallPath = "Assets/_ZB/Prefabs/Wall.prefab";
        private const string PrefabFolder = "Assets/_ZB/Prefabs/Rooms";
        private const string DataFolder = "Assets/_ZB/Data/Rooms";
        
        private TextField roomNameField;
        private IntegerField sizeField;

        [MenuItem("Tools/Room Generator")]
        public static void Open()
        {
            GetWindow<RoomGeneratorWindow>("Room Generator");
        }

        public void CreateGUI()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_ZB/Scripts/Maze/Editor/RoomGenerator.uxml");
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/_ZB/Scripts/Maze/Editor/RoomGenerator.uss");

            visualTree.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(styleSheet);
            
            roomNameField = rootVisualElement.Q<TextField>("room-name");
            sizeField = rootVisualElement.Q<IntegerField>("size");

            rootVisualElement.Q<Button>("generate-button").clicked += GenerateRoom;
        }
        
        private void GenerateRoom()
        {
            if (!ValidateInput())
                return;
            
            var roomObject = CreateRoomObject();
            var prefab = SaveRoomPrefab(roomObject);
            
            DestroyImmediate(roomObject);
            SaveRoomData(prefab);

            Debug.Log($"Room generated: {prefab.name}");
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(roomNameField.value))
            {
                Debug.LogError("name is empty");
                return false;
            }

            if (sizeField.value < 2)
            {
                Debug.LogError("size must be greater than two");
                return false;
            }

            return true;
        }

        private GameObject CreateRoomObject()
        {
            var roomObject = new GameObject(roomNameField.value.Trim());
            var floorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FloorPath);
            var wallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WallPath);
            var doors = GetDefaultDoors(sizeField.value);

            BuildFloor(roomObject.transform, floorPrefab, sizeField.value);
            BuildWalls(roomObject.transform, wallPrefab, sizeField.value, doors);

            return roomObject;
        }
        
        private List<Vector2Int> GetDefaultDoors(int size)
        {
            var middle = size / 2;

            return new List<Vector2Int>
            {
                new(middle, size), // N
                new(size, middle), // E
                new(middle, -1),   // S
                new(-1, middle)    // W
            };
        }

        private GameObject SaveRoomPrefab(GameObject roomObject)
        {
            var path = AssetDatabase.GenerateUniqueAssetPath($"{PrefabFolder}/{roomObject.name}.prefab");

            return PrefabUtility.SaveAsPrefabAsset(roomObject, path);
        }

        private void SaveRoomData(GameObject prefab)
        {
            var roomData = CreateInstance<MazeRoom>();

            roomData.Size = sizeField.value;
            roomData.Prefab = prefab;
            roomData.Doors = GetDefaultDoors(sizeField.value).ToArray();

            var path = AssetDatabase.GenerateUniqueAssetPath($"{DataFolder}/{roomData.name}.asset");

            roomData.name = prefab.name;
            
            path = AssetDatabase.GenerateUniqueAssetPath($"{DataFolder}/{roomData.name}.asset");

            AssetDatabase.CreateAsset(roomData, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        private void BuildFloor(Transform parent, GameObject floorPrefab, int size)
        {
            for (var x = 0; x < size; x++)
            {
                for (var z = 0; z < size; z++)
                {
                    var position = new Vector3((x + 0.5f) * CellSize, 0f, (z + 0.5f) * CellSize);
                    
                    CreatePart(floorPrefab, parent, position, Quaternion.identity);
                }
            }
        }
        
        private void BuildWalls(Transform parent, GameObject wallPrefab, int size, List<Vector2Int> doors)
        {
            BuildHorizontalWall(parent, wallPrefab, size, 0, doors);
            BuildHorizontalWall(parent, wallPrefab, size, size, doors);

            BuildVerticalWall(parent, wallPrefab, size, 0, doors);
            BuildVerticalWall(parent, wallPrefab, size, size, doors);
        }

        private void BuildHorizontalWall(Transform parent, GameObject wallPrefab, int size, int z, List<Vector2Int> doors)
        {
            for (var x = 0; x < size; x++)
            {
                if (doors.Contains(new Vector2Int(x, z == 0 ? -1 : size)))
                    continue;

                CreateWall(wallPrefab, parent, x, z, false);
            }
        }

        private void BuildVerticalWall(Transform parent, GameObject wallPrefab, int size, int x, List<Vector2Int> doors)
        {
            for (var z = 0; z < size; z++)
            {
                if (doors.Contains(new Vector2Int(x == 0 ? -1 : size, z)))
                    continue;

                CreateWall(wallPrefab, parent, x, z, true);
            }
        }

        private void CreateWall(GameObject wallPrefab, Transform parent, int x, int z, bool rotated)
        {
            var position = rotated
                ? new Vector3(x * CellSize, 0f, (z + 0.5f) * CellSize)
                : new Vector3((x + 0.5f) * CellSize, 0f, z * CellSize);

            var rotation = rotated
                ? Quaternion.Euler(0f, 90f, 0f)
                : Quaternion.identity;

            CreatePart(wallPrefab, parent, position, rotation);
        }

        private void CreatePart(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = position;
            instance.transform.localRotation = rotation;
        }
    }
}
